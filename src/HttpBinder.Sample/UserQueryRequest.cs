using HttpBinder.Generator;

namespace Sample
{
    [HttpBinder(HttpBinderType = HttpBinderType.Form)]
    public partial class UserQueryRequest : PagedRequestBase
    {
        public string? Search { get; set; }

        public ComplexTypeToo ComplexTypeToo { get; set; } = new();
    }

    public class ComplexTypeToo
    {
        public Guid Guid { get; set; }
        public List<int> Ints { get; set; } = [];
        public List<ComplexTypeThree> ComplexTypeThrees { get; set; } = [];

        public class ComplexTypeThree
        {
            public Guid Guid { get; set; }
        }
    }
}
