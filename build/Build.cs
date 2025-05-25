using AutoCollection.Build.Targets;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Octokit;

namespace AutoCollection.Build;
/// <summary>
///     Represents the build pipeline configuration and execution logic for the project.
///     Inherits functionality from the NukeBuild class and implements various interfaces
///     (ICanInitialize, ICanCompile, ICanTest, ICanPublish) to define the build stages.
///     Includes integration with GitHub Actions for continuous integration operations.
/// </summary>
[GitHubActions("continuous",
               GitHubActionsImage.MacOsLatest,
               OnPushBranchesIgnore = ["main",],
               InvokedTargets = [nameof(ICanTest.Test),],
               CacheKeyFiles = ["**/global.json", "**/Directory.Packages.props",],
               ImportSecrets = [nameof(NugetApiKey),],
               EnableGitHubToken = true)]
[GitHubActions("inspect code",
               GitHubActionsImage.MacOsLatest,
               OnPushBranchesIgnore = ["main",],
               InvokedTargets = [nameof(ICanInspectCode.Inspect),],
               CacheKeyFiles = ["**/global.json", "**/Directory.Packages.props",],
               ImportSecrets = [nameof(NugetApiKey),],
               EnableGitHubToken = true)]
[GitHubActions("main",
               GitHubActionsImage.MacOsLatest,
               OnPushBranches = ["main",],
               InvokedTargets = [nameof(ICanPublish.PublishNuget),],
               CacheKeyFiles = ["**/global.json", "**/Directory.Packages.props",],
               ImportSecrets = [nameof(NugetApiKey),],
               EnableGitHubToken = true,
               WritePermissions = [GitHubActionsPermissions.Actions, GitHubActionsPermissions.Contents,])]
internal class Build
	: NukeBuild,
	  ICanInitialize,
	  ICanCompile,
	  ICanTest,
	  ICanPublish,
	  ICanInspectCode
{
	/// <inheritdoc />
	[Parameter]
	[Secret]
	public string? GitHubToken { get; }

	/// <inheritdoc />
	[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] public Configuration Configuration { get; set; } = Configuration.Release;

	/// <inheritdoc />
	[Solution] public Solution? Solution { get; }

	/// <inheritdoc />
	[GitRepository] public GitRepository? Repository { get; }

	/// <inheritdoc />
	[Parameter] [Secret] public string? NugetApiKey { get; }

	/// <inheritdoc />
	public NewRelease? NewRelease { get; set; }

	/// <inheritdoc />
	public Release? Release { get; set; }

	public static int Main() => Execute<Build>(x => x.Run);

	/// <summary>
	///     Represents the primary build target in the pipeline.
	///     Responsible for orchestrating the execution of unit tests by depending on the
	///     <see cref="ICanTest.Test" /> target. This target ensures tests are run after the
	///     compilation process and continues execution even if some tests fail.
	/// </summary>
	private Target Run =>
		target =>
			target
				.DependsOn<ICanTest>(static x => x.Test)
				.DependsOn<ICanInspectCode>(static x => x.Inspect);
}