using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace AutoCollection.Build.Targets;

/// <summary>
/// Represents a contract for performing unit tests in the build process.
/// This interface extends <see cref="IHaveConfiguration"/> and defines a target for executing unit tests.
/// </summary>
/// <remarks>
/// The testing process includes:
/// - Setting up test configurations such as result loggers and directories.
/// - Executing tests for all matching test assemblies found in the build artifacts.
/// - Proceeding with the build process even if some tests fail.
/// - Leveraging dependencies, ensuring that testing is performed after compilation.
/// </remarks>
public interface ICanTest : IHaveConfiguration
{
	/// <summary>
	/// Represents the target for executing unit tests in the build process.
	/// </summary>
	/// <remarks>
	/// This target is responsible for running unit tests across all specified test assemblies.
	/// It performs the following actions:
	/// - Logs the test results in TRX format.
	/// - Stores the results in the predefined test results directory.
	/// - Executes tests in the configuration defined by the build process (e.g., Debug or Release).
	/// - Handles multiple test assemblies by combining them from the build artifacts directory.
	/// - Can proceed even if some tests fail, allowing the build pipeline to continue execution.
	/// The target depends on the successful completion of the <c>Compile</c> target, and any necessary setups
	/// must be completed before invoking the test execution.
	/// </remarks>
	Target Test =>
		target =>
			target
				.Description("Execute Unit Tests")
				.DependsOn<ICanCompile>(static x => x.Compile)
				.ProceedAfterFailure()
				.Executes(() => DotNetTasks
					.DotNetTest(c => c
							.SetLoggers("trx")
							.SetResultsDirectory(TestResultsDirectory)
							.SetConfiguration(Configuration)
							.CombineWith(BuildArtifactsDirectory
									.GlobFiles("**/*Tests.dll"),
								static (s, value) =>
									s.SetProjectFile(value)),
						completeOnFailure: true));
}
