using Blueprint.HttpBinder;
using Blueprint.HttpBinder.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Generator.Tests.Generator.Analyzers;

internal class FormFilesDetectedOnRouteOrQueryBinderAnalyzerTests : CSharpSourceGeneratorTest<HttpBinderGenerator, DefaultVerifier>
{
    [Test]
    public async Task GivenAQueryClass_WhenAFormFileIsPresent_ThenShowsDiagnostic()
    {
        var code = @"
        using Blueprint.HttpBinder;

        [HttpBinder(HttpBinderType = HttpBinderType.Route)]
        public partial class UserQueryRequest
        {
            public Microsoft.AspNetCore.Http.IFormFile FormFile { get; set; }
        }
        ";

        var compilation = TestBase.Create(code);

        var generator = new HttpBinderGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation);

        var result = driver.GetRunResult();

        var diagnostic = result.Diagnostics.SingleOrDefault(d => d.Id == FormFilesDetectedOnRouteOrQueryBinderAnalyzer.Id);

        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Warning);
    }

    [Test]
    public async Task GivenAQueryClass_WhenAFormFileCollectionIsPresent_ThenShowsDiagnostic()
    {
        var code = @"
        using Blueprint.HttpBinder;

        [HttpBinder(HttpBinderType = HttpBinderType.Route)]
        public partial class UserQueryRequest
        {
            public Microsoft.AspNetCore.Http.IFormFileCollection FormFile { get; set; }
        }
        ";

        var compilation = TestBase.Create(code);

        var generator = new HttpBinderGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation);

        var result = driver.GetRunResult();

        var diagnostic = result.Diagnostics.SingleOrDefault(d => d.Id == FormFilesDetectedOnRouteOrQueryBinderAnalyzer.Id);

        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Warning);
    }

    [Test]
    public async Task GivenARouteClass_WhenAFormFileIsPresent_ThenShowsDiagnostic()
    {
        var code = @"
        using Blueprint.HttpBinder;

        [HttpBinder(HttpBinderType = HttpBinderType.Route)]
        public partial class UserQueryRequest
        {
            public Microsoft.AspNetCore.Http.IFormFile FormFile { get; set; }
        }
        ";

        var compilation = TestBase.Create(code);

        var generator = new HttpBinderGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation);

        var result = driver.GetRunResult();

        var diagnostic = result.Diagnostics.SingleOrDefault(d => d.Id == FormFilesDetectedOnRouteOrQueryBinderAnalyzer.Id);

        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Warning);
    }

    [Test]
    public async Task GivenARouteClass_WhenAFormFileCollectionIsPresent_ThenShowsDiagnostic()
    {
        var code = @"
        using Blueprint.HttpBinder;

        [HttpBinder(HttpBinderType = HttpBinderType.Route)]
        public partial class UserQueryRequest
        {
            public Microsoft.AspNetCore.Http.IFormFileCollection FormFile { get; set; }
        }
        ";

        var compilation = TestBase.Create(code);

        var generator = new HttpBinderGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation);

        var result = driver.GetRunResult();

        var diagnostic = result.Diagnostics.SingleOrDefault(d => d.Id == FormFilesDetectedOnRouteOrQueryBinderAnalyzer.Id);

        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Warning);
    }

    [Test]
    public async Task GivenAFormClass_WhenFormFilePropertiesArePresent_ThenDoesNotShowDiagnostic()
    {
        var code = @"
        using Blueprint.HttpBinder;

        [HttpBinder(HttpBinderType = HttpBinderType.Form)]
        public partial class UserQueryRequest
        {
            public Microsoft.AspNetCore.Http.IFormFile FormFile { get; set; }
            public Microsoft.AspNetCore.Http.IFormFileCollection FormFiles { get; set; }
        }
        ";

        var compilation = TestBase.Create(code);

        var generator = new HttpBinderGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation);

        var result = driver.GetRunResult();

        var diagnostic = result.Diagnostics.SingleOrDefault(d => d.Id == FormFilesDetectedOnRouteOrQueryBinderAnalyzer.Id);

        await Assert.That(diagnostic).IsNull();
    }

    [Test]
    public async Task GivenAnyBinderType_WhenComplexPropertyOverridesWithBindFromForm_ThenDoesNotShowDiagnostic()
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

        var diagnostic = result.Diagnostics.SingleOrDefault(d => d.Id == FormFilesDetectedOnRouteOrQueryBinderAnalyzer.Id);

        await Assert.That(diagnostic).IsNull();
    }
}

