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
		          .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is {})
		          .Executes(async () =>
			                    await GitHubTasks
			                          .GitHubClient
			                          .Git
			                          .Tag
			                          .Create(GitHubActions.Instance.RepositoryOwner, RepoName, new NewTag {Tag = $"v{Version}", Message = $"Release version {Version}", Object = GitHubActions.Instance.Sha, Type = TaggedType.Tag})
			                          .ConfigureAwait(false));

	/// <summary>
	/// Represents the target responsible for creating a new GitHub reference
	/// associated with the current commit.
	/// </summary>
	/// <remarks>
	/// This target creates a new reference on GitHub, pointing to the commit
	/// hash specified in the current GitHub Actions context. It ensures the
	/// creation of a tag reference in the format "v{Version}" by using the
	/// repository owner, repository name, and SHA of the commit provided by
	/// the GitHub Actions environment.
	/// </remarks>
	Target CreateGitHubReference =>
		target => target
		          .Description("Creates a new GitHub reference for the current commit")
		          .DependsOn(SetGitHubCredentials)
		          .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is {})
		          .Executes(async () =>
			                    await GitHubTasks
			                          .GitHubClient
			                          .Git
			                          .Reference
			                          .Create(GitHubActions.Instance.RepositoryOwner, RepoName, new NewReference($"refs/tags/v{Version}", GitHubActions.Instance.Sha))
			                          .ConfigureAwait(false));

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
		          .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is {})
		          .Executes(() => NewRelease = new NewRelease($"v{Version}")
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
		          .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is {})
		          .Executes(async () =>
			                    Release = await GitHubTasks
			                                    .GitHubClient
			                                    .Repository
			                                    .Release
			                                    .Create(GitHubActions.Instance.RepositoryOwner,
			                                            RepoName,
			                                            NewRelease)
			                                    .ConfigureAwait(false));

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
		          .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is {})
		          .Executes(async () =>
		          {
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
			                .UploadAsset(Release, assetUpload)
			                .ConfigureAwait(false);
		          }).Consumes(CreateGitHubRelease);

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
		          .DependsOn(CreateGitHubReference)
		          .DependsOn(UploadGitHubAsset)
		          .DependsOn<ICanTest>(x => x.Test)
		          .DependsOn<ICanInspectCode>(x => x.Inspect)
		          .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is {})
		          .WhenSkipped(DependencyBehavior.Skip)
		          .Executes(() => DotNetTasks.DotNetNuGetPush(cfg => cfg
		                                                             .SetApiKey(NugetApiKey)
		                                                             .SetSource("https://api.nuget.org/v3/index.json")
		                                                             .SetTargetPath(BuildArtifactsDirectory / RepoName / $"AutoCollection.{Version}.nupkg")
		                                                             .SetSymbolSource(BuildArtifactsDirectory / RepoName / $"AutoCollection.{Version}.snupkg")));

	/// <summary>
	/// Represents the metadata and configuration for creating a new GitHub release.
	/// </summary>
	/// <remarks>
	/// This property is used to define the details of a new release, including its version, title, and description.
	/// It also specifies additional attributes such as whether the release is a draft, a pre-release, or the target commit SHA.
	/// The property is primarily used during the execution of tasks that involve creating and publishing new GitHub releases.
	/// </remarks>
	protected internal NewRelease? NewRelease { get; set; }

	/// <summary>
	/// Represents the GitHub release associated with the current project version.
	/// </summary>
	/// <remarks>
	/// This property holds the release object created within the GitHub repository
	/// for a specific version of the project. It is set during the execution of the
	/// release process, which includes creating the release, uploading assets,
	/// and associating metadata such as tags and descriptions.
	/// </remarks>
	protected internal Release? Release { get; set; }
}