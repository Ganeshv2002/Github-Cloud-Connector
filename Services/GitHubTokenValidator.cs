using System.Net.Http.Headers;

namespace GithubCloudConnector.Services;

public class GitHubTokenValidator : IGitHubTokenValidator
{
    private readonly IHttpClientFactory _httpClientFactory;

    public GitHubTokenValidator(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> ValidateAsync(string token, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("github-validator");
        using var request = new HttpRequestMessage(HttpMethod.Get, "user");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
