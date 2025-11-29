using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HttpBinder.Generator.Analyzers;

internal class ComplexTypeDetectedOnRouteOrQueryBinder
{
    private static readonly DiagnosticDescriptor _analyzer = new(
            id: "HB001",
            title: "Complex types cannot be bound from query or route",
            messageFormat: "Property '{0}' on type '{1}' is a complex type and cannot be bound from {2}. Use HttpBinderType.Form or [BindFromForm] instead.",
            category: "HttpBinder",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

    public static void ReportDiagnostics(SourceProductionContext context, BoundType model)
    {
        foreach (var prop in model.Properties)
        {
            if (prop.HttpBinderType is HttpBinderType.Query or HttpBinderType.Route)
            {
                if (!prop.IsComplex)
                    continue;

                var location = prop.Symbol.Locations.FirstOrDefault() ?? Location.None;
                var sourceText = prop.HttpBinderType == HttpBinderType.Query ? "the query string" : "route data";

                var diag = Diagnostic.Create(
                    _analyzer,
                    location,
                    prop.Name,
                    model.TypeSymbol.Name,
                    sourceText);

                context.ReportDiagnostic(diag);
            }
        }
    }
}
