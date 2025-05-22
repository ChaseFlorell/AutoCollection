using Microsoft.CodeAnalysis;

namespace AutoCollection.Builders;
internal static class ListBuilder
{
	public static string Build(ITypeSymbol arg)
	{
		return "namespace Example;";
	}
}