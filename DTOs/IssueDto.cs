namespace GithubCloudConnector.DTOs;

public record IssueDto(
    long Id,
    int Number,
    string Title,
    string? Body,
    string State,
    string HtmlUrl,
    string AuthorLogin,
    IReadOnlyList<string> Labels,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
