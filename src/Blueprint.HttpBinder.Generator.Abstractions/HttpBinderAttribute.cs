namespace Blueprint.HttpBinder
{
    /// <summary>
    /// Specifies the HTTP binder type to associate with a class for source-generated HTTP binding behavior using BindAsync().
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class HttpBinderAttribute : System.Attribute
    {
        /// <summary>
        /// The HTTP binder type that determines how the attribute will bind HTTP data.
        /// </summary>
        public HttpBinderType HttpBinderType { get; set; } = HttpBinderType.Form;
    }
}