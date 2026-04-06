using GithubCloudConnector.Extensions;
using GithubCloudConnector.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithSecurity(builder.Configuration);
builder.Services.AddMemoryCache();

builder.Services.AddGitHubServices(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options => options.RouteTemplate = "api/swagger/{documentName}/swagger.json");
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/api/swagger/v1/swagger.json", "GitHub Cloud Connector v1");
        options.RoutePrefix = "api/swagger";
        options.OAuthClientId(builder.Configuration.GetValue<string>("GitHub:ClientId") ?? string.Empty);
    });
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
