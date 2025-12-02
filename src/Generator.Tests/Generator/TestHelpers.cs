using Microsoft.CodeAnalysis;

namespace Generator.Tests.Generator;

internal class TestHelpers
{
    internal static ITypeSymbol GetSymbol(string code, string metadataName)
    {
        var compilation = TestBase.Create(code);
        var symbol = compilation.GetTypeByMetadataName(metadataName);
        return symbol!;
    }
}
