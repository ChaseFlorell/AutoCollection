using AutoCollection.Builders;
using Microsoft.CodeAnalysis;

namespace AutoCollection;
/// <summary>
/// A source generator that processes types annotated with the 'GenerateListAttribute' attribute
/// to dynamically create collections and associated backing fields. The generator leverages
/// the attribute's specifications to generate appropriate code at compile time.
/// </summary>
/// <remarks>
/// Inherits from <see cref="AutoCollectionGeneratorBase"/> and uses a custom builder defined by the
/// <see cref="Helper"/> class to generate the final source code for the target types.
/// </remarks>
[Generator]
public sealed class ListGenerator : AutoCollectionGeneratorBase
{
	/// <inheritdoc />
	public override void Initialize(IncrementalGeneratorInitializationContext context) => Initialize(context, Constants.LIST_ATTRIBUTE_NAME, Helper.Build);
}