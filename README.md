# GitHub Cloud Connector

A production-style ASP.NET Core 8 Web API that integrates with the GitHub REST API using Personal Access Token authentication. Supports fetching repositories, listing issues, and creating issues across user accounts and organizations.

---

## Features

- Authenticate with GitHub using a PAT stored in configuration, never in source code
- Fetch repositories for a GitHub user or organization
- List open issues for any repository (pull requests are automatically excluded)
- Create a new issue in any repository you have write access to
- Centralized error handling with accurate HTTP status codes
- Clean layered architecture with typed HttpClient and the Options pattern

---

## Tech Stack

- .NET 8 / ASP.NET Core Web API
- System.Text.Json
- FluentValidation
- Typed HttpClient
- Swashbuckle (Swagger UI)

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A GitHub Personal Access Token with `repo` scope (or `public_repo` for public repositories only)

---

## Configuration

The API reads the GitHub token from `GitHub:Token` in configuration. The token must never be placed in `appsettings.json` or committed to source control.

### Option 1: .NET User Secrets (recommended for local development)

```bash
cd "Github Cloud Connector"
dotnet user-secrets init
dotnet user-secrets set "GitHub:Token" "ghp_your_token_here"
```

### Option 2: Environment Variable

```bash
# Linux / macOS
export GitHub__Token="ghp_your_token_here"

# Windows (PowerShell)
$env:GitHub__Token = "ghp_your_token_here"

# Windows (Command Prompt)
set GitHub__Token=ghp_your_token_here
```

Note the double underscore (`__`) when using environment variables to represent the `:` hierarchy separator.

### appsettings.json (non-sensitive defaults only)

```json
{
  "GitHub": {
    "BaseUrl": "https://api.github.com",
    "Token": ""
  }
}
```

Do not put a real token here.

---

## Running Locally

```bash
dotnet run
```

The API starts at `http://localhost:5000`. In Development mode, Swagger UI is available at:

```
http://localhost:5000/swagger
```

---

## API Endpoints

### GET /api/github/repos

Retrieve repositories for a GitHub user or organization.

| Parameter | Type   | Required | Description                     |
|-----------|--------|----------|---------------------------------|
| owner     | string | Yes      | GitHub username or org name     |
| type      | string | No       | `user` (default) or `org`       |

**Request**

```
GET /api/github/repos?owner=torvalds&type=user
```

**Response** `200 OK`

```json
[
  {
    "id": 2325298,
    "name": "linux",
    "fullName": "torvalds/linux",
    "description": "Linux kernel source tree",
    "htmlUrl": "https://github.com/torvalds/linux",
    "language": "C",
    "stars": 185000,
    "forks": 54000,
    "isPrivate": false,
    "createdAt": "2011-09-04T22:48:12Z",
    "updatedAt": "2024-01-15T10:22:00Z"
  }
]
```

---

### GET /api/github/issues/{owner}/{repo}

Retrieve open issues for a repository. Pull requests are excluded from the results.

**Request**

```
GET /api/github/issues/microsoft/vscode
```

**Response** `200 OK`

```json
[
  {
    "id": 2134567890,
    "number": 201234,
    "title": "Terminal: cursor blinks after focus loss",
    "body": "Steps to reproduce...",
    "state": "open",
    "htmlUrl": "https://github.com/microsoft/vscode/issues/201234",
    "authorLogin": "user123",
    "labels": ["bug", "terminal"],
    "createdAt": "2024-01-10T08:30:00Z",
    "updatedAt": "2024-01-14T17:00:00Z"
  }
]
```

---

### POST /api/github/issues

Create a new issue in a repository.

**Request body**

```json
{
  "owner": "your-username",
  "repo": "your-repo",
  "title": "Something is broken",
  "body": "Here is a detailed description of the problem."
}
```

The `body` field is optional. All other fields are required.

**Response** `201 Created`

```json
{
  "id": 2234567890,
  "number": 42,
  "title": "Something is broken",
  "body": "Here is a detailed description of the problem.",
  "state": "open",
  "htmlUrl": "https://github.com/your-username/your-repo/issues/42"
}
```

---

## Error Handling

All error responses follow a consistent structure:

```json
{
  "message": "A descriptive error message."
}
```

| Status | Cause                                                                  |
|--------|------------------------------------------------------------------------|
| 400    | Missing or invalid request parameters                                  |
| 401    | GitHub token is missing, expired, or invalid                           |
| 403    | Token lacks required permissions, or the GitHub rate limit is exceeded |
| 404    | The specified user, organization, or repository does not exist         |
| 422    | GitHub rejected the issue payload (e.g., issues disabled on the repo)  |
| 500    | Unexpected server-side failure                                         |

---

## Design Decisions

**Layered architecture** — Controllers handle HTTP binding and response shaping. Services coordinate business logic and mapping. The API client handles all raw GitHub communication. Each layer has a single, well-defined responsibility.

**Typed HttpClient** — `GitHubApiClient` is registered as a typed client via `IHttpClientFactory`. All HTTP configuration (base URL, auth header, versioning headers) is centralized in the registration. This keeps the client class clean and makes it straightforward to mock in tests.

**Options pattern** — `GitHubOptions` binds `GitHub:Token` and `GitHub:BaseUrl` from configuration. Using `IOptions<GitHubOptions>` keeps credential access explicit and out of the application logic.

**Global exception middleware** — `GlobalExceptionMiddleware` is a single catch-all that translates `GitHubApiException` to the correct HTTP status code and ensures internal details are never exposed in responses. Unexpected exceptions produce a 500 with a safe generic message.

**FluentValidation** — Validation for `CreateIssueRequest` is defined in a dedicated validator class. The controller calls it explicitly, keeping validation decoupled from model annotations and controller logic.

**PR filtering** — The GitHub Issues API returns pull requests in its results. The service layer filters them out by checking for the presence of the `pull_request` field, so callers always receive genuine issues only.

---

## Generating a GitHub PAT

1. Go to GitHub Settings > Developer settings > Personal access tokens > Tokens (classic)
2. Click "Generate new token"
3. Select the `repo` scope for full repository access, or `public_repo` for public repositories only
4. Copy the token immediately; it will not be shown again
5. Configure it via user secrets or an environment variable as described above
