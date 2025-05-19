using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.Tools.DotNet;

namespace AutoCollection.Build.Targets;
public interface ICanPublish : IHaveConfiguration
{
	Target Publish =>
		d => d
		     .Description("Publish Nuget")
		     .DependsOn<ICanTest>(x => x.Test)
		     .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch() && GitHubActions.Instance is {})
		     .WhenSkipped(DependencyBehavior.Skip)
		     .Executes(() => DotNetTasks.DotNetNuGetPush(cfg => cfg
		                                                        .SetApiKey(NugetApiKey)
		                                                        .SetSource("https://api.nuget.org/v3/index.json")
		                                                        .SetTargetPath(PublishDirectory / $"AutoCollection.{Version}.nupkg")
		                                                        .SetSymbolSource(PublishDirectory / $"AutoCollection.{Version}.snupkg")));
}