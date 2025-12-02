namespace Blueprint.HttpBinder;

/// <summary>
/// Specifies the source from which HTTP request data is bound to a parameter or model.
/// </summary>
/// <remarks>Use this  to indicate whether properties should be bound from form fields, query string
/// parameters, or route values when processing HTTP requests. The selected binder type determines how incoming data
/// is mapped.</remarks>
public enum HttpBinderType
{
    /// <summary>
    /// Bind from the HTTP request's form data.
    /// </summary>
    Form,
    /// <summary>
    /// Bind from the HTTP request's query parameters.
    /// </summary>
    Query,
    /// <summary>
    /// Bind from the HTTP request's route values.
    /// </summary>
    Route
}