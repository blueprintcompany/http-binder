namespace BlueprintSoftware.HttpBinder
{
    /// <summary>
    /// Apply this attribute to a property to ignore any binding by the generator.
    /// <para>Base example:</para>
    /// <example>
    /// <code>
    /// [BindFromIgnore]
    /// public int PageNumber { get; set }
    /// </code>
    /// </example>
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public sealed class BindFromIgnoreAttribute : System.Attribute;
}