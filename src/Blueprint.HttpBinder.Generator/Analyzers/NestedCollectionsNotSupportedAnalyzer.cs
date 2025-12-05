using Blueprint.HttpBinder.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Blueprint.HttpBinder.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class NestedCollectionsNotSupportedAnalyzer : DiagnosticAnalyzer
{
    public const string Id = "HB003";

    private static readonly DiagnosticDescriptor _rule = new(
        Id,
        "Nested collections are not supported for HTTP binding",
        "Property '{0}' on type '{1}' is a nested collection type and cannot be bound",
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

        if (!prop.Type.IsNestedCollection())
            return;

        var diagnostic = Diagnostic.Create(
            _rule,
            prop.Locations.FirstOrDefault(),
            prop.Name,
            prop.ContainingType.Name);

        ctx.ReportDiagnostic(diagnostic);
    }
}
