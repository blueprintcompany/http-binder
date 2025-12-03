using Blueprint.HttpBinder.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Generator.Tests.Generator.Analyzers;

internal class FormFilesDetectedOnRouteOrQueryBinderAnalyzerTests : CSharpAnalyzerTest<FormFilesDetectedOnRouteOrQueryBinderAnalyzer, DefaultVerifier>
{
    [Before(Test)]
    public void Setup()
    {
        TestState.AdditionalReferences.Add(
            MetadataReference.CreateFromFile(typeof(Microsoft.AspNetCore.Http.IFormFile).Assembly.Location)
        );

        ReferenceAssemblies = ReferenceAssemblies.Net.Net90;
    }

    [Test]
    public async Task GivenAQueryClass_WhenAFormFileIsPresent_ThenShowsDiagnostic()
    {
        var code = TestHelpers.GetTestCode($$"""
            using Microsoft.AspNetCore.Http;

            [HttpBinder(HttpBinderType = HttpBinderType.Query)]
            public partial class UserQueryRequest
            {
                public IFormFile {|HB004:FormFile|} { get; set; }
            }
            """);

        TestCode = code;

        await RunAsync();
    }

    [Test]
    public async Task GivenAQueryClass_WhenAFormFileCollectionIsPresent_ThenShowsDiagnostic()
    {
        TestCode = TestHelpers.GetTestCode($$"""
            using Microsoft.AspNetCore.Http;

            [HttpBinder(HttpBinderType = HttpBinderType.Query)]
            public partial class UserQueryRequest
            {
                public IFormFileCollection {|HB004:FormFile|} { get; set; }
            }
            """);

        await RunAsync();
    }

    [Test]
    public async Task GivenARouteClass_WhenAFormFileIsPresent_ThenShowsDiagnostic()
    {
        TestCode = TestHelpers.GetTestCode($$"""
            using Microsoft.AspNetCore.Http;

            [HttpBinder(HttpBinderType = HttpBinderType.Route)]
            public partial class UserQueryRequest
            {
                public IFormFile {|HB004:FormFile|} { get; set; }
            }
            """);

        await RunAsync();
    }

    [Test]
    public async Task GivenARouteClass_WhenAFormFileCollectionIsPresent_ThenShowsDiagnostic()
    {
        TestCode = TestHelpers.GetTestCode($$"""
            using Microsoft.AspNetCore.Http;

            [HttpBinder(HttpBinderType = HttpBinderType.Route)]
            public partial class UserQueryRequest
            {
                public IFormFileCollection {|HB004:FormFile|} { get; set; }
            }
            """);

        await RunAsync();
    }

    [Test]
    public async Task GivenAFormClass_WhenFormFilePropertiesArePresent_ThenDoesNotShowDiagnostic()
    {
        TestCode = TestHelpers.GetTestCode("""
            using Microsoft.AspNetCore.Http;

            [HttpBinder(HttpBinderType = HttpBinderType.Form)]
            public partial class UserQueryRequest
            {
                public IFormFile FormFile { get; set; }
                public IFormFileCollection FormFiles { get; set; }
            }
            """);

        await RunAsync();
    }

    [Test]
    public async Task GivenAnyBinderType_WhenComplexPropertyOverridesWithBindFromForm_ThenDoesNotShowDiagnostic()
    {
        TestCode = TestHelpers.GetTestCode("""
            using Microsoft.AspNetCore.Http;

            [HttpBinder(HttpBinderType = HttpBinderType.Route)]
            public partial class UserQueryRequest
            {
                [BindFrom(HttpBinderType.Form)]
                public ComplexType ComplexType { get; set; } = new();
            }

            public class ComplexType {}
            """);

        await RunAsync();
    }
}
