using Blueprint.HttpBinder.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Blueprint.HttpBinder.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ComplexTypeDetectedOnRouteOrQueryBinderAnalyzer : DiagnosticAnalyzer
{
    public const string Id = "HB001";

    private static readonly DiagnosticDescriptor _rule = new(
        Id,
        "Complex types cannot be bound from query or route",
        "Property '{0}' on type '{1}' is a complex type and cannot be bound from {2}",
        "HttpBinder",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [_rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
    }

    private static void AnalyzeProperty(SymbolAnalysisContext ctx)
    {
        var prop = (IPropertySymbol)ctx.Symbol;

        var bindFrom = prop.GetAttribute(AttributeConstants.BindFromAttribute);

        HttpBinderType binderType;

        if (bindFrom != null && bindFrom.TryGetBinderType(out var fromProperty))
        {
            binderType = fromProperty;
        }
        else
        {
            var classAttr = prop.ContainingType.GetAttribute(AttributeConstants.HttpBinderAttribute);
            if (classAttr == null || !classAttr.TryGetBinderType(out var fromClass))
                return;

            binderType = fromClass;
        }

        if (binderType != HttpBinderType.Query && binderType != HttpBinderType.Route)
            return;

        var type = prop.Type;
        if (type.IsValueType || type.SpecialType == SpecialType.System_String || type.TypeKind == TypeKind.Enum)
            return;

        string source = binderType == HttpBinderType.Query ? "the query string" : "route data";

        var diagnostic = Diagnostic.Create(
            _rule,
            prop.Locations.FirstOrDefault(),
            prop.Name,
            prop.ContainingType.Name,
            source);

        ctx.ReportDiagnostic(diagnostic);
    }
}
