using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Assembly = System.Reflection.Assembly;

namespace Blueprint.HttpBinder.Generator.Tests.Generator;

internal class TestHelpers
{
    internal static string GetTestCode(string code) => $$"""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using System.Threading;
        using System.Threading.Tasks;
        using System.Diagnostics;
        using Blueprint.HttpBinder;

        {{LoadBinderAttributes()}}

        namespace ConsoleApplication1
        {
            {{code}}
        }
    """;

    internal static ITypeSymbol GetTestSymbol(string code, string metadataName)
    {
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [CSharpSyntaxTree.ParseText(code)],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(HttpContext).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(IFormFile).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(HttpBinderAttribute).Assembly.Location)
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        var symbol = compilation.GetTypeByMetadataName(metadataName);
        return symbol!;
    }

    internal static string LoadBinderAttributes() =>
        LoadEmbedded("BindFromIgnoreAttribute.cs") +
        LoadEmbedded("BindFromAttribute.cs") +
        LoadEmbedded("HttpBinderType.cs") +
        LoadEmbedded("HttpBinderAttribute.cs");

    internal static string LoadEmbedded(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var path = assembly.GetManifestResourceNames()
            .Single(n => n.EndsWith(name, StringComparison.Ordinal));

        using var stream = assembly.GetManifestResourceStream(path)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
