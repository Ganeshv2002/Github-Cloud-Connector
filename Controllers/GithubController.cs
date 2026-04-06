using FluentValidation;
using GithubCloudConnector.DTOs;
using GithubCloudConnector.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GithubCloudConnector.Controllers;

[ApiController]
[Route("api/github")]
[Authorize]
public class GithubController : ControllerBase
{
    private readonly IGitHubService _gitHubService;
    private readonly IValidator<CreateIssueRequest> _issueValidator;
    private readonly IValidator<CreatePullRequestRequest> _prValidator;

    public GithubController(
        IGitHubService gitHubService,
        IValidator<CreateIssueRequest> issueValidator,
        IValidator<CreatePullRequestRequest> prValidator)
    {
        _gitHubService = gitHubService;
        _issueValidator = issueValidator;
        _prValidator = prValidator;
    }

    [HttpGet("repos")]
    public async Task<IActionResult> GetRepositories(
        [FromQuery] string owner,
        [FromQuery] string type = "user",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(owner))
            return BadRequest(new ErrorResponse("The 'owner' query parameter is required."));

        if (!type.Equals("user", StringComparison.OrdinalIgnoreCase) &&
            !type.Equals("org", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new ErrorResponse("The 'type' parameter must be 'user' or 'org'."));

        var repositories = await _gitHubService.GetRepositoriesAsync(owner, type, cancellationToken);
        return Ok(repositories);
    }

    [HttpGet("issues/{owner}/{repo}")]
    public async Task<IActionResult> GetIssues(
        string owner,
        string repo,
        CancellationToken cancellationToken = default)
    {
        var issues = await _gitHubService.GetIssuesAsync(owner, repo, cancellationToken);
        return Ok(issues);
    }

    [HttpPost("issues")]
    public async Task<IActionResult> CreateIssue(
        [FromBody] CreateIssueRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _issueValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var message = string.Join(" ", validation.Errors.Select(e => e.ErrorMessage));
            return BadRequest(new ErrorResponse(message));
        }

        var result = await _gitHubService.CreateIssueAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetIssues), new { owner = request.Owner, repo = request.Repo }, result);
    }

    [HttpGet("commits/{owner}/{repo}")]
    public async Task<IActionResult> GetCommits(
        string owner,
        string repo,
        CancellationToken cancellationToken = default)
    {
        var commits = await _gitHubService.GetCommitsAsync(owner, repo, cancellationToken);
        return Ok(commits);
    }

    [HttpPost("pulls")]
    public async Task<IActionResult> CreatePullRequest(
        [FromBody] CreatePullRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _prValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var message = string.Join(" ", validation.Errors.Select(e => e.ErrorMessage));
            return BadRequest(new ErrorResponse(message));
        }

        var result = await _gitHubService.CreatePullRequestAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetIssues), new { owner = request.Owner, repo = request.Repo }, result);
    }
}
