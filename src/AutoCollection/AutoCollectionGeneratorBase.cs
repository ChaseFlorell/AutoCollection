using System;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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
		RegisterDefaultAttribute(context, attributeName, Constants.NAMESPACE_NAME);
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

			context.AddSource($"{typeNamespace}.{type.Name}.g", builder(type, attributeName));
		}
	}

	private static IncrementalGeneratorInitializationContext RegisterDefaultAttribute(
		IncrementalGeneratorInitializationContext context,
		string attributeName,
		string namespaceName)
	{
		context.RegisterPostInitializationOutput(postInitializationContext =>
		{
			var attribute = $$$"""
			                   #nullable enable
			                   {{{Constants.AUTO_GENERATED_MESSAGE}}}
			                   using System;
			                   using System.Collections.Generic;

			                   namespace {{{namespaceName}}}
			                   {
			                       /// <summary>
			                       /// When you decorate a partial class with this attribute,
			                       /// Roslyn will autogenerate a collection
			                       /// implementation for your class
			                       /// </summary>
			                       /// <example>
			                       /// <code lang="csharp">
			                       /// [{{{attributeName}}}(typeof(string))]
			                       /// public partial class MyStringCollection;
			                       /// </code>
			                       /// </example>
			                       /// <example>
			                       /// <code lang="csharp">
			                       /// [{{{attributeName}}}(typeof(Thing), nameof(_things))]
			                       /// public partial class MyThingCollection(IEnumerable&lt;Thing&gt; things)
			                       /// {
			                       ///     private readonly IReadOnlyList&lt;Thing&gt; _things = things.ToArray();
			                       /// }
			                       /// </code>
			                       /// </example>
			                       /// <returns>A boilerplate implementation of your specified collection type.</returns>
			                       [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			                       internal sealed class {{{attributeName}}} : Attribute
			                       {
			                           internal {{{attributeName}}}(Type {{{Constants.COLLECTION_TYPE_PARAMETER_NAME}}}, string? {{{Constants.BACKING_FIELD_PARAMETER_NAME}}} = null) { }
			                       }
			                   }
			                   """;

			var className = $"{attributeName}.Attribute.g.cs";
			postInitializationContext.AddSource(className, SourceText.From(attribute, Encoding.UTF8));
		});

		return context;
	}
}