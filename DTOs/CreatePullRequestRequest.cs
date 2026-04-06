namespace GithubCloudConnector.DTOs;

public record CreatePullRequestRequest(
    string Owner,
    string Repo,
    string Title,
    string Head,
    string Base,
    string? Body,
    bool Draft = false
);
