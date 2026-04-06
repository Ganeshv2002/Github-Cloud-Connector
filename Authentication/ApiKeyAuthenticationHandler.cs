using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using GithubCloudConnector.Configuration;
using GithubCloudConnector.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace GithubCloudConnector.Authentication;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IOptions<ApiOptions> _apiOptions;
    private readonly IGitHubTokenValidator _tokenValidator;

    public ApiKeyAuthenticationHandler(
        IOptions<ApiOptions> apiOptions,
        IGitHubTokenValidator tokenValidator,
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        _apiOptions = apiOptions;
        _tokenValidator = tokenValidator;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return AuthenticateResult.Fail("Missing Authorization header.");

        var headerValue = authHeader.ToString();

        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.Fail("Invalid authorization scheme. Expected Bearer.");

        var token = headerValue["Bearer ".Length..].Trim();
        var configuredKey = _apiOptions.Value.Key;

        if (!string.IsNullOrWhiteSpace(configuredKey) && ConstantTimeEquals(token, configuredKey))
            return AuthenticateResult.Success(CreateTicket());

        if (await _tokenValidator.ValidateAsync(token, Context.RequestAborted))
            return AuthenticateResult.Success(CreateTicket());

        return AuthenticateResult.Fail("Invalid credentials.");
    }

    private AuthenticationTicket CreateTicket()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "api-client") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        return new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
    }

    private static bool ConstantTimeEquals(string a, string b)
    {
        var aHash = SHA256.HashData(Encoding.UTF8.GetBytes(a));
        var bHash = SHA256.HashData(Encoding.UTF8.GetBytes(b));
        return CryptographicOperations.FixedTimeEquals(aHash, bHash);
    }
}
