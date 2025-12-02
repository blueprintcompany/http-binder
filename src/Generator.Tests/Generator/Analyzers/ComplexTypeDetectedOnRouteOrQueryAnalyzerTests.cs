using Blueprint.HttpBinder;
using Blueprint.HttpBinder.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Generator.Tests.Generator.Analyzers;

internal class ComplexTypeDetectedOnRouteOrQueryBinderAnalyzerTests : CSharpSourceGeneratorTest<HttpBinderGenerator, DefaultVerifier>
{
    [Test]
    public async Task GivenAQueryClass_WhenAComplexObjectIsPresent_ThenShowsDiagnostic()
    {
        var code = @"
        using Blueprint.HttpBinder;

        [HttpBinder(HttpBinderType = HttpBinderType.Route)]
        public partial class UserQueryRequest
        {
            public ComplexType ComplexType { get; set; } = new();
        }

        public class ComplexType {}
        ";

        var compilation = TestBase.Create(code);

        var generator = new HttpBinderGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation);

        var result = driver.GetRunResult();

        var diagnostic = result.Diagnostics.SingleOrDefault(d => d.Id == ComplexTypeDetectedOnRouteOrQueryBinderAnalyzer.Id);

        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Warning);
    }

    [Test]
    public async Task GivenAFormClass_WhenAComplexObjectIsPresent_ThenDoesNotShowDiagnostic()
    {
        var code = @"
        using Blueprint.HttpBinder;

        [HttpBinder(HttpBinderType = HttpBinderType.Form)]
        public partial class UserQueryRequest
        {
            public ComplexType ComplexType { get; set; } = new();
        }

        public class ComplexType {}
        ";

        var compilation = TestBase.Create(code);

        var generator = new HttpBinderGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation);

        var result = driver.GetRunResult();

        var diagnostic = result.Diagnostics.SingleOrDefault(d => d.Id == ComplexTypeDetectedOnRouteOrQueryBinderAnalyzer.Id);

        await Assert.That(diagnostic).IsNull();
    }

    [Test]
    public async Task GivenARouteClass_WhenAComplexObjectIsPresent_ThenShowsDiagnostic()
    {
        var code = @"
        using Blueprint.HttpBinder;

        [HttpBinder(HttpBinderType = HttpBinderType.Route)]
        public partial class UserQueryRequest
        {
            public ComplexType ComplexType { get; set; } = new();
        }

        public class ComplexType {}
        ";

        var compilation = TestBase.Create(code);

        var generator = new HttpBinderGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation);

        var result = driver.GetRunResult();

        var diagnostic = result.Diagnostics.SingleOrDefault(d => d.Id == ComplexTypeDetectedOnRouteOrQueryBinderAnalyzer.Id);

        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Warning);
    }

    [Test]
    public async Task GivenARouteClass_WhenComplexPropertyOverridesWithBindFromForm_ThenDoesNotShowDiagnostic()
    {
        var code = @"
        using Blueprint.HttpBinder;

        [HttpBinder(HttpBinderType = HttpBinderType.Route)]
        public partial class UserQueryRequest
        {
            [BindFrom(HttpBinderType.Form)]
            public ComplexType ComplexType { get; set; } = new();
        }

        public class ComplexType {}
        ";

        var compilation = TestBase.Create(code);

        var generator = new HttpBinderGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation);

        var result = driver.GetRunResult();

        var diagnostic = result.Diagnostics.SingleOrDefault(d => d.Id == ComplexTypeDetectedOnRouteOrQueryBinderAnalyzer.Id);

        await Assert.That(diagnostic).IsNull();
    }
}

