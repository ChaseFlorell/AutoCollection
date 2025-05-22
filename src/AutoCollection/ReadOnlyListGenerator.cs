using AutoCollection.Builders;
using Microsoft.CodeAnalysis;

namespace AutoCollection;
/// <inheritdoc />
/// <summary>
///     A sample source generator that creates a custom report based on class properties. The target class should be annotated with the 'Generators.ReportAttribute' attribute.
///     When using the source code as a baseline, an incremental source generator is preferable because it reduces the performance overhead.
/// </summary>
[Generator]
public sealed class ReadOnlyListGenerator : AutoCollectionGenerator
{
	/// <inheritdoc />
	public override void Initialize(IncrementalGeneratorInitializationContext context) => Initialize(context, Constants.READ_ONLY_LIST_ATTRIBUTE_NAME, ReadOnlyListBuilder.Build);
}