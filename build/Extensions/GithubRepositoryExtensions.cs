using Nuke.Common.Git;

namespace AutoCollection.Build.Extensions;
internal static class GitHubRepositoryExtensions
{
	public static string GetGitHubOwner(this GitRepository repository)
	{
		return repository.Identifier.Split('/')[0];
	}

	public static string GetGitHubName(this GitRepository repository)
	{
		return repository.Identifier.Split('/')[1].Replace(".git", string.Empty);
	}
}