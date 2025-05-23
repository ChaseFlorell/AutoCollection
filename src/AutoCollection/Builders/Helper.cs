using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Text;

namespace AutoCollection.Builders;
internal static class Helper
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

		using var reader = Helper.Load(attributeName, needsAutoBackingField);
		var formatted = Helper.Format(reader, collectionType, backingField.ToString(), type);
		var builder = new StringBuilder(Constants.AUTO_GENERATED_MESSAGE);
		builder.AppendLine();
		builder.Append(formatted);

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
	internal static Dictionary<string, object?> GetParameterDictionary(this AttributeData generationAttribute)
	{
		var dict = new Dictionary<string, object?>();
		for(var index = 0; index < generationAttribute.AttributeConstructor!.Parameters.Length; index++)
		{
			var parameter = generationAttribute.AttributeConstructor!.Parameters[index];
			dict.Add(parameter.Name, generationAttribute.ConstructorArguments[index].Value);
		}

		return dict;
	}

	public static StreamReader? Load(
		string attributeName,
		bool needsAutoBackingField)
	{
		var assembly = Assembly.GetExecutingAssembly();

		// The resource name is usually: "{DefaultNamespace}.{Folder}.{filename}"
		// Use your project's default namespace and folder structure here
		var flavor = needsAutoBackingField ? "Backed" : "Default"; // "Backed" or "Default"
		var resourceName = $"AutoCollection.Templates.{attributeName}.{flavor}.txt";

		var stream = assembly.GetManifestResourceStream(resourceName);
		if(stream == null)
		{
			return null;
		}

		return new(stream);
	}

	public static string Format(StreamReader? reader, string collectionType, string backingField, ITypeSymbol type)
	{
		var template = reader!.ReadToEnd();
		return template
		       .Replace("{CollectionType}", collectionType)
		       .Replace("{BackingField}", backingField)
		       .Replace("{Namespace}", type.ContainingNamespace.ToString())
		       .Replace("{ClassName}", type.Name)
		       .Replace("{Accessor}", SyntaxFacts.GetText(type.DeclaredAccessibility));
	}
}