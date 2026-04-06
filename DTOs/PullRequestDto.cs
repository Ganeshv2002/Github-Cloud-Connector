namespace GithubCloudConnector.DTOs;

public record PullRequestDto(
    long Id,
    int Number,
    string Title,
    string? Body,
    string State,
    string HtmlUrl,
    string AuthorLogin,
    string HeadBranch,
    string BaseBranch,
    bool IsDraft,
    bool IsMerged,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
