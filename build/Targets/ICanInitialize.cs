using System;
using Nuke.Common;
using Nuke.Common.IO;

namespace AutoCollection.Build.Targets;
/// <summary>
///     Represents an interface that enables initialization capabilities in the build process.
/// </summary>
/// <remarks>
///     The <c>ICanInitialize</c> interface is responsible for defining a target that initializes
///     the build process. It inherits from the <c>IHaveConfiguration</c> interface, which provides
///     configuration-related properties and capabilities.
/// </remarks>
public interface ICanInitialize : IHaveConfiguration
{
	/// <summary>
	///     Represents a build target used to initialize the build process.
	/// </summary>
	/// <remarks>
	///     This target is typically responsible for executing preliminary setup tasks or operations
	///     that are necessary before proceeding to subsequent build steps.
	///     It may include tasks such as cleaning directories, restoring tools, or configuring the environment.
	/// </remarks>
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