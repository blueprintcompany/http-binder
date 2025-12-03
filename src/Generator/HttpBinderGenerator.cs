using Blueprint.HttpBinder.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

[assembly: InternalsVisibleTo("Generator.Tests")]

namespace Blueprint.HttpBinder
{
    [Generator]
    public sealed class HttpBinderGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var candidateTypes = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeConstants.HttpBinderAttribute,
                static (node, _) => node is TypeDeclarationSyntax,
                (syntaxCtx, cancellationToken) => BuildModel((INamedTypeSymbol?)syntaxCtx.TargetSymbol, syntaxCtx.Attributes[0], cancellationToken)
            ).WithTrackingName(TrackingConstants.InitialExtraction)
            .Where(static m => m is not null)
            .Select(static (m, _) => m!)
            .WithTrackingName(TrackingConstants.RemovingNulls);

            context.RegisterSourceOutput(candidateTypes, static (sourceProductionContext, model) =>
            {
                var source = CodeRenderer.Render(model);
                sourceProductionContext.AddSource($"{model.Name}.g.cs", source);
            });
        }

        private static IEnumerable<IPropertySymbol> GetAllInstanceProperties(INamedTypeSymbol type)
        {
            var map = new Dictionary<string, IPropertySymbol>(StringComparer.OrdinalIgnoreCase);

            for (var current = type; current is not null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
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

        private static BoundType? BuildModel(INamedTypeSymbol? typeSymbol, AttributeData attributeData, CancellationToken cancellation)
        {
            if (typeSymbol is null)
            {
                return null;
            }

            cancellation.ThrowIfCancellationRequested();

            var classHttpBinderType = HttpBinderType.Form;

            if (attributeData.TryGetBinderType(out var parsedBinderType))
            {
                classHttpBinderType = parsedBinderType;
            }

            var properties = GetAllInstanceProperties(typeSymbol)
                .Select(p => BuildBoundProperty(p, classHttpBinderType))
                .ToImmutableArray();

            // Choose the "largest" public constructor, if any
            var ctors = typeSymbol.InstanceConstructors
                .Where(c => c.DeclaredAccessibility == Accessibility.Public)
                .OrderByDescending(c => c.Parameters.Length)
                .ToList();

            var ctor = ctors.FirstOrDefault(c => c.Parameters.Length > 0);

            var ctorParameterNames = new ImmutableArray<string>();

            if (ctor is not null)
            {
                var names = ImmutableArray.CreateBuilder<string>(ctor.Parameters.Length);

                foreach (var parameter in ctor.Parameters)
                {
                    names.Add(parameter.Name);
                }

                ctorParameterNames = names.MoveToImmutable();
            }

            var @namespace = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
            var name = typeSymbol.Name;
            var fullName = typeSymbol.GetFullTypeName();

            return new BoundType(
                name,
                @namespace,
                fullName,
                classHttpBinderType,
                properties,
                ctorParameterNames);
        }

        private static BoundProperty BuildBoundProperty(IPropertySymbol propertySymbol, HttpBinderType parentHttpBinderType)
        {
            var httpBinderType = parentHttpBinderType;
            var type = propertySymbol.Type;

            if (type.IsDictionary() || type.IsNestedCollection())
            {
                return BoundProperty.Ignore(propertySymbol.Name, httpBinderType);
            }

            var keyName = propertySymbol.Name;
            var attributes = propertySymbol.GetAttributes();

            foreach (var attribute in attributes)
            {
                var attrName = attribute.AttributeClass?.ToDisplayString();
                if (attrName is null)
                    continue;

                if (attrName == AttributeConstants.BindFromIgnoreAttribute)
                {
                    return BoundProperty.Ignore(propertySymbol.Name, httpBinderType);
                }

                if (attrName == AttributeConstants.BindFromAttribute)
                {
                    attribute.TryGetBinderType(out var parsedBinderType);
                    httpBinderType = parsedBinderType;

                    foreach (var namedArgument in attribute.NamedArguments)
                    {
                        if (namedArgument.Key == nameof(BindFromAttribute.Name) && namedArgument.Value.Value is string newName)
                        {
                            keyName = newName;
                        }
                    }
                }
            }

            (bool isNullable,
             bool isGuid,
             bool isEnum,
             bool isPrimitive,
             bool isString,
             bool isComplex,
             bool isFormFile) = type.GetPropertyAttributes();

            var collectionType = type.FindCollectionType();
            var isCollection = collectionType is not null;
            var typeName = isCollection ? collectionType!.ToDisplayString() : type.ToDisplayString();

            ImmutableArray<BoundProperty> children = [];

            if (!isFormFile)
            {
                if (isCollection)
                {
                    (bool _,
                         bool collectionTypeIsGuid,
                         bool collectionTypeIsEnum,
                         bool collectionTypeIsPrimitive,
                         bool collectionTypeIsString,
                         bool collectionTypeIsComplex,
                         bool collectionTypeIsFormFile) = collectionType!.GetPropertyAttributes();

                    if (collectionTypeIsComplex && collectionType is INamedTypeSymbol elementNamed)
                    {
                        children = [.. GetAllInstanceProperties(elementNamed).Select(ps => BuildBoundProperty(ps, httpBinderType))];
                    }
                }
                else if (!isCollection && isComplex && type is INamedTypeSymbol typeNamed)
                {
                    children = [.. GetAllInstanceProperties(typeNamed).Select(ps => BuildBoundProperty(ps, httpBinderType))];
                }
            }

            return new BoundProperty(
                Name: propertySymbol.Name,
                KeyName: keyName,
                TypeName: typeName,
                DeclaredTypeName: type.ToDisplayString(),
                HttpBinderType: httpBinderType,
                IsNullable: isNullable,
                IsCollection: isCollection,
                IsEnum: isEnum,
                IsGuid: isGuid,
                IsPrimitive: isPrimitive,
                IsString: isString,
                IsComplex: isComplex,
                IsFormFile: isFormFile,
                IsIgnored: false,
                ChildProperties: children);
        }
    }
}
