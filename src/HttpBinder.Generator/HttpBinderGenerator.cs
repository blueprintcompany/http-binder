using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpBinder.Generator
{
    /// <summary>
    /// Main implementation of the HttpBinder incremental source generator. This
    /// generator scans the compilation for types annotated with
    /// <c>[HttpBinder.HttpBinder]</c> and emits a partial class with a
    /// <c>BindAsync(HttpContext)</c> method that binds values from route,
    /// query and form sources.
    /// </summary>
    [Generator]
    public sealed class HttpBinderGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context
                .RegisterPostInitializationOutput(ctx =>
                {
                    ctx.AddEmbeddedAttributeDefinition();
                    foreach (var attribute in AttributeHelpers.List)
                    {
                        ctx.AddSource(attribute.FileName, SourceText.From(attribute.Source, Encoding.UTF8));
                    }
                });


            // Discover candidate types annotated with [HttpBinder]
            var candidateTypes = context.SyntaxProvider.ForAttributeWithMetadataName(
                "HttpBinder.Generator.HttpBinderAttribute",
                static (node, ct) => node is TypeDeclarationSyntax,
                static (ctx, ct) =>
                {
                    var typeDecl = (TypeDeclarationSyntax)ctx.TargetNode;
                    var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
                    return symbol;
                });

            // Build a bound model for each candidate type
            var modelProvider = candidateTypes.Select((typeSymbol, ct) => BuildModel(typeSymbol));

            // Generate source for each model
            context.RegisterSourceOutput(modelProvider, (spc, model) =>
            {
                var source = CodeRenderer.Render(model);
                var hintName = $"HttpBinder.{model.TypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace('.', '_')}.g.cs";
                spc.AddSource(hintName, source);
            });
        }

        /// <summary>
        /// Constructs a bound model describing the target type. This includes
        /// collecting all properties (including inherited ones), determining
        /// source information via attributes, identifying primitive or complex
        /// types and choosing an appropriate constructor for initialisation.
        /// </summary>
        private static BoundType BuildModel(INamedTypeSymbol typeSymbol)
        {
            // Gather properties from the type and its base types. Derived
            // properties override base definitions by name (case insensitive).
            var propertyMap = new Dictionary<string, IPropertySymbol>(StringComparer.OrdinalIgnoreCase);
            for (var current = typeSymbol; current != null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
            {
                foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
                {
                    if (member.IsStatic) continue;
                    // Accept public or protected properties
                    if (member.DeclaredAccessibility != Accessibility.Public
                        && member.DeclaredAccessibility != Accessibility.Protected
                        && member.DeclaredAccessibility != Accessibility.ProtectedOrInternal)
                        continue;
                    if (!propertyMap.ContainsKey(member.Name))
                    {
                        propertyMap[member.Name] = member;
                    }
                }
            }
            var boundProperties = new List<BoundProperty>();
            foreach (var propSymbol in propertyMap.Values)
            {
                boundProperties.Add(BuildBoundProperty(propSymbol));
            }
            // Determine the constructor used for initialisation. Prefer the
            // constructor with the largest parameter count. If no public
            // constructor exists or all constructors are parameterless then
            // return null and the binder will instantiate via the default
            // constructor.
            IMethodSymbol? chosenCtor = null;
            var publicConstructors = typeSymbol.InstanceConstructors.Where(c => c.DeclaredAccessibility == Accessibility.Public).ToList();
            var paramCtors = publicConstructors.Where(c => c.Parameters.Length > 0).ToList();
            if (paramCtors.Count > 0)
            {
                chosenCtor = paramCtors.OrderByDescending(c => c.Parameters.Length).First();
            }
            // Map constructor parameters to bound properties by name
            var constructorMapping = new List<(IParameterSymbol parameter, BoundProperty property)>();
            if (chosenCtor != null)
            {
                foreach (var param in chosenCtor.Parameters)
                {
                    var match = boundProperties.FirstOrDefault(p => string.Equals(p.Name, param.Name, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        constructorMapping.Add((param, match));
                    }
                }
            }
            return new BoundType(typeSymbol, boundProperties, chosenCtor, constructorMapping);
        }

        /// <summary>
        /// Builds a <see cref="BoundProperty"/> for the given property symbol.
        /// This method examines custom attributes to determine the source and
        /// key name, evaluates type characteristics such as primitives,
        /// enumerations, GUIDs and collections, and recursively discovers
        /// nested child properties for complex types.
        /// </summary>
        private static BoundProperty BuildBoundProperty(IPropertySymbol propSymbol)
        {
            // Determine source kind via attributes
            var source = SourceKind.Form; // default
            var attrs = propSymbol.GetAttributes();
            bool hasFromForm = false, hasFromQuery = false, hasFromRoute = false;
            string? customName = null;
            foreach (var attr in attrs)
            {
                var attrName = attr.AttributeClass?.ToDisplayString();
                switch (attrName)
                {
                    case "HttpBinder.BindFromFormAttribute":
                        hasFromForm = true;
                        break;
                    case "HttpBinder.BindFromQueryAttribute":
                        hasFromQuery = true;
                        break;
                    case "HttpBinder.BindFromRouteAttribute":
                        hasFromRoute = true;
                        break;
                    case "HttpBinder.BindFormFieldAttribute":
                        if (attr.ConstructorArguments.Length == 1 && attr.ConstructorArguments[0].Value is string s)
                        {
                            customName = s;
                        }
                        break;
                }
            }
            if (hasFromQuery) source = SourceKind.Query;
            else if (hasFromRoute) source = SourceKind.Route;
            else if (hasFromForm) source = SourceKind.Form;
            var keyName = customName ?? propSymbol.Name;
            // Determine type characteristics
            var type = propSymbol.Type;
            bool isNullable = propSymbol.NullableAnnotation == NullableAnnotation.Annotated;
            // Nullable<T> struct is considered nullable even if NullableAnnotation.None
            if (type is INamedTypeSymbol namedType && namedType.IsGenericType && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                isNullable = true;
                type = namedType.TypeArguments[0];
            }
            bool isString = type.SpecialType == SpecialType.System_String;
            var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            bool isGuid = fullName == "global::System.Guid";
            bool isEnum = type.TypeKind == TypeKind.Enum;

            bool isPrimitive = false;
            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_Byte:
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Decimal:
                    isPrimitive = true;
                    break;
                default:
                    break;
            }
            // Determine collection and element type
            bool isCollection = false;
            ITypeSymbol? elementType = null;
            if (propSymbol.Type is IArrayTypeSymbol arrayType)
            {
                isCollection = true;
                elementType = arrayType.ElementType;
            }
            else if (propSymbol.Type is INamedTypeSymbol named && named.IsGenericType && named.TypeArguments.Length == 1)
            {
                var typeName = named.ConstructedFrom.ToDisplayString();
                // Accept List<T>, IList<T>, ICollection<T>, IEnumerable<T>, IReadOnlyList<T>, IReadOnlyCollection<T>
                if (typeName.StartsWith("System.Collections.Generic.IEnumerable") || typeName.StartsWith("System.Collections.Generic.IList") || typeName.StartsWith("System.Collections.Generic.List") || typeName.StartsWith("System.Collections.Generic.ICollection") || typeName.StartsWith("System.Collections.Generic.IReadOnlyCollection") || typeName.StartsWith("System.Collections.Generic.IReadOnlyList"))
                {
                    isCollection = true;
                    elementType = named.TypeArguments[0];
                }
            }

            bool elementIsComplex = false;
            List<BoundProperty>? children = null;
            if (isCollection && elementType != null)
            {
                // Determine element characteristics
                var elemFullName = elementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                bool elementIsString = elementType.SpecialType == SpecialType.System_String;
                bool elementIsGuid = elemFullName == "global::System.Guid";
                bool elementIsEnum = elementType.TypeKind == TypeKind.Enum;
                if (elementType is INamedTypeSymbol ntElem && ntElem.IsGenericType && ntElem.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                {
                    var underlying = ntElem.TypeArguments[0];
                    elementType = underlying;
                    elementIsString = underlying.SpecialType == SpecialType.System_String;
                    elementIsGuid = underlying.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Guid";
                    elementIsEnum = underlying.TypeKind == TypeKind.Enum;
                }
                bool elementIsPrimitive = false;
                switch (elementType.SpecialType)
                {
                    case SpecialType.System_Boolean:
                    case SpecialType.System_Byte:
                    case SpecialType.System_SByte:
                    case SpecialType.System_Int16:
                    case SpecialType.System_UInt16:
                    case SpecialType.System_Int32:
                    case SpecialType.System_UInt32:
                    case SpecialType.System_Int64:
                    case SpecialType.System_UInt64:
                    case SpecialType.System_Single:
                    case SpecialType.System_Double:
                    case SpecialType.System_Decimal:
                        elementIsPrimitive = true;
                        break;
                    default:
                        break;
                }
                elementIsComplex = !elementIsPrimitive && !elementIsString && !elementIsGuid && !elementIsEnum;
            }
            bool isComplex;
            if (!isCollection)
            {
                // Determine if type is complex (not primitive/string/guid/enum)
                isComplex = !isPrimitive && !isString && !isGuid && !isEnum;
            }
            else
            {
                // For collection properties we use elementIsComplex to indicate complexity
                isComplex = elementIsComplex;
            }
            // Recursively build child properties for complex types
            if (isComplex && !isCollection)
            {
                // Build children for complex non-collection types
                var complexType = propSymbol.Type;
                // If nullable<T> we should use underlying type
                if (propSymbol.Type is INamedTypeSymbol nt && nt.IsGenericType && nt.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                {
                    complexType = nt.TypeArguments[0];
                }
                var complexTypeSymbol = (INamedTypeSymbol)complexType;
                children = new List<BoundProperty>();
                // iterate properties of complex type
                var complexPropertyMap = new Dictionary<string, IPropertySymbol>(StringComparer.OrdinalIgnoreCase);
                for (var current = complexTypeSymbol; current != null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
                {
                    foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
                    {
                        if (member.IsStatic) continue;
                        if (member.DeclaredAccessibility != Accessibility.Public && member.DeclaredAccessibility != Accessibility.Protected && member.DeclaredAccessibility != Accessibility.ProtectedOrInternal)
                            continue;
                        if (!complexPropertyMap.ContainsKey(member.Name))
                        {
                            complexPropertyMap[member.Name] = member;
                        }
                    }
                }
                foreach (var p in complexPropertyMap.Values)
                {
                    children.Add(BuildBoundProperty(p));
                }
            }
            else if (isComplex && isCollection && elementType != null)
            {
                // Build children for complex element types inside collection
                var elemNamed = (INamedTypeSymbol)elementType;
                children = new List<BoundProperty>();
                var complexPropertyMap = new Dictionary<string, IPropertySymbol>(StringComparer.OrdinalIgnoreCase);
                for (var current = elemNamed; current != null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
                {
                    foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
                    {
                        if (member.IsStatic) continue;
                        if (member.DeclaredAccessibility != Accessibility.Public && member.DeclaredAccessibility != Accessibility.Protected && member.DeclaredAccessibility != Accessibility.ProtectedOrInternal)
                            continue;
                        if (!complexPropertyMap.ContainsKey(member.Name))
                        {
                            complexPropertyMap[member.Name] = member;
                        }
                    }
                }
                foreach (var p in complexPropertyMap.Values)
                {
                    children.Add(BuildBoundProperty(p));
                }
            }
            return new BoundProperty(propSymbol, keyName, source, isNullable, isCollection, elementType, isEnum, isGuid, isPrimitive, isString, isComplex, children);
        }
    }
}