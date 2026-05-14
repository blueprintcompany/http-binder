# BlueprintSoftware.HttpBinder

Source-generated request binding for ASP.NET Core Minimal APIs.

```xml
<PackageReference Include="BlueprintSoftware.HttpBinder" Version="1.0.0" />
```

## Query Example

Bind `/search?term=blueprint&page=2`.

```csharp
using BlueprintSoftware.HttpBinder;

app.MapGet("/search", (SearchQuery query) => Results.Ok(query));

[HttpBinder(HttpBinderType = HttpBinderType.Query)]
public partial class SearchQuery
{
    public string? Term { get; set; }
    public int Page { get; set; }
}
```

## Route + Query Example

Bind `/users/42?includeInactive=true`.

```csharp
using BlueprintSoftware.HttpBinder;

app.MapGet("/users/{id:int}", (UserRequest request) => Results.Ok(request));

[HttpBinder(HttpBinderType = HttpBinderType.Query)]
public partial class UserRequest
{
    [BindFrom(HttpBinderType.Route)]
    public int Id { get; set; }

    public bool IncludeInactive { get; set; }
}
```

## Form Example

Bind posted form fields and files.

```csharp
using BlueprintSoftware.HttpBinder;

app.MapPost("/uploads", (UploadRequest request) => Results.Ok(request.File.FileName));

[HttpBinder] // Defaults to HttpBinderType.Form
public partial class UploadRequest
{
    public string? Title { get; set; }
    public IFormFile File { get; set; } = null!;
}
```

## Property Names

Use `Name` when HTTP names do not match C# names.

```csharp
[HttpBinder(HttpBinderType = HttpBinderType.Query)]
public partial class PagedSearch
{
    [BindFrom(HttpBinderType.Query, Name = "q")]
    public string? SearchTerm { get; set; }

    [BindFrom(HttpBinderType.Query, Name = "page_size")]
    public int PageSize { get; set; }
}
```

## Ignore Properties

```csharp
[HttpBinder(HttpBinderType = HttpBinderType.Query)]
public partial class ReportRequest
{
    public int Year { get; set; }

    [BindFromIgnore]
    public string? InternalTraceId { get; set; }
}
```

## Supported

- .NET 10 apps
- `int`, `bool`, `double`, `long`, `Guid`, `DateTime`, `DateTimeOffset`
- nullable primitives
- `string`
- enums
- arrays and `List<T>`
- form-bound complex objects
- form-bound `IFormFile`, `IFormFileCollection`, and `List<IFormFile>`

## Diagnostics

These produce build diagnostics:

- dictionary properties
- nested collections like `List<List<T>>`
- complex objects from query or route
- complex objects without parameterless constructors
- files from query or route
- `[BindFromIgnore]` combined with another binding attribute

## Release

Create a GitHub release with tag format:

```text
blueprintsoftware-httpbinder-v1.0.0
```

The release workflow publishes to NuGet.org using `NUGET_API_KEY`.

## Contributing

Please see the [Contributing](CONTRIBUTING.md) guidlines
