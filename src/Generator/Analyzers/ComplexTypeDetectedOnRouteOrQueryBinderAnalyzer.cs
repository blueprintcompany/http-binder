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
        if (bindFrom == null)
            return;

        var arg = bindFrom.ConstructorArguments.FirstOrDefault();
        if (arg.Value is not int binderType)
            return;

        if (binderType != (int)HttpBinderType.Query && binderType != (int)HttpBinderType.Route)
            return;

        var type = prop.Type;
        if (type.IsValueType || type.SpecialType == SpecialType.System_String || type.TypeKind == TypeKind.Enum)
            return;

        string source = binderType == 1 ? "the query string" : "route data";

        var diagnostic = Diagnostic.Create(
            _rule,
            prop.Locations.FirstOrDefault(),
            prop.Name,
            prop.ContainingType.Name,
            source);

        ctx.ReportDiagnostic(diagnostic);
    }
}
