namespace HttpBinder.Generator
{
    /// <summary>
    /// Specifies that a property should be bound from a route parameter during model binding.
    /// </summary>
    /// <remarks>
    /// Apply this attribute to a property to indicate that its value should be populated from the
    /// corresponding route value in the request URL.
    /// </remarks>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public sealed class BindFromRouteAttribute : System.Attribute
    {
        /// <summary>
        /// The name of the route parameter to bind to the associated property. If null, the property name is used.
        /// </summary>
        public string Name { get; }
    }
}
