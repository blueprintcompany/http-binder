using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("HttpBinder.Generator.Tests")]

namespace HttpBinder.Generator
{
    // TODO:
    // - Support ignoring attribute
    // - Support IFormFile and IFormFileCollection (add analyzer too)
    [Generator]
    public sealed class HttpBinderGenerator : IIncrementalGenerator
    {
        private static readonly DiagnosticDescriptor _complexQueryOrRouteComplexType = new(
            id: "HB001",
            title: "Complex types cannot be bound from query or route",
            messageFormat: "Property '{0}' on type '{1}' is a complex type and cannot be bound from {2}. Use HttpBinderType.Form or [BindFromForm] instead.",
            category: "HttpBinder",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddEmbeddedAttributeDefinition();
                ctx.AddSource(AttributeHelpers.Name, SourceText.From(AttributeHelpers.Source, Encoding.UTF8));
            });

            var candidateTypes = context.SyntaxProvider.ForAttributeWithMetadataName(
                "HttpBinder.Generator.HttpBinderAttribute",
                static (node, _) => node is TypeDeclarationSyntax,
                static (syntaxCtx, _) => (INamedTypeSymbol)syntaxCtx.TargetSymbol
            );

            var models = candidateTypes.Select((type, _) => BuildModel(type));

            // Emit generated files + diagnostics
            context.RegisterSourceOutput(models, static (sourceProductionContext, model) =>
            {
                ReportComplexQueryRouteDiagnostics(sourceProductionContext, model);

                var source = CodeRenderer.Render(model);
                var hintName = GetHintName(model.TypeSymbol);
                sourceProductionContext.AddSource(hintName, source);
            });

            static string GetHintName(INamedTypeSymbol type)
            {
                var name = type.Name.Replace('<', '_').Replace('>', '_');
                return $"{name}.g.cs";
            }
        }

        private static void ReportComplexQueryRouteDiagnostics(SourceProductionContext context, BoundType model)
        {
            foreach (var prop in model.Properties)
            {
                if (prop.HttpBinderType is HttpBinderType.Query or HttpBinderType.Route)
                {
                    if (!prop.IsComplex)
                        continue;

                    var location = prop.Symbol.Locations.FirstOrDefault() ?? Location.None;
                    var sourceText = prop.HttpBinderType == HttpBinderType.Query ? "the query string" : "route data";

                    var diag = Diagnostic.Create(
                        _complexQueryOrRouteComplexType,
                        location,
                        prop.Name,
                        model.TypeSymbol.Name,
                        sourceText);

                    context.ReportDiagnostic(diag);
                }
            }
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
            // Find the [HttpBinder] attribute on the type
            var binderAttr = typeSymbol.GetAttributes()
                .FirstOrDefault(a =>
                    a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    == "global::HttpBinder.Generator.HttpBinderAttribute");

            var classDefaultSource = HttpBinderType.Form;

            if (binderAttr is not null)
            {
                foreach (var na in binderAttr.NamedArguments)
                {
                    if (na.Key == "HttpBinderType")
                    {
                        var tc = na.Value;

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
            var boundProperties = propertySymbols.Select(BuildBoundProperty).ToList();

            // Apply default source if property has no override
            foreach (var properties in boundProperties)
            {
                if (properties.HttpBinderType == HttpBinderType.Form && classDefaultSource != HttpBinderType.Form)
                {
                    bool hasOverride = properties.Symbol.GetAttributes()
                        .Any(a => Utilities.GetFullTypeName(a.AttributeClass!)
                                  == "global::HttpBinder.BindFromAttribute");

                    if (!hasOverride)
                        properties.HttpBinderType = classDefaultSource;
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
                foreach (var param in ctor.Parameters)
                {
                    var match = boundProperties.FirstOrDefault(p =>
                        string.Equals(p.Name, param.Name, StringComparison.OrdinalIgnoreCase));

                    if (match is not null)
                        ctorMap.Add((param, match));
                }
            }

            return new BoundType(typeSymbol, boundProperties, ctor, ctorMap);
        }

        private static BoundProperty BuildBoundProperty(IPropertySymbol propSymbol)
        {
            var httpBinderType = HttpBinderType.Form;

            string? customName = null;

            foreach (var attr in propSymbol.GetAttributes())
            {
                var fullName = Utilities.GetFullTypeName(attr.AttributeClass!);

                if (fullName == "global::HttpBinder.BindFromAttribute")
                {
                    if (attr.ConstructorArguments[0].Value is int raw)
                    {
                        httpBinderType = raw switch
                        {
                            1 => HttpBinderType.Query,
                            2 => HttpBinderType.Route,
                            _ => HttpBinderType.Form
                        };
                    }

                    foreach (var named in attr.NamedArguments)
                    {
                        if (named.Key == "Name" && named.Value.Value is string s)
                            customName = s;
                    }
                }
            }

            var keyName = customName ?? propSymbol.Name;

            var type = propSymbol.Type;
            (bool isNullable, bool isEnum, bool isGuid, bool isPrimitive, bool isString, bool isComplex) = GetPropertyAttributes(type);

            // Detect collections
            var isCollection = false;
            ITypeSymbol? elementType = null;

            if (propSymbol.Type is IArrayTypeSymbol arr)
            {
                isCollection = true;
                elementType = arr.ElementType;
            }
            else if (propSymbol.Type is INamedTypeSymbol named &&
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
                    isCollection = true;
                    elementType = named.TypeArguments[0];
                }
            }

            List<BoundProperty>? children = null;

            if (!isCollection)
            {
                if (isComplex && type is INamedTypeSymbol typeNamed)
                {
                    children = [.. GetAllInstanceProperties(typeNamed).Select(BuildBoundProperty)];
                }
            }
            else
            {
                if (elementType is not null)
                {
                    (bool _, bool elementIsEnum, bool elementIsGuid, bool elementIsPrimitive, bool elementIsString, bool elementIsComplex) = GetPropertyAttributes(elementType);

                    if (elementIsComplex && elementType is INamedTypeSymbol elementNamed)
                    {
                        children = [.. GetAllInstanceProperties(elementNamed).Select(BuildBoundProperty)];
                    }
                }
                else
                {
                    isComplex = false;
                }
            }

            return new BoundProperty(
                propSymbol,
                keyName,
                httpBinderType,
                isNullable,
                isCollection,
                elementType,
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
                    SpecialType.System_Boolean or SpecialType.System_Byte or SpecialType.System_SByte
                    or SpecialType.System_Int16 or SpecialType.System_UInt16
                    or SpecialType.System_Int32 or SpecialType.System_UInt32
                    or SpecialType.System_Int64 or SpecialType.System_UInt64
                    or SpecialType.System_Single or SpecialType.System_Double
                    or SpecialType.System_Decimal;

                var isComplex = !(isPrimitive || isString || isGuid || isEnum);

                return (isNullable, isGuid, isEnum, isPrimitive, isString, isComplex);
            }
        }
    }
}
