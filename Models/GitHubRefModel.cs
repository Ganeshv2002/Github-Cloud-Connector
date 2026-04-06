using System.Text.Json.Serialization;

namespace GithubCloudConnector.Models;

public class GitHubRefModel
{
    [JsonPropertyName("ref")]
    public string Ref { get; set; } = string.Empty;

    [JsonPropertyName("sha")]
    public string Sha { get; set; } = string.Empty;
}
