using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoCollection.Builders;
internal static class ReadOnlyListBuilder
{
	public static string Build(ITypeSymbol type, string attributeName)
	{
		var generationAttribute = type
		                          .GetAttributes()
		                          .First(x =>
			                                 x.AttributeClass != null
			                                 && x.AttributeClass.Name.Contains(attributeName)
		                                );

		var dict = generationAttribute.GetParameterDictionary();

		var collectionType = dict[Constants.COLLECTION_TYPE_PARAMETER_NAME]!.ToString();
		var backingField = dict[Constants.BACKING_FIELD_PARAMETER_NAME];

		var needsAutoBackingField = backingField is null;
		backingField ??= "_items";

		var builder = new StringBuilder(Constants.AUTO_GENERATED_MESSAGE);
		builder.AppendLine();
		builder.AppendLine("using System.Collections;");
		builder.AppendLine("using System.Collections.Generic;");
		builder.AppendLine("using System.Linq;");
		builder.AppendLine();
		builder.AppendLine($"namespace {type.ContainingNamespace}");
		builder.AppendLine("{");

		builder.AppendLine("\t/// <inheritdoc cref=\"IReadOnlyList{T}\" />");
		builder.AppendLine($"\t{SyntaxFacts.GetText(type.DeclaredAccessibility)} partial class {type.Name} : IReadOnlyList<{collectionType}>");
		builder.AppendLine("\t{");

		if(needsAutoBackingField)
		{
			builder.AppendLine($"\t\tpublic {type.Name}(IEnumerable<{collectionType}> items) =>");
			builder.AppendLine("\t\t\t_items = items?.ToArray() ?? throw new System.ArgumentNullException(nameof(items));");
			builder.AppendLine();
		}

		builder.AppendLine("\t\t/// <inheritdoc cref=\"IReadOnlyList{T}\" />");
		builder.AppendLine($"\t\tpublic IEnumerator<{collectionType}> GetEnumerator() => {backingField}.GetEnumerator();");
		builder.AppendLine();
		builder.AppendLine("\t\t/// <inheritdoc cref=\"IEnumerable\" />");
		builder.AppendLine("\t\tIEnumerator IEnumerable.GetEnumerator() => GetEnumerator();");
		builder.AppendLine();
		builder.AppendLine("\t\t/// <inheritdoc cref=\"IReadOnlyCollection{T}\" />");
		builder.AppendLine($"\t\tpublic int Count => {backingField}.Count;");
		builder.AppendLine();
		builder.AppendLine("\t\t/// <inheritdoc cref=\"IReadOnlyList{T}\" />");
		builder.AppendLine($"\t\tpublic {collectionType} this[int index] => {backingField}[index];");

		if(needsAutoBackingField)
		{
			builder.AppendLine();
			builder.AppendLine($"\t\tprivate readonly IReadOnlyList<{collectionType}> _items;");
		}

		builder.AppendLine("\t}");
		builder.AppendLine("}");

		return builder.ToString();
	}
}