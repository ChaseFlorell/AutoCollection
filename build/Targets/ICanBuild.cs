using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;

namespace AutoCollection.Build.Targets;

public interface ICanCompile : IHaveConfiguration
{
	Target Compile =>
		target => target
			.Description("Build Projects")
			.DependsOn<ICanInitialize>(static x => x.Initialize)
			.Executes(() => Solution!
				.AllProjects
				.Where(project => project.Name != "AutoCollection.Build")
				.ForEach(project =>
					DotNetTasks
						.DotNetBuild(s => s
							.SetProjectFile(project)
							.SetAssemblyVersion(Version)
							.SetFileVersion(__todaysBuild)
							.SetVersion(Version)
							.SetInformationalVersion(Version)
							.SetOutputDirectory(BuildArtifactsDirectory / project.Name)
							.SetConfiguration(Configuration))));

	private static readonly string __todaysBuild = $"{DateTime.Today:yyyy.MM.dd}.{GitHubActions.Instance?.RunNumber ?? 0}";
}
