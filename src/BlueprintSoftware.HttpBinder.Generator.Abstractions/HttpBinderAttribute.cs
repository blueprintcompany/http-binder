namespace BlueprintSoftware.HttpBinder
{
    /// <summary>
    /// Marks a partial class or record class for source-generated Minimal API binding.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class HttpBinderAttribute : System.Attribute
    {
        /// <summary>
        /// Gets or sets the default HTTP source used for properties without a <see cref="BindFromAttribute"/>.
        /// </summary>
        public HttpBinderType HttpBinderType { get; set; } = HttpBinderType.Form;
    }
}
