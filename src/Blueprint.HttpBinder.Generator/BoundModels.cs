using System.Linq;

namespace Blueprint.HttpBinder;

//// <summary>
/// Represents a single property that will be bound at runtime. This model must remain equatable for Roslyn to perform incremental caching.
/// </summary>
internal sealed record BoundProperty
{
    public BoundProperty(
        string name,
        string keyName,
        string declaredTypeName,
        HttpBinderType httpBinderType,
        ScalarTypeInfo scalarType,
        bool isNullable,
        bool isCollection,
        bool isConstructorParameter,
        EquatableArray<BoundProperty> childProperties)
    {
        Name = name;
        SafeCamelCaseName = "@" + ToCamelCase(Name);
        KeyName = keyName;
        DeclaredTypeName = declaredTypeName;
        HttpBinderType = httpBinderType;
        ScalarType = scalarType;
        IsNullable = isNullable;
        IsCollection = isCollection;
        IsConstructorParameter = isConstructorParameter;
        ChildProperties = childProperties;
    }

    public string Name { get; init; }
    public string KeyName { get; init; }
    public string DeclaredTypeName { get; init; }
    public HttpBinderType HttpBinderType { get; init; }
    public ScalarTypeInfo ScalarType { get; init; }
    public bool IsNullable { get; init; }
    public bool IsCollection { get; init; }
    public bool IsConstructorParameter { get; init; }
    public EquatableArray<BoundProperty> ChildProperties { get; init; }

    public string SafeCamelCaseName { get; init; }
    public string NonNullTypeName => IsNullable ? TypeName.TrimEnd('?') : TypeName;
    public string TypeName => ScalarType.TypeName;
    public bool IsEnum => ScalarType.IsEnum;
    public bool IsGuid => ScalarType.IsGuid;
    public bool IsString => ScalarType.IsString;
    public bool IsPrimitive => ScalarType.IsPrimitive;
    public bool IsFormFile => ScalarType.IsFormFile;
    public bool IsValueType => ScalarType.IsValueType;
    public bool IsReferenceType => ScalarType.IsReferenceType;

    public bool UsesFormValues()
    {
        if (HttpBinderType == HttpBinderType.Form)
            return true;

        foreach (var childProperty in ChildProperties)
            if (childProperty.UsesFormValues())
                return true;

        return false;
    }

    public static string ToCamelCase(string str) => char.ToLowerInvariant(str[0]) + str.Substring(1);

    public string GetTryParseMethod() =>
        NonNullTypeName switch
        {
            "bool" => "bool.TryParse",
            "byte" => "byte.TryParse",
            "sbyte" => "sbyte.TryParse",
            "short" => "short.TryParse",
            "ushort" => "ushort.TryParse",
            "int" => "int.TryParse",
            "uint" => "uint.TryParse",
            "long" => "long.TryParse",
            "ulong" => "ulong.TryParse",
            "float" => "float.TryParse",
            "double" => "double.TryParse",
            "decimal" => "decimal.TryParse",
            "System.Guid" => "System.Guid.TryParse",
            "System.DateTime" => "System.DateTime.TryParse",
            "System.DateTimeOffset" => "System.DateTimeOffset.TryParse",
            "System.DateOnly" => "System.DateOnly.TryParse",
            "System.TimeOnly" => "System.TimeOnly.TryParse",
            "System.TimeSpan" => "System.TimeSpan.TryParse",
            _ => "int.TryParse"
        };
}

/// <summary>
/// Represents a DTO type for which a binder will be generated. This model must remain equatable for Roslyn to perform incremental caching.
/// </summary>
internal sealed record BoundType(
    string Name,
    string Namespace,
    string FullName,
    BoundTypeKind Kind,
    HttpBinderType ClassHttpBinderType,
    EquatableArray<BoundProperty> Properties,
    EquatableArray<string> ConstructorParameterNames)
{
    public bool UsesFormValues() => Properties.Any(p => p.UsesFormValues());
    public bool UsesQueryValues() => Properties.Any(p => p.HttpBinderType == HttpBinderType.Query);
    public bool UsesRouteValues() => Properties.Any(p => p.HttpBinderType == HttpBinderType.Route);
}

internal enum BoundTypeKind
{
    Class,
    RecordClass
}