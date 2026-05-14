using Microsoft.CodeAnalysis;
using System.Linq;

namespace BlueprintSoftware.HttpBinder.Extensions;

internal static class SymbolExtensions
{
    extension(ISymbol symbol)
    {
        public bool HasAttribute(string fullName) => GetAttribute(symbol, fullName) is not null;

        public AttributeData? GetAttribute(string fullName) => symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == fullName);
    }
}
