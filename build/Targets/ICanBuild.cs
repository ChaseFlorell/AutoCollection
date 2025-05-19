using System;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.DotNet;

namespace AutoCollection.Build.Targets;
public interface ICanCompile : IHaveConfiguration
{
	Target Compile =>
		target => target
		          .Description("Build Solution")
		          .DependsOn<ICanInitialize>(static x => x.Initialize)
		          .Executes(() => DotNetTasks
			                    .DotNetBuild(s => s
			                                      .SetProjectFile(Solution)
			                                      .SetAssemblyVersion(Version)
			                                      .SetFileVersion(__todaysBuild)
			                                      .SetVersion(Version)
			                                      .SetInformationalVersion(Version)
			                                      .SetOutputDirectory(BuildArtifactsDirectory)
			                                      .SetConfiguration(Configuration)));

	private static readonly string __todaysBuild = $"{DateTime.Today:yyyy.MM.dd}.{GitHubActions.Instance?.RunId ?? 0}";
}