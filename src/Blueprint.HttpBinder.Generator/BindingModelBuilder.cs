using Blueprint.HttpBinder.Extensions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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


    public static BoundProperty BuildBoundProperty(
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
            return BoundProperty.Ignore(propertySymbol.Name, httpBinderType);
        }

        // Dictionaries and nested collections are out of scope
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
                    if (namedArgument.Key == nameof(BindFromAttribute.Name) &&
                        namedArgument.Value.Value is string newName)
                    {
                        keyName = newName;
                    }
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
        if (isCollection && scalarInfo.IsReferenceType && collectionElementType is INamedTypeSymbol elementNamed)
        {
            children = new EquatableArray<BoundProperty>(
                [.. GetAllViableProperties(elementNamed).Select(ps => BuildBoundProperty(ps, httpBinderType, recursionGuard, ctorParameterNames))]);
        }
        else if (!isCollection && scalarInfo.IsReferenceType && type is INamedTypeSymbol typeNamed)
        {
            children = new EquatableArray<BoundProperty>(
                [.. GetAllViableProperties(typeNamed).Select(ps => BuildBoundProperty(ps, httpBinderType, recursionGuard, ctorParameterNames))]);
        }

        // Remove from recursion guard as we unwind.
        recursionGuard.Remove(declaredTypeName);

        return new BoundProperty(
            Name: propertySymbol.Name,
            KeyName: keyName,
            DeclaredTypeName: declaredTypeName,
            HttpBinderType: httpBinderType,
            IsNullable: isNullable,
            IsCollection: isCollection,
            IsConstructorParameter: ctorParameterNames.Contains(propertySymbol.Name),
            ScalarType: scalarInfo,
            IsIgnored: false,
            ChildProperties: children);
    }
}