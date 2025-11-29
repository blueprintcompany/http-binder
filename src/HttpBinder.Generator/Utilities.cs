using Microsoft.CodeAnalysis;

namespace HttpBinder.Generator;

internal static class Utilities
{
    public static string GetFullTypeName(ITypeSymbol type)
        => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public static string GetMinimalTypeName(ITypeSymbol type)
        => type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
}
