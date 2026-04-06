using System.Text.Json.Serialization;

namespace GithubCloudConnector.Models;

public class GitHubLabelModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
