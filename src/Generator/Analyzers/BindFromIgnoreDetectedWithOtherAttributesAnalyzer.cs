using Blueprint.HttpBinder.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Blueprint.HttpBinder.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BindFromIgnoreDetectedWithOtherAttributesAnalyzer : DiagnosticAnalyzer
{
    public const string Id = "HB005";

    private static readonly DiagnosticDescriptor _rule = new(
        Id,
        "The [BindFromIgnore] attribute cannot be used with any other binding attribute",
        "Property '{0}' in '{1}' uses [BindFromIgnore] and another [Bind*] attribute. Only [BindFromIgnore] should be applied.",
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

        var hasIgnore = prop.HasAttribute(AttributeConstants.BindFromIgnoreAttribute);
        if (!hasIgnore)
            return;

        var hasOther = prop.HasAttribute(AttributeConstants.BindFromAttribute);
        if (!hasOther)
            return;

        var diagnostic = Diagnostic.Create(
            _rule,
            prop.Locations.FirstOrDefault(),
            prop.Name,
            prop.ContainingType.Name);

        ctx.ReportDiagnostic(diagnostic);
    }
}
