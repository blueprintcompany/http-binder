using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blueprint.HttpBinder.Extensions;

internal static class AttributeDataExtensions
{
    extension(AttributeData data)
    {
        internal bool TryGetBinderType(out HttpBinderType result)
        {
            var arg = data.ConstructorArguments.FirstOrDefault();
            if (arg.Value is int raw)
            {
                result = (HttpBinderType)raw;
                return true;
            }

            result = default;
            return false;
        }
    }
}
