namespace GithubCloudConnector.DTOs;

public record RepositoryDto(
    long Id,
    string Name,
    string FullName,
    string? Description,
    string HtmlUrl,
    string? Language,
    int Stars,
    int Forks,
    bool IsPrivate,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
