using Microsoft.CodeAnalysis;

namespace HttpBinder.Generator;

internal sealed class AttributeRegistry(Compilation compilation)
{
    public INamedTypeSymbol? HttpBinderAttribute => compilation.GetTypeByMetadataName("HttpBinder.Generator.HttpBinderAttribute");
    public INamedTypeSymbol? BindFromAttribute => compilation.GetTypeByMetadataName("HttpBinder.Generator.BindFromAttribute");
}
