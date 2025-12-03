using Blueprint.HttpBinder.Analyzers;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Generator.Tests.Generator.Analyzers;

internal class NestedCollectionsNotSupportedAnalyzerTests
    : CSharpAnalyzerVerifier<NestedCollectionsNotSupportedAnalyzer, DefaultVerifier>
{
    [Test]
    public async Task GivenANestedCollectionProperty_ThenShowsDiagnostic()
    {
        var code = TestHelpers.GetTestCode($$"""
            [HttpBinder]
            public partial class UserQueryRequest
            {
                public List<List<string>> {|HB003:Filters|} { get; set; } = [];
            }
            """);

        await VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GivenANonNestedCollectionProperty_ThenDoesNotShowDiagnostic()
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
