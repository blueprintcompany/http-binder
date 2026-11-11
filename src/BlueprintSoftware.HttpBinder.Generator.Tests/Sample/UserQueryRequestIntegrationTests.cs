using BlueprintSoftware.HttpBinder.Sample.Enums;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http.Json;

namespace BlueprintSoftware.HttpBinder.Generator.Tests.Sample;

internal class UserQueryRequestIntegrationTests
{
    private sealed class TestAppFactory : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            return base.CreateHost(builder);
        }
    }

    private readonly TestAppFactory _factory = new();

    private async Task<HttpResponseMessage> PostFormAsync(params (string Name, string Value)[] headers)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/users/1")
        {
            Content = new MultipartFormDataContent
            {
                { new StringContent("42"), "IntProperty" }
            }
        };

        foreach (var (name, value) in headers)
            request.Headers.Add(name, value);

        return await _factory.CreateClient().SendAsync(request);
    }

    [Test]
    public async Task WhenEndpointUsesFormBinder_ThenRequiresAntiforgeryValidation()
    {
        var endpointDataSource = _factory.Services.GetRequiredService<EndpointDataSource>();
        var endpoint = endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Single(e => e.RoutePattern.RawText == "/users/{routeParam:int}");

        var antiforgeryMetadata = endpoint.Metadata.GetMetadata<IAntiforgeryMetadata>();

        await Assert.That(antiforgeryMetadata).IsNotNull();
        await Assert.That(antiforgeryMetadata!.RequiresValidation).IsTrue();
    }

    [Test]
    public async Task WhenCrossSiteBrowserPostsForm_ThenReturnsBadRequest()
    {
        var response = await PostFormAsync(("Sec-Fetch-Site", "cross-site"));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task WhenUntrustedOriginPostsFormWithoutFetchMetadata_ThenReturnsBadRequest()
    {
        var response = await PostFormAsync(("Origin", "https://attacker.example"));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task WhenSameOriginBrowserPostsForm_ThenBindsRequest()
    {
        var response = await PostFormAsync(("Sec-Fetch-Site", "same-origin"));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserQueryResponse>();
        await Assert.That(result!.IntProperty).IsEqualTo(42);
    }

    [Test]
    public async Task WhenPostingJsonToFormBinder_ThenReturnsUnsupportedMediaType()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/users/1",
            new { intProperty = 42 });

        await Assert.That(response.StatusCode)
            .IsEqualTo(HttpStatusCode.UnsupportedMediaType);
    }

    [Test]
    public async Task WhenPostingMultipartAndQuery_ThenBinderDeserializesCorrectly()
    {
        // Arrange query params
        var url = "/users/1?some_guid=11111111-1111-1111-1111-111111111111"
                + "&Page=2&PageSize=50&Search=abc"
                + "&IntCollection=10&IntCollection=20&IntCollection=30";

        // multipart/form-data body
        var form = new MultipartFormDataContent
        {
            // Single IFormFile
            {
                new StreamContent(new MemoryStream("file-one"u8.ToArray())),
                "FormFile",
                "file1.txt"
            },

            // IFormFileCollection
            {
                new StreamContent(new MemoryStream("file-two"u8.ToArray())),
                "FormFiles",
                "file2.txt"
            },
            {
                new StreamContent(new MemoryStream("file-three"u8.ToArray())),
                "FormFiles",
                "file3.txt"
            },

            // List<IFormFile>
            {
                new StreamContent(new MemoryStream("file-four"u8.ToArray())),
                "FormFileList",
                "file4.txt"
            },
            {
                new StreamContent(new MemoryStream("file-five"u8.ToArray())),
                "FormFileList",
                "file5.txt"
            },

            // Primitive values (Form binder)
            { new StringContent("1"), "InitOnlyProperty" },
            { new StringContent("42"), "IntProperty" },
            { new StringContent("Two"), "EnumProperty" },
            { new StringContent(""), "NullableEnumProperty" },
            { new StringContent("true"), "BoolProperty" },
            { new StringContent(""), "NullableBoolProperty" },
            { new StringContent("operator"), "Operator" },
            { new StringContent("2024-01-01T12:00:00Z"), "NullableDateTime" },
            { new StringContent("2024-01-02T15:30:00Z"), "NullableDateTimeOffset" },
            { new StringContent("2024-03-01T00:00:00Z"), "DateTime" },
            { new StringContent("2024-03-02T10:00:00Z"), "DateTimeOffset" },
            { new StringContent("hello"), "StringCollection" },
            { new StringContent("world"), "StringCollection" },
            { new StringContent(""), "NestedClasses.NestedProperty" },
            { new StringContent("123"), "NestedClasses.OtherProperty" },
            { new StringContent("x"), "NestedClassList[0].NestedProperty" },
            { new StringContent("1"), "NestedClassList[0].OtherProperty" },
            { new StringContent("y"), "NestedClassList[1].NestedProperty" },
            { new StringContent("2"), "NestedClassList[1].OtherProperty" },
        };

        // Act
        var response = await _factory.CreateClient().PostAsync(url, form);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UserQueryResponse>();

        await Assert.That(result).IsNotNull();

        // Assert
        await Assert.That(result.RouteParameter).IsEqualTo(1);
        await Assert.That(result.InitOnlyProperty).IsEqualTo(1);
        await Assert.That(result.FormFileCount).IsEqualTo(1);
        await Assert.That(result.FormFilesCount).IsEqualTo(2);
        await Assert.That(result.FormFileListCount).IsEqualTo(2);

        await Assert.That(result.Operator).IsEqualTo("operator");

        await Assert.That(result.BoolProperty).IsTrue();
        await Assert.That(result.NullableBoolProperty).IsNull();

        await Assert.That(result.NullableDateTime)
            .IsEqualTo(DateTime.Parse("2024-01-01T12:00:00Z"));

        await Assert.That(result.NullableDateTimeOffset)
            .IsEqualTo(DateTimeOffset.Parse("2024-01-02T15:30:00Z"));

        await Assert.That(result.DateTime)
            .IsEqualTo(DateTime.Parse("2024-03-01T00:00:00Z"));

        await Assert.That(result.DateTimeOffset)
            .IsEqualTo(DateTimeOffset.Parse("2024-03-02T10:00:00Z"));

        await Assert.That(result.IntProperty).IsEqualTo(42);
        await Assert.That(result.EnumProperty).IsEqualTo(EnumExample.Two);
        await Assert.That(result.NullableEnumProperty).IsNull();

        await Assert.That(result.GuidProperty)
            .IsEqualTo(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        await Assert.That(result.Page).IsEqualTo(2);
        await Assert.That(result.PageSize).IsEqualTo(50);
        await Assert.That(result.Search).IsEqualTo("abc");

        await Assert.That(result.NestedClass.NestedProperty).IsNull();
        await Assert.That(result.NestedClass.OtherProperty).IsEqualTo(123);

        await Assert.That(result.StringCollection).IsEquivalentTo(["hello", "world"]);
        await Assert.That(result.IntCollection).IsEquivalentTo([10, 20, 30]);

        await Assert.That(result.NestedClassList.Count).IsEqualTo(2);
        await Assert.That(result.NestedClassList[0].NestedProperty).IsEqualTo("x");
        await Assert.That(result.NestedClassList[0].OtherProperty).IsEqualTo(1);
        await Assert.That(result.NestedClassList[1].NestedProperty).IsEqualTo("y");
        await Assert.That(result.NestedClassList[1].OtherProperty).IsEqualTo(2);
    }
}
