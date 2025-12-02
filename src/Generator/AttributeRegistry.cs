using Microsoft.CodeAnalysis;

namespace Blueprint.HttpBinder;

internal sealed class AttributeRegistry(Compilation compilation)
{
    public INamedTypeSymbol? HttpBinderAttribute => compilation.GetTypeByMetadataName("Blueprint.HttpBinder.HttpBinderAttribute");
    public INamedTypeSymbol? BindFromAttribute => compilation.GetTypeByMetadataName("Blueprint.HttpBinder.BindFromAttribute");
}
