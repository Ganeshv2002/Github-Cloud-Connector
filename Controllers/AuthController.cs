using System.Net.Http.Json;
using System.Security.Cryptography;
using GithubCloudConnector.Configuration;
using GithubCloudConnector.DTOs;
using GithubCloudConnector.Models;
using GithubCloudConnector.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GithubCloudConnector.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IOptions<GitHubOptions> _options;
    private readonly ITokenStore _tokenStore;
    private readonly IMemoryCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(
        IOptions<GitHubOptions> options,
        ITokenStore tokenStore,
        IMemoryCache cache,
        IHttpClientFactory httpClientFactory)
    {
        _options = options;
        _tokenStore = tokenStore;
        _cache = cache;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var opts = _options.Value;

        if (string.IsNullOrWhiteSpace(opts.ClientId))
            return BadRequest(new ErrorResponse("OAuth is not configured. Set GitHub:ClientId and GitHub:ClientSecret in configuration."));

        var state = GenerateState();
        _cache.Set(state, true, TimeSpan.FromMinutes(5));

        var redirectUri = Uri.EscapeDataString(opts.RedirectUri);
        var authUrl = $"https://github.com/login/oauth/authorize?client_id={opts.ClientId}&redirect_uri={redirectUri}&scope=repo&state={state}";

        return Redirect(authUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
            return BadRequest(new ErrorResponse("Missing code or state parameter from GitHub."));

        if (!_cache.TryGetValue(state, out _))
            return BadRequest(new ErrorResponse("Invalid or expired OAuth state. Please restart the login flow."));

        _cache.Remove(state);

        var opts = _options.Value;
        var client = _httpClientFactory.CreateClient("github-oauth");

        var payload = new
        {
            client_id = opts.ClientId,
            client_secret = opts.ClientSecret,
            code,
            redirect_uri = opts.RedirectUri
        };

        var response = await client.PostAsJsonAsync("login/oauth/access_token", payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return StatusCode(502, new ErrorResponse("Failed to exchange the authorization code with GitHub."));

        var tokenResponse = await response.Content.ReadFromJsonAsync<GitHubTokenResponse>(cancellationToken);

        if (tokenResponse is null || !string.IsNullOrWhiteSpace(tokenResponse.Error))
            return StatusCode(502, new ErrorResponse(tokenResponse?.ErrorDescription ?? tokenResponse?.Error ?? "GitHub returned an invalid token response."));

        if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            return StatusCode(502, new ErrorResponse("GitHub returned an empty access token."));

        _tokenStore.SetToken(tokenResponse.AccessToken);

        return Ok(new AuthStatusDto("Authenticated successfully via OAuth. You can now use the API.", tokenResponse.Scope));
    }

    [HttpGet("status")]
    public IActionResult Status()
    {
        if (_tokenStore.GetToken() is not null)
            return Ok(new AuthStatusDto("Authenticated via OAuth."));

        if (!string.IsNullOrWhiteSpace(_options.Value.Token))
            return Ok(new AuthStatusDto("Authenticated via Personal Access Token."));

        return Unauthorized(new ErrorResponse("Not authenticated. Visit /api/auth/login to begin the OAuth flow, or configure a PAT via GitHub:Token."));
    }

    private static string GenerateState() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> ExchangeToken(
        [FromForm] string? code,
        [FromForm(Name = "redirect_uri")] string? redirectUri,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest(new ErrorResponse("Missing code parameter."));

        var opts = _options.Value;

        if (string.IsNullOrWhiteSpace(opts.ClientId) || string.IsNullOrWhiteSpace(opts.ClientSecret))
            return BadRequest(new ErrorResponse("OAuth is not configured. Set GitHub:ClientId and GitHub:ClientSecret."));

        var client = _httpClientFactory.CreateClient("github-oauth");

        var payload = new
        {
            client_id = opts.ClientId,
            client_secret = opts.ClientSecret,
            code,
            redirect_uri = redirectUri
        };

        var response = await client.PostAsJsonAsync("login/oauth/access_token", payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return StatusCode(502, new ErrorResponse("Failed to exchange the authorization code with GitHub."));

        var tokenResponse = await response.Content.ReadFromJsonAsync<GitHubTokenResponse>(cancellationToken);

        if (tokenResponse is null || !string.IsNullOrWhiteSpace(tokenResponse.Error))
            return StatusCode(502, new ErrorResponse(tokenResponse?.ErrorDescription ?? tokenResponse?.Error ?? "Token exchange failed."));

        return Ok(new
        {
            access_token = tokenResponse.AccessToken,
            token_type = "bearer",
            scope = tokenResponse.Scope
        });
    }
}
