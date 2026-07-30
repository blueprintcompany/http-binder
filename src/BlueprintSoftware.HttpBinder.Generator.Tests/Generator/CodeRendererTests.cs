namespace BlueprintSoftware.HttpBinder.Generator.Tests.Generator;

public sealed class CodeRendererTests
{
    private static BoundType CreateBoundType(
        HttpBinderType binderType,
        bool hasEndpointParameterMetadataProvider = false)
    {
        var property = new BoundProperty(
            name: "Value",
            keyName: "Value",
            declaredTypeName: "int",
            httpBinderType: binderType,
            scalarType: new(
                IsNullable: false,
                IsGuid: false,
                IsEnum: false,
                IsPrimitive: true,
                IsString: false,
                IsFormFile: false,
                TypeName: "int"),
            isNullable: false,
            isCollection: false,
            isConstructorParameter: false,
            childProperties: []);

        return new(
            Name: "Request",
            Namespace: "Tests",
            FullName: "global::Tests.Request",
            Kind: BoundTypeKind.Class,
            HasBindableBase: false,
            HasEndpointParameterMetadataProvider: hasEndpointParameterMetadataProvider,
            Properties: new([property]),
            ConstructorParameterNames: []);
    }

    [Test]
    public async Task GivenFormBinding_WhenRendering_ThenContributesAntiforgeryMetadata()
    {
        var source = CodeRenderer.Render(CreateBoundType(HttpBinderType.Form));

        await Assert.That(source.Contains(
            "IEndpointParameterMetadataProvider",
            StringComparison.Ordinal)).IsTrue();
        await Assert.That(source.Contains(
            "IAntiforgeryValidationFeature",
            StringComparison.Ordinal)).IsTrue();
        await Assert.That(source.Contains(
            "public bool RequiresValidation => true;",
            StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task GivenExistingMetadataProvider_WhenRendering_ThenDoesNotGenerateDuplicateProvider()
    {
        var source = CodeRenderer.Render(CreateBoundType(
            HttpBinderType.Form,
            hasEndpointParameterMetadataProvider: true));

        await Assert.That(source.Contains(
            "IEndpointParameterMetadataProvider",
            StringComparison.Ordinal)).IsFalse();
        await Assert.That(source.Contains(
            "IAntiforgeryValidationFeature",
            StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task GivenQueryBinding_WhenRendering_ThenDoesNotContributeAntiforgeryMetadata()
    {
        var source = CodeRenderer.Render(CreateBoundType(HttpBinderType.Query));

        await Assert.That(source.Contains(
            "IEndpointParameterMetadataProvider",
            StringComparison.Ordinal)).IsFalse();
        await Assert.That(source.Contains(
            "IAntiforgeryValidationFeature",
            StringComparison.Ordinal)).IsFalse();
    }
}
