using System;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.DotNet;

namespace AutoCollection.Build.Targets;
public interface ICanCompile : IHaveConfiguration
{
	Target Compile =>
		target => target
			.Description("Build Projects")
			.DependsOn<ICanInitialize>(static x => x.Initialize)
			.Executes(() =>
			{
				// Get all projects in the solution
				var projects = Solution!.AllProjects;

				// Build each project separately
				foreach (var project in projects)
				{
					DotNetTasks.DotNetBuild(s => s
						.SetProjectFile(project)
						.SetAssemblyVersion(Version)
						.SetFileVersion(__todaysBuild)
						.SetVersion(Version)
						.SetInformationalVersion(Version)
						.SetOutputDirectory(BuildArtifactsDirectory / project.Name)
						.SetConfiguration(Configuration));
				}
			});


	private static readonly string __todaysBuild = $"{DateTime.Today:yyyy.MM.dd}.{GitHubActions.Instance?.RunNumber ?? 0}";
}
