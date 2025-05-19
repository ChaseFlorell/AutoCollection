using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace AutoCollection.Build.Targets;
public interface ICanTest : IHaveConfiguration
{
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