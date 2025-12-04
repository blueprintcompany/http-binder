namespace Blueprint.HttpBinder;

public readonly record struct ScalarTypeInfo(
    bool IsNullable,
    bool IsGuid,
    bool IsEnum,
    bool IsPrimitive,
    bool IsString,
    bool IsFormFile,
    string TypeName)
{
    public bool IsValueType => IsPrimitive || IsString || IsEnum || IsGuid && !IsFormFile;
    public bool IsReferenceType => !IsValueType;

    public static ScalarTypeInfo Unknown { get; } =
        new(
            IsNullable: false,
            IsGuid: false,
            IsEnum: false,
            IsPrimitive: false,
            IsString: false,
            IsFormFile: false,
            TypeName: "unknown");
}
