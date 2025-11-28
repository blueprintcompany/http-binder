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

        context.TestCode = "class Dummy { }";

        // List of expected generated sources
        context.TestState.GeneratedSources.Add((typeof(HttpBinderGenerator), "Sample.g.cs", """
        internal static class Sample
        {
            public const string AssemblyName = "TestProject";
        }
        """));

        await context.RunAsync(TestContext.Current!.Execution.CancellationToken);
    }
}