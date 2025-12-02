using System;

namespace Blueprint.HttpBinder;

/// <summary>
/// Specifies the HTTP binder type to associate with a class for source-generated HTTP binding behavior using BindAsync().
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class HttpBinderAttribute : Attribute
{
    /// <summary>
    /// The HTTP binder type that determines how the attribute will bind HTTP data.
    /// </summary>
    public HttpBinderType HttpBinderType { get; set; } = HttpBinderType.Form;
}