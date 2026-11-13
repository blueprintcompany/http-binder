using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BlueprintSoftware.HttpBinder.Generator.Tests.Generator;

public sealed class HttpBinderGeneratorTests
{
    [Test]
    public async Task GivenCollectionWithNonIdentifierName_WhenBinding_ThenBindsAllValues()
    {
        var request = await BindAsync(
            """
            [HttpBinder(HttpBinderType = HttpBinderType.Query)]
            public partial class Request
            {
                [BindFrom(HttpBinderType.Query, Name = "tag-id")]
                public global::System.Collections.Generic.List<int> TagIds { get; set; } = [];
            }
            """,
            "?tag-id=1&tag-id=2");

        await Assert.That((List<int>)Get(request, "TagIds")).IsEquivalentTo([1, 2]);
    }

    [Test]
    public async Task GivenEnumAndCharWithoutImplicitUsings_WhenBinding_ThenParsesValues()
    {
        var request = await BindAsync(
            """
            public enum Color { Red, Green }

            [HttpBinder(HttpBinderType = HttpBinderType.Query)]
            public partial class Request
            {
                public Color Color { get; set; }
                public char Grade { get; set; }
            }
            """,
            "?Color=green&Grade=B");

        await Assert.That(Get(request, "Color").ToString()).IsEqualTo("Green");
        await Assert.That((char)Get(request, "Grade")).IsEqualTo('B');
    }

    [Test]
    public async Task GivenNonInvariantServerCulture_WhenBinding_ThenParsesUsingInvariantCulture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

        try
        {
            var request = await BindAsync(
                """
                [HttpBinder(HttpBinderType = HttpBinderType.Query)]
                public partial class Request
                {
                    public decimal Amount { get; set; }
                    public global::System.DateOnly Date { get; set; }
                }
                """,
                "?Amount=1.5&Date=03/04/2025");

            await Assert.That((decimal)Get(request, "Amount")).IsEqualTo(1.5m);
            await Assert.That((DateOnly)Get(request, "Date")).IsEqualTo(new DateOnly(2025, 3, 4));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Test]
    public async Task GivenComplexTypeNestedInComplexType_WhenGenerating_ThenEmitsNoWarnings()
    {
        var output = Generate(
            """
            public sealed class Address
            {
                public string Street { get; set; } = string.Empty;
            }

            public sealed class Facility
            {
                public Address Address { get; set; } = new();
            }

            [HttpBinder(HttpBinderType = HttpBinderType.Form)]
            public partial class Request
            {
                public Facility Facility { get; set; } = new();
            }
            """);
        var userTree = output.SyntaxTrees.First();

        var warnings = output.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Warning
                && diagnostic.Location.SourceTree != userTree)
            .Select(diagnostic => diagnostic.ToString());

        await Assert.That(warnings).IsEmpty();
    }

    private static object Get(object instance, string propertyName) =>
        instance.GetType().GetProperty(propertyName)!.GetValue(instance)!;

    private static async Task<object> BindAsync(string source, string queryString)
    {
        var type = CompileGeneratedType(source, "Request");
        var http = new DefaultHttpContext();
        http.Request.QueryString = new QueryString(queryString);

        var bindAsync = type.GetMethod("BindAsync", BindingFlags.Public | BindingFlags.Static)!;
        var result = bindAsync.Invoke(null, [http])!;

        // ValueTask<T> has no non-generic base, so await it through AsTask().
        var task = (Task)result.GetType().GetMethod("AsTask")!.Invoke(result, null)!;
        await task;
        return task.GetType().GetProperty("Result")!.GetValue(task)!;
    }

    private static Compilation Generate(string source)
    {
        // No usings on purpose: generated code must not depend on consumer implicit usings.
        var compilation = CSharpCompilation.Create(
            $"GeneratedBinder_{Guid.NewGuid():N}",
            [CSharpSyntaxTree.ParseText("using BlueprintSoftware.HttpBinder;\n" + source)],
            ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
                .Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        CSharpGeneratorDriver.Create(new HttpBinderGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);
        return output;
    }

    private static Type CompileGeneratedType(string source, string typeName)
    {
        var output = Generate(source);

        using var stream = new MemoryStream();
        var emit = output.Emit(stream);
        if (!emit.Success)
            throw new InvalidOperationException(string.Join(Environment.NewLine,
                emit.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));

        stream.Position = 0;
        var assembly = new AssemblyLoadContext(null, isCollectible: true).LoadFromStream(stream);
        return assembly.GetType(typeName)!;
    }
}
