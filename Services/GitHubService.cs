using GithubCloudConnector.Clients;
using GithubCloudConnector.DTOs;
using GithubCloudConnector.Models;

namespace GithubCloudConnector.Services;

public class GitHubService : IGitHubService
{
    private readonly IGitHubApiClient _apiClient;

    public GitHubService(IGitHubApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IReadOnlyList<RepositoryDto>> GetRepositoriesAsync(string owner, string type, CancellationToken cancellationToken = default)
    {
        var repos = type.Equals("org", StringComparison.OrdinalIgnoreCase)
            ? await _apiClient.GetOrgRepositoriesAsync(owner, cancellationToken)
            : await _apiClient.GetUserRepositoriesAsync(owner, cancellationToken);

        return repos.Select(ToRepositoryDto).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<IssueDto>> GetIssuesAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        var issues = await _apiClient.GetRepositoryIssuesAsync(owner, repo, cancellationToken);

        return issues
            .Where(i => i.PullRequest is null)
            .Select(ToIssueDto)
            .ToList()
            .AsReadOnly();
    }

    public async Task<CreateIssueResponse> CreateIssueAsync(CreateIssueRequest request, CancellationToken cancellationToken = default)
    {
        var issue = await _apiClient.CreateIssueAsync(request.Owner, request.Repo, request.Title, request.Body, cancellationToken);
        return ToCreateIssueResponse(issue);
    }

    public async Task<IReadOnlyList<CommitDto>> GetCommitsAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        var commits = await _apiClient.GetRepositoryCommitsAsync(owner, repo, cancellationToken);
        return commits.Select(ToCommitDto).ToList().AsReadOnly();
    }

    public async Task<PullRequestDto> CreatePullRequestAsync(CreatePullRequestRequest request, CancellationToken cancellationToken = default)
    {
        var pr = await _apiClient.CreatePullRequestAsync(
            request.Owner, request.Repo, request.Title,
            request.Head, request.Base, request.Body, request.Draft, cancellationToken);
        return ToPullRequestDto(pr);
    }

    private static RepositoryDto ToRepositoryDto(GitHubRepositoryModel model) =>
        new(
            model.Id,
            model.Name,
            model.FullName,
            model.Description,
            model.HtmlUrl,
            model.Language,
            model.StargazersCount,
            model.ForksCount,
            model.Private,
            model.CreatedAt,
            model.UpdatedAt
        );

    private static IssueDto ToIssueDto(GitHubIssueModel model) =>
        new(
            model.Id,
            model.Number,
            model.Title,
            model.Body,
            model.State,
            model.HtmlUrl,
            model.User?.Login ?? string.Empty,
            model.Labels.Select(l => l.Name).ToList().AsReadOnly(),
            model.CreatedAt,
            model.UpdatedAt
        );

    private static CreateIssueResponse ToCreateIssueResponse(GitHubIssueModel model) =>
        new(
            model.Id,
            model.Number,
            model.Title,
            model.Body,
            model.State,
            model.HtmlUrl
        );

    private static CommitDto ToCommitDto(GitHubCommitModel model) =>
        new(
            model.Sha,
            model.Commit?.Message ?? string.Empty,
            model.Commit?.Author?.Name ?? string.Empty,
            model.Commit?.Author?.Email ?? string.Empty,
            model.Author?.Login,
            model.Commit?.Author?.Date ?? default,
            model.HtmlUrl
        );

    private static PullRequestDto ToPullRequestDto(GitHubPullRequestModel model) =>
        new(
            model.Id,
            model.Number,
            model.Title,
            model.Body,
            model.State,
            model.HtmlUrl,
            model.User?.Login ?? string.Empty,
            model.Head?.Ref ?? string.Empty,
            model.Base?.Ref ?? string.Empty,
            model.Draft,
            model.Merged,
            model.CreatedAt,
            model.UpdatedAt
        );
}
