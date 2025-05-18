using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace AutoCollection;
internal static class Builder
{
	public static string BuildReadOnlyList(ITypeSymbol type)
	{
		var generationAttribute = type
		                          .GetAttributes()
		                          .First(x =>
			                                 x.AttributeClass != null
			                                 && x.AttributeClass.Name.Contains(Constants.READ_ONLY_LIST_ATTRIBUTE_NAME)
		                                );

		var dict = GetParameterDictionary(generationAttribute);

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

	/// <summary>
	///     Converts the attribute data of a given attribute into a dictionary where the keys are the
	///     parameter names from the attribute's constructor and the values are the corresponding
	///     argument values passed to the attribute.
	/// </summary>
	/// <param name="generationAttribute">
	///     The attribute data from which to extract parameter
	///     information.
	/// </param>
	/// <returns>
	///     A dictionary containing the parameter names as keys and their corresponding
	///     values as dictionary values.
	/// </returns>
	private static Dictionary<string, object?> GetParameterDictionary(AttributeData generationAttribute)
	{
		var dict = new Dictionary<string, object?>();
		for(var index = 0; index < generationAttribute.AttributeConstructor!.Parameters.Length; index++)
		{
			var parameter = generationAttribute.AttributeConstructor!.Parameters[index];
			dict.Add(parameter.Name, generationAttribute.ConstructorArguments[index].Value);
		}

		return dict;
	}
}