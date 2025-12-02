# HttpBinder

HttpBinder is an incremental C# source generator that provides strongly-typed request binding for Minimal API handlers. You annotate a request DTO, map an endpoint that takes that DTO as a parameter, and HttpBinder generates the binder logic that ASP.NET Core automatically invokes.

There are no runtime conventions, no reflection, and no manual calls to a BindAsync method. If your endpoint delegate takes a bindable DTO, HttpBinder handles the population.

## Why this exists

Minimal APIs handle simple primitives well, but anything beyond a few parameters often becomes repetitive parsing code or forces developers into MVC-style model binding. HttpBinder fills the gap by providing:

- Explicit attribute-driven binding  
- Deterministic, readable generated code  
- Compile-time diagnostics for invalid patterns  
- Zero runtime reflection  

You get predictable binding behavior without relying on conventions.

## How it works

1. Create a partial class representing your request.
2. Annotate it with `[HttpBinder(...)]`.
3. Optionally annotate properties with `[BindFrom]` or `[BindFromIgnore]`.
4. Use the class as a parameter in your Minimal API handler.

ASP.NET Core automatically uses the generated binder:

```csharp
app.MapPost("/search", (SearchQuery query) =>
{
    // query is fully populated by HttpBinder
});
```

The generator creates all binding logic during compilation.

## Example

```csharp
[HttpBinder(HttpBinderType.Query)]
public partial class SearchQuery
{
    public string? Term { get; set; }
    public int Page { get; set; }

    [BindFrom(HttpBinderType.Form, Name = "override_name")]
    public string? Extra { get; set; }
}
```

Endpoint:

```csharp
app.MapPost("/search", (SearchQuery query) =>
{
    // HttpBinder handled everything
});
```

## Attributes

### `[HttpBinder]`

Marks a class as bindable and defines the default binding source:

```csharp
[HttpBinder(HttpBinderType.Form)]
public partial class UploadRequest { }
```

### `[BindFrom]`

Overrides how an individual property is bound:

```csharp
[BindFrom(HttpBinderType.Query, Name = "page")]
public int PageNumber { get; set; }
```

### `[BindFromIgnore]`

Excludes a property from binding:

```csharp
[BindFromIgnore]
public string InternalOnly { get; set; }
```

## Supported types

HttpBinder supports:

- Primitive types (int, bool, double, long, etc.)
- Nullable primitives  
- string  
- Enums  
- Complex types (Form-only)  
- Arrays and `List<T>`  
- `IFormFile` and `IFormFileCollection` (Form-only)

## Unsupported patterns

These produce compile-time diagnostics:

- Dictionary types  
- Nested collections (`List<List<T>>`, `T[][]`)  
- Complex objects bound from Query or Route  
- Complex objects missing a parameterless constructor  
- `IFormFile` bound from Query or Route  
- Using `[BindFromIgnore]` with any other `[Bind*]` attributes  

Diagnostics always include the property name, containing type, and the rule broken.

## Diagnostics

The generator ships analyzers that validate DTOs before emitting code. Diagnostic cases include:

- Complex types detected on Query/Route binders  
- Nested collection detection  
- Dictionary detection  
- Conflicting attributes  
- Invalid binding types for file uploads  

Diagnostics are surfaced during build with clear IDs and messages.

## Performance

HttpBinder is fully incremental:

- Syntax and symbol analysis is cached  
- Only affected files are reprocessed  
- Generated binders are plain C#  
- No reflection is used at runtime  

The generated binders are typically faster than the default model binder for complex DTOs.

## Package usage

Add the source generator to the project that defines your DTOs:

```xml
<ItemGroup>
    <PackageReference Include="HttpBinder.Generator" Version="1.0.0" />
</ItemGroup>
```

No separate runtime package is needed.

## When to use HttpBinder

- You want explicit HTTP binding rules  
- You want compile-time validation  
- You prefer predictability over conventions  
- You want strong performance without reflection  

## When not to use it

- You need automatic deeply nested binding  
- You rely heavily on MVC-style conventions  
- You want binder behavior without annotating anything  
