using Blueprint.HttpBinder;
using Microsoft.CodeAnalysis.CSharp;

namespace Blueprint.HttpBinder.Generator.Tests.Generator;

public sealed class BoundPropertyTests
{
    private static BoundProperty Create(
        string name = "Age",
        string keyName = "Age",
        string declaredType = "int",
        HttpBinderType binder = HttpBinderType.Form,
        ScalarTypeInfo? scalar = null,
        bool isNullable = false,
        bool isCollection = false,
        bool isCtor = false,
        EquatableArray<BoundProperty>? children = null)
    {
        return new BoundProperty(
            name,
            keyName,
            declaredType,
            binder,
            scalar ?? new ScalarTypeInfo(
                IsNullable: isNullable,
                IsGuid: declaredType == "System.Guid",
                IsEnum: false,
                IsPrimitive: declaredType == "int",
                IsString: declaredType == "string",
                IsFormFile: false,
                TypeName: declaredType),
            isNullable,
            isCollection,
            isCtor,
            children ?? []
        );
    }

    [Test, Arguments("class", "@class"),
        Arguments("Operator", "@Operator")]
    public async Task GivenKeywordName_WhenConstructed_ThenNameIsEscaped(string value, string expectedValue)
    {
        var p = Create(name: value);
        await Assert.That(p.Name).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("class", "@class"), Arguments("operator", "@operator")]
    public async Task GivenKeywordName_WhenConstructed_ThenKeyNameIsEscaped(string value, string expectedValue)
    {
        var p = Create(keyName: value);
        await Assert.That(p.KeyName).IsEqualTo(expectedValue);
    }

    [Test, Arguments("Operator", "@operator")]
    public async Task GivenKeywordName_WhenConstructed_ThenCamelCaseNameIsEscaped(string value, string expectedValue)
    {
        var p = Create(name: value);
        await Assert.That(p.CamelCaseName).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task GivenNormalIdentifier_WhenConstructed_ThenNameIsUnchanged()
    {
        var p = Create(name: "First");
        await Assert.That(p.Name).IsEqualTo("First");
    }

    [Test]
    public async Task GivenName_WhenConstructed_ThenCamelCaseNameComputed()
    {
        var p = Create(name: "FirstName");
        await Assert.That(p.CamelCaseName).IsEqualTo("firstName");
    }

    [Test]
    public async Task GivenNonNullableType_WhenRequested_ThenTypeNamesAreCorrect()
    {
        var p = Create(declaredType: "int", isNullable: false);
        await Assert.That(p.TypeName).IsEqualTo("int");
        await Assert.That(p.NonNullTypeName).IsEqualTo("int");
    }

    [Test]
    public async Task GivenNullableType_WhenRequested_ThenNonNullTypeNameStripsSuffix()
    {
        var p = Create(declaredType: "int?", isNullable: true);
        await Assert.That(p.TypeName).IsEqualTo("int?");
        await Assert.That(p.NonNullTypeName).IsEqualTo("int");
    }

    [Test]
    public async Task GivenScalarTypeInfo_WhenConstructed_ThenScalarFlagsPropagate()
    {
        var scalar = new ScalarTypeInfo(
            IsNullable: false,
            IsGuid: true,
            IsEnum: false,
            IsPrimitive: false,
            IsString: false,
            IsFormFile: false,
            TypeName: "System.Guid");

        var p = new BoundProperty(
            "Id",
            "Id",
            "System.Guid",
            HttpBinderType.Form,
            scalar,
            isNullable: false,
            isCollection: false,
            isConstructorParameter: false,
            childProperties: []);

        await Assert.That(p.IsGuid).IsTrue();
        await Assert.That(p.IsEnum).IsFalse();
        await Assert.That(p.IsValueType).IsTrue();
        await Assert.That(p.IsReferenceType).IsFalse();
    }

    [Test]
    public async Task GivenFormBinder_WhenUsesFormValuesCalled_ThenReturnsTrue()
    {
        var p = Create(binder: HttpBinderType.Form);
        await Assert.That(p.UsesFormValues()).IsTrue();
    }

    [Test]
    public async Task GivenChildFormBinder_WhenUsesFormValuesCalled_ThenReturnsTrue()
    {
        var child = Create(binder: HttpBinderType.Form);
        var parent = Create(binder: HttpBinderType.Query, children: new EquatableArray<BoundProperty>([child]));

        await Assert.That(parent.UsesFormValues()).IsTrue();
    }

    [Test]
    public async Task GivenNoFormBinders_WhenUsesFormValuesCalled_ThenReturnsFalse()
    {
        var child = Create(binder: HttpBinderType.Query);
        var parent = Create(binder: HttpBinderType.Route, children: new EquatableArray<BoundProperty>([child]));

        await Assert.That(parent.UsesFormValues()).IsFalse();
    }

    [Test]
    public async Task GivenCtorFlagTrue_WhenConstructed_ThenIsConstructorParameterIsTrue()
    {
        var p = Create(isCtor: true);
        await Assert.That(p.IsConstructorParameter).IsTrue();
    }

    [Test]
    public async Task GivenChildProperties_WhenConstructed_ThenChildrenStoredCorrectly()
    {
        var c1 = Create(name: "A");
        var c2 = Create(name: "B");

        var p = Create(children: new EquatableArray<BoundProperty>([c1, c2]));

        await Assert.That(p.ChildProperties.Count).IsEqualTo(2);
        await Assert.That(p.ChildProperties.Contains(c1)).IsTrue();
        await Assert.That(p.ChildProperties.Contains(c2)).IsTrue();
    }

    [Test]
    public async Task GivenUnknownType_WhenGetTryParseMethodCalled_ThenFallbackToIntTryParse()
    {
        var p = Create(declaredType: "Custom.Type");
        await Assert.That(p.GetTryParseMethod()).IsEqualTo("int.TryParse");
    }

    [Test]
    public async Task GivenKnownScalarType_WhenGetTryParseMethodCalled_ThenReturnsExpectedMethod()
    {
        var cases = new (string TypeName, string Expected)[]
        {
            ("bool", "bool.TryParse"),
            ("byte", "byte.TryParse"),
            ("sbyte", "sbyte.TryParse"),
            ("short", "short.TryParse"),
            ("ushort", "ushort.TryParse"),
            ("int", "int.TryParse"),
            ("uint", "uint.TryParse"),
            ("long", "long.TryParse"),
            ("ulong", "ulong.TryParse"),
            ("float", "float.TryParse"),
            ("double", "double.TryParse"),
            ("decimal", "decimal.TryParse"),
            ("System.Guid", "System.Guid.TryParse"),
            ("System.DateTime", "System.DateTime.TryParse"),
            ("System.DateTimeOffset", "System.DateTimeOffset.TryParse"),
            ("System.DateOnly", "System.DateOnly.TryParse"),
            ("System.TimeOnly", "System.TimeOnly.TryParse"),
            ("System.TimeSpan", "System.TimeSpan.TryParse")
        };

        foreach (var (typeName, expected) in cases)
        {
            var scalar = new ScalarTypeInfo(
                IsNullable: false,
                IsGuid: typeName == "System.Guid",
                IsEnum: false,
                IsPrimitive: typeName is "bool" or "byte" or "sbyte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "decimal",
                IsString: typeName == "string",
                IsFormFile: false,
                TypeName: typeName);

            var p = Create(declaredType: typeName, scalar: scalar);

            await Assert.That(p.GetTryParseMethod()).IsEqualTo(expected);
        }
    }
}
