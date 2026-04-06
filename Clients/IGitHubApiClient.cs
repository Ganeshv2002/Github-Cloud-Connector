using GithubCloudConnector.Models;

namespace GithubCloudConnector.Clients;

public interface IGitHubApiClient
{
    Task<IReadOnlyList<GitHubRepositoryModel>> GetUserRepositoriesAsync(string username, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GitHubRepositoryModel>> GetOrgRepositoriesAsync(string org, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GitHubIssueModel>> GetRepositoryIssuesAsync(string owner, string repo, CancellationToken cancellationToken = default);
    Task<GitHubIssueModel> CreateIssueAsync(string owner, string repo, string title, string? body, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GitHubCommitModel>> GetRepositoryCommitsAsync(string owner, string repo, CancellationToken cancellationToken = default);
    Task<GitHubPullRequestModel> CreatePullRequestAsync(string owner, string repo, string title, string head, string baseBranch, string? body, bool draft, CancellationToken cancellationToken = default);
}
