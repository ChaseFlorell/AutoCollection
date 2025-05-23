using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using System.Linq;

namespace AutoCollection.VerifyTests.Util;
public static class Infrastructure
{
	public static string GenerateCode(IIncrementalGenerator generator, string code)
	{
		var syntaxTree = CSharpSyntaxTree.ParseText(code);
		var references = AppDomain
		                 .CurrentDomain.GetAssemblies()
		                 .Where(assembly => !assembly.IsDynamic)
		                 .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
		                 .Cast<MetadataReference>();

		var compilation = CSharpCompilation.Create(
		                                           "SourceGeneratorTests",
		                                           [syntaxTree,],
		                                           references,
		                                           new(OutputKind.DynamicallyLinkedLibrary)
		                                          );

		// defensive check to avoid errors in the source text, which can happen with manually written code
		var sourceDiagnostics = compilation.GetDiagnostics();
		sourceDiagnostics
			.Where(d => d.Severity == DiagnosticSeverity.Error)
			.Where(x => x.Id != "CS0246") // missing references are ok
			.ShouldBeEmpty();

		CSharpGeneratorDriver
			.Create(generator)
			.RunGeneratorsAndUpdateCompilation(
			                                   compilation,
			                                   out var outputCompilation,
			                                   out var diagnostics
			                                  );

		diagnostics
			.Where(d => d.Severity == DiagnosticSeverity.Error)
			.ShouldBeEmpty();

		if(outputCompilation.SyntaxTrees.Count() <= 2)
		{
			return string.Empty;
		}

		var result = outputCompilation.SyntaxTrees.Skip(2).Last().ToString();
		return result;
	}
}