using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.Tools.DotNet;

namespace AutoCollection.Build.Targets;

public interface ICanPublish : IHaveConfiguration
{
	Target Publish =>
		target => target
			.Description("Publish Nuget")
			.DependsOn<ICanTest>(x => x.Test)
			.OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is { })
			.WhenSkipped(DependencyBehavior.Skip)
			.Executes(() => DotNetTasks.DotNetNuGetPush(cfg => cfg
				.SetApiKey(NugetApiKey)
				.SetSource("https://api.nuget.org/v3/index.json")
				.SetTargetPath(BuildArtifactsDirectory / "AutoCollection" / $"AutoCollection.{Version}.nupkg")
				.SetSymbolSource(BuildArtifactsDirectory / "AutoCollection" / $"AutoCollection.{Version}.snupkg")));
}
