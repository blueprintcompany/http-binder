using BlueprintSoftware.HttpBinder.Extensions;
using static BlueprintSoftware.HttpBinder.Generator.Tests.Generator.TestHelpers;

namespace BlueprintSoftware.HttpBinder.Generator.Tests.Generator.Extensions;

public class SymbolExtensionsTests
{
    [Test]
    public async Task HasAttribute_WhenAttributePresent_ReturnsTrue()
    {
        const string code = """
                            using System;

                            [AttributeUsage(AttributeTargets.Class)]
                            public class TestAttr : Attribute {}

                            [TestAttr]
                            public class T {}
                            """;

        var symbol = GetTestSymbol(code, "T");
        var result = symbol.HasAttribute("TestAttr");

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task HasAttribute_WhenAttributeMissing_ReturnsFalse()
    {
        const string code = """
                            using System;

                            [AttributeUsage(AttributeTargets.Class)]
                            public class TestAttr : Attribute {}

                            public class T {}
                            """;

        var symbol = GetTestSymbol(code, "T");
        var result = symbol.HasAttribute("TestAttr");

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GetAttribute_WhenAttributePresent_ReturnsAttributeData()
    {
        const string code = """
                            using System;

                            [AttributeUsage(AttributeTargets.Class)]
                            public class TestAttr : Attribute {}

                            [TestAttr]
                            public class T {}
                            """;

        var symbol = GetTestSymbol(code, "T");
        var result = symbol.GetAttribute("TestAttr");

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.AttributeClass!.Name).IsEqualTo("TestAttr");
    }

    [Test]
    public async Task GetAttribute_WhenAttributeMissing_ReturnsNull()
    {
        const string code = """
                            using System;

                            [AttributeUsage(AttributeTargets.Class)]
                            public class TestAttr : Attribute {}

                            public class T {}
                            """;

        var symbol = GetTestSymbol(code, "T");
        var result = symbol.GetAttribute("TestAttr");

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task HasAttribute_FullNamespaceMatch_WorksCorrectly()
    {
        const string code = """
                            using System;

                            namespace A.B {
                                [AttributeUsage(AttributeTargets.Class)]
                                public class TestAttr : Attribute {}
                            }

                            [A.B.TestAttr]
                            public class T {}
                            """;

        var symbol = GetTestSymbol(code, "T");
        var result = symbol.HasAttribute("A.B.TestAttr");

        await Assert.That(result).IsTrue();
    }
}

