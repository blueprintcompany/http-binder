using System;

namespace HttpBinder.Generator
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
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class BindFromAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindFromAttribute"/> class,
        /// specifying the HTTP source from which the property should be bound.
        /// </summary>
        /// <param name="type">
        /// The HTTP binding source to use when retrieving the property's value
        /// (e.g., <see cref="HttpBinderType.Query"/>, <see cref="HttpBinderType.Form"/>,
        /// or <see cref="HttpBinderType.Route"/>).
        /// </param>
        public BindFromAttribute(HttpBinderType type) => Type = type;

        /// <inheritdoc />
        public HttpBinderType Type { get; }

        /// <summary>
        /// The name of the form field to bind to the associated property. If null, the property name is used.
        /// </summary>
        public string Name { get; set; }
    }
}
