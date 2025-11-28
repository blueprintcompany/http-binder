namespace HttpBinder.Generator
{
    /// <summary>
    /// Specifies that a property should be bound from form data in an HTTP request.
    /// </summary>
    /// <remarks>Apply this attribute to a property to indicate that its value should be populated from form
    /// fields when model binding occurs.</remarks>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public sealed class BindFromFormAttribute : System.Attribute
    {
        /// <summary>
        /// The name of the form field to bind to the associated property. If null, the property name is used.
        /// </summary>
        public string Name { get; set; }
    }
}
