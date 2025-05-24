using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;

namespace AutoCollection.Build.Targets;
/// <summary>
/// Interface representing a target that is responsible for compiling projects within the build pipeline.
/// </summary>
/// <remarks>
/// The Compile target defines the steps necessary to build projects, excluding specific configurations
/// such as the build environment or tests. It is intended to be dependent on the initialization process
/// defined in <see cref="ICanInitialize" /> prior to its execution, ensuring proper setup has occurred.
/// </remarks>
public interface ICanCompile : IHaveConfiguration
{
	/// <summary>
	/// Defines a build target for compiling projects within the solution.
	/// The target builds all projects except for a specified one, sets versioning attributes,
	/// and outputs the build artifacts to designated directories.
	/// </summary>
	/// <remarks>
	/// This target depends on the <c>Initialize</c> target, ensuring that any initialization
	/// steps are completed before the compilation begins.
	/// </remarks>
	/// <seealso cref="ICanInitialize"/>
	/// <seealso cref="IHaveConfiguration"/>
	Target Compile =>
		target => target
		          .Description("Build Projects")
		          .DependsOn<ICanInitialize>(static x => x.Initialize)
		          .Executes(() => Solution!
		                          .AllProjects
		                          .Where(project => project.Name != "AutoCollection.Build")
		                          .ForEach(project =>
			                                   DotNetTasks
				                                   .DotNetBuild(s => s
				                                                     .SetProjectFile(project)
				                                                     .SetTreatWarningsAsErrors(true)
				                                                     .SetWarningLevel(4)
				                                                     .SetAssemblyVersion(Version)
				                                                     .SetFileVersion(__today)
				                                                     .SetVersion(Version)
				                                                     .SetInformationalVersion(Version)
				                                                     .SetOutputDirectory(BuildArtifactsDirectory / project.Name)
				                                                     .SetConfiguration(Configuration)
				                                                     .SetProperty("ContinuousIntegrationBuild", true))));

	private static readonly string __today = $"{DateTime.Today:yyyy.MM.dd}.{GitHubActions.Instance?.RunNumber ?? 0}";
}