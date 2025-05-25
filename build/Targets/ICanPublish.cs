using System.IO;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;
using Octokit;

namespace AutoCollection.Build.Targets;
/// <inheritdoc />
/// <summary>
///     The <c>ICanPublish</c> interface is responsible for managing the publication process, particularly
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
	/// Configures GitHub client authentication by setting credentials using the token provided by GitHub Actions.
	/// </summary>
	/// <remarks>
	/// This target ensures that the <see cref="GitHubTasks.GitHubClient"/> is properly authenticated by using
	/// the token available from the GitHub Actions workflow. This is essential for performing various GitHub
	/// operations such as creating tags, releases, or references in a CI/CD pipeline.
	/// </remarks>
	Target SetGitHubCredentials =>
		target => target
		          .Description("Configures GitHub client authentication using the token provided by GitHub Actions")
		          .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is {})
		          .Executes(() => GitHubTasks.GitHubClient.Credentials = new Credentials(GitHubActions.Instance.Token));

	/// <summary>
	/// Represents the target responsible for creating a GitHub tag for the current version of the project.
	/// </summary>
	/// <remarks>
	/// This target interacts with the GitHub API to create a new tag associated with the
	/// commit currently being executed within a GitHub Actions workflow. The tag includes
	/// the version number derived from the build configuration and a release message.
	/// This ensures that a versioned tag is added in the repository for tracking purposes.
	/// </remarks>
	Target CreateGitHubTag =>
		target => target
		          .Description("Creates a new GitHub tag for the current version of the project")
		          .DependsOn(SetGitHubCredentials)
		          .DependsOn(UploadGitHubAsset)
		          .DependsOn<ICanTest>(x => x.Test)
		          .DependsOn<ICanInspectCode>(x => x.Inspect)
		          .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is {})
		          .Executes(async () =>
			                    await GitHubTasks
			                          .GitHubClient
			                          .Git
			                          .Tag
			                          .Create(GitHubActions.Instance.RepositoryOwner, RepositoryName, new NewTag {Tag = $"v{Version}", Message = $"Release version {Version}", Object = GitHubActions.Instance.Sha, Type = TaggedType.Tag}));

	/// <summary>
	/// Represents the target responsible for preparing a release by initializing
	/// a new GitHub release object with the current version and commit hash.
	/// </summary>
	/// <remarks>
	/// The PrepareRelease target creates a structured GitHub release entry. It performs the following:
	/// 1. Generates a release object with the current version string as the tag (e.g., "v{Version}").
	/// 2. Sets the name and body of the release to include the version and release details.
	/// 3. Links the release to the specified commit SHA (GitHubActions.Instance.Sha).
	/// 4. Marks the release as neither a draft nor a pre-release, making it immediately visible to users.
	/// This target is typically part of the release process, following tests and other preparatory steps.
	/// </remarks>
	Target PrepareRelease =>
		target => target
		          .Description("Prepare a new GitHub release for the current version of the project")
		          .DependsOn<ICanTest>(x => x.Test)
		          .DependsOn<ICanInspectCode>(x => x.Inspect)
		          .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is {})
		          .Executes(() => Build.NewRelease = new NewRelease($"v{Version}")
		          {
			          Name = $"Release v{Version}",
			          Body = $"Release of version {Version}",
			          Draft = false,
			          Prerelease = false,
			          TargetCommitish = GitHubActions.Instance.Sha,
		          });

	/// <summary>
	/// Represents the target responsible for creating a release on GitHub for the current version of the project.
	/// </summary>
	/// <remarks>
	/// The CreateGitHubRelease target performs the following actions:
	/// 1. Utilizes GitHub API to create a new release tied to the current version of the project.
	/// 2. Ensures authentication by leveraging GitHub Actions token or pre-configured credentials.
	/// 3. Associates the release with the corresponding GitHub tag and repository.
	/// </remarks>
	Target CreateGitHubRelease =>
		target => target
		          .Description("Creates a new GitHub release for the current version of the project")
		          .DependsOn(PrepareRelease)
		          .DependsOn(SetGitHubCredentials)
		          .DependsOn<ICanTest>(x => x.Test)
		          .DependsOn<ICanInspectCode>(x => x.Inspect)
		          .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is {})
		          .Executes(() => GitHubTasks
		                          .GitHubClient
		                          .Repository
		                          .Release
		                          .Create(GitHubActions.Instance.RepositoryOwner,
		                                  RepositoryName,
		                                  Build.NewRelease)
		                          .ContinueWith(task => Build.Release = task.Result, TaskScheduler.Default));

	/// <summary>
	/// Represents the target responsible for uploading assets to a GitHub release.
	/// </summary>
	/// <remarks>
	/// The UploadGitHubAsset target is designed to handle the process of attaching assets,
	/// such as binaries or other files, to an existing GitHub release. This ensures
	/// that release artifacts are available for download. The target leverages GitHub's API
	/// and requires proper authentication using a GitHub token. Key actions include:
	/// 1. Identifying the GitHub release to which the assets will be added.
	/// 2. Uploading specified files or artifacts to the targeted release.
	/// 3. Handling the association of uploaded assets with the correct release version.
	/// </remarks>
	Target UploadGitHubAsset =>
		target => target
		          .Description("Uploads assets to a GitHub release")
		          .DependsOn(CreateGitHubRelease)
		          .DependsOn(SetGitHubCredentials)
		          .DependsOn<ICanTest>(x => x.Test)
		          .DependsOn<ICanInspectCode>(x => x.Inspect)
		          .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is {})
		          .Executes(async () =>
		          {
			          await using var packageStream = File.OpenRead(NugetPackageReference);
			          var assetUpload = new ReleaseAssetUpload
			          {
				          FileName = Path.GetFileName(NugetPackageReference),
				          ContentType = "application/octet-stream",
				          RawData = packageStream,
			          };

			          await GitHubTasks
			                .GitHubClient
			                .Repository
			                .Release
			                .UploadAsset(Build.Release, assetUpload);
		          });

	/// <summary>
	/// Defines the target for publishing NuGet packages to the NuGet repository.
	/// </summary>
	/// <remarks>
	/// This target performs several chained actions to facilitate the publishing process:
	/// - It depends on the successful execution of the <c>Test</c> target from <see cref="ICanTest"/> and the <c>Inspect</c> target from <see cref="ICanInspectCode"/>.
	/// - It executes only when the repository is on the main or master branch and is in a GitHub Actions context.
	/// - When skipped, it ensures dependent targets are also skipped, following the defined dependency behavior.
	/// - It pushes the NuGet package to the official NuGet repository using the specified API key, source URL, and package file path.
	/// </remarks>
	Target PublishNuget =>
		target => target
		          .Description("Publishes NuGet packages to the official NuGet repository")
		          .DependsOn(CreateGitHubTag)
		          .DependsOn(UploadGitHubAsset)
		          .DependsOn<ICanTest>(x => x.Test)
		          .DependsOn<ICanInspectCode>(x => x.Inspect)
		          .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is {})
		          .WhenSkipped(DependencyBehavior.Skip)
		          .Executes(() => DotNetTasks.DotNetNuGetPush(cfg => cfg
		                                                             .SetApiKey(NugetApiKey)
		                                                             .SetSource("https://api.nuget.org/v3/index.json")
		                                                             .SetTargetPath(NugetPackageReference)
		                                                             .SetSymbolSource(SymbolPackageReference)));
}