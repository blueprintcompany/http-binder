using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace HttpBinder.Generator.Tests;

public class HttpBinderGeneratorTests
{
    [Test]
    public async Task GeneratesBindAsyncForPrimitives()
    {
        var context = new CSharpSourceGeneratorTest<HttpBinderGenerator, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90
        };

        context.TestCode = @"
            [HttpBinder(HttpBinderType = HttpBinderType.Form)]
            public partial class UserQueryRequest : PagedRequestBase
            {
                public string? Search { get; set; }
            }
        ";

        // List of expected generated sources
        context.TestState.GeneratedSources.Add(
            (typeof(HttpBinderGenerator), AttributeHelpers.Name, AttributeHelpers.Source)
        );

        await context.RunAsync(TestContext.Current!.Execution.CancellationToken);
    }
}