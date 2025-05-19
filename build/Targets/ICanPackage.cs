using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.Tools.DotNet;

namespace AutoCollection.Build.Targets;
public interface ICanPublish : IHaveConfiguration
{
	Target Publish =>
		d => d
		     .Description("Publish Nuget")
		     .DependsOn<ICanTest>(x => x.Test)
		     .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch())
		     .WhenSkipped(DependencyBehavior.Skip)
		     .Executes(() => DotNetTasks.DotNetNuGetPush(cfg => cfg
		                                                        .SetApiKey(NugetApiKey)
		                                                        .SetSymbolSource(PublishDirectory / "*.snupkg")
		                                                        .SetSource(PublishDirectory / "*.nupkg")));
}