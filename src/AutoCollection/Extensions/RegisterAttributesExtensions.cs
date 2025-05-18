using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AutoCollection.Extensions;
internal static class RegisterAttributesExtensions
{
	public static IncrementalGeneratorInitializationContext RegisterDefaultAttribute(
		this IncrementalGeneratorInitializationContext context,
		string attributeName,
		string namespaceName)
	{
		context.RegisterPostInitializationOutput(postInitializationContext =>
		{
			var attribute = $$$"""
			                   #nullable enable
			                   {{{Constants.AUTO_GENERATED_MESSAGE}}}
			                   using System;

			                   namespace {{{namespaceName}}}
			                   {
			                       /// <summary>
			                       /// Use source generator to automatically wire up a collection
			                       /// </summary>
			                       [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			                       internal sealed class {{{attributeName}}} : Attribute
			                       {
			                           internal {{{attributeName}}}(
			                               Type {{{Constants.COLLECTION_TYPE_PARAMETER_NAME}}},
			                               string? {{{Constants.BACKING_FIELD_PARAMETER_NAME}}} = null
			                               ) { }
			                       }
			                   }
			                   """;

			var className = $"{attributeName}.Attribute.g.cs";
			postInitializationContext
				.AddSource(className, SourceText.From(attribute, Encoding.UTF8));
		});
		return context;
	}
}