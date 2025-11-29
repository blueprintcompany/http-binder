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
            // Add embedded attributes (EmbeddedAttribute + HttpBinderAttribute)
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddEmbeddedAttributeDefinition();
                ctx.AddSource(AttributeHelpers.Name, SourceText.From(AttributeHelpers.Source, Encoding.UTF8));
            });

            // Find attributed types
            var candidateTypes = context.SyntaxProvider.ForAttributeWithMetadataName(
                "HttpBinder.Generator.HttpBinderAttribute",
                static (node, _) => node is TypeDeclarationSyntax,
                static (syntaxCtx, _) => (INamedTypeSymbol)syntaxCtx.TargetSymbol
            );

            // Build models
            var models = candidateTypes.Select((type, _) => BuildModel(type));

            // Emit generated files + diagnostics
            context.RegisterSourceOutput(models, static (spc, model) =>
            {
                ReportComplexQueryRouteDiagnostics(spc, model);

                var source = CodeRenderer.Render(model); // your existing renderer
                spc.AddSource(GetHintName(model.TypeSymbol), source);
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
                if (prop.Source is SourceKind.Query or SourceKind.Route)
                {
                    if (!prop.IsComplex)
                        continue;

                    var location = prop.Symbol.Locations.FirstOrDefault() ?? Location.None;
                    var sourceText = prop.Source == SourceKind.Query ? "the query string" : "route data";

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

        // -----------------------------------------------------------------
        // Property collection helpers
        // -----------------------------------------------------------------

        private static IEnumerable<IPropertySymbol> GetAllInstanceProperties(INamedTypeSymbol type)
        {
            var map = new Dictionary<string, IPropertySymbol>(StringComparer.OrdinalIgnoreCase);

            for (var current = type;
                 current != null && current.SpecialType != SpecialType.System_Object;
                 current = current.BaseType)
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

            var classDefaultSource = SourceKind.Form;

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
                                1 => SourceKind.Query,
                                2 => SourceKind.Route,
                                _ => SourceKind.Form
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
            foreach (var prop in boundProperties)
            {
                if (prop.Source == SourceKind.Form && classDefaultSource != SourceKind.Form)
                {
                    bool hasOverride = prop.Symbol.GetAttributes()
                        .Any(a => a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                  is "global::HttpBinder.BindFromFormAttribute"
                                  or "global::HttpBinder.BindFromQueryAttribute"
                                  or "global::HttpBinder.BindFromRouteAttribute");

                    if (!hasOverride)
                        prop.Source = classDefaultSource;
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
            // Detect attribute overrides
            var source = SourceKind.Form;
            string? customName = null;

            foreach (var attr in propSymbol.GetAttributes())
            {
                var name = attr.AttributeClass?.ToDisplayString();

                switch (name)
                {
                    case "HttpBinder.BindFromFormAttribute":
                        source = SourceKind.Form;
                        break;

                    case "HttpBinder.BindFromQueryAttribute":
                        source = SourceKind.Query;
                        break;

                    case "HttpBinder.BindFromRouteAttribute":
                        source = SourceKind.Route;
                        break;

                    case "HttpBinder.BindFormFieldAttribute":
                        if (attr.ConstructorArguments.Length == 1 &&
                            attr.ConstructorArguments[0].Value is string s)
                        {
                            customName = s;
                        }
                        break;
                }
            }

            var keyName = customName ?? propSymbol.Name;

            // Determine core type info
            var type = propSymbol.Type;
            var isNullable = propSymbol.NullableAnnotation == NullableAnnotation.Annotated;

            if (type is INamedTypeSymbol nt &&
                nt.IsGenericType &&
                nt.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                type = nt.TypeArguments[0];
                isNullable = true;
            }

            var isString = type.SpecialType == SpecialType.System_String;
            var isEnum = type.TypeKind == TypeKind.Enum;
            var isGuid = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Guid";

            var isPrimitive = type.SpecialType is
                SpecialType.System_Boolean or SpecialType.System_Byte or SpecialType.System_SByte
                or SpecialType.System_Int16 or SpecialType.System_UInt16
                or SpecialType.System_Int32 or SpecialType.System_UInt32
                or SpecialType.System_Int64 or SpecialType.System_UInt64
                or SpecialType.System_Single or SpecialType.System_Double
                or SpecialType.System_Decimal;

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

            bool isComplex;
            List<BoundProperty>? children = null;

            if (!isCollection)
            {
                isComplex = !(isPrimitive || isString || isGuid || isEnum);

                if (isComplex && type is INamedTypeSymbol typeNamed)
                {
                    children = GetAllInstanceProperties(typeNamed)
                        .Select(BuildBoundProperty)
                        .ToList();
                }
            }
            else
            {
                bool elementIsPrimitive = false;
                bool elementIsString = false;
                bool elementIsGuid = false;
                bool elementIsEnum = false;

                if (elementType is not null)
                {
                    if (elementType is INamedTypeSymbol nte &&
                        nte.IsGenericType &&
                        nte.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                    {
                        elementType = nte.TypeArguments[0];
                    }

                    elementIsString = elementType.SpecialType == SpecialType.System_String;
                    elementIsEnum = elementType.TypeKind == TypeKind.Enum;
                    elementIsGuid = elementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Guid";
                    elementIsPrimitive = elementType.SpecialType is
                        SpecialType.System_Boolean or SpecialType.System_Byte or SpecialType.System_SByte
                        or SpecialType.System_Int16 or SpecialType.System_UInt16
                        or SpecialType.System_Int32 or SpecialType.System_UInt32
                        or SpecialType.System_Int64 or SpecialType.System_UInt64
                        or SpecialType.System_Single or SpecialType.System_Double
                        or SpecialType.System_Decimal;

                    isComplex = !(elementIsPrimitive || elementIsString || elementIsGuid || elementIsEnum);

                    if (isComplex && elementType is INamedTypeSymbol elementNamed)
                    {
                        children = GetAllInstanceProperties(elementNamed)
                            .Select(BuildBoundProperty)
                            .ToList();
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
                source,
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
        }
    }
}
