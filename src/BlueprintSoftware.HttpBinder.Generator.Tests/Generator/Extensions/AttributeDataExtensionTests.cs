using BlueprintSoftware.HttpBinder.Extensions;
using static BlueprintSoftware.HttpBinder.Generator.Tests.Generator.TestHelpers;

namespace BlueprintSoftware.HttpBinder.Generator.Tests.Generator.Extensions;

public class AttributeDataExtensionsTests
{

    [Test]
    public async Task TryGetBinderType_WhenNamedArgPresent_ThenReturnsTrueAndParsesEnum()
    {
        const string code = """
                                public enum HttpBinderType
                                {
                                    Form,
                                    Query,
                                    Route
                                }
                                public class HttpBinderAttribute : System.Attribute
                                {
                                    public HttpBinderType HttpBinderType { get; set; }
                                }

                                [HttpBinder(HttpBinderType = HttpBinderType.Query)]
                                public class T {}
                            """;

        var symbol = GetTestSymbol(code, "T");
        var attr = symbol.GetAttributes().Single(a =>
            a.AttributeClass!.Name == nameof(HttpBinderAttribute));

        var ok = attr.TryGetBinderType(out var result);

        await Assert.That(ok).IsTrue();
        await Assert.That(result).IsEqualTo(HttpBinderType.Query);
    }

    [Test]
    public async Task TryGetBinderType_WhenNamedArgMissing_ThenReturnsFalse()
    {
        const string code = """
                                public class HttpBinderAttribute : System.Attribute
                                {
                                    public HttpBinderType HttpBinderType { get; set; }
                                }
                                [HttpBinder]
                                public class T {}
                            """;

        var symbol = GetTestSymbol(code, "T");
        var attr = symbol.GetAttributes().Single(a =>
            a.AttributeClass!.Name == nameof(HttpBinderAttribute));

        var ok = attr.TryGetBinderType(out var result);

        await Assert.That(ok).IsFalse();
        await Assert.That(result).IsEqualTo(default(HttpBinderType));
    }


    [Test]
    public async Task TryGetBinderType_WhenNamedArgIsWrongType_ThenReturnsFalse()
    {
        const string code = """
                                public enum SomethingElse { X = 123 }

                                public class HttpBinderAttribute : System.Attribute
                                {
                                    public HttpBinderType HttpBinderType { get; set; }
                                }

                                [HttpBinder(HttpBinderType = (SomethingElse)123)]
                                public class T {}
                            """;

        var symbol = GetTestSymbol(code, "T");
        var attr = symbol.GetAttributes().Single(a =>
            a.AttributeClass!.Name == nameof(HttpBinderAttribute));

        var ok = attr.TryGetBinderType(out var result);

        await Assert.That(ok).IsFalse();
        await Assert.That(result).IsEqualTo(default(HttpBinderType));
    }

    [Test]
    public async Task TryGetBinderType_WhenDifferentNamedArgExists_ThenReturnsFalse()
    {
        const string code = """
                                public enum HttpBinderType
                                {
                                    Form,
                                    Query,
                                    Route
                                }
                                public class HttpBinderAttribute : System.Attribute
                                {
                                    public HttpBinderType HttpBinderType { get; set; }
                                    public int SomethingElse { get; set; }
                                }

                                [HttpBinder(SomethingElse = 99)]
                                public class T {}
                            """;

        var symbol = GetTestSymbol(code, "T");
        var attr = symbol.GetAttributes().Single(a =>
            a.AttributeClass!.Name == nameof(HttpBinderAttribute));

        var ok = attr.TryGetBinderType(out var result);

        await Assert.That(ok).IsFalse();
        await Assert.That(result).IsEqualTo(default(HttpBinderType));
    }

    [Test]
    public async Task TryGetBinderType_WhenMultipleNamedArgs_ThenStillUsesCorrectOne()
    {
        const string code = """
                            public enum HttpBinderType
                            {
                                Form,
                                Query,
                                Route
                            }

                            public class HttpBinderAttribute : System.Attribute
                            {
                                public HttpBinderType HttpBinderType { get; set; }
                                public int Other { get; set; }
                            }

                            [HttpBinder(Other = 12, HttpBinderType = HttpBinderType.Route)]
                            public class T {}
                            """;

        var symbol = GetTestSymbol(code, "T");
        var attr = symbol.GetAttributes().Single(a =>
            a.AttributeClass!.Name == nameof(HttpBinderAttribute));

        var ok = attr.TryGetBinderType(out var result);

        await Assert.That(ok).IsTrue();
        await Assert.That(result).IsEqualTo(HttpBinderType.Route);
    }

    [Test]
    public async Task TryGetBinderType_WhenAttributeMissing_ThenReturnsFalse()
    {
        const string code = "public class T {}";

        var symbol = GetTestSymbol(code, "T");

        var attr = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass!.Name == nameof(HttpBinderAttribute));

        await Assert.That(attr).IsNull();
    }
}
