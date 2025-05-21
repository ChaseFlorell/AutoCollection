using AutoCollection.Build.Targets;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;

namespace AutoCollection.Build;

/// <summary>
/// Represents the build pipeline configuration and execution logic for the project.
/// Inherits functionality from the NukeBuild class and implements various interfaces
/// (ICanInitialize, ICanCompile, ICanTest, ICanPublish) to define the build stages.
/// Includes integration with GitHub Actions for continuous integration operations.
/// </summary>
[GitHubActions("continuous",
	GitHubActionsImage.MacOsLatest,
	On = [GitHubActionsTrigger.Push,],
	InvokedTargets = [nameof(ICanPublish.Publish),],
	CacheKeyFiles = ["**/global.json", "**/Directory.Packages.props",],
	ImportSecrets = [nameof(NugetApiKey)],
	EnableGitHubToken = true)]
internal class Build
	: NukeBuild,
		ICanInitialize,
		ICanCompile,
		ICanTest,
		ICanPublish
{
	[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] public Configuration Configuration { get; set; } = Configuration.Release;

	[Solution] public Solution? Solution { get; }

	[GitRepository] public GitRepository? Repository { get; }

	[Parameter] [Secret] public string? NugetApiKey { get; }

	public static int Main() => Execute<Build>(x => x.Run);

	/// <summary>
	/// Represents the primary build target in the pipeline.
	/// Responsible for orchestrating the execution of unit tests by depending on the
	/// <see cref="ICanTest.Test"/> target. This target ensures tests are run after the
	/// compilation process and continues execution even if some tests fail.
	/// </summary>
	private Target Run => target => target.DependsOn<ICanTest>(static x => x.Test);
}
