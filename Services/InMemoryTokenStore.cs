namespace GithubCloudConnector.Services;

public class InMemoryTokenStore : ITokenStore
{
    private volatile string? _token;

    public string? GetToken() => _token;

    public void SetToken(string token) => _token = token;
}
