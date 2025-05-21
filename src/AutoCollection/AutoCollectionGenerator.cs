using System;
using System.Collections.Immutable;
using AutoCollection.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoCollection;
/// <inheritdoc />
/// <summary>
///     A sample source generator that creates a custom report based on class properties. The target class should be annotated with the 'Generators.ReportAttribute' attribute.
///     When using the source code as a baseline, an incremental source generator is preferable because it reduces the performance overhead.
/// </summary>
[Generator]
public sealed class AutoCollectionGenerator : IIncrementalGenerator
{
	/// <inheritdoc />
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterDefaultAttribute(Constants.READ_ONLY_LIST_ATTRIBUTE_NAME, Constants.NAMESPACE_NAME);

		const string READ_ONLY_LIST_METADATA_NAME = $"{Constants.NAMESPACE_NAME}.{Constants.READ_ONLY_LIST_ATTRIBUTE_NAME}";
		var classes =
			context
				.SyntaxProvider
				.ForAttributeWithMetadataName(READ_ONLY_LIST_METADATA_NAME,
				                              (node, _) => node is ClassDeclarationSyntax,
				                              (ctx, _) => (ITypeSymbol)ctx.TargetSymbol)
				.Collect();

		// Generate the source code.
		context.RegisterSourceOutput(classes, GenerateCode);
	}

	private static void GenerateCode(SourceProductionContext context, ImmutableArray<ITypeSymbol> enumerations)
	{
		if(enumerations.IsDefaultOrEmpty)
		{
			return;
		}

		foreach(var type in enumerations)
		{
			var typeNamespace = type.ContainingNamespace.IsGlobalNamespace
				? $"${Guid.NewGuid()}"
				: $"{type.ContainingNamespace}";

			context.AddSource($"{typeNamespace}.{type.Name}", Builder.BuildReadOnlyList(type));
		}
	}
}