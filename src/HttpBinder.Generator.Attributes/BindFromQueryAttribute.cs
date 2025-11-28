namespace HttpBinder.Generator
{
    /// <summary>
    /// Specifies that a property should be bound from a query string parameter during model binding.
    /// </summary>
    /// <remarks>Apply this attribute to a property to indicate that its value should be populated from the
    /// query string of an HTTP request.</remarks>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public sealed class BindFromQueryAttribute : System.Attribute
    {
        /// <summary>
        /// Initializes a new instance of the BindFromQueryAttribute class with an optional query parameter name to bind from.
        /// </summary>
        /// <remarks>Use the constructor to specify a custom name when binding data from an
        /// HTTP query parameter. If no name is provided, the default binding behavior uses the property name.</remarks>
        /// <param name="name">The name of the query parameter to bind to the associated property. If null, the property name is used.</param>
        public BindFromQueryAttribute(string name = null) { Name = name; }
        /// <summary>
        /// Gets the name of the form field.
        /// </summary>
        public string Name { get; }
    }
}
