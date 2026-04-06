using FluentValidation;
using GithubCloudConnector.Authentication;
using GithubCloudConnector.Clients;
using GithubCloudConnector.Configuration;
using GithubCloudConnector.Services;
using GithubCloudConnector.Validators;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace GithubCloudConnector.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGitHubServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<GitHubOptions>()
            .Bind(configuration.GetSection(GitHubOptions.SectionName));

        services.AddSingleton<ITokenStore, InMemoryTokenStore>();
        services.AddTransient<AuthorizationHandler>();

        services.AddHttpClient<IGitHubApiClient, GitHubApiClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<GitHubOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            client.DefaultRequestHeaders.Add("User-Agent", "GithubCloudConnector/1.0");
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
        })
        .AddHttpMessageHandler<AuthorizationHandler>();

        services.AddHttpClient("github-oauth", client =>
        {
            client.BaseAddress = new Uri("https://github.com/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "GithubCloudConnector/1.0");
        });

        services.AddHttpClient("github-validator", client =>
        {
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            client.DefaultRequestHeaders.Add("User-Agent", "GithubCloudConnector/1.0");
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
        });

        services.AddScoped<IGitHubService, GitHubService>();
        services.AddScoped<IGitHubTokenValidator, GitHubTokenValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateIssueRequestValidator>();

        return services;
    }

    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ApiOptions>()
            .Bind(configuration.GetSection(ApiOptions.SectionName));

        services.AddAuthentication("ApiKey")
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddSwaggerWithSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        var apiBaseUrl = configuration.GetValue<string>("Api:BaseUrl") ?? "http://localhost:5000";

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "ApiKey",
                In = ParameterLocation.Header,
                Description = "Enter your API key as a Bearer token. Example: mysecretkey123"
            });

            options.AddSecurityDefinition("GitHub OAuth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri("https://github.com/login/oauth/authorize"),
                        TokenUrl = new Uri($"{apiBaseUrl.TrimEnd('/')}/api/auth/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "repo", "Full repository access" },
                            { "read:user", "Read user profile" }
                        }
                    }
                }
            });

            var bearerScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            };

            var oauthScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "GitHub OAuth2" }
            };

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { bearerScheme, Array.Empty<string>() }
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { oauthScheme, new[] { "repo" } }
            });
        });

        return services;
    }
}
