using System;

namespace Blueprint.HttpBinder;

/// <summary>
/// Apply this attribute to a property to ignore any binding by the generator.
/// <para>Base example:</para>
/// <example>
/// <code>
/// [BindFromIgnore]
/// public int PageNumber { get; set }
/// </code>
/// </example>
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class BindFromIgnoreAttribute : Attribute;