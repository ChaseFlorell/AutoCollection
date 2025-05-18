using System;
using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

namespace AutoCollection.Build.Targets;
public interface IHaveConfiguration : INukeBuild
{
	[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] Configuration Configuration { get; }

	[Solution] Solution? Solution { get; }

	[GitRepository] GitRepository? Repository { get; }

	AbsolutePath ArtifactsRootDirectory => RootDirectory / "artifacts";

	AbsolutePath BuildArtifactsDirectory => ArtifactsRootDirectory / "build";

	AbsolutePath TestArtifactsDirectory => ArtifactsRootDirectory / "test";

	AbsolutePath PublishDirectory => ArtifactsRootDirectory / "publish";

	public AbsolutePath NugetDirectory => RootDirectory / ".nuget" / "packages";

	AbsolutePath TestResultsDirectory => ArtifactsRootDirectory / "test-results";

	AbsolutePath SourceDirectory => RootDirectory / "src";

	string Version => $"{DateTime.Today:yyyy}.{Quarter}.{AzurePipelines.Instance?.BuildId ?? 0}";

	int Quarter => (DateTime.Today.Month + 2) / 3;
}