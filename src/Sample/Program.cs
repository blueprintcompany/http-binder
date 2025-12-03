using Blueprint.HttpBinder;
using Sample;
using Sample.Enums;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Example endpoint demonstrating usage of the generated binder. The
// UserQueryRequest parameter is bound by the generated BindAsync method
// according to the attributes applied to its properties and inherited
// properties from PagedRequestBase.
app.MapPost("/users", async (UserQueryRequest req, HttpContext httpContext) =>
{
    // Here you can access req.Page, req.PageSize and req.Search which have
    // been bound from the query string. You might query a database or
    // otherwise handle the request. For demonstration we simply return
    // the bound object as JSON.

    return new UserQueryResponse
    {
        FormFileCount = req.FormFile is not null ? 1 : 0,
        FormFilesCount = req.FormFiles.Count,
        FormFileListCount = req.FormFileList.Count,
        IntProperty = req.IntProperty,
        GuidProperty = req.GuidProperty,
        EnumProperty = req.EnumProperty,
        Search = req.Search,
        Page = req.Page,
        PageSize = req.PageSize,
        NestedClassNestedProperty = req.NestedClasses.NestedProperty,
        NestedClassOtherProperty = req.NestedClasses.OtherProperty
    };
});

app.Run();

[HttpBinder(HttpBinderType = HttpBinderType.Form)]
public partial class UserQueryRequest : PagedRequestBase
{
    public IFormFile? FormFile { get; set; } = null!;
    public IFormFileCollection FormFiles { get; set; } = null!;
    public List<IFormFile> FormFileList { get; set; } = [];
    public int IntProperty { get; set; }
    public bool BoolProperty { get; set; }
    [BindFrom(HttpBinderType.Query, Name = "some_guid")]
    public Guid GuidProperty { get; set; }
    public EnumExample EnumProperty { get; set; }
    //public DateTime? NullableDateTime { get; set; }
    //public DateTimeOffset? NullableDateTimeOffset { get; set; }
    //public DateTime DateTime { get; set; }
    //public DateTimeOffset? DateTimeOffset { get; set; }
    [BindFrom(HttpBinderType.Query)]
    public string? Search { get; set; }
    public NestedClass NestedClasses { get; set; } = new();

    public partial class NestedClass
    {
        public string? NestedProperty { get; set; }
        public int OtherProperty { get; set; }
    }
}



public class UserQueryResponse
{
    public int FormFileCount { get; set; }
    public int FormFilesCount { get; set; }
    public int FormFileListCount { get; set; }
    public int IntProperty { get; set; }
    public Guid GuidProperty { get; set; }
    public EnumExample EnumProperty { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? NestedClassNestedProperty { get; set; }
    public int NestedClassOtherProperty { get; set; }
}

public partial class Program { }