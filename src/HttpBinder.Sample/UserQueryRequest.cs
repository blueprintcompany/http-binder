using HttpBinder.Generator;

namespace Sample
{
    [HttpBinder(HttpBinderType = HttpBinderType.)]
    public partial class UserQueryRequest : PagedRequestBase
    {
        public string? Search { get; set; }
    }
}