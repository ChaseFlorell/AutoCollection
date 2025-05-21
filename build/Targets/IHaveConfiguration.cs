using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

namespace AutoCollection.Build.Targets;

/// <summary>
/// Provides a standardized set of properties for build configuration in the Nuke build system.
/// This interface defines common paths, version information, and build parameters.
/// </summary>
public interface IHaveConfiguration : INukeBuild
{
    /// <summary>
    /// Gets the build configuration to use.
    /// Defaults to 'Debug' for local builds or 'Release' for server builds.
    /// </summary>
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    Configuration Configuration { get; }

    /// <summary>
    /// Gets the solution file reference.
    /// This property is automatically resolved by Nuke using the [Solution] attribute.
    /// </summary>
    [Solution]
    Solution? Solution { get; }

    /// <summary>
    /// Gets the Git repository information.
    /// This property is automatically resolved by Nuke using the [GitRepository] attribute.
    /// </summary>
    [GitRepository]
    GitRepository? Repository { get; }

    /// <summary>
    /// Gets the API key for publishing to NuGet.
    /// This property is marked as [Secret] and should be provided externally and handled securely.
    /// </summary>
    [Parameter]
    [Secret]
    string? NugetApiKey { get; }

    /// <summary>
    /// Gets the root directory for all artifacts.
    /// Located at RootDirectory/artifacts.
    /// </summary>
    AbsolutePath ArtifactsRootDirectory => RootDirectory / "artifacts";

    /// <summary>
    /// Gets the directory for build artifacts.
    /// Located at ArtifactsRootDirectory/build.
    /// </summary>
    AbsolutePath BuildArtifactsDirectory => ArtifactsRootDirectory / "build";

    /// <summary>
    /// Gets the directory for NuGet packages.
    /// Located at RootDirectory/.nuget/packages.
    /// </summary>
    AbsolutePath NugetDirectory => RootDirectory / ".nuget" / "packages";

    /// <summary>
    /// Gets the directory for test results.
    /// Located at ArtifactsRootDirectory/test-results.
    /// </summary>
    AbsolutePath TestResultsDirectory => ArtifactsRootDirectory / "test-results";

    /// <summary>
    /// Gets the directory containing source code.
    /// Located at RootDirectory/src.
    /// </summary>
    AbsolutePath SourceDirectory => RootDirectory / "src";

    /// <summary>
    /// Gets the version string in the format "0.0.0.X" where X is the GitHub Actions run number
    /// or 1 if not running in GitHub Actions.
    /// </summary>
    string Version => $"0.0.{GitHubActions.Instance?.RunNumber ?? 1}";
}