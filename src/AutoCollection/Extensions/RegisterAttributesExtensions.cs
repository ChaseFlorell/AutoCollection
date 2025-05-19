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
			                   using System.Collections.Generic;

			                   namespace {{{namespaceName}}}
			                   {
			                       /// <summary>
			                       /// When you decorate a partial class with this attribute,
			                       /// Roslyn will autogenerate an <see cref="IReadOnlyList{T}" />
			                       /// implementation for your class
			                       /// </summary>
			                       /// <example>
			                       /// <code>
			                       /// [[attributeName(typeof(string))]]
			                       /// public partial class MyStringCollection;
			                       /// </code>
			                       /// </example>
			                       /// <example>
			                       /// <code>
			                       /// [[attributeName(typeof(Thing), nameof(_things))]]
			                       /// public partial class MyThingCollection(IEnumerable&lt;Thing&gt; things)
			                       /// {
			                       ///     private readonly IReadOnlyList&lt;Thing&gt; _things = things.ToArray();
			                       /// }
			                       /// </code>
			                       /// </example>
			                       /// <returns>A boilerplate implementation of <see cref="IReadOnlyList{T}" /></returns>
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