using Microsoft.AspNetCore.Mvc;

namespace Sample
{
    /// <summary>
    /// Base type representing a pageable request. Properties are bound
    /// from the query string by the generated binder in the derived
    /// request type. This demonstrates inherited properties participating
    /// in the binding model.
    /// </summary>
    public abstract class PagedRequestBase
    {
        [FromQuery]
        public int Page { get; set; }

        [FromQuery]
        public int PageSize { get; set; }
    }
}