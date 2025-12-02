using Blueprint.HttpBinder;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Blueprint.HttpBinder.Analyzers;

internal sealed class DictionaryTypeNotSupportedAnalyzer
{
    public const string Id = "HB002";

    private static readonly DiagnosticDescriptor _rule = new(
        Id,
        "Dictionary types are not supported for HTTP binding",
        "Property '{0}' is a dictionary type which cannot be bound from form, query, or route sources",
        "HttpBinder",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        "Dictionaries are ambiguous in HTTP binding and cannot be reliably parsed.");

    internal static void ReportDiagnostics(SourceProductionContext context, BoundType boundType)
    {
        foreach (var property in boundType.Properties)
        {
            if (!IsDictionary(property.Symbol.Type))
                continue;

            var location = property.Symbol.Locations.FirstOrDefault() ?? Location.None;

            var diagnostic = Diagnostic.Create(
                _rule,
                location,
                property.Name,
                boundType.TypeSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsDictionary(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol named &&
            named.IsGenericType &&
            named.ConstructedFrom?.ToDisplayString() ==
            "System.Collections.Generic.Dictionary<TKey, TValue>")
        {
            return true;
        }

        return type.AllInterfaces.Any(i =>
            i.ToDisplayString() == "System.Collections.Generic.IDictionary<TKey, TValue>");
    }
}
