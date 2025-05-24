using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.Tools.Git;

namespace AutoCollection.Build.Targets;
/// <summary>
/// Represents build target functionality for creating a Git tag as part of the release process.
/// </summary>
/// <remarks>
/// This interface is intended to standardize the implementation of a release target in the build pipeline.
/// It provides a target that can handle Git tagging for releases, ensuring the process is dependent on other necessary build steps
/// and conditional based on the repository's main or master branch and GitHub Actions context.
/// </remarks>
public interface ICanRelease : IHaveConfiguration
{
	/// <summary>
	/// Represents the build target for creating a Git tag as part of the release process.
	/// </summary>
	/// <remarks>
	/// This property defines a build target that:
	/// - Adds a Git tag for the release.
	/// - Depends on the successful completion of the "Publish" target.
	/// - Executes only when:
	/// - The current branch is the main or master branch.
	/// - The build is running in the GitHub Actions CI context.
	/// </remarks>
	Target TagRelease =>
		target =>
			target
				.Description("Creates a Git tag for the release")
				.DependsOn<ICanPublish>(x => x.Publish)
				.OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is {})
				.Executes(() =>
				{
					GitTasks.Git($"tag -a v{Version} -m \"Release version {Version}\"", workingDirectory: RootDirectory);
					GitTasks.Git("push --tags", workingDirectory: RootDirectory);
				});
}