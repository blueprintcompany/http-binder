namespace HttpBinder.Generator
{
    /// <summary>
    /// Specifies that a property should be bound from form data in an HTTP request.
    /// </summary>
    /// <remarks>Apply this attribute to a property to indicate that its value should be populated from form
    /// fields when model binding occurs./remarks>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public sealed class BindFromFormAttribute : System.Attribute
    {
        /// <summary>
        /// Initializes a new instance of the BindFromFormAttribute class with an optional field name to bind from.
        /// </summary>
        /// <remarks>Use the constructor to specify a custom form field name when binding data from an
        /// HTTP form. If no name is provided, the default binding behavior uses the property name.</remarks>
        /// <param name="name">The name of the form field to bind to the associated property. If null, the property name is used.</param>
        public BindFromFormAttribute(string name = null) { Name = name; }
        /// <summary>
        /// Gets the name of the form field.
        /// </summary>
        public string Name { get; }
    }
}
