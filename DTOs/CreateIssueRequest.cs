namespace GithubCloudConnector.DTOs;

public record CreateIssueRequest(
    string Owner,
    string Repo,
    string Title,
    string? Body
);
