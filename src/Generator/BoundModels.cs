using System.Collections.Immutable;

namespace Blueprint.HttpBinder;

//// <summary>
/// Represents a single property that will be bound at runtime. This model must remain equatable for Roslyn to perform incremental caching.
/// </summary>
internal sealed record BoundProperty(
    string Name,
    string KeyName,
    string TypeName,
    HttpBinderType HttpBinderType,
    bool IsNullable,
    bool IsCollection,
    bool IsEnum,
    bool IsGuid,
    bool IsPrimitive,
    bool IsString,
    bool IsComplex,
    bool IsFormFile,
    bool IsIgnored,
    ImmutableArray<BoundProperty> Children)
{
    public static BoundProperty Ignore(string name, HttpBinderType httpBinderType) =>
        new(
            Name: name,
            KeyName: name,
            HttpBinderType: httpBinderType,
            IsNullable: true,
            IsCollection: false,
            TypeName: string.Empty,
            IsEnum: false,
            IsGuid: false,
            IsPrimitive: false,
            IsString: false,
            IsComplex: false,
            IsFormFile: false,
            IsIgnored: true,
            Children: []);
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