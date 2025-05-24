using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace AutoCollection.Build.Targets;
/// <summary>
///     Represents an ability to analyze code using specific tools or configurations.
/// </summary>
public interface ICanInspectCode : IHaveConfiguration
{
	/// <summary>
	///     Represents a target that installs required .NET tools necessary for the build process.
	/// </summary>
	/// <remarks>
	///     This target ensures that all specified .NET tools are installed and configured before executing
	///     other dependent targets. It is commonly executed before inspection or compilation tasks.
	/// </remarks>
	Target InstallTools =>
		target => target
		          .Description("Installs required .NET tools")
		          .Before<ICanInspectCode>(x => x.Inspect)
		          .Executes(() => DotNetTasks
			                    .DotNetToolInstall(config =>
				                                       config
					                                       .SetPackageName("JetBrains.ReSharper.GlobalTools")
					                                       .SetVersion("2024.1.3")
					                                       .SetGlobal(true)));

	/// <summary>
	///     Represents a target that runs ReSharper InspectCode using the EditorConfig for code analysis.
	/// </summary>
	/// <remarks>
	///     This target uses ReSharper's code inspection tool to analyze the codebase according to the specified EditorConfig rules.
	///     The inspection severity level can be adjusted using the InspectCodeSeverity parameter, defaulting to 'WARNING' if not specified.
	/// </remarks>
	Target Inspect =>
		target => target
		          .Description("Runs ReSharper InspectCode using EditorConfig")
		          .DependsOn<ICanInspectCode>(static x => x.InstallTools)
		          .Executes(() => ProcessTasks
		                          .StartProcess("jb",
		                                        $"inspectcode {Solution} --output={InspectionResults} --severity=WARNING --format=Sarif --properties:Configuration={Configuration} --disable-settings-layers=SolutionPersonal --no-buildin-settings --no-build",
		                                        RootDirectory)
		                          .AssertZeroExitCode());
}