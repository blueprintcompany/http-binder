namespace BlueprintSoftware.HttpBinder
{
    /// <summary>
    /// Applies an explicit binding source to a property.
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
        /// <summary>
        /// Gets the HTTP source used to bind this property.
        /// </summary>
        public HttpBinderType Type { get; } = type;

        /// <summary>
        /// Gets or sets the HTTP field, query parameter, or route value name. If not set, the property name is used.
        /// </summary>
        public string? Name { get; set; }
    }
}
