using Blueprint.HttpBinder.Extensions;
using static Generator.Tests.Generator.TestHelpers;

namespace Generator.Tests.Generator.Extensions;

public class SymbolExtensionsTests
{
    [Test]
    public async Task HasAttribute_WhenAttributePresent_ReturnsTrue()
    {
        var code = """
        using System;

        [AttributeUsage(AttributeTargets.Class)]
        public class TestAttr : Attribute {}

        [TestAttr]
        public class T {}
        """;

        var symbol = GetSymbol(code, "T");
        var result = symbol.HasAttribute("TestAttr");

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task HasAttribute_WhenAttributeMissing_ReturnsFalse()
    {
        var code = """
        using System;

        [AttributeUsage(AttributeTargets.Class)]
        public class TestAttr : Attribute {}

        public class T {}
        """;

        var symbol = GetSymbol(code, "T");
        var result = symbol.HasAttribute("TestAttr");

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GetAttribute_WhenAttributePresent_ReturnsAttributeData()
    {
        var code = """
        using System;

        [AttributeUsage(AttributeTargets.Class)]
        public class TestAttr : Attribute {}

        [TestAttr]
        public class T {}
        """;

        var symbol = GetSymbol(code, "T");
        var result = symbol.GetAttribute("TestAttr");

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.AttributeClass!.Name).IsEqualTo("TestAttr");
    }

    [Test]
    public async Task GetAttribute_WhenAttributeMissing_ReturnsNull()
    {
        var code = """
        using System;

        [AttributeUsage(AttributeTargets.Class)]
        public class TestAttr : Attribute {}

        public class T {}
        """;

        var symbol = GetSymbol(code, "T");
        var result = symbol.GetAttribute("TestAttr");

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task HasAttribute_FullNamespaceMatch_WorksCorrectly()
    {
        var code = """
        using System;

        namespace A.B {
            [AttributeUsage(AttributeTargets.Class)]
            public class TestAttr : Attribute {}
        }

        [A.B.TestAttr]
        public class T {}
        """;

        var symbol = GetSymbol(code, "T");
        var result = symbol.HasAttribute("A.B.TestAttr");

        await Assert.That(result).IsTrue();
    }
}

