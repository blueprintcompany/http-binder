using Sample;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Example endpoint demonstrating usage of the generated binder. The
// UserQueryRequest parameter is bound by the generated BindAsync method
// according to the attributes applied to its properties and inherited
// properties from PagedRequestBase.
app.MapPost("/users", async (UserQueryRequest req) =>
{
    // Here you can access req.Page, req.PageSize and req.Search which have
    // been bound from the query string. You might query a database or
    // otherwise handle the request. For demonstration we simply return
    // the bound object as JSON.
    return Results.Json(req);
});

app.Run();