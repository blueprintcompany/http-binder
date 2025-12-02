using Blueprint.HttpBinder.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Generator.Tests")]

namespace Blueprint.HttpBinder
{
    // TODO:
    // - Support ignoring attribute
    // - Support IFormFile and IFormFileCollection (add analyzer too for not being valid unless form)
    // - Analyzer for lists of lists.
    // - Fix complex object analyzer
    // - Caching - https://andrewlock.net/creating-a-source-generator-part-10-testing-your-incremental-generator-pipeline-outputs-are-cacheable/
    [Generator]
    public sealed class HttpBinderGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddEmbeddedAttributeDefinition();
                ctx.AddSource(AttributeHelpers.Name, SourceText.From(AttributeHelpers.Source, Encoding.UTF8));
            });

            var registryProvider = context.CompilationProvider.Select((compilation, _) => new AttributeRegistry(compilation));

            var candidateTypes = context.SyntaxProvider.ForAttributeWithMetadataName(
                "Blueprint.HttpBinder.HttpBinderAttribute",
                static (node, _) => node is TypeDeclarationSyntax,
                static (syntaxCtx, _) => (INamedTypeSymbol)syntaxCtx.TargetSymbol
            );

            var models = candidateTypes
                .Combine(registryProvider)
                .Select((tuple, _) => BuildModel(tuple.Left, tuple.Right));

            // Emit generated files + diagnostics
            context.RegisterSourceOutput(models, static (sourceProductionContext, model) =>
            {
                ComplexTypeDetectedOnRouteOrQueryBinderAnalyzer.ReportDiagnostics(sourceProductionContext, model);
                DictionaryTypeNotSupportedAnalyzer.ReportDiagnostics(sourceProductionContext, model);

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

        private static BoundType BuildModel(INamedTypeSymbol typeSymbol, AttributeRegistry registry)
        {
            // Find the [HttpBinder] attribute on the type
            var binderAttr = typeSymbol.GetAttributes().FirstOrDefault(a =>
                SymbolEqualityComparer.Default.Equals(a.AttributeClass, registry.HttpBinderAttribute));

            var classDefaultSource = HttpBinderType.Form;

            if (binderAttr is not null)
            {
                foreach (var namedArgument in binderAttr.NamedArguments)
                {
                    if (namedArgument.Key == nameof(HttpBinderType))
                    {
                        var tc = namedArgument.Value;

                        if (tc.Value is int raw)
                        {
                            classDefaultSource = raw switch
                            {
                                1 => HttpBinderType.Query,
                                2 => HttpBinderType.Route,
                                _ => HttpBinderType.Form
                            };
                        }

                        break;
                    }
                }
            }

            // Collect properties (including base classes)
            var propertySymbols = GetAllInstanceProperties(typeSymbol);

            // Build per-property models
            var boundProperties = propertySymbols
                .Select(p => BuildBoundProperty(p, registry))
                .ToList();

            // Apply default source if property has no override
            foreach (var prop in boundProperties)
            {
                if (prop.HttpBinderType == HttpBinderType.Form &&
                    classDefaultSource != HttpBinderType.Form)
                {
                    bool hasOverride = prop.Symbol.GetAttributes().Any(a =>
                        SymbolEqualityComparer.Default.Equals(a.AttributeClass, registry.BindFromAttribute));

                    if (!hasOverride)
                        prop.HttpBinderType = classDefaultSource;
                }
            }

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

        private static BoundProperty BuildBoundProperty(IPropertySymbol propertySymbol, AttributeRegistry registry)
        {
            var httpBinderType = HttpBinderType.Form;

            var keyName = propertySymbol.Name;

            var attributes = propertySymbol.GetAttributes();
            foreach (var attribute in attributes)
            {
                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, registry.BindFromAttribute))
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
                        if (namedArgument.Key == "Name" && namedArgument.Value.Value is string newName)
                            keyName = newName;
                    }
                }
            }

            var type = propertySymbol.Type;
            (bool isNullable, bool isGuid, bool isEnum, bool isPrimitive, bool isString, bool isComplex) = GetPropertyAttributes(type);

            // Detect collections
            var collectionType = FindCollectionType(type);
            var isCollection = collectionType is not null;

            List<BoundProperty>? children = null;

            if (!isCollection)
            {
                if (isComplex && type is INamedTypeSymbol typeNamed)
                {
                    children = [.. GetAllInstanceProperties(typeNamed).Select(ps => BuildBoundProperty(ps, registry))];
                }
            }
            else
            {
                if (collectionType is not null)
                {
                    (bool _, bool collectionTypeIsGuid, bool collectionTypeIsEnum, bool collectionTypeIsPrimitive, bool collectionTypeIsString, bool collectionTypeIsComplex) = GetPropertyAttributes(collectionType);

                    if (collectionTypeIsComplex && collectionType is INamedTypeSymbol elementNamed)
                    {
                        children = [.. GetAllInstanceProperties(elementNamed).Select(ps => BuildBoundProperty(ps, registry))];
                    }
                }
                else
                {
                    isComplex = false;
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
                children
            );

            static (bool isNullable, bool isGuid, bool isEnum, bool isPrimitive, bool isString, bool isComplex) GetPropertyAttributes(ITypeSymbol typeSymbol)
            {
                var isNullable = typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;

                if (typeSymbol is INamedTypeSymbol namedTypeSymbol &&
                    namedTypeSymbol.IsGenericType &&
                    namedTypeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                {
                    typeSymbol = namedTypeSymbol.TypeArguments[0];
                    isNullable = true;
                }

                var isString = typeSymbol.SpecialType == SpecialType.System_String;
                var isEnum = typeSymbol.TypeKind == TypeKind.Enum;
                var isGuid = Utilities.GetFullTypeName(typeSymbol) == "global::System.Guid";
                var isPrimitive = typeSymbol.SpecialType is
                    SpecialType.System_Boolean or
                    SpecialType.System_Byte or
                    SpecialType.System_SByte or
                    SpecialType.System_Int16 or
                    SpecialType.System_UInt16 or
                    SpecialType.System_Int32 or
                    SpecialType.System_UInt32 or
                    SpecialType.System_Int64 or
                    SpecialType.System_UInt64 or
                    SpecialType.System_Single or
                    SpecialType.System_Double or
                    SpecialType.System_Decimal or
                    SpecialType.System_Char or
                    SpecialType.System_String;

                var isComplex = !(isPrimitive || isString || isGuid || isEnum);

                return (isNullable, isGuid, isEnum, isPrimitive, isString, isComplex);
            }

            static ITypeSymbol? FindCollectionType(ITypeSymbol type)
            {
                if (type is IArrayTypeSymbol arr)
                {
                    return arr.ElementType;
                }
                else if (type is INamedTypeSymbol named &&
                         named.IsGenericType &&
                         named.TypeArguments.Length == 1)
                {
                    var baseName = named.ConstructedFrom.ToDisplayString();

                    if (baseName.StartsWith("System.Collections.Generic.IEnumerable", StringComparison.Ordinal) ||
                        baseName.StartsWith("System.Collections.Generic.IList", StringComparison.Ordinal) ||
                        baseName.StartsWith("System.Collections.Generic.List", StringComparison.Ordinal) ||
                        baseName.StartsWith("System.Collections.Generic.ICollection", StringComparison.Ordinal) ||
                        baseName.StartsWith("System.Collections.Generic.IReadOnlyCollection", StringComparison.Ordinal) ||
                        baseName.StartsWith("System.Collections.Generic.IReadOnlyList", StringComparison.Ordinal))
                    {
                        return named.TypeArguments[0];
                    }
                }

                return null;
            }
        }
    }
}
