using Blueprint.HttpBinder.Extensions;
using Microsoft.CodeAnalysis;
using static Blueprint.HttpBinder.Generator.Tests.Generator.TestHelpers;

namespace Blueprint.HttpBinder.Generator.Tests.Generator.Extensions;

public class TypeSymbolExtensionsTests
{
    [Test]
    public async Task GetFullTypeName_GivenATypeSymbol_ThenReturnsFullyQualifiedName()
    {
        const string code = "namespace A { public class Foo {} }";
        var symbol = GetTestSymbol(code, "A.Foo");

        var result = symbol.GetFullTypeName();

        await Assert.That(result).IsEqualTo("global::A.Foo");
    }

    [Test]
    public async Task GetMinimalTypeName_GivenATypeSymbol_ThenReturnsMinimalName()
    {
        const string code = "namespace A { public class Foo {} }";
        var symbol = GetTestSymbol(code, "A.Foo");

        var result = symbol.GetMinimalTypeName();

        await Assert.That(result).IsEqualTo("Foo");
    }

    [Test]
    public async Task IsDictionary_GivenADictionaryType_ThenReturnsTrue()
    {
        const string code = """
                            using System.Collections.Generic;
                            namespace A { public class T { public Dictionary<string,int> X {get;set;} } }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsDictionary()).IsTrue();
    }

    [Test]
    public async Task IsDictionary_GivenAnIDictionaryType_ThenReturnsTrue()
    {
        const string code = """
                            using System.Collections.Generic;
                            namespace A { public class T { public IDictionary<string,int> X {get;set;} } }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsDictionary()).IsTrue();
    }

    [Test]
    public async Task IsDictionary_GivenAIReadOnlyDictionaryType_ThenReturnsTrue()
    {
        const string code = """
                            using System.Collections.Generic;
                            namespace A { public class T { public IReadOnlyDictionary<string,int> X {get;set;} } }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsDictionary()).IsTrue();
    }

    [Test]
    public async Task IsDictionary_GivenAListType_ThenReturnsFalse()
    {
        const string code = """
                            using System.Collections.Generic;
                            namespace A { public class T { public List<int> X {get;set;} } }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsDictionary()).IsFalse();
    }

    [Test]
    public async Task IsDictionary_GivenAGenericTypeThatIsNotADictionary_ThenReturnsFalse()
    {
        const string code = """
                            using System.Collections.Generic;
                            namespace A { public class T { public List<string> X {get;set;} } }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsDictionary()).IsFalse();
    }

    [Test]
    public async Task IsDictionary_GivenANonGenericType_ThenReturnsFalse()
    {
        const string code = "namespace A { public class T { public int X {get;set;} } }";

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsDictionary()).IsFalse();
    }

    [Test]
    public async Task IsNestedCollection_GivenANestedCollection_ThenReturnsTrue()
    {
        const string code = """
                            using System.Collections.Generic;
                            namespace A { public class T { public List<List<int>> X {get;set;} } }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsNestedCollection()).IsTrue();
    }

    [Test]
    public async Task IsNestedCollection_GivenANonNestedCollection_ThenReturnsFalse()
    {
        const string code = """
                            using System.Collections.Generic;
                            namespace A { public class T { public List<int> X {get;set;} } }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsNestedCollection()).IsFalse();
    }

    [Test]
    public async Task IsNestedCollection_GivenANonCollectionType_ThenReturnsFalse()
    {
        const string code = "namespace A { public class T { public int X {get;set;} } }";

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        await Assert.That(prop.Type.IsNestedCollection()).IsFalse();
    }


    [Test]
    public async Task GetScalarTypeInfo_GivenNullableInt_ThenIsNullableIsTrue()
    {
        const string code = """
                            namespace A { 
                                public class T { 
                                    public int? X { get; set; } 
                                } 
                            }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        var scalarTypeInfo = prop.Type.GetScalarTypeInfo();

        await Assert.That(scalarTypeInfo.IsNullable).IsTrue();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenGuid_ThenIsGuidIsTrue()
    {
        const string code = """
                            using System;
                                namespace A { 
                                public class T { 
                                    public Guid X { get; set; } 
                                } 
                            }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        var scalarTypeInfo = prop.Type.GetScalarTypeInfo();

        await Assert.That(scalarTypeInfo.IsGuid).IsTrue();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenEnum_ThenIsEnumIsTrue()
    {
        const string code = """
                            namespace A {
                                public enum E { A, B }
                                public class T { public E X {get;set;} }
                             }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        var scalarTypeInfo = prop.Type.GetScalarTypeInfo();

        await Assert.That(scalarTypeInfo.IsEnum).IsTrue();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenPrimitiveInt_ThenIsPrimitiveIsTrue()
    {
        const string code = """
                            namespace A { 
                                public class T { 
                                    public int X { get; set; } 
                                } 
                            }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        var scalarTypeInfo = prop.Type.GetScalarTypeInfo();

        await Assert.That(scalarTypeInfo.IsPrimitive).IsTrue();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenString_ThenIsStringIsTrue()
    {
        const string code = """
                            namespace A { 
                                public class T { 
                                    public string X { get; set; } 
                                } 
                            }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        var scalarTypeInfo = prop.Type.GetScalarTypeInfo();

        await Assert.That(scalarTypeInfo.IsString).IsTrue();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenCustomClass_ThenIsReferenceTypeIsTrue()
    {
        const string code = """
                            namespace A { 
                                public class C {} 
                                public class T { 
                                    public C X { get; set; } 
                                } 
                            }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        var scalarTypeInfo = prop.Type.GetScalarTypeInfo();

        await Assert.That(scalarTypeInfo.IsReferenceType).IsTrue();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenAFormFile_ThenIsFormFileIsTrue()
    {
        const string code = """
                            namespace A { 
                                public class T {
                                    public Microsoft.AspNetCore.Http.IFormFile X { get; set; } 
                                } 
                            }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        var scalarTypeInfo = prop.Type.GetScalarTypeInfo();

        await Assert.That(scalarTypeInfo.IsFormFile).IsTrue();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenAFormFileCollection_ThenIsFormFileIsTrue()
    {
        const string code = """
                            namespace A { 
                                        public class T {
                                            public Microsoft.AspNetCore.Http.IFormFileCollection X { get; set; } 
                                        } 
                                    }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        var scalarTypeInfo = prop.Type.GetScalarTypeInfo();

        await Assert.That(scalarTypeInfo.IsFormFile).IsTrue();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenDateTime_ThenIsPrimitiveIsTrue()
    {
        const string code = """
                            namespace A {
                                        public class T {
                                            public System.DateTime X { get; set; }
                                        }
                                    }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];
        var scalarTypeInfo = prop.Type.GetScalarTypeInfo();

        await Assert.That(scalarTypeInfo.IsPrimitive).IsTrue();
        await Assert.That(scalarTypeInfo.IsReferenceType).IsFalse();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenNullableDateTime_ThenIsNullableIsTrueAndPrimitive()
    {
        const string code = """
                            namespace A {
                                        public class T {
                                            public System.DateTime? X { get; set; }
                                        }
                                    }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];
        var scalarTypeInfo = prop.Type.GetScalarTypeInfo();

        await Assert.That(scalarTypeInfo.IsNullable).IsTrue();
        await Assert.That(scalarTypeInfo.IsPrimitive).IsTrue();
        await Assert.That(scalarTypeInfo.IsReferenceType).IsFalse();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenDateTimeOffset_ThenIsPrimitiveIsTrue()
    {
        const string code = """
                            namespace A {
                                        public class T {
                                            public System.DateTimeOffset X { get; set; }
                                        }
                                    }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];
        var scalarTypeInfo = prop.Type.GetScalarTypeInfo();

        await Assert.That(scalarTypeInfo.IsPrimitive).IsTrue();
        await Assert.That(scalarTypeInfo.IsReferenceType).IsFalse();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenNullableDateTimeOffset_ThenIsNullableIsTrueAndPrimitive()
    {
        const string code = """
                            namespace A {
                                        public class T {
                                            public System.DateTimeOffset? X { get; set; }
                                        }
                                    }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];
        var scalarTypeInfo = prop.Type.GetScalarTypeInfo();

        await Assert.That(scalarTypeInfo.IsNullable).IsTrue();
        await Assert.That(scalarTypeInfo.IsPrimitive).IsTrue();
        await Assert.That(scalarTypeInfo.IsReferenceType).IsFalse();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenDateOnly_ThenIsPrimitiveIsTrue()
    {
        const string code = """
                            namespace A {
                                        public class T {
                                            public System.DateOnly X { get; set; }
                                        }
                                    }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];
        var scalarTypeInfo = prop.Type.GetScalarTypeInfo();

        await Assert.That(scalarTypeInfo.IsPrimitive).IsTrue();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenTimeOnly_ThenIsPrimitiveIsTrue()
    {
        const string code = """
                            namespace A {
                                        public class T {
                                            public System.TimeOnly X { get; set; }
                                        }
                                    }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];
        var scalarTypeInfo = prop.Type.GetScalarTypeInfo();

        await Assert.That(scalarTypeInfo.IsPrimitive).IsTrue();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenTimeSpan_ThenIsPrimitiveIsTrue()
    {
        const string code = """
                            namespace A {
                                        public class T {
                                            public System.TimeSpan X { get; set; }
                                        }
                                    }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];
        var scalarTypeInfo = prop.Type.GetScalarTypeInfo();

        await Assert.That(scalarTypeInfo.IsPrimitive).IsTrue();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenIntArray_ThenIsCollectionPrimitiveNotReferenceType()
    {
        const string code = """
            namespace A {
                public class T {
                    public int[] X { get; set; }
                }
            }
            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        var isCollection = prop.Type.FindCollectionType() != null;
        var elementType = prop.Type.FindCollectionType()!;
        var scalar = elementType.GetScalarTypeInfo();

        await Assert.That(isCollection).IsTrue();
        await Assert.That(scalar.IsPrimitive).IsTrue();
        await Assert.That(scalar.IsReferenceType).IsFalse();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenListInt_ThenIsCollectionPrimitiveNotReferenceType()
    {
        const string code = """
        using System.Collections.Generic;
        namespace A {
            public class T {
                public List<int> X { get; set; }
            }
        }
        """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        var elementType = prop.Type.FindCollectionType()!;
        var scalar = elementType.GetScalarTypeInfo();

        await Assert.That(scalar.IsPrimitive).IsTrue();
        await Assert.That(scalar.IsReferenceType).IsFalse();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenIEnumerableInt_ThenIsCollectionPrimitiveNotReferenceType()
    {
        const string code = """
            using System.Collections.Generic;
            namespace A {
                public class T {
                    public IEnumerable<int> X { get; set; }
                }
            }
            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];
        var elementType = prop.Type.FindCollectionType()!;
        var scalar = elementType.GetScalarTypeInfo();

        await Assert.That(scalar.IsPrimitive).IsTrue();
        await Assert.That(scalar.IsReferenceType).IsFalse();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenNullableIntArray_ThenElementIsNullablePrimitive()
    {
        const string code = """
            namespace A {
                public class T {
                    public int?[] X { get; set; }
                }
            }
            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        var elementType = prop.Type.FindCollectionType()!;
        var scalar = elementType.GetScalarTypeInfo();

        await Assert.That(scalar.IsNullable).IsTrue();
        await Assert.That(scalar.IsPrimitive).IsTrue();
        await Assert.That(scalar.IsReferenceType).IsFalse();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenListGuid_ThenIsGuidNotReferenceType()
    {
        const string code = """
        using System;
        using System.Collections.Generic;

        namespace A {
            public class T {
                public List<Guid> X { get; set; }
            }
        }
        """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];
        var elementType = prop.Type.FindCollectionType()!;
        var scalar = elementType.GetScalarTypeInfo();

        await Assert.That(scalar.IsGuid).IsTrue();
        await Assert.That(scalar.IsReferenceType).IsFalse();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenListOfReferenceTypeType_ThenIsReferenceTypeIsTrue()
    {
        const string code = """
        namespace A {
            public class C { public int A { get; set; } }
            public class T {
                public System.Collections.Generic.List<C> X { get; set; }
            }
        }
        """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        var elementType = prop.Type.FindCollectionType()!;
        var scalar = elementType.GetScalarTypeInfo();

        await Assert.That(scalar.IsReferenceType).IsTrue();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenReferenceTypeScalar_ThenIsReferenceTypeIsTrue()
    {
        const string code = """
        namespace A {
            public class C {}
            public class T {
                public C X { get; set; }
            }
        }
        """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];
        var scalar = prop.Type.GetScalarTypeInfo();

        await Assert.That(scalar.IsReferenceType).IsTrue();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenReferenceTypeArray_ThenIsReferenceTypeIsTrue()
    {
        const string code = """
        namespace A {
            public class C {}
            public class T {
                public C[] X { get; set; }
            }
        }
        """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];
        var elementType = prop.Type.FindCollectionType()!;
        var scalar = elementType.GetScalarTypeInfo();

        await Assert.That(scalar.IsReferenceType).IsTrue();
    }

    [Test]
    public async Task GetScalarTypeInfo_GivenFormFileArray_ThenIsFormFileIsTrue()
    {
        const string code = """
        using Microsoft.AspNetCore.Http;

        public class T {
            public IFormFile[] Files { get; set; }
        }
        """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "T").GetMembers("Files")[0];
        var element = prop.Type.FindCollectionType()!;
        var scalar = element.GetScalarTypeInfo();

        await Assert.That(scalar.IsFormFile).IsTrue();
    }

    [Test]
    public async Task FindCollectionType_GivenArrayType_ThenReturnsElementType()
    {
        const string code = """
                            namespace A { 
                                        public class T { 
                                            public int[] X { get; set; } 
                                        } 
                                    }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        var result = prop.Type.FindCollectionType();

        await Assert.That(result!.ToString()).IsEqualTo("int");
    }

    [Test]
    public async Task FindCollectionType_GivenListType_ThenReturnsElementType()
    {
        const string code = """

                                        using System.Collections.Generic;

                                        namespace A { 
                                        public class T { 
                                            public List<string> X { get; set; } 
                                        } 
                                    }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        var result = prop.Type.FindCollectionType();

        await Assert.That(result!.ToString()).IsEqualTo("string");
    }

    [Test]
    public async Task FindCollectionType_GivenNonCollectionType_ThenReturnsNull()
    {
        const string code = """
                            namespace A { 
                                        public class T { 
                                            public int X { get; set; } 
                                        } 
                                    }
                            """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "A.T").GetMembers("X")[0];

        var result = prop.Type.FindCollectionType();

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task IsFormFile_GivenAIFormFile_ReturnsTrue()
    {
        const string code = """
        using Microsoft.AspNetCore.Http;

        public class T {
            public IFormFile File { get; set; }
        }
        """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "T").GetMembers("File")[0];
        var result = prop.Type.IsSingleFormFile();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsFormFile_GivenAIFormFileCollection_ReturnsTrue()
    {
        const string code = """
        using Microsoft.AspNetCore.Http;

        public class T {
            public IFormFileCollection Files { get; set; }
        }
        """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "T").GetMembers("Files")[0];
        var result = prop.Type.IsListOfFormFiles();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsFormFile_GivenAIEnumerableIFormFile_ReturnsTrue()
    {
        const string code = """
        using Microsoft.AspNetCore.Http;
        using System.Collections.Generic;

        public class T {
            public IEnumerable<IFormFile> Files { get; set; }
        }
        """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "T").GetMembers("Files")[0];
        var result = prop.Type.IsListOfFormFiles();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsFormFile_GivenAnArrayIFormFile_ReturnsTrue()
    {
        const string code = """
        using Microsoft.AspNetCore.Http;
        using System.Collections.Generic;

        public class T {
            public IFormFile[] Files { get; set; }
        }
        """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "T").GetMembers("Files")[0];
        var result = prop.Type.IsListOfFormFiles();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsFormFile_GivenAListIFormFile_ReturnsTrue()
    {
        const string code = """
        using Microsoft.AspNetCore.Http;
        using System.Collections.Generic;

        public class T {
            public List<IFormFile> Files { get; set; }
        }
        """;

        var prop = (IPropertySymbol)GetTestSymbol(code, "T").GetMembers("Files")[0];
        var result = prop.Type.IsListOfFormFiles();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsFormFile_NonFileTypes_ReturnFalse()
    {
        const string code = """
        public class T {
            public string Name { get; set; }
            public int Age { get; set; }
            public object Obj { get; set; }
        }
        """;

        var t = GetTestSymbol(code, "T");

        foreach (var prop in t.GetMembers().OfType<IPropertySymbol>())
        {
            var result = prop.Type.IsSingleFormFile();
            await Assert.That(result).IsFalse();
        }
    }

}
