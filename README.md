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

## CSRF Protection

Form-bound generated types contribute antiforgery metadata, just like Minimal API
`[FromForm]` parameters. A cross-origin request that fails validation gets `400 Bad Request`
before any form value is read.

`WebApplication` adds cross-origin CSRF protection automatically. It uses the
`Sec-Fetch-Site` and `Origin` headers, so no tokens, cookies, or middleware setup are needed:

- Same-origin browser requests are allowed.
- Cross-site browser requests are rejected, unless the endpoint's CORS policy trusts the origin
  and allows credentials.
- Non-browser clients (no `Sec-Fetch-Site` or `Origin` header) are allowed.

Token-based antiforgery still works. To require a token on top of the origin check, add:

```csharp
builder.Services.AddAntiforgery();

var app = builder.Build();
app.UseAntiforgery();
```

Opt out per endpoint with `.DisableAntiforgery()`, or for the whole app with the
`DisableCsrfProtection` configuration setting. If the built-in protection is disabled and
`UseAntiforgery()` is not configured, form endpoints throw at request time because no
antiforgery middleware handles their metadata.

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

- .NET 11 apps
- `bool`, `char`, integer and floating-point numbers, `decimal`, `Guid`
- `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`, parsed with the invariant culture
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

The release workflow publishes to NuGet.org with [trusted publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).
It needs a nuget.org trusted publishing policy for `blueprintcompany/http-binder` and workflow
`publish-generator.yml`, plus a `NUGET_USER` repository secret set to the nuget.org username.

## Contributing

Please see the [Contributing](CONTRIBUTING.md) guidelines
