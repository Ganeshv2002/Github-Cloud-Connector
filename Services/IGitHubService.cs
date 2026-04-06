using GithubCloudConnector.DTOs;

namespace GithubCloudConnector.Services;

public interface IGitHubService
{
    Task<IReadOnlyList<RepositoryDto>> GetRepositoriesAsync(string owner, string type, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IssueDto>> GetIssuesAsync(string owner, string repo, CancellationToken cancellationToken = default);
    Task<CreateIssueResponse> CreateIssueAsync(CreateIssueRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommitDto>> GetCommitsAsync(string owner, string repo, CancellationToken cancellationToken = default);
    Task<PullRequestDto> CreatePullRequestAsync(CreatePullRequestRequest request, CancellationToken cancellationToken = default);
}
