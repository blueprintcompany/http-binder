using Microsoft.CodeAnalysis;
using Blueprint.HttpBinder;

namespace Generator.Tests.Generator;

public class TypeSymbolExtensionsTests
{
    private static ITypeSymbol GetSymbol(string code, string metadataName)
    {
        var compilation = TestBase.Create(code);
        var symbol = compilation.GetTypeByMetadataName(metadataName);
        return symbol!;
    }

    // =====================================================================
    // GetFullTypeName
    // =====================================================================

    [Test]
    public async Task GetFullTypeName_GivenATypeSymbol_ThenReturnsFullyQualifiedName()
    {
        var code = @"namespace A { public class Foo {} }";
        var symbol = GetSymbol(code, "A.Foo");

        var result = symbol.GetFullTypeName();

        await Assert.That(result).IsEqualTo("global::A.Foo");
    }

    // =====================================================================
    // GetMinimalTypeName
    // =====================================================================

    [Test]
    public async Task GetMinimalTypeName_GivenATypeSymbol_ThenReturnsMinimalName()
    {
        var code = @"namespace A { public class Foo {} }";
        var symbol = GetSymbol(code, "A.Foo");

        var result = symbol.GetMinimalTypeName();

        await Assert.That(result).IsEqualTo("Foo");
    }

    // =====================================================================
    // IsDictionary
    // =====================================================================

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

    // =====================================================================
    // IsNestedCollection
    // =====================================================================

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
        var code = @"namespace A { public class T { public int? X {get;set;} } }";

        var prop = (IPropertySymbol)GetSymbol(code, "A.T").GetMembers("X")[0];

        var (isNullable, _, _, _, _, _) = prop.Type.GetPropertyAttributes();

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

        var (_, isGuid, _, _, _, _) = prop.Type.GetPropertyAttributes();

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

        var (_, _, isEnum, _, _, _) = prop.Type.GetPropertyAttributes();

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

        var (_, _, _, isPrimitive, _, _) = prop.Type.GetPropertyAttributes();

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

        var (_, _, _, _, isString, _) = prop.Type.GetPropertyAttributes();

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

        var (_, _, _, _, _, isComplex) = prop.Type.GetPropertyAttributes();

        await Assert.That(isComplex).IsTrue();
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
}

