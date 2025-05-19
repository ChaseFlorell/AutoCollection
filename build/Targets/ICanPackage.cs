using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;

namespace AutoCollection.Build.Targets;
public interface ICanPublish : IHaveConfiguration
{
	Target Publish =>
		d => d
		     .Description("Publish Nuget")
		     .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch())
		     .WhenSkipped(DependencyBehavior.Skip)
		     .Executes(() => DotNetTasks.DotNetNuGetPush(cfg => cfg
		                                                        .SetApiKey(NugetApiKey)
		                                                        .SetSymbolSource(PublishDirectory / "*.snupkg")
		                                                        .SetSource(PublishDirectory / "*.nupkg")));
}