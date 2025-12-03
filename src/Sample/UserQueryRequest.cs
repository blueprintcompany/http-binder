using Blueprint.HttpBinder;

namespace Sample;

[HttpBinder(HttpBinderType = HttpBinderType.Form)]
public partial class UserQueryRequest : PagedRequestBase
{
    public IFormFile FormFile { get; set; } = null!;

    public IFormFileCollection FormFiles { get; set; } = null!;

    public List<IFormFile> FormFileList { get; set; } = [];

    public int Derp { get; set; }


    public Guid Guid { get; set; }

    public EnumExample EnumExample { get; set; }
}

public enum EnumExample
{
    One,
    Two,
    Three
}
