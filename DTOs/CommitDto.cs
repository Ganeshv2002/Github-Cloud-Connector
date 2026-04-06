namespace GithubCloudConnector.DTOs;

public record CommitDto(
    string Sha,
    string Message,
    string AuthorName,
    string AuthorEmail,
    string? GitHubLogin,
    DateTime CommittedAt,
    string HtmlUrl
);
