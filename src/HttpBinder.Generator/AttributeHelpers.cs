using System.Collections.Generic;

namespace HttpBinder.Generator;

public static class AttributeHelpers
{
    public static List<AttributeDefinition> List = [
        new() {
            FileName = "HttpBinderAttribute.g.cs",
            Source = @"
            namespace HttpBinder
            {
                [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Record)]
                public sealed class HttpBinderAttribute : System.Attribute 
                {
                    public HttpBinderAttribute(HttpBinderType httpBinderType) { HttpBinderType = httpBinderType; }
                    public HttpBinderType HttpBinderType { get; }
                }
            }"
        },
        new() {
            FileName = "BindFromFormAttribute.g.cs",
            Source =@"
            namespace HttpBinder
            {
                [System.AttributeUsage(System.AttributeTargets.Property)]
                public sealed class BindFromFormAttribute : System.Attribute { }
            }"
        },
        new() {
            FileName = "BindFromQueryAttribute.g.cs",
            Source = @"
            namespace HttpBinder
            {
                [System.AttributeUsage(System.AttributeTargets.Property)]
                public sealed class BindFromQueryAttribute : System.Attribute { }
            }"
        },
        new() {
            FileName = "BindFromRouteAttribute.g.cs",
            Source = @"
            namespace HttpBinder
            {
                [System.AttributeUsage(System.AttributeTargets.Property)]
                public sealed class BindFromRouteAttribute : System.Attribute { }
            }"
        },
        new() {
            FileName = "BindFormFieldAttribute.g.cs",
            Source = @"
            namespace HttpBinder
            {
                [System.AttributeUsage(System.AttributeTargets.Property)]
                public sealed class BindFormFieldAttribute : System.Attribute
                {
                    public BindFormFieldAttribute(string name) { Name = name; }
                    public string Name { get; }
                }
            }"
        }
    ];

    public class AttributeDefinition
    {
        public string FileName { get; set; } = null!;
        public string Source { get; set; } = null!;
    }
}