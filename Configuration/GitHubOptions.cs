namespace GithubCloudConnector.Configuration;

public class GitHubOptions
{
    public const string SectionName = "GitHub";

    public string Token { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.github.com";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = "http://localhost:5000/api/auth/callback";
}
