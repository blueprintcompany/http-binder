using Blueprint.HttpBinder.Sample.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace Blueprint.HttpBinder.Generator.Tests.Sample;

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
                new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("file-one"))),
                "FormFile",
                "file1.txt"
            },

            // IFormFileCollection
            {
                new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("file-two"))),
                "FormFiles",
                "file2.txt"
            },
            {
                new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("file-three"))),
                "FormFiles",
                "file3.txt"
            },

            // List<IFormFile>
            {
                new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("file-four"))),
                "FormFileList",
                "file4.txt"
            },
            {
                new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("file-five"))),
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
