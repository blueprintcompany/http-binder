using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace HttpBinder.Generator
{
    /// <summary>
    /// Represents the source from which a property is bound. Properties may be
    /// explicitly decorated with one of the supported attributes or fall back to
    /// the default of <see cref="Form"/>.
    /// </summary>
    internal enum SourceKind
    {
        Form,
        Query,
        Route
    }

    /// <summary>
    /// Represents a single property that will be bound at runtime. This model
    /// captures the semantic information required to generate binding code
    /// including the target name, source and whether the property is a
    /// collection or nested complex type.
    /// </summary>
    internal sealed class BoundProperty
    {
        public BoundProperty(IPropertySymbol symbol, string keyName, SourceKind source, bool isNullable,
            bool isCollection, ITypeSymbol? elementType, bool isEnum, bool isGuid, bool isPrimitive,
            bool isString, bool isComplex, List<BoundProperty>? children)
        {
            Symbol = symbol;
            Name = symbol.Name;
            KeyName = keyName;
            Source = source;
            IsNullable = isNullable;
            IsCollection = isCollection;
            ElementType = elementType;
            IsEnum = isEnum;
            IsGuid = isGuid;
            IsPrimitive = isPrimitive;
            IsString = isString;
            IsComplex = isComplex;
            Children = children ?? new List<BoundProperty>();
        }

        /// <summary>
        /// The original property symbol.
        /// </summary>
        public IPropertySymbol Symbol { get; }

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The key name to look up in the HTTP sources. This can be overridden
        /// with the <c>FormField</c> attribute.
        /// </summary>
        public string KeyName { get; }

        /// <summary>
        /// Indicates the source from which the value will be read (form, query or
        /// route). If none of the explicit attributes are applied the default
        /// source is form.
        /// </summary>
        public SourceKind Source { get; }

        /// <summary>
        /// Whether the property can accept a null value. Nullable reference
        /// types and <see cref="System.Nullable{T}"/> struct types are
        /// considered nullable.
        /// </summary>
        public bool IsNullable { get; }

        /// <summary>
        /// Whether the property is a collection type (arrays or generic lists).
        /// </summary>
        public bool IsCollection { get; }

        /// <summary>
        /// For collection properties, the element type contained in the
        /// collection. This will be null for non‑collection properties.
        /// </summary>
        public ITypeSymbol? ElementType { get; }

        /// <summary>
        /// Whether the property type is an enum.
        /// </summary>
        public bool IsEnum { get; }

        /// <summary>
        /// Whether the property type is a <see cref="System.Guid"/>.
        /// </summary>
        public bool IsGuid { get; }

        /// <summary>
        /// Whether the property type is a primitive numeric or boolean type.
        /// </summary>
        public bool IsPrimitive { get; }

        /// <summary>
        /// Whether the property type is a string.
        /// </summary>
        public bool IsString { get; }

        /// <summary>
        /// Whether the property is a complex type requiring nested binding.
        /// </summary>
        public bool IsComplex { get; }

        /// <summary>
        /// For complex and collection of complex properties, the list of child
        /// properties discovered on the element type. Empty for primitive or
        /// simple collection properties.
        /// </summary>
        public List<BoundProperty> Children { get; }
    }

    /// <summary>
    /// Represents an entire DTO type to which a binder will be generated. This
    /// contains the full list of properties (including inherited ones),
    /// information about whether the type should be initialised via a
    /// constructor or a parameterless instance and the mapping of constructor
    /// parameters to properties.
    /// </summary>
    internal sealed class BoundType
    {
        public BoundType(INamedTypeSymbol typeSymbol, List<BoundProperty> properties,
            IMethodSymbol? constructor, List<(IParameterSymbol parameter, BoundProperty property)> constructorMapping)
        {
            TypeSymbol = typeSymbol;
            Properties = properties;
            Constructor = constructor;
            ConstructorMapping = constructorMapping;
        }

        /// <summary>
        /// The target type symbol decorated with <c>GenerateHttpBinder</c>.
        /// </summary>
        public INamedTypeSymbol TypeSymbol { get; }

        /// <summary>
        /// The list of all bound properties including those inherited from base
        /// types. Derived type definitions override base definitions by name.
        /// </summary>
        public List<BoundProperty> Properties { get; }

        /// <summary>
        /// If the type defines a meaningful public constructor with parameters
        /// then this will be set. Otherwise it will be null and the binder
        /// initialises via the default constructor.
        /// </summary>
        public IMethodSymbol? Constructor { get; }

        /// <summary>
        /// For constructor initialisation, maps each constructor parameter to
        /// the corresponding bound property (matched by name, case insensitive).
        /// </summary>
        public List<(IParameterSymbol parameter, BoundProperty property)> ConstructorMapping { get; }
    }
}