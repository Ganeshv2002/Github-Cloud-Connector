namespace GithubCloudConnector.Services;

public interface IGitHubTokenValidator
{
    Task<bool> ValidateAsync(string token, CancellationToken cancellationToken = default);
}
