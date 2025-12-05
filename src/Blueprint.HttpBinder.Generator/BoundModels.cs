using System.Linq;

namespace Blueprint.HttpBinder;

//// <summary>
/// Represents a single property that will be bound at runtime. This model must remain equatable for Roslyn to perform incremental caching.
/// </summary>
internal sealed record BoundProperty(
    string Name,
    string KeyName,
    string DeclaredTypeName,
    HttpBinderType HttpBinderType,
    ScalarTypeInfo ScalarType,
    bool IsNullable,
    bool IsCollection,
    bool IsConstructorParameter,
    bool IsIgnored,
    EquatableArray<BoundProperty> ChildProperties)
{
    public static BoundProperty Ignore(string name, HttpBinderType httpBinderType) =>
        new(
            Name: name,
            KeyName: name,
            DeclaredTypeName: name,
            HttpBinderType: httpBinderType,
            ScalarType: ScalarTypeInfo.Unknown,
            IsNullable: false,
            IsCollection: false,
            IsConstructorParameter: false,
            IsIgnored: true,
            ChildProperties: []);

    public string CamelCaseName => ToCamelCase(Name);
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
        if (IsIgnored)
            return false;

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
    public bool UsesFormValues() => Properties.Any(p => !p.IsIgnored && p.UsesFormValues());
    public bool UsesQueryValues() => Properties.Any(p => !p.IsIgnored && p.HttpBinderType == HttpBinderType.Query);
    public bool UsesRouteValues() => Properties.Any(p => !p.IsIgnored && p.HttpBinderType == HttpBinderType.Route);
}

internal enum BoundTypeKind
{
    Class,
    RecordClass
}