using System.Collections.Immutable;

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
    bool IsIgnored,
    ImmutableArray<BoundProperty> ChildProperties)
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
            IsIgnored: true,
            ChildProperties: []);

    public string NonNullTypeName => IsNullable ? TypeName.TrimEnd('?') : TypeName;
    public string TypeName => ScalarType.TypeName;
    public bool IsEnum => ScalarType.IsEnum;
    public bool IsGuid => ScalarType.IsGuid;
    public bool IsString => ScalarType.IsString;
    public bool IsPrimitive => ScalarType.IsPrimitive;
    public bool IsFormFile => ScalarType.IsFormFile;
    public bool IsValueType => ScalarType.IsValueType;
    public bool IsReferenceType => ScalarType.IsReferenceType;

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
    ImmutableArray<BoundProperty> Properties,
    ImmutableArray<string> ConstructorParameterNames)
{
    public bool HasPrimaryConstructor => ConstructorParameterNames.Length > 0;
}

internal enum BoundTypeKind
{
    Class,
    RecordClass
}