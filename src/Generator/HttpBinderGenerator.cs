using Blueprint.HttpBinder.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

[assembly: InternalsVisibleTo("Generator.Tests")]

namespace Blueprint.HttpBinder
{
    [Generator]
    public sealed class HttpBinderGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var candidateTypes = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeConstants.HttpBinderAttribute,
                static (node, _) => node is TypeDeclarationSyntax,
                (syntaxCtx, cancellationToken) => BuildModel((INamedTypeSymbol?)syntaxCtx.TargetSymbol, syntaxCtx.Attributes[0], cancellationToken)
            ).WithTrackingName(TrackingConstants.InitialExtraction)
            .Where(static m => m is not null)
            .Select(static (m, _) => m!)
            .WithTrackingName(TrackingConstants.RemovingNulls);

            context.RegisterSourceOutput(candidateTypes, static (sourceProductionContext, model) =>
            {
                var source = CodeRenderer.Render(model);
                sourceProductionContext.AddSource($"{model.Name}.g.cs", source);
            });
        }

        private static BoundType? BuildModel(INamedTypeSymbol? typeSymbol, AttributeData attributeData, CancellationToken cancellation)
        {
            if (typeSymbol is null)
            {
                return null;
            }

            cancellation.ThrowIfCancellationRequested();

            var classHttpBinderType = HttpBinderType.Form;

            if (attributeData.TryGetBinderType(out var parsedBinderType))
            {
                classHttpBinderType = parsedBinderType;
            }

            var properties = BindingModelBuilder.GetAllViableProperties(typeSymbol)
                .Select(p => BindingModelBuilder.BuildBoundProperty(p, classHttpBinderType, []))
                .ToImmutableArray();

            // Choose the "largest" public constructor, if any
            var ctors = typeSymbol.InstanceConstructors
                .Where(c => c.DeclaredAccessibility == Accessibility.Public)
                .OrderByDescending(c => c.Parameters.Length)
                .ToList();

            var ctor = ctors.FirstOrDefault(c => c.Parameters.Length > 0);

            var ctorParameterNames = new ImmutableArray<string>();

            if (ctor is not null)
            {
                var names = ImmutableArray.CreateBuilder<string>(ctor.Parameters.Length);

                foreach (var parameter in ctor.Parameters)
                {
                    names.Add(parameter.Name);
                }

                ctorParameterNames = names.MoveToImmutable();
            }

            var @namespace = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
            var isSingleFileProgram = @namespace == "<global namespace>";
            if (isSingleFileProgram)
            {
                @namespace = string.Empty;
            }

            var name = typeSymbol.Name;
            var fullName = typeSymbol.GetFullTypeName();
            var boundTypeKind = typeSymbol.IsRecord ? BoundTypeKind.RecordClass : BoundTypeKind.Class;

            return new BoundType(
                name,
                @namespace,
                fullName,
                boundTypeKind,
                classHttpBinderType,
                properties,
                ctorParameterNames);
        }
    }
}
