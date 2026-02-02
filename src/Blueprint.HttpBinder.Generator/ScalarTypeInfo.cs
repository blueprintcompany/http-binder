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
}
