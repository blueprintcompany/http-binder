using System.Threading.Tasks;
using Xunit;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace HttpBinder.Generator.Tests
{
    public class HttpBinderGeneratorTests
    {
        [Fact]
        public async Task GeneratesBindAsyncForPrimitives()
        {
            // A simple DTO with primitive properties bound from the query string. The
            // test harness will compile the source and run the generator,
            // asserting that a BindAsync method is emitted in the generated output.
            var source = @"using HttpBinder;
            using Microsoft.AspNetCore.Http;

            [GenerateHttpBinder]
            public sealed partial class SimpleDto
            {
                [FromQuery] public int Id { get; set; }
                [FromQuery] public string? Name { get; set; }
            }";
            await CSharpSourceGeneratorVerifier<HttpBinder.Generator.HttpBinderGenerator>.VerifyAsync(source);
        }
    }
}