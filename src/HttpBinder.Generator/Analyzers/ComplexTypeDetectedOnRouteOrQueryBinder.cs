using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HttpBinder.Generator.Analyzers;

internal class ComplexTypeDetectedOnRouteOrQueryBinder
{
    internal const string _id = "HB001";

    private static readonly DiagnosticDescriptor _analyzer = new(
            id: _id,
            title: "Complex types cannot be bound from query or route",
            messageFormat: "Property '{0}' on type '{1}' is a complex type and cannot be bound from {2}. Use [HttpBinder(HttpBinderType = HttpBinderType.Form]) on the class or [BindFrom(HttpBinderType.Form)] on the property instead.",
            category: "HttpBinder",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

    public static void ReportDiagnostics(SourceProductionContext context, BoundType boundType)
    {
        foreach (var property in boundType.Properties)
        {
            if (property.HttpBinderType is HttpBinderType.Query or HttpBinderType.Route)
            {
                if (!property.IsComplex)
                    continue;

                var location = property.Symbol.Locations.FirstOrDefault() ?? Location.None;
                var sourceText = property.HttpBinderType == HttpBinderType.Query ? "the query string" : "route data";

                var diag = Diagnostic.Create(
                    _analyzer,
                    location,
                    property.Name,
                    boundType.TypeSymbol.Name,
                    sourceText);

                context.ReportDiagnostic(diag);
            }
        }
    }
}
