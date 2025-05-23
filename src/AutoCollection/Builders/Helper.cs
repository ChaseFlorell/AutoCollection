using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace AutoCollection.Builders;
internal static class Helper
{
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
}