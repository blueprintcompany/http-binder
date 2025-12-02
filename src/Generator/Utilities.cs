using Microsoft.CodeAnalysis;

namespace Blueprint.HttpBinder;

internal static class Utilities
{
    public static string GetFullTypeName(ITypeSymbol type)
        => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public static string GetMinimalTypeName(ITypeSymbol type)
        => type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
}
