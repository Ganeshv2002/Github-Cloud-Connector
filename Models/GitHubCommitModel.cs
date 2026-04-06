using System.Text.Json.Serialization;

namespace GithubCloudConnector.Models;

public class GitHubCommitModel
{
    [JsonPropertyName("sha")]
    public string Sha { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("commit")]
    public GitHubCommitDetailModel? Commit { get; set; }

    [JsonPropertyName("author")]
    public GitHubUserModel? Author { get; set; }
}

public class GitHubCommitDetailModel
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public GitHubCommitAuthorModel? Author { get; set; }
}

public class GitHubCommitAuthorModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }
}
