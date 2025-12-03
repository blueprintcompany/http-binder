namespace Blueprint.HttpBinder
{
    /// <summary>
    /// Apply this attribute to a property to override what method its property is bound from.
    /// <para>Base example:</para>
    /// <example>
    /// <code>
    /// [BindFrom(HttpBinderType.Query)]
    /// public int PageNumber { get; set }
    /// </code>
    /// </example>
    /// <para>Overriding name example:</para>
    /// <example>
    /// <code>
    /// [BindFrom(HttpBinderType.Query, Name = "Page")]
    /// public int PageNumber { get; set }
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="type">
    /// The HTTP binding source to use when retrieving the property's value
    /// (e.g., <see cref="HttpBinderType.Query"/>, <see cref="HttpBinderType.Form"/>,
    /// or <see cref="HttpBinderType.Route"/>).
    /// </param>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public sealed class BindFromAttribute(HttpBinderType type) : System.Attribute
    {
        /// <inheritdoc />
        public HttpBinderType Type { get; } = type;

        /// <summary>
        /// The name of the form field to bind to the associated property. If not set, the property name is used.
        /// </summary>
        public string Name { get; set; }
    }
}