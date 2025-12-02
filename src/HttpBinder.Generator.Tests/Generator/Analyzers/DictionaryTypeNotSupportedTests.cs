using HttpBinder.Generator.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace HttpBinder.Generator.Tests.Generator.Analyzers;

internal class DictionaryTypeNotSupportedAnalyzerTests : CSharpSourceGeneratorTest<HttpBinderGenerator, DefaultVerifier>
{
    [Test]
    public async Task GivenADictionaryProperty_WhenPresent_ThenShowsDiagnostic()
    {
        var code = @"
            using HttpBinder.Generator;
            using System.Collections.Generic;

            [HttpBinder]
            public partial class UserQueryRequest
            {
                public Dictionary<string, string> Filters { get; set; } = new();
            }
        ";

        var compilation = TestBase.Create(code);

        var driver = CSharpGeneratorDriver
            .Create(new HttpBinderGenerator())
            .RunGenerators(compilation);

        var result = driver.GetRunResult();

        var diagnostic = result.Diagnostics
            .SingleOrDefault(d => d.Id == DictionaryTypeNotSupportedAnalyzer.Id);

        await Assert.That(diagnostic).IsNotNull();
        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Warning);
    }

    [Test]
    public async Task GivenANonDictionaryProperty_WhenPresent_ThenDoesNotShowDiagnostic()
    {
        var code = @"
            using HttpBinder.Generator;

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
            .SingleOrDefault(d => d.Id == DictionaryTypeNotSupportedAnalyzer.Id);

        await Assert.That(diagnostic).IsNull();
    }
}
