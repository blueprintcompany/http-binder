using System;
using System.Collections.Generic;
using System.Text;

namespace HttpBinder.Generator
{
    /// <summary>
    /// Specifies that a property should be bound from a query string parameter during model binding.
    /// </summary>
    /// <remarks>Apply this attribute to a property to indicate that its value should be populated from the
    /// query string of an HTTP request.</remarks>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public sealed class BindFromQueryAttribute : System.Attribute
    {
        /// <summary>
        /// The name of the query parameter to bind to the associated property. If null, the property name is used.
        /// </summary>
        public string Name { get; set; }
    }
}