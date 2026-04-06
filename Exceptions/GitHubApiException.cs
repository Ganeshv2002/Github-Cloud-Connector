using System.Net;

namespace GithubCloudConnector.Exceptions;

public class GitHubApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public GitHubApiException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
