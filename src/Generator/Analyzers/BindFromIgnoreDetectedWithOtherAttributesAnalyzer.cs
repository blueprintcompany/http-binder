using Blueprint.HttpBinder;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Blueprint.HttpBinder.Analyzers;

internal sealed class BindFromIgnoreDetectedWithOtherAttributesAnalyzer
{
    public const string Id = "HB005";

    private static readonly DiagnosticDescriptor _rule = new(
        Id,
        "The [BindFromIgnore] attribute cannot be used with any other binding attribute",
        "Property '{0}' in '{1}' uses [BindFromIgnore] and another [Bind*] attribute. Only [BindFromIgnore] should be applied.",
        "HttpBinder",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A property marked with [BindFromIgnore] must not also specify any binding source attribute."
    );

    internal static void ReportDiagnostics(SourceProductionContext context, BoundType boundType)
    {
        foreach (var property in boundType.Properties)
        {
            var hasIgnore = property.Symbol
                .GetAttributes()
                .Any(a => a.AttributeClass!.GetFullTypeName() ==
                          "global::Blueprint.HttpBinder.BindFromIgnoreAttribute");

            if (!hasIgnore)
                continue;

            var hasOtherBind = property.Symbol
                .GetAttributes()
                .Any(a =>
                {
                    var name = a.AttributeClass!.GetFullTypeName();
                    return name switch
                    {
                        "global::Blueprint.HttpBinder.BindFromAttribute" => true,
                        _ => false
                    };
                });

            if (!hasOtherBind)
                continue;

            var location = property.Symbol.Locations.FirstOrDefault() ?? Location.None;

            var diagnostic = Diagnostic.Create(
                _rule,
                location,
                property.Name,
                boundType.TypeSymbol.Name
            );

            context.ReportDiagnostic(diagnostic);
        }
    }
}

