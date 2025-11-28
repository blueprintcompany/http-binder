using HttpBinder.Generator;

namespace Sample
{
    [HttpBinder]
    public partial class UserQueryRequest : PagedRequestBase
    {
        public string? Search { get; set; }
    }
}