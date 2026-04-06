namespace GithubCloudConnector.DTOs;

public record CreateIssueResponse(
    long Id,
    int Number,
    string Title,
    string? Body,
    string State,
    string HtmlUrl
);
