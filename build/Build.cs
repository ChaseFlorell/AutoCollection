using AutoCollection.Build.Targets;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;

namespace AutoCollection.Build;
[GitHubActions("continuous",
               GitHubActionsImage.MacOsLatest,
               On = [GitHubActionsTrigger.Push,],
               InvokedTargets = [nameof(ICanPublish.Publish),],
               CacheKeyFiles = ["**/global.json", "**/Directory.Packages.props",],
               ImportSecrets = [nameof(NugetApiKey)],
               EnableGitHubToken = true)]
class Build
	: NukeBuild,
	  ICanInitialize,
	  ICanCompile,
	  ICanTest,
	  ICanPublish
{
	[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] public Configuration Configuration { get; set; } = Configuration.Release;

	[Solution] public Solution? Solution { get; }

	[GitRepository] public GitRepository? Repository { get; }

	[Parameter] [Secret] public string? NugetApiKey { get; }

	public static int Main() => Execute<Build>(x => x.Run);

	Target Run => target => target.DependsOn<ICanTest>(static x => x.Test);
}