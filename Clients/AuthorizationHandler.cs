using System.Net.Http.Headers;
using GithubCloudConnector.Configuration;
using GithubCloudConnector.Services;
using Microsoft.Extensions.Options;

namespace GithubCloudConnector.Clients;

public class AuthorizationHandler : DelegatingHandler
{
    private readonly ITokenStore _tokenStore;
    private readonly IOptions<GitHubOptions> _options;

    public AuthorizationHandler(ITokenStore tokenStore, IOptions<GitHubOptions> options)
    {
        _tokenStore = tokenStore;
        _options = options;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _tokenStore.GetToken() ?? _options.Value.Token;

        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
