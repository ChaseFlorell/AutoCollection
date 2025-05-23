using System;
using System.Collections.Immutable;
using AutoCollection.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoCollection;
/// <summary>
/// Serves as an abstract base class for creating incremental source generators that process specific attributes and generate code dynamically.
/// Provides utilities to initialize the generator for defined attributes and handle the generation process.
/// </summary>
public abstract class AutoCollectionGeneratorBase : IIncrementalGenerator
{
	/// <inheritdoc />
	public abstract void Initialize(IncrementalGeneratorInitializationContext context);

	/// <summary>
	/// Initializes the generator with the provided context and sets up the necessary configurations.
	/// </summary>
	/// <param name="context">The context used to initialize the incremental generator.</param>
	/// <param name="attributeName">The name of the attribute used to identify applicable types.</param>
	/// <param name="builder">A function used to generate code for the specified types.</param>
	protected static void Initialize(IncrementalGeneratorInitializationContext context, string attributeName, Func<ITypeSymbol, string, string> builder)
	{
		context.RegisterDefaultAttribute(attributeName, Constants.NAMESPACE_NAME);
		var readOnlyListClasses = CollectClassesForAttribute(context, attributeName);
		context.RegisterSourceOutput(readOnlyListClasses, (productionContext, array) => GenerateCode(productionContext, array, attributeName, builder));
	}

	private static IncrementalValueProvider<ImmutableArray<ITypeSymbol>> CollectClassesForAttribute(
		IncrementalGeneratorInitializationContext context,
		string attributeName) =>
		context
			.SyntaxProvider
			.ForAttributeWithMetadataName($"{Constants.NAMESPACE_NAME}.{attributeName}",
			                              (node, _) => node is ClassDeclarationSyntax,
			                              (ctx, _) => (ITypeSymbol)ctx.TargetSymbol)
			.Collect();

	private static void GenerateCode(SourceProductionContext context, ImmutableArray<ITypeSymbol> enumerations, string attributeName, Func<ITypeSymbol, string, string> builder)
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

			context.AddSource($"{typeNamespace}.{type.Name}", builder(type, attributeName));
		}
	}
}