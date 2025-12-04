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

    public string TypeName => ScalarType.TypeName;
    public bool IsEnum => ScalarType.IsEnum;
    public bool IsGuid => ScalarType.IsGuid;
    public bool IsString => ScalarType.IsString;
    public bool IsPrimitive => ScalarType.IsPrimitive;
    public bool IsFormFile => ScalarType.IsFormFile;
    public bool IsValueType => ScalarType.IsValueType;
    public bool IsReferenceType => ScalarType.IsReferenceType;

    public string GetTryParseMethod() =>
        TypeName switch
        {
            "bool" or "bool?" => "bool.TryParse",
            "byte" or "byte?" => "byte.TryParse",
            "sbyte" or "sbyte?" => "sbyte.TryParse",
            "short" or "short?" => "short.TryParse",
            "ushort" or "ushort?" => "ushort.TryParse",
            "int" or "int?" => "int.TryParse",
            "uint" or "uint?" => "uint.TryParse",
            "long" or "long?" => "long.TryParse",
            "ulong" or "ulong?" => "ulong.TryParse",
            "float" or "float?" => "float.TryParse",
            "double" or "double?" => "double.TryParse",
            "decimal" or "decimal?" => "decimal.TryParse",
            "System.Guid" or "System.Guid?" => "System.Guid.TryParse",
            "System.DateTime" or "System.DateTime?" => "System.DateTime.TryParse",
            "System.DateTimeOffset" or "System.DateTimeOffset?" => "System.DateTimeOffset.TryParse",
            "System.DateOnly" or "System.DateOnly?" => "System.DateOnly.TryParse",
            "System.TimeOnly" or "System.TimeOnly?" => "System.TimeOnly.TryParse",
            "System.TimeSpan" or "System.TimeSpan?" => "System.TimeSpan.TryParse",
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
    HttpBinderType ClassHttpBinderType,
    ImmutableArray<BoundProperty> Properties,
    ImmutableArray<string> ConstructorParameterNames);