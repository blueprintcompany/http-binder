using Blueprint.HttpBinder.Extensions;
using Microsoft.CodeAnalysis;
using static Generator.Tests.Generator.TestHelpers;

namespace Generator.Tests.Generator.Extensions;

public class TypeSymbolExtensionsTests
{
    [Test]
    public async Task GetFullTypeName_GivenATypeSymbol_ThenReturnsFullyQualifiedName()
    {
        var code = @"namespace A { public class Foo {} }";
        var symbol = GetSymbol(code, "A.Foo");

        var result = symbol.GetFullTypeName();

        await Assert.That(result).IsEqualTo("global::A.Foo");
    }

    [Test]
    public async Task GetMinimalTypeName_GivenATypeSymbol_ThenReturnsMinimalName()
    {
        var code = @"namespace A { public class Foo {} }";
        var symbol = GetSymbol(code, "A.Foo");

        var result = symbol.GetMinimalTypeName();

        await Assert.That(result).IsEqualTo("Foo");
    }

    [Test]
    public async Task IsDictionary_GivenADictionaryType_ThenReturnsTrue()
    {
        var code = @"using System.Collections.Generic;
                     namespace A { public class T { public Dictionary<string,int> X {get;set;} } }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsDictionary()).IsTrue();
    }

    [Test]
    public async Task IsDictionary_GivenAnIDictionaryType_ThenReturnsTrue()
    {
        var code = @"using System.Collections.Generic;
                     namespace A { public class T { public IDictionary<string,int> X {get;set;} } }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsDictionary()).IsTrue();
    }

    [Test]
    public async Task IsDictionary_GivenAIReadOnlyDictionaryType_ThenReturnsTrue()
    {
        var code = @"using System.Collections.Generic;
                     namespace A { public class T { public IReadOnlyDictionary<string,int> X {get;set;} } }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsDictionary()).IsTrue();
    }

    [Test]
    public async Task IsDictionary_GivenAListType_ThenReturnsFalse()
    {
        var code = @"using System.Collections.Generic;
                     namespace A { public class T { public List<int> X {get;set;} } }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsDictionary()).IsFalse();
    }

    [Test]
    public async Task IsDictionary_GivenAGenericTypeThatIsNotADictionary_ThenReturnsFalse()
    {
        var code = @"using System.Collections.Generic;
                     namespace A { public class T { public List<string> X {get;set;} } }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsDictionary()).IsFalse();
    }

    [Test]
    public async Task IsDictionary_GivenANonGenericType_ThenReturnsFalse()
    {
        var code = @"namespace A { public class T { public int X {get;set;} } }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsDictionary()).IsFalse();
    }

    [Test]
    public async Task IsNestedCollection_GivenANestedCollection_ThenReturnsTrue()
    {
        var code = @"using System.Collections.Generic;
                     namespace A { public class T { public List<List<int>> X {get;set;} } }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsNestedCollection()).IsTrue();
    }

    [Test]
    public async Task IsNestedCollection_GivenANonNestedCollection_ThenReturnsFalse()
    {
        var code = @"using System.Collections.Generic;
                     namespace A { public class T { public List<int> X {get;set;} } }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsNestedCollection()).IsFalse();
    }

    [Test]
    public async Task IsNestedCollection_GivenANonCollectionType_ThenReturnsFalse()
    {
        var code = @"namespace A { public class T { public int X {get;set;} } }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsNestedCollection()).IsFalse();
    }


    [Test]
    public async Task GetPropertyAttributes_GivenNullableInt_ThenIsNullableIsTrue()
    {
        var code = @"namespace A { 
            public class T { 
                public int? X { get; set; } 
            } 
        }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        var (isNullable, _, _, _, _, _, _) = prop.Type.GetPropertyAttributes();

        await Assert.That(isNullable).IsTrue();
    }

    [Test]
    public async Task GetPropertyAttributes_GivenGuid_ThenIsGuidIsTrue()
    {
        var code = @"using System;
            namespace A { 
            public class T { 
                public Guid X { get; set; } 
            } 
        }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        var (_, isGuid, _, _, _, _, _) = prop.Type.GetPropertyAttributes();

        await Assert.That(isGuid).IsTrue();
    }

    [Test]
    public async Task GetPropertyAttributes_GivenEnum_ThenIsEnumIsTrue()
    {
        var code = @"namespace A {
                        public enum E { A, B }
                        public class T { public E X {get;set;} }
                     }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        var (_, _, isEnum, _, _, _, _) = prop.Type.GetPropertyAttributes();

        await Assert.That(isEnum).IsTrue();
    }

    [Test]
    public async Task GetPropertyAttributes_GivenPrimitiveInt_ThenIsPrimitiveIsTrue()
    {
        var code = @"namespace A { 
            public class T { 
                public int X { get; set; } 
            } 
        }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        var (_, _, _, isPrimitive, _, _, _) = prop.Type.GetPropertyAttributes();

        await Assert.That(isPrimitive).IsTrue();
    }

    [Test]
    public async Task GetPropertyAttributes_GivenString_ThenIsStringIsTrue()
    {
        var code = @"namespace A { 
            public class T { 
                public string X { get; set; } 
            } 
        }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        var (_, _, _, _, isString, _, _) = prop.Type.GetPropertyAttributes();

        await Assert.That(isString).IsTrue();
    }

    [Test]
    public async Task GetPropertyAttributes_GivenCustomClass_ThenIsComplexIsTrue()
    {
        var code = @"namespace A { 
            public class C {} 
            public class T { 
                public C X { get; set; } 
            } 
        }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        var (_, _, _, _, _, isComplex, _) = prop.Type.GetPropertyAttributes();

        await Assert.That(isComplex).IsTrue();
    }

    [Test]
    public async Task GetPropertyAttributes_GivenAFormFile_ThenIsFormFileIsTrue()
    {
        var code = @"namespace A { 
            public class T {
                public Microsoft.AspNetCore.Http.IFormFile X { get; set; } 
            } 
        }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        var (_, _, _, _, _, _, isFormFile) = prop.Type.GetPropertyAttributes();

        await Assert.That(isFormFile).IsTrue();
    }

    [Test]
    public async Task GetPropertyAttributes_GivenAFormFileCollection_ThenIsFormFileIsTrue()
    {
        var code = @"namespace A { 
            public class T {
                public Microsoft.AspNetCore.Http.IFormFileCollection X { get; set; } 
            } 
        }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        var (_, _, _, _, _, _, isFormFile) = prop.Type.GetPropertyAttributes();

        await Assert.That(isFormFile).IsTrue();
    }

    [Test]
    public async Task FindCollectionType_GivenArrayType_ThenReturnsElementType()
    {
        var code = @"namespace A { 
            public class T { 
                public int[] X { get; set; } 
            } 
        }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        var result = prop.Type.FindCollectionType();

        await Assert.That(result!.ToString()).IsEqualTo("int");
    }

    [Test]
    public async Task FindCollectionType_GivenListType_ThenReturnsElementType()
    {
        var code = @"using System.Collections.Generic;
                     namespace A { 
            public class T { 
                public List<string> X { get; set; } 
            } 
        }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        var result = prop.Type.FindCollectionType();

        await Assert.That(result!.ToString()).IsEqualTo("string");
    }

    [Test]
    public async Task FindCollectionType_GivenNonCollectionType_ThenReturnsNull()
    {
        var code = @"namespace A { 
            public class T { 
                public int X { get; set; } 
            } 
        }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        var result = prop.Type.FindCollectionType();

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task IsFormFile_IFormFile_ReturnsTrue()
    {
        var code = """
        using Microsoft.AspNetCore.Http;

        public class T {
            public IFormFile File { get; set; }
        }
        """;

        var prop = (IPropertySymbol)GetSymbol(code, "T").GetMembers("File")[0];
        var result = prop.Type.IsFormFile();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsFormFile_IFormFileCollection_ReturnsTrue()
    {
        var code = """
        using Microsoft.AspNetCore.Http;

        public class T {
            public IFormFileCollection Files { get; set; }
        }
        """;

        var prop = (IPropertySymbol)GetSymbol(code, "T").GetMembers("Files")[0];
        var result = prop.Type.IsFormFile();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsFormFile_IEnumerableIFormFile_ReturnsTrue()
    {
        var code = """
        using Microsoft.AspNetCore.Http;
        using System.Collections.Generic;

        public class T {
            public IEnumerable<IFormFile> Files { get; set; }
        }
        """;

        var prop = (IPropertySymbol)GetSymbol(code, "T").GetMembers("Files")[0];
        var result = prop.Type.IsFormFile();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsFormFile_NonFileTypes_ReturnFalse()
    {
        var code = """
        public class T {
            public string Name { get; set; }
            public int Age { get; set; }
            public object Obj { get; set; }
        }
        """;

        var t = GetSymbol(code, "T");

        foreach (var prop in t.GetMembers().OfType<IPropertySymbol>())
        {
            var result = prop.Type.IsFormFile();
            await Assert.That(result).IsFalse();
        }
    }

}
