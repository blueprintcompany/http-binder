using Blueprint.HttpBinder.Analyzers;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Blueprint.HttpBinder.Generator.Tests.Generator.Analyzers;

internal class DictionaryTypeNotSupportedAnalyzerTests : CSharpAnalyzerVerifier<DictionaryTypeNotSupportedAnalyzer, DefaultVerifier>
{
    [Test]
    public async Task GivenADictionaryProperty_WhenPresent_ThenShowsDiagnostic()
    {
        var code = TestHelpers.GetTestCode("""
            [HttpBinder]
            public partial class UserQueryRequest
            {
                public Dictionary<string, string> {|HB002:Filters|} { get; set; } = new();
            }
            """);

        await VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GivenANonDictionaryProperty_WhenPresent_ThenDoesNotShowDiagnostic()
    {
        var code = TestHelpers.GetTestCode("""
            [HttpBinder]
            public partial class UserQueryRequest
            {
                public string Name { get; set; } = "";
            }
            """);

        await VerifyAnalyzerAsync(code);
    }
}
