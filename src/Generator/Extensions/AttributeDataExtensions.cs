using Microsoft.CodeAnalysis;

namespace Blueprint.HttpBinder.Extensions;

internal static class AttributeDataExtensions
{
    extension(AttributeData data)
    {
        internal bool TryGetBinderType(out HttpBinderType result)
        {
            if (data.ConstructorArguments.Length == 1 &&
                data.ConstructorArguments[0].Value is int rawCtor)
            {
                result = (HttpBinderType)rawCtor;
                return true;
            }

            foreach (var namedArgument in data.NamedArguments)
            {
                if (namedArgument.Key == nameof(HttpBinderAttribute.HttpBinderType))
                {
                    var typedConstant = namedArgument.Value;

                    if (typedConstant.Value is int raw)
                    {
                        result = (HttpBinderType)raw;

                        return true;
                    }
                }
            }

            result = default;
            return false;
        }
    }
}
