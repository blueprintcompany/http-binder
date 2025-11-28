using Microsoft.AspNetCore.Mvc;

namespace Sample
{
    /// <summary>
    /// Represents a request to query users. Inherits pageable properties
    /// from <see cref="PagedRequestBase"/> and adds an additional search term.
    /// The <c>GenerateHttpBinder</c> attribute instructs the source
    /// generator to emit a binder for this type.
    /// </summary>
    [HttpBinder]
    public partial class UserQueryRequest : PagedRequestBase
    {
        [BindFromQuery]
        public string? Search { get; set; }
    }
}