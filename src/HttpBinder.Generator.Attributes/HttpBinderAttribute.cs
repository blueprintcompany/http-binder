namespace HttpBinder.Generator
{
    /// <summary>
    /// Specifies the HTTP binder type to associate with a class for source-generated HTTP binding behavior using BindAsync().
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class HttpBinderAttribute : System.Attribute
    {
        /// <summary>
        /// Initializes a new instance of the HttpBinderAttribute class with the specified HTTP binder type.
        /// </summary>
        /// <param name="httpBinderType">The HTTP binder type that determines how the attribute will bind HTTP data.</param>
        public HttpBinderAttribute(HttpBinderType httpBinderType) { HttpBinderType = httpBinderType; }
        /// <summary>
        /// Gets the type of HTTP binder used for deserializing HTTP request and response bodies.
        /// </summary>
        public HttpBinderType HttpBinderType { get; }
    }
}
