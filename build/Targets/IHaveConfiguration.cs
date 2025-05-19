using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

namespace AutoCollection.Build.Targets;
public interface IHaveConfiguration : INukeBuild
{
	[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] Configuration Configuration { get; }

	[Solution] Solution? Solution { get; }

	[GitRepository] GitRepository? Repository { get; }

	[Parameter] [Secret] string? NugetApiKey { get; }

	AbsolutePath ArtifactsRootDirectory => RootDirectory / "artifacts";

	AbsolutePath BuildArtifactsDirectory => ArtifactsRootDirectory / "build";

	public AbsolutePath NugetDirectory => RootDirectory / ".nuget" / "packages";

	AbsolutePath TestResultsDirectory => ArtifactsRootDirectory / "test-results";

	AbsolutePath SourceDirectory => RootDirectory / "src";

	string Version => $"0.0.0.{GitHubActions.Instance?.RunNumber ?? 1}";
}