using Blueprint.HttpBinder.Extensions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blueprint.HttpBinder;

internal static class BindingModelBuilder
{
    public static IEnumerable<IPropertySymbol> GetAllViableProperties(INamedTypeSymbol type)
    {
        var map = new Dictionary<string, IPropertySymbol>(StringComparer.OrdinalIgnoreCase);

        for (var current = type; current is not null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
        {
            foreach (var propertySymbol in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (propertySymbol.IsStatic)
                    continue;

                if (propertySymbol.DeclaredAccessibility is not Accessibility.Public
                    and not Accessibility.Protected
                    and not Accessibility.ProtectedOrInternal)
                    continue;

                if (propertySymbol.SetMethod is null)
                    continue;

                if (!map.ContainsKey(propertySymbol.Name))
                    map[propertySymbol.Name] = propertySymbol;
            }
        }

        return [.. map.Values];
    }

    public static bool BaseDefinesBindAsync(INamedTypeSymbol type)
    {
        for (var b = type.BaseType; b is not null && b.SpecialType != SpecialType.System_Object; b = b.BaseType)
        {
            if (b.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == AttributeConstants.HttpBinderAttribute)) return true;
            
            foreach (var m in b.GetMembers("BindAsync").OfType<IMethodSymbol>())
            {
                if (!m.IsStatic) continue;
                if (m.Parameters.Length != 1) continue;
                if (m.Parameters[0].Type.ToDisplayString() != "Microsoft.AspNetCore.Http.HttpContext") continue;

                return true;
            }
        }
        return false;
    }

    public static BoundProperty? BuildBoundProperty(
        IPropertySymbol propertySymbol,
        HttpBinderType parentHttpBinderType,
        HashSet<string> recursionGuard,
        EquatableArray<string> ctorParameterNames)
    {
        var httpBinderType = parentHttpBinderType;
        var type = propertySymbol.Type;

        var declaredTypeName = type.ToDisplayString();

        // If we have already seen this type in the current recursion chain, ignore it to prevent infinite loops.
        if (!recursionGuard.Add(declaredTypeName))
        {
            return null;
        }

        // Dictionaries and nested collections are out of scope
        if (type.IsDictionary() || type.IsNestedCollection())
        {
            return null;
        }

        var keyName = propertySymbol.Name;
        var attributes = propertySymbol.GetAttributes();

        foreach (var attribute in attributes)
        {
            var attrName = attribute.AttributeClass?.ToDisplayString();
            switch (attrName)
            {
                case null:
                    continue;
                case AttributeConstants.BindFromIgnoreAttribute:
                    return null;
                case AttributeConstants.BindFromAttribute:
                {
                    attribute.TryGetBinderType(out var parsedBinderType);
                    httpBinderType = parsedBinderType;

                    foreach (var namedArgument in attribute.NamedArguments)
                    {
                        if (namedArgument.Key == nameof(BindFromAttribute.Name) &&
                            namedArgument.Value.Value is string newName)
                        {
                            keyName = newName;
                        }
                    }

                    break;
                }
            }

        }

        var collectionElementType = type.FindCollectionType();
        var isCollection = collectionElementType is not null;

        // Decide which symbol to classify as "scalar" (property type or element type)
        var scalarType = isCollection ? collectionElementType! : type;

        // Scalar classification (no collection awareness)
        var scalarInfo = scalarType.GetScalarTypeInfo();
        var isNullable = scalarInfo.IsNullable;

        // Ensure collection-level nullability is treated as non-nullable scalar
        if (isCollection)
        {
            isNullable = false;
        }

        var children = new EquatableArray<BoundProperty>();

        // Build children only for complex types.
        if (scalarInfo.IsReferenceType)
        {
            var referenceType = isCollection ? collectionElementType as INamedTypeSymbol : type as INamedTypeSymbol;

            var properties = GetAllViableProperties(referenceType!)
               .Select(ps => BuildBoundProperty(ps, httpBinderType, recursionGuard, ctorParameterNames))
               .Where(bp => bp is not null)
               .Select(bp => bp!)
               .ToArray();

            children = new(properties);
        }

        // Remove from recursion guard as we unwind.
        recursionGuard.Remove(declaredTypeName);

        return new(
            propertySymbol.Name,
            keyName,
            declaredTypeName,
            httpBinderType,
            scalarInfo,
            isNullable,
            isCollection,
            ctorParameterNames.Contains(propertySymbol.Name),
            children);
    }
}