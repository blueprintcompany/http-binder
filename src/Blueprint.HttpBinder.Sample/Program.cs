using Blueprint.HttpBinder;
using Blueprint.HttpBinder.Sample;
using Blueprint.HttpBinder.Sample.Enums;
using static UserQueryRequest;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Example endpoint demonstrating usage of the generated binder. The
// UserQueryRequest parameter is bound by the generated BindAsync method
// according to the attributes applied to its properties and inherited
// properties from PagedRequestBase.
app.MapPost("/users/{routeParam:int}", async (UserQueryRequest req, HttpContext httpContext) =>
{
    // Here you can access req.Page, req.PageSize, etc. which have
    // been bound from the query string and form. You might query a database or
    // otherwise handle the request. For demonstration we simply return
    // the bound object.

    return new UserQueryResponse
    {
        RouteParameter = req.RouteParameter,
        InitOnlyProperty = req.InitOnlyProperty,
        IntProperty = req.IntProperty,
        BoolProperty = req.BoolProperty,
        NullableBoolProperty = req.NullableBoolProperty,

        GuidProperty = req.GuidProperty,
        EnumProperty = req.EnumProperty,
        NullableEnumProperty = req.NullableEnumProperty,

        NullableDateTime = req.NullableDateTime,
        NullableDateTimeOffset = req.NullableDateTimeOffset,
        DateTime = req.DateTime,
        DateTimeOffset = req.DateTimeOffset,
        StringCollection = req.StringCollection,

        IntCollection = req.IntCollection,
        Search = req.Search,
        Page = req.Page,
        PageSize = req.PageSize,

        FormFileCount = req.FormFile is null ? 0 : 1,
        FormFilesCount = req.FormFiles?.Count ?? 0,
        FormFileListCount = req.FormFileList?.Count ?? 0,

        NestedClass = new()
        {
            NestedProperty = req.NestedClasses?.NestedProperty,
            OtherProperty = req.NestedClasses?.OtherProperty ?? 0,
        },
        NestedClassList = req.NestedClassList.Select(nc => new NestedClass
        {
            NestedProperty = nc.NestedProperty,
            OtherProperty = nc.OtherProperty,
        }).ToList() ?? []
    };
});

app.Run();

[HttpBinder(HttpBinderType = HttpBinderType.Form)]
public partial class UserQueryRequest : PagedRequestBase
{
    [BindFrom(HttpBinderType.Route, Name = "routeParam")]
    public int RouteParameter { get; set; }
    public int InitOnlyProperty { get; init; }
    public IFormFile? FormFile { get; set; } = null!;
    public IFormFileCollection FormFiles { get; set; } = null!;
    public List<IFormFile> FormFileList { get; set; } = [];
    public int IntProperty { get; set; }
    public bool BoolProperty { get; set; }
    public bool? NullableBoolProperty { get; set; }
    [BindFrom(HttpBinderType.Query, Name = "some_guid")]
    public Guid GuidProperty { get; set; }
    public EnumExample EnumProperty { get; set; }
    public EnumExample? NullableEnumProperty { get; set; }
    public DateTime? NullableDateTime { get; set; }
    public DateTimeOffset? NullableDateTimeOffset { get; set; }
    public DateTime DateTime { get; set; }
    public DateTimeOffset? DateTimeOffset { get; set; }
    [BindFrom(HttpBinderType.Query)]
    public string? Search { get; set; }
    [BindFrom(HttpBinderType.Query)]
    public int[] IntCollection { get; set; } = [];
    public List<string> StringCollection { get; set; } = [];
    public NestedClass NestedClasses { get; set; } = new();
    public List<NestedClass> NestedClassList { get; set; } = [];

    public class NestedClass
    {
        public string? NestedProperty { get; set; }
        public int OtherProperty { get; set; }

    }
}

public class UserQueryResponse
{
    public int RouteParameter { get; set; }
    public int InitOnlyProperty { get; set; }
    public int FormFileCount { get; set; }
    public int FormFilesCount { get; set; }
    public int FormFileListCount { get; set; }

    public int IntProperty { get; set; }
    public bool BoolProperty { get; set; }
    public bool? NullableBoolProperty { get; set; }

    public Guid GuidProperty { get; set; }
    public EnumExample EnumProperty { get; set; }
    public EnumExample? NullableEnumProperty { get; set; }

    public DateTime? NullableDateTime { get; set; }
    public DateTimeOffset? NullableDateTimeOffset { get; set; }
    public DateTime DateTime { get; set; }
    public DateTimeOffset? DateTimeOffset { get; set; }

    public string? Search { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int[] IntCollection { get; set; } = [];
    public List<string> StringCollection { get; set; } = [];

    public NestedClass NestedClass { get; set; } = new();
    public List<NestedClass> NestedClassList { get; set; } = [];

}


public partial class Program { }