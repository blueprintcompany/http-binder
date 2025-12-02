using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace HttpBinder.Generator.Tests.Generator;

public class HttpBinderGeneratorTests : CSharpSourceGeneratorTest<HttpBinderGenerator, DefaultVerifier>
{
    [Test]
    public async Task GeneratesBindAsyncForPrimitives()
    {
        ReferenceAssemblies = ReferenceAssemblies.Net.Net90;
        TestCode = @"
        [HttpBinder(HttpBinderType = HttpBinderType.Form)]
        public partial class UserQueryRequest : PagedRequestBase
        {
            public string? Search { get; set; }
        }";

        // List of expected generated sources
        TestState.GeneratedSources.Add(
            (typeof(HttpBinderGenerator), AttributeHelpers.Name, AttributeHelpers.Source)
        );

        await RunAsync(TestContext.Current!.Execution.CancellationToken);
    }
}