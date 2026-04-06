using System.Text.Json.Serialization;

namespace GithubCloudConnector.Models;

public class GitHubUserModel
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;
}
