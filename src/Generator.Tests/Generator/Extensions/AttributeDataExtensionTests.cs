using Blueprint.HttpBinder;
using Blueprint.HttpBinder.Extensions;
using Microsoft.CodeAnalysis;
using static Generator.Tests.Generator.TestHelpers;

namespace Generator.Tests.Generator.Extensions;

public class AttributeDataExtensionsTests
{
    [Test]
    public async Task TryGetBinderType_WhenConstructorArgIsInt_ReturnsTrueAndParsedEnum()
    {
        var code = """
        public enum HttpBinderType { Form = 0, Query = 1, Route = 2 }

        public class HttpBinderAttribute : System.Attribute
        {
            public HttpBinderAttribute(HttpBinderType type) {}
        }

        [HttpBinder(HttpBinderType.Query)]
        public class T {}
        """;

        var symbol = GetSymbol(code, "T");
        var attr = symbol.GetAttributes().First();

        var ok = attr.TryGetBinderType(out var result);

        await Assert.That(ok).IsTrue();
        await Assert.That(result).IsEqualTo((HttpBinderType)1);
    }

    [Test]
    public async Task TryGetBinderType_WhenConstructorArgIsNotInt_ReturnsFalse()
    {
        var code = """
        public enum HttpBinderType { Form = 0 }

        public class HttpBinderAttribute : System.Attribute
        {
            public HttpBinderAttribute(string name) {}
        }

        [HttpBinder("x")]
        public class T {}
        """;

        var symbol = GetSymbol(code, "T");
        var attr = symbol.GetAttributes().First();

        var ok = attr.TryGetBinderType(out var result);

        await Assert.That(ok).IsFalse();
        await Assert.That(result).IsEqualTo(default(HttpBinderType)); // 0
    }

    [Test]
    public async Task TryGetBinderType_WhenNoConstructorArgs_ReturnsFalse()
    {
        var code = """
        public enum HttpBinderType { Query = 1 }

        public class HttpBinderAttribute : System.Attribute
        {
            public HttpBinderAttribute() {}
        }

        [HttpBinder]
        public class T {}
        """;

        var symbol = GetSymbol(code, "T");
        var attr = symbol.GetAttributes().First();

        var ok = attr.TryGetBinderType(out var result);

        await Assert.That(ok).IsFalse();
        await Assert.That(result).IsEqualTo(default(HttpBinderType));
    }
}
