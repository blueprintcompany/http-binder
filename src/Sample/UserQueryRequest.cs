using Blueprint.HttpBinder;

namespace Sample;

[HttpBinder(HttpBinderType = HttpBinderType.Route)]
public partial class UserQueryRequest : PagedRequestBase
{
    public string? Search { get; set; }

    public string SearchTwo { get; set; } = null!;

    public int PageNumber { get; set; }

    public Guid Id { get; set; }

    //public IFormFile FormFile { get; set; } = null!;

    //[BindFrom(HttpBinderType.Form)]
    //public ComplexTypeTwo ComplexTypeTwo { get; set; } = new();
}

public class ComplexTypeTwo
{
    public Guid Guid { get; set; }
    public List<int> Ints { get; set; } = [];
    public List<ComplexTypeThree> ComplexTypeThrees { get; set; } = [];

    public class ComplexTypeThree
    {
        public Guid Guid { get; set; }
    }
}
