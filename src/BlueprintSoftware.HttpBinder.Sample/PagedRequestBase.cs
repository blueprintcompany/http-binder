namespace BlueprintSoftware.HttpBinder.Sample;

/// <summary>
/// Base type representing a pageable request. Properties are bound
/// from the query string by the generated binder in the derived
/// request type. This demonstrates inherited properties participating
/// in the binding model.
/// </summary>
public abstract class PagedRequestBase
{
    [BindFrom(HttpBinderType.Query)]
    public int Page { get; set; }

    [BindFrom(HttpBinderType.Query)]
    public int PageSize { get; set; }

    public int NoSetterItem => 10;
}