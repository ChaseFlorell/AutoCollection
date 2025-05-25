using System.IO;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.Tools.DotNet;
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
#pragma warning disable CA1506
	/// <summary>
	/// Represents the target responsible for publishing NuGet packages
	/// and creating GitHub release tags.
	/// </summary>
	/// <remarks>
	/// The Publish target depends on the Test and Inspect targets.
	/// It ensures publishing only occurs under specific conditions,
	/// such as being on the main or master branch and when triggered
	/// by GitHub Actions. It performs the following actions:
	/// 1. Pushes the NuGet package to the NuGet repository.
	/// 2. Sets up GitHub client authentication using GitHub Actions token.
	/// 3. Creates a new GitHub tag and release for the current version of the project.
	/// </remarks>
	Target Publish =>
#pragma warning restore CA1506
#pragma warning disable CA1506
		target => target
		          .Description("Publish Nuget")
		          .DependsOn<ICanTest>(x => x.Test)
		          .DependsOn<ICanInspectCode>(x => x.Inspect)
		          .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is {})
		          .WhenSkipped(DependencyBehavior.Skip)
		          .Executes(() => DotNetTasks.DotNetNuGetPush(cfg => cfg
		                                                             .SetApiKey(NugetApiKey)
		                                                             .SetSource("https://api.nuget.org/v3/index.json")
		                                                             .SetTargetPath(BuildArtifactsDirectory / RepoName / $"AutoCollection.{Version}.nupkg")
		                                                             .SetSymbolSource(BuildArtifactsDirectory / RepoName / $"AutoCollection.{Version}.snupkg")))
		          .Executes(() => GitHubTasks.GitHubClient.Credentials = new Credentials(GitHubActions.Instance.Token))
		          .Executes(async () => await GitHubTasks
		                                      .GitHubClient
		                                      .Git
		                                      .Tag
		                                      .Create(GitHubActions.Instance.RepositoryOwner, RepoName, new NewTag {Tag = $"v{Version}", Message = $"Release version {Version}", Object = GitHubActions.Instance.Sha, Type = TaggedType.Tag})
		                                      .ConfigureAwait(false))
		          .Executes(async () => await GitHubTasks
		                                      .GitHubClient
		                                      .Git
		                                      .Reference
		                                      .Create(GitHubActions.Instance.RepositoryOwner, "AutoCollection", new NewReference($"refs/tags/v{Version}", GitHubActions.Instance.Sha)))
		          .Executes(async () =>
		          {
			          var release = await GitHubTasks
			                              .GitHubClient
			                              .Repository
			                              .Release
			                              .Create(GitHubActions.Instance.RepositoryOwner,
			                                      RepoName,
			                                      new NewRelease($"v{Version}")
			                                      {
				                                      Name = $"Release v{Version}",
				                                      Body = $"Release of version {Version}",
				                                      Draft = false,
				                                      Prerelease = false,
				                                      TargetCommitish = GitHubActions.Instance.Sha,
			                                      })
			                              .ConfigureAwait(false);

			          // Optionally, upload the NuGet package as an asset to the GitHub release
			          var packagePath = BuildArtifactsDirectory / RepoName / $"AutoCollection.{Version}.nupkg";
			          await using var packageStream = File.OpenRead(packagePath);
			          var assetUpload = new ReleaseAssetUpload
			          {
				          FileName = Path.GetFileName(packagePath),
				          ContentType = "application/octet-stream",
				          RawData = packageStream,
			          };

			          await GitHubTasks
			                .GitHubClient
			                .Repository
			                .Release
			                .UploadAsset(release, assetUpload)
			                .ConfigureAwait(false);
		          });
#pragma warning restore CA1506
}