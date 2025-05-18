using System;
using Nuke.Common;
using Nuke.Common.IO;

namespace AutoCollection.Build.Targets;
public interface ICanInitialize : IHaveConfiguration
{
	Target Initialize =>
		target => target
		          .Description("Initialize the build")
		          .Executes(() =>
		          {
			          Console.WriteLine($"Preparing Version: {Version}");
			          ArtifactsRootDirectory.DeleteDirectory();
			          NugetDirectory.CreateDirectory();
		          });
}