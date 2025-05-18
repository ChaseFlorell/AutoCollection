using System;
using AutoCollection.Build.Targets;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;

namespace AutoCollection.Build;
[GitHubActions("continuous",
               GitHubActionsImage.MacOsLatest,
               On = [GitHubActionsTrigger.Push,],
               InvokedTargets = [nameof(Run),],
               CacheKeyFiles = ["**/global.json", "**/Directory.Packages.props",])]
class Build
	: NukeBuild,
	  ICanInitialize,
	  ICanBuild,
	  ICanTest
{
	public Build()
	{
		var msBuildExtensionPath = Environment.GetEnvironmentVariable("MSBuildExtensionsPath");
		var msBuildExePath = Environment.GetEnvironmentVariable("MSBUILD_EXE_PATH");
		var msBuildSdkPath = Environment.GetEnvironmentVariable("MSBuildSDKsPath");

		MSBuildLocator.RegisterDefaults();
		TriggerAssemblyResolution();

		Environment.SetEnvironmentVariable("MSBuildExtensionsPath", msBuildExtensionPath);
		Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", msBuildExePath);
		Environment.SetEnvironmentVariable("MSBuildSDKsPath", msBuildSdkPath);
	}

	public static int Main() => Execute<Build>(x => x.Run);

	[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] public Configuration Configuration { get; set; } = Configuration.Release;

	[Solution] public Solution? Solution { get; }

	[GitRepository] public GitRepository? Repository { get; }

	Target Run =>
		target => target
		          .Description("Build Solution")
		          .DependsOn<ICanInitialize>(static x => x.Initialize)
		          .DependsOn<ICanBuild>(static x => x.Build)
		          .DependsOn<ICanTest>(static x => x.Test);

	private static void TriggerAssemblyResolution() => _ = new ProjectCollection();
}