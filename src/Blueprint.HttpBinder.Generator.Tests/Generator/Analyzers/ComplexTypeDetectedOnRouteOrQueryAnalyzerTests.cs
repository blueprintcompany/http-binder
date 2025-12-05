using Blueprint.HttpBinder.Analyzers;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Blueprint.HttpBinder.Generator.Tests.Generator.Analyzers;

internal class ComplexTypeDetectedOnRouteOrQueryBinderAnalyzerTests : CSharpAnalyzerVerifier<ComplexTypeDetectedOnRouteOrQueryBinderAnalyzer, DefaultVerifier>
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
    public async Task GivenAQueryClass_WhenAComplexObjectIsPresent_ThenShowsDiagnostic()
    {
        var code = TestHelpers.GetTestCode("""
            [HttpBinder(HttpBinderType = HttpBinderType.Query)]
            public partial class UserQueryRequest
            {
                public ComplexType {|HB001:ComplexType|} { get; set; } = new();
            }

            public class ComplexType {}
            """);

        await VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GivenAQueryClass_WhenAListOfPrimitivesIsPresent_ThenDoesNotShowsDiagnostic()
    {
        var code = TestHelpers.GetTestCode("""
            [HttpBinder(HttpBinderType = HttpBinderType.Query)]
            public partial class UserQueryRequest
            {
                public List<int> Ints { get; set; } = [];
            }
            """);

        await VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GivenAQueryClass_WhenAListOfComplexTypesIsPresent_ThenShowsDiagnostic()
    {
        var code = TestHelpers.GetTestCode("""
            [HttpBinder(HttpBinderType = HttpBinderType.Query)]
            public partial class UserQueryRequest
            {
                public List<ComplexType> {|HB001:ComplexType|} { get; set; } = [];
            }

            public class ComplexType {}
            """);

        await VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GivenAFormClass_WhenAComplexObjectIsPresent_ThenDoesNotShowDiagnostic()
    {
        var code = TestHelpers.GetTestCode("""
            [HttpBinder(HttpBinderType = HttpBinderType.Form)]
            public partial class UserQueryRequest
            {
                public ComplexType ComplexType { get; set; } = new();
            }

            public class ComplexType {}
            """);

        await VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GivenARouteClass_WhenAComplexObjectIsPresent_ThenShowsDiagnostic()
    {
        var code = TestHelpers.GetTestCode("""
            [HttpBinder(HttpBinderType = HttpBinderType.Route)]
            public partial class UserQueryRequest
            {
                public ComplexType {|HB001:ComplexType|} { get; set; } = new();
            }

            public class ComplexType {}
            """);

        await VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GivenARouteClass_WhenComplexPropertyOverridesWithBindFromForm_ThenDoesNotShowDiagnostic()
    {
        var code = TestHelpers.GetTestCode("""
            [HttpBinder(HttpBinderType = HttpBinderType.Route)]
            public partial class UserQueryRequest
            {
                [BindFrom(HttpBinderType.Form)]
                public ComplexType ComplexType { get; set; } = new();
            }

            public class ComplexType {}
            """);

        await VerifyAnalyzerAsync(code);
    }
}
