using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace HttpBinder.Generator
{
    /// <summary>
    /// Represents a single property that will be bound at runtime. This model
    /// captures the semantic information required to generate binding code
    /// including the target name, source and whether the property is a
    /// collection or nested complex type.
    /// </summary>
    internal sealed class BoundProperty(IPropertySymbol symbol, string keyName, HttpBinderType httpBinderType, bool isNullable,
        bool isCollection, ITypeSymbol? elementType, bool isEnum, bool isGuid, bool isPrimitive,
        bool isString, bool isComplex, List<BoundProperty>? children)
    {

        /// <summary>
        /// The original property symbol.
        /// </summary>
        public IPropertySymbol Symbol { get; } = symbol;

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; } = symbol.Name;

        /// <summary>
        /// The key name to look up in the HTTP sources. This can be overridden
        /// with the <c>FormField</c> attribute.
        /// </summary>
        public string KeyName { get; } = keyName;

        /// <summary>
        /// Indicates the source from which the value will be read (form, query or
        /// route). If none of the explicit attributes are applied the default
        /// source is form.
        /// </summary>
        public HttpBinderType HttpBinderType { get; set; } = httpBinderType;

        /// <summary>
        /// Whether the property can accept a null value. Nullable reference
        /// types and <see cref="System.Nullable{T}"/> struct types are
        /// considered nullable.
        /// </summary>
        public bool IsNullable { get; } = isNullable;

        /// <summary>
        /// Whether the property is a collection type (arrays or generic lists).
        /// </summary>
        public bool IsCollection { get; } = isCollection;

        /// <summary>
        /// For collection properties, the element type contained in the
        /// collection. This will be null for non‑collection properties.
        /// </summary>
        public ITypeSymbol? ElementType { get; } = elementType;

        /// <summary>
        /// Whether the property type is an enum.
        /// </summary>
        public bool IsEnum { get; } = isEnum;

        /// <summary>
        /// Whether the property type is a <see cref="System.Guid"/>.
        /// </summary>
        public bool IsGuid { get; } = isGuid;

        /// <summary>
        /// Whether the property type is a primitive numeric or boolean type.
        /// </summary>
        public bool IsPrimitive { get; } = isPrimitive;

        /// <summary>
        /// Whether the property type is a string.
        /// </summary>
        public bool IsString { get; } = isString;

        /// <summary>
        /// Whether the property is a complex type requiring nested binding.
        /// </summary>
        public bool IsComplex { get; } = isComplex;

        /// <summary>
        /// For complex and collection of complex properties, the list of child
        /// properties discovered on the element type. Empty for primitive or
        /// simple collection properties.
        /// </summary>
        public List<BoundProperty> Children { get; } = children ?? [];
    }

    /// <summary>
    /// Represents an entire DTO type to which a binder will be generated. This
    /// contains the full list of properties (including inherited ones),
    /// information about whether the type should be initialised via a
    /// constructor or a parameterless instance and the mapping of constructor
    /// parameters to properties.
    /// </summary>
    internal sealed class BoundType(INamedTypeSymbol typeSymbol, List<BoundProperty> properties,
        IMethodSymbol? constructor, List<(IParameterSymbol parameter, BoundProperty property)> constructorMapping)
    {

        /// <summary>
        /// The target type symbol decorated with <c>GenerateHttpBinder</c>.
        /// </summary>
        public INamedTypeSymbol TypeSymbol { get; } = typeSymbol;

        /// <summary>
        /// The list of all bound properties including those inherited from base
        /// types. Derived type definitions override base definitions by name.
        /// </summary>
        public List<BoundProperty> Properties { get; } = properties;

        /// <summary>
        /// If the type defines a meaningful public constructor with parameters
        /// then this will be set. Otherwise it will be null and the binder
        /// initialises via the default constructor.
        /// </summary>
        public IMethodSymbol? Constructor { get; } = constructor;

        /// <summary>
        /// For constructor initialisation, maps each constructor parameter to
        /// the corresponding bound property (matched by name, case insensitive).
        /// </summary>
        public List<(IParameterSymbol parameter, BoundProperty property)> ConstructorMapping { get; } = constructorMapping;
    }
}