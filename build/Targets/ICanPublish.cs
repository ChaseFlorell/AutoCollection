using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitHub;
using Octokit;

namespace AutoCollection.Build.Targets;
/// <inheritdoc />
/// <summary>
///     The <c>ICanPublish</c> interface is responsible for managing the publish process, particularly
///     for publishing packages to a NuGet repository.
///     It extends the <c>IHaveConfiguration</c> interface to leverage common configuration properties
///     and utilities for the build process.
/// </summary>
/// <remarks>
///     This interface defines a build target named <c>Publish</c> that depends on the <c>Test</c> target
///     from the <c>ICanTest</c> interface. It will only execute when the current repository branch
///     is identified as either "main" or "master", and when executed within a GitHub Actions context.
/// </remarks>
/// <dependencies>
///     - Requires access to <c>ICanTest.Test</c> to ensure all tests are successfully executed before publishing.
///     - Depends on the NuGet API key (via the configuration <c>NugetApiKey</c>) and ensures the package is
///     pushed to the NuGet repository.
/// </dependencies>
public interface ICanPublish : IHaveConfiguration
{
	/// <summary>
	///     Defines the target responsible for publishing the NuGet package.
	/// </summary>
	/// <remarks>
	///     The <c>Publish</c> target is dependent on the execution of the <c>Test</c> target from the <c>ICanTest</c> interface.
	///     It is executed only when certain conditions are met:
	///     - The repository is on the main or master branch.
	///     - The process is running in a GitHub Actions context.
	///     In case the target is skipped, dependent actions will also be skipped following the defined dependency behavior.
	///     When executed, it publishes the NuGet package to the NuGet registry, leveraging the provided API key and version information.
	/// </remarks>
	Target Publish =>
		d => d
		     .Description("Publish Nuget")
		     .DependsOn<ICanTest>(x => x.Test)
		     .DependsOn<ICanInspectCode>(x => x.Inspect)
		     .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is {})
		     .WhenSkipped(DependencyBehavior.Skip)
		     .Executes(() => DotNetTasks.DotNetNuGetPush(cfg => cfg
		                                                        .SetApiKey(NugetApiKey)
		                                                        .SetSource("https://api.nuget.org/v3/index.json")
		                                                        .SetTargetPath(BuildArtifactsDirectory / "AutoCollection" / $"AutoCollection.{Version}.nupkg")
		                                                        .SetSymbolSource(BuildArtifactsDirectory / "AutoCollection" / $"AutoCollection.{Version}.snupkg")))
		     .Executes(async () =>
		     {
			     GitHubTasks.GitHubClient.Credentials = new Credentials(GitHubActions.Instance?.Token);

			     // Get the SHA of the current commit
			     var sha = GitTasks.Git("rev-parse HEAD", logOutput: false, logInvocation: false)
			                       .FirstOrDefault().Text;

			     // Create a tag and reference object
			     var tagObject = new NewTag {Tag = $"v{Version}", Message = $"Release version {Version}", Object = sha, Type = TaggedType.Tag};
			     var tagRef = new NewReference($"refs/tags/v{Version}", sha);

			     await GitHubTasks
			           .GitHubClient
			           .Git
			           .Tag
			           .Create("ChaseFlorell", "AutoCollection", tagObject)
			           .ConfigureAwait(false);

			     await GitHubTasks
			           .GitHubClient
			           .Git
			           .Reference
			           .Create("ChaseFlorell", "AutoCollection", tagRef);
		     });
}