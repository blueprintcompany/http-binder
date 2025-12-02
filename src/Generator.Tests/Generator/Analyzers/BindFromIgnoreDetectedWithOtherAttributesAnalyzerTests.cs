using Blueprint.HttpBinder;
using Blueprint.HttpBinder.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Generator.Tests.Generator.Analyzers;

internal class BindFromIgnoreDetectedWithOtherAttributesAnalyzerTests : CSharpSourceGeneratorTest<HttpBinderGenerator, DefaultVerifier>
{
    [Test]
    public async Task GivenIgnoreOnly_WhenNoOtherBindAttributes_ThenNoDiagnostic()
    {
        var code = @"
            using HttpBinder;

            public enum HttpBinderType { Form, Query, Route }

            public partial class Request
            {
                [BindFromIgnore]
                public int X { get; set; }
            }
            ";

        var compilation = TestBase.Create(code);

        var generator = new HttpBinderGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation);

        var result = driver.GetRunResult();

        var diagnostic = result.Diagnostics.SingleOrDefault(d => d.Id == BindFromIgnoreDetectedWithOtherAttributesAnalyzer.Id);

        await Assert.That(diagnostic).IsNull();
    }

    [Test]
    public async Task GivenIgnoreAndBindAttribute_WhenBothPresent_ThenShowsDiagnostic()
    {
        var code = @"
            using HttpBinder;

            public enum HttpBinderType { Form, Query, Route }

            public partial class Request
            {
                [BindFromIgnore]
                [BindFrom(HttpBinderType.Query)]
                public int X { get; set; }
            }
            ";

        var compilation = TestBase.Create(code);

        var generator = new HttpBinderGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation);

        var result = driver.GetRunResult();

        var diagnostic = result.Diagnostics.SingleOrDefault(d => d.Id == BindFromIgnoreDetectedWithOtherAttributesAnalyzer.Id);

        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Warning);
    }
}
