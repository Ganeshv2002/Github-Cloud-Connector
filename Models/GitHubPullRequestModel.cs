using System.Text.Json.Serialization;

namespace GithubCloudConnector.Models;

public class GitHubPullRequestModel
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("user")]
    public GitHubUserModel? User { get; set; }

    [JsonPropertyName("head")]
    public GitHubRefModel? Head { get; set; }

    [JsonPropertyName("base")]
    public GitHubRefModel? Base { get; set; }

    [JsonPropertyName("draft")]
    public bool Draft { get; set; }

    [JsonPropertyName("merged")]
    public bool Merged { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
