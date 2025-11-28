using System.Collections.Generic;

namespace HttpBinder.Generator;

internal static class AttributeHelpers
{
    public static List<AttributeDefinition> List = [
        new() {
            FileName = "HttpBinderAttribute.g.cs",
            Source = @"
            namespace HttpBinder.Generator
            {
                /// <summary>
                /// Specifies the HTTP binder type to associate with a class for source-generated HTTP binding behavior using BindAsync().
                /// </summary>
                [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
                [System.AttributeUsage(System.AttributeTargets.Class)]
                public sealed class HttpBinderAttribute : System.Attribute
                {
                    /// <summary>
                    /// The HTTP binder type that determines how the attribute will bind HTTP data.
                    /// </summary>
                    public HttpBinderType HttpBinderType { get; set; }
                }
            }"
        },
        new() {
            FileName = "BindFromFormAttribute.g.cs",
            Source =@"
            namespace HttpBinder.Generator
            {
                /// <summary>
                /// Specifies that a property should be bound from form data in an HTTP request.
                /// </summary>
                /// <remarks>Apply this attribute to a property to indicate that its value should be populated from form
                /// fields when model binding occurs./remarks>
                [global::Microsoft.CodeAnalysis.EmbeddedAttribute]                
                [System.AttributeUsage(System.AttributeTargets.Property)]
                public sealed class BindFromFormAttribute : System.Attribute
                {
                    /// <summary>
                    /// The name of the form field to bind to the associated property. If null, the property name is used.
                    /// </summary>
                    public string Name { get; set; }
                }
            }"
        },
        new() {
            FileName = "BindFromQueryAttribute.g.cs",
            Source = @"
            namespace HttpBinder.Generator
            {
                /// <summary>
                /// Specifies that a property should be bound from a query string parameter during model binding.
                /// </summary>
                /// <remarks>Apply this attribute to a property to indicate that its value should be populated from the
                /// query string of an HTTP request.</remarks>
                [global::Microsoft.CodeAnalysis.EmbeddedAttribute]                
                [System.AttributeUsage(System.AttributeTargets.Property)]
                public sealed class BindFromQueryAttribute : System.Attribute
                {
                    /// <summary>
                    /// The name of the query parameter to bind to the associated property. If null, the property name is used.
                    /// </summary>
                    public string Name { get; set; }
                }
            }"
        },
        new() {
            FileName = "BindFromRouteAttribute.g.cs",
            Source = @"
            namespace HttpBinder.Generator
            {
                /// <summary>
                /// Specifies that a property should be bound from a route parameter during model binding.
                /// </summary>
                /// <remarks>
                /// Apply this attribute to a property to indicate that its value should be populated from the
                /// corresponding route value in the request URL.
                /// </remarks>
                [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
                [System.AttributeUsage(System.AttributeTargets.Property)]
                public sealed class BindFromRouteAttribute : System.Attribute
                {
                    /// <summary>
                    /// The name of the route parameter to bind to the associated property. If null, the property name is used.
                    /// </summary>
                    public string Name { get; }
                }
            }"
        }
    ];

    internal class AttributeDefinition
    {
        public string FileName { get; set; } = null!;
        public string Source { get; set; } = null!;
    }
}