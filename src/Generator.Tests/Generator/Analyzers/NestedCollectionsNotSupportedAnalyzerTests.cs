using Blueprint.HttpBinder;
using Blueprint.HttpBinder.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Generator.Tests.Generator.Analyzers;

internal class NestedCollectionsNotSupportedAnalyzerTests : CSharpSourceGeneratorTest<HttpBinderGenerator, DefaultVerifier>
{
    [Test]
    public async Task GivenANestedCollectionProperty_ThenShowsDiagnostic()
    {
        var code = @"
            using Blueprint.HttpBinder;
            using System.Collections.Generic;

            [HttpBinder]
            public partial class UserQueryRequest
            {
                public List<List<string>> Filters { get; set; } = [];
            }
        ";

        var compilation = TestBase.Create(code);

        var driver = CSharpGeneratorDriver
            .Create(new HttpBinderGenerator())
            .RunGenerators(compilation);

        var result = driver.GetRunResult();

        var diagnostic = result.Diagnostics
            .SingleOrDefault(d => d.Id == NestedCollectionsNotSupportedAnalyzer.Id);

        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Warning);
    }

    [Test]
    public async Task GivenANonNestedCollectionProperty_ThenDoesNotShowDiagnostic()
    {
        var code = @"
            using Blueprint.HttpBinder;

            [HttpBinder]
            public partial class UserQueryRequest
            {
                public string Name { get; set; } = """";
            }
        ";

        var compilation = TestBase.Create(code);

        var driver = CSharpGeneratorDriver
            .Create(new HttpBinderGenerator())
            .RunGenerators(compilation);

        var result = driver.GetRunResult();

        var diagnostic = result.Diagnostics
            .SingleOrDefault(d => d.Id == NestedCollectionsNotSupportedAnalyzer.Id);

        await Assert.That(diagnostic).IsNull();
    }
}
