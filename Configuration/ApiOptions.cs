namespace GithubCloudConnector.Configuration;

public class ApiOptions
{
    public const string SectionName = "Api";

    public string Key { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "http://localhost:5000";
}
