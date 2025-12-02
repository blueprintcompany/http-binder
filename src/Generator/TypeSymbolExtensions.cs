using Microsoft.CodeAnalysis;
using System;

namespace Blueprint.HttpBinder;

internal static class TypeSymbolExtensions
{
    extension(ITypeSymbol typeSymbol)
    {
        public string GetFullTypeName()
        => typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        public string GetMinimalTypeName()
            => typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        public bool IsDictionary()
        {
            if (typeSymbol is INamedTypeSymbol named && named.IsGenericType)
            {
                var baseType = named.ConstructedFrom;

                return baseType.ToDisplayString() switch
                {
                    "System.Collections.Generic.IDictionary<TKey, TValue>" => true,
                    "System.Collections.Generic.Dictionary<TKey, TValue>" => true,
                    "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>" => true,
                    _ => false
                };
            }

            return false;
        }

        public bool IsNestedCollection()
        {
            var inner = FindCollectionType(typeSymbol);
            if (inner is null) return false;

            return FindCollectionType(inner) is not null;
        }

        public (bool isNullable, bool isGuid, bool isEnum, bool isPrimitive, bool isString, bool isComplex, bool isFormFile) GetPropertyAttributes()
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
            var isGuid = typeSymbol.GetFullTypeName() == "global::System.Guid";
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

            var isFormFile = typeSymbol.IsFormFile();

            var isComplex = !(isPrimitive || isString || isGuid || isEnum);

            return (isNullable, isGuid, isEnum, isPrimitive, isString, isComplex, isFormFile);
        }

        public ITypeSymbol? FindCollectionType()
        {
            if (typeSymbol is IArrayTypeSymbol arr)
            {
                return arr.ElementType;
            }
            else if (typeSymbol is INamedTypeSymbol named &&
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

        public bool IsFormFile() =>
            typeSymbol.ToDisplayString() is "Microsoft.AspNetCore.Http.IFormFile"
            or "Microsoft.AspNetCore.Http.IFormFileCollection"
            or "System.Collections.Generic.IEnumerable<Microsoft.AspNetCore.Http.IFormFile>";
    }
}
