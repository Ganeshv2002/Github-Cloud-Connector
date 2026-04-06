using System.Net;
using System.Net.Http.Json;
using GithubCloudConnector.Exceptions;
using GithubCloudConnector.Models;

namespace GithubCloudConnector.Clients;

public class GitHubApiClient : IGitHubApiClient
{
    private readonly HttpClient _httpClient;

    public GitHubApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<GitHubRepositoryModel>> GetUserRepositoriesAsync(string username, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"users/{username}/repos?per_page=100&sort=updated", cancellationToken);
        EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<List<GitHubRepositoryModel>>(cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<GitHubRepositoryModel>> GetOrgRepositoriesAsync(string org, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"orgs/{org}/repos?per_page=100&sort=updated", cancellationToken);
        EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<List<GitHubRepositoryModel>>(cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<GitHubIssueModel>> GetRepositoryIssuesAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"repos/{owner}/{repo}/issues?per_page=100&state=open", cancellationToken);
        EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<List<GitHubIssueModel>>(cancellationToken) ?? [];
    }

    public async Task<GitHubIssueModel> CreateIssueAsync(string owner, string repo, string title, string? body, CancellationToken cancellationToken = default)
    {
        var payload = new { title, body };
        var response = await _httpClient.PostAsJsonAsync($"repos/{owner}/{repo}/issues", payload, cancellationToken);
        EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<GitHubIssueModel>(cancellationToken)
            ?? throw new GitHubApiException(HttpStatusCode.InternalServerError, "Received an empty response from the GitHub API.");
    }

    public async Task<IReadOnlyList<GitHubCommitModel>> GetRepositoryCommitsAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"repos/{owner}/{repo}/commits?per_page=100", cancellationToken);
        EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<List<GitHubCommitModel>>(cancellationToken) ?? [];
    }

    public async Task<GitHubPullRequestModel> CreatePullRequestAsync(string owner, string repo, string title, string head, string baseBranch, string? body, bool draft, CancellationToken cancellationToken = default)
    {
        var payload = new { title, head, @base = baseBranch, body, draft };
        var response = await _httpClient.PostAsJsonAsync($"repos/{owner}/{repo}/pulls", payload, cancellationToken);
        EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<GitHubPullRequestModel>(cancellationToken)
            ?? throw new GitHubApiException(HttpStatusCode.InternalServerError, "Received an empty response from the GitHub API.");
    }

    private static void EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new GitHubApiException(
                HttpStatusCode.Unauthorized,
                "Authentication failed. Verify that the GitHub token is configured and valid."),

            HttpStatusCode.Forbidden => new GitHubApiException(
                HttpStatusCode.Forbidden,
                "Access denied. The token may lack required permissions or the rate limit has been exceeded."),

            HttpStatusCode.NotFound => new GitHubApiException(
                HttpStatusCode.NotFound,
                "The requested resource was not found on GitHub."),

            HttpStatusCode.UnprocessableEntity => new GitHubApiException(
                HttpStatusCode.UnprocessableEntity,
                "GitHub rejected the request. Ensure the owner, repository, and title are valid."),

            _ => new GitHubApiException(
                HttpStatusCode.InternalServerError,
                $"GitHub API returned an unexpected error ({(int)response.StatusCode}).")
        };
    }
}
