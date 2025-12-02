using Blueprint.HttpBinder.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Generator.Tests")]

namespace Blueprint.HttpBinder
{
    // TODO:
    // - Support ignoring attribute
    // - Fix complex object analyzer
    // - Caching - https://andrewlock.net/creating-a-source-generator-part-10-testing-your-incremental-generator-pipeline-outputs-are-cacheable/
    [Generator]
    public sealed class HttpBinderGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var candidateTypes = context.SyntaxProvider.ForAttributeWithMetadataName(
                "Blueprint.HttpBinder.HttpBinderAttribute",
                static (node, _) => node is TypeDeclarationSyntax,
                static (syntaxCtx, _) => (INamedTypeSymbol)syntaxCtx.TargetSymbol
            ).WithTrackingName(TrackingConstants.InitialExtraction)
            .Where(static m => m is not null)
            .WithTrackingName(TrackingConstants.RemovingNulls);

            var models = candidateTypes.Select(
                static (typeSymbol, _) => BuildModel(typeSymbol)
            );

            // Emit generated files + diagnostics
            context.RegisterSourceOutput(models, static (sourceProductionContext, model) =>
            {
                ComplexTypeDetectedOnRouteOrQueryBinderAnalyzer.ReportDiagnostics(sourceProductionContext, model);
                DictionaryTypeNotSupportedAnalyzer.ReportDiagnostics(sourceProductionContext, model);
                NestedCollectionsNotSupportedAnalyzer.ReportDiagnostics(sourceProductionContext, model);
                FormFilesDetectedOnRouteOrQueryBinderAnalyzer.ReportDiagnostics(sourceProductionContext, model);
                BindFromIgnoreDetectedWithOtherAttributesAnalyzer.ReportDiagnostics(sourceProductionContext, model);

                var source = CodeRenderer.Render(model);
                sourceProductionContext.AddSource($"{model.TypeSymbol.Name}.g.cs", source);
            });
        }

        private static IEnumerable<IPropertySymbol> GetAllInstanceProperties(INamedTypeSymbol type)
        {
            var map = new Dictionary<string, IPropertySymbol>(StringComparer.OrdinalIgnoreCase);

            for (var current = type; current != null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
            {
                foreach (var p in current.GetMembers().OfType<IPropertySymbol>())
                {
                    if (p.IsStatic)
                        continue;

                    if (p.DeclaredAccessibility is not Accessibility.Public
                        and not Accessibility.Protected
                        and not Accessibility.ProtectedOrInternal)
                        continue;

                    if (!map.ContainsKey(p.Name))
                        map[p.Name] = p;
                }
            }

            return map.Values;
        }

        private static BoundType BuildModel(INamedTypeSymbol typeSymbol)
        {
            var httpBinderAttribute = typeSymbol.GetAttributes().First(a => a.AttributeClass!.GetFullTypeName() == AttributeConstants.HttpBinderAttribute);

            var classHttpBinderType = HttpBinderType.Form;

            foreach (var namedArgument in httpBinderAttribute.NamedArguments)
            {
                if (namedArgument.Key == "HttpBinderType")
                {
                    var tc = namedArgument.Value;

                    if (tc.Value is int raw)
                    {
                        classHttpBinderType = raw switch
                        {
                            1 => HttpBinderType.Query,
                            2 => HttpBinderType.Route,
                            _ => HttpBinderType.Form
                        };
                    }

                    break;
                }
            }

            var propertySymbols = GetAllInstanceProperties(typeSymbol);

            var boundProperties = propertySymbols
                .Select(p => BuildBoundProperty(p, classHttpBinderType))
                .ToList();

            // Pick the "largest" public constructor
            var ctors = typeSymbol.InstanceConstructors
                .Where(c => c.DeclaredAccessibility == Accessibility.Public)
                .OrderByDescending(c => c.Parameters.Length)
                .ToList();

            var ctor = ctors.FirstOrDefault(c => c.Parameters.Length > 0);

            var ctorMap = new List<(IParameterSymbol parameter, BoundProperty property)>();

            if (ctor is not null)
            {
                foreach (var parameter in ctor.Parameters)
                {
                    var match = boundProperties.FirstOrDefault(p =>
                        string.Equals(p.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));

                    if (match is not null)
                        ctorMap.Add((parameter, match));
                }
            }

            return new BoundType(typeSymbol, boundProperties, ctor, ctorMap);
        }

        private static BoundProperty BuildBoundProperty(IPropertySymbol propertySymbol, HttpBinderType parentHttpBinderType)
        {
            var httpBinderType = parentHttpBinderType;
            var type = propertySymbol.Type;

            if (type.IsDictionary() || type.IsNestedCollection())
            {
                return BoundProperty.Ignore(propertySymbol, httpBinderType);
            }

            var keyName = propertySymbol.Name;
            var attributes = propertySymbol.GetAttributes();
            foreach (var attribute in attributes)
            {
                if (attribute.AttributeClass!.GetFullTypeName() == AttributeConstants.BindFromIgnoreAttribute)
                    return BoundProperty.Ignore(propertySymbol, httpBinderType);

                if (attribute.AttributeClass!.GetFullTypeName() == AttributeConstants.BindFromAttribute)
                {
                    if (attribute.ConstructorArguments[0].Value is int raw)
                    {
                        httpBinderType = raw switch
                        {
                            1 => HttpBinderType.Query,
                            2 => HttpBinderType.Route,
                            _ => HttpBinderType.Form
                        };
                    }

                    foreach (var namedArgument in attribute.NamedArguments)
                    {
                        if (namedArgument.Key == nameof(BindFromAttribute.Name) && namedArgument.Value.Value is string newName)
                            keyName = newName;
                    }
                }
            }

            (bool isNullable, bool isGuid, bool isEnum, bool isPrimitive, bool isString, bool isComplex, bool isFormFile) = type.GetPropertyAttributes();

            var collectionType = type.FindCollectionType();
            var isCollection = collectionType is not null;

            List<BoundProperty>? children = null;

            if (isCollection)
            {
                (bool _, bool collectionTypeIsGuid, bool collectionTypeIsEnum, bool collectionTypeIsPrimitive, bool collectionTypeIsString, bool collectionTypeIsComplex, bool collectionTypeIsFormFile) = collectionType!.GetPropertyAttributes();

                if (collectionTypeIsComplex && collectionType is INamedTypeSymbol elementNamed)
                {
                    children = [.. GetAllInstanceProperties(elementNamed).Select(ps => BuildBoundProperty(ps, httpBinderType))];
                }
            }
            else
            {
                if (isComplex && type is INamedTypeSymbol typeNamed)
                {
                    children = [.. GetAllInstanceProperties(typeNamed).Select(ps => BuildBoundProperty(ps, httpBinderType))];
                }
            }

            return new BoundProperty(
                propertySymbol,
                keyName,
                httpBinderType,
                isNullable,
                isCollection,
                collectionType,
                isEnum,
                isGuid,
                isPrimitive,
                isString,
                isComplex,
                isFormFile,
                false,
                children
            );
        }
    }
}
