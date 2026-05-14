using BlueprintSoftware.HttpBinder.Analyzers;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace BlueprintSoftware.HttpBinder.Generator.Tests.Generator.Analyzers;

internal class BindFromIgnoreDetectedWithOtherAttributesAnalyzerTests : CSharpAnalyzerVerifier<BindFromIgnoreDetectedWithOtherAttributesAnalyzer, DefaultVerifier>
{
    [Test]
    public async Task GivenNoSource_ThenHasNoDiagnostics()
    {
        var code = TestHelpers.GetTestCode(string.Empty);
        await VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GivenAnNonHttpBinderClass_ThenHasNoDiagnostics()
    {
        var code = TestHelpers.GetTestCode("""
            public class T
            {
                public int X { get; set; }
            }
            """);
        await VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GivenIgnoreOnly_WhenNoOtherBindAttributes_ThenNoDiagnostic()
    {
        var code = TestHelpers.GetTestCode("""
            public partial class Request
            {
                [BindFromIgnore]
                public int X { get; set; }
            }
            """);

        await VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GivenIgnoreAndBindAttribute_WhenBothPresent_ThenShowsDiagnostic()
    {
        var code = TestHelpers.GetTestCode("""
            public partial class Request
            {
                [BindFromIgnore]
                [BindFrom(HttpBinderType.Query)]
                public int {|HB005:X|} { get; set; }
            }
            """);

        await VerifyAnalyzerAsync(code);
    }
}
