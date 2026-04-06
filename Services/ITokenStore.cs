namespace GithubCloudConnector.Services;

public interface ITokenStore
{
    string? GetToken();
    void SetToken(string token);
}
