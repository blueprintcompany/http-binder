using Blueprint.HttpBinder;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Blueprint.HttpBinder.Analyzers;

internal sealed class NestedCollectionsNotSupportedAnalyzer
{
    public const string Id = "HB003";

    private static readonly DiagnosticDescriptor _rule = new(
        Id,
        "Nested collections are not supported for HTTP binding",
        "Property '{0}' is a collection of collection type which cannot be bound from form, query, or route sources",
        "HttpBinder",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        "Nested collections are ambiguous in HTTP binding and cannot be reliably parsed.");

    internal static void ReportDiagnostics(SourceProductionContext context, BoundType boundType)
    {
        foreach (var property in boundType.Properties)
        {
            if (!property.Symbol.Type.IsNestedCollection())
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
}
