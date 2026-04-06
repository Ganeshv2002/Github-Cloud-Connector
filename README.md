# GitHub Cloud Connector

A production-style ASP.NET Core 8 Web API that integrates with the GitHub REST API. Supports two authentication modes — Personal Access Token (PAT) and GitHub OAuth 2.0 Authorization Code flow. Covers repositories, issues, commits, and pull requests across user accounts and organizations.

---

## Features

- Authenticate with GitHub using a PAT stored in user secrets, never in source code
- Authenticate interactively via GitHub OAuth 2.0 — authorize directly from the Swagger UI
- Fetch repositories for a GitHub user or organization
- List open issues for any repository (pull requests are automatically excluded)
- Create a new issue in any repository you have write access to
- List commits for any repository
- Create pull requests
- API-level Bearer token protection on all GitHub endpoints
- Centralized error handling with accurate HTTP status codes
- Clean layered architecture with typed HttpClient and the Options pattern

---

## Tech Stack

- .NET 8 / ASP.NET Core Web API
- System.Text.Json
- FluentValidation
- Typed HttpClient (`IHttpClientFactory`)
- Swashbuckle (Swagger UI)
- `IMemoryCache` (OAuth CSRF state protection)

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A GitHub account
- A GitHub Personal Access Token with `repo` scope for PAT authentication (optional if using OAuth)
- A GitHub OAuth App registered for the OAuth 2.0 flow (optional if using PAT only)

---

## Quick Start

The minimum steps to get the API running locally with PAT authentication:

```powershell
# 1. Clone the repository
git clone https://github.com/your-username/github-cloud-connector.git
cd "github-cloud-connector"

# 2. Initialize user secrets (once per machine)
dotnet user-secrets init

# 3. Set required secrets
dotnet user-secrets set "GitHub:Token" "ghp_your_github_pat_here"
dotnet user-secrets set "Api:Key" "any-strong-secret-you-choose"

# 4. Trust the HTTPS development certificate (once per machine)
dotnet dev-certs https --trust

# 5. Run
dotnet run
```

Then open `https://localhost:5000/api/swagger` in your browser, click **Authorize**, enter your API key, and start calling endpoints.

For GitHub OAuth 2.0 setup, see the [OAuth 2.0 Authentication](#oauth-20-authentication) section below.

---

## Configuration

All secrets are stored outside of `appsettings.json` using .NET User Secrets or environment variables. Never commit tokens, client IDs, or client secrets to source control.

---

### PAT Authentication

The simplest way to authenticate. Provides a static token used for all GitHub API calls.

**User Secrets (recommended)**

```powershell
dotnet user-secrets init
dotnet user-secrets set "GitHub:Token" "ghp_your_token_here"
```

> Run `dotnet user-secrets init` only once per project. Skip it if you already have a `UserSecretsId` in the `.csproj`.

**Environment Variable**

```powershell
# Windows (PowerShell)
$env:GitHub__Token = "ghp_your_token_here"
```

Note the double underscore (`__`) as the hierarchy separator when using environment variables.

---

### OAuth 2.0 Authentication

Allows users to authorize the API through GitHub's login page. Credentials are obtained interactively and stored in memory for the session. The Swagger UI has an **Authorize** button wired to this flow.

#### Step 1 — Register a GitHub OAuth App

1. Go to [GitHub Settings > Developer settings > OAuth Apps](https://github.com/settings/developers)
2. Click **New OAuth App**
3. Fill in the fields:

   | Field | Value |
   |---|---|
   | Application name | GitHub Cloud Connector (or any name) |
   | Homepage URL | `https://localhost:5000` |
   | Authorization callback URL | `https://localhost:5000/api/swagger/oauth2-redirect.html` |

   > If you also want to use the direct `/api/auth/login` browser flow, add a second callback URL: `https://localhost:5000/api/auth/callback`. GitHub OAuth Apps support multiple callback URLs.

4. Click **Register application**
5. On the next page, note the **Client ID** and click **Generate a new client secret** to obtain the **Client Secret**

#### Step 2 — Set the OAuth credentials via User Secrets

```powershell
dotnet user-secrets set "GitHub:ClientId" "your-client-id"
dotnet user-secrets set "GitHub:ClientSecret" "your-client-secret"
```

#### Step 3 — Restart the application

User secrets are read at startup. If the app is already running, stop it and run it again.

---

### API Key (Bearer token for endpoint protection)

All `/api/github/*` endpoints require a Bearer token in the `Authorization` header. This is your own API key used to protect the gateway — not a GitHub credential. Set it via user secrets:

```powershell
dotnet user-secrets set "Api:Key" "your-api-key-here"
```

Choose any strong secret string. You will pass this as `Authorization: Bearer your-api-key-here` when calling the API directly, or enter it in the Swagger **Authorize** dialog.

> The authentication handler also accepts a valid **GitHub OAuth access token** directly as the Bearer value. If the token does not match the configured API key, it is validated against the GitHub `/user` endpoint as a fallback. This means you can also use the OAuth flow and pass the resulting GitHub token directly to the API.

---

### appsettings.json (non-sensitive defaults only)

```json
{
  "GitHub": {
    "BaseUrl": "https://api.github.com",
    "Token": "",
    "ClientId": "",
    "ClientSecret": "",
    "RedirectUri": "https://localhost:5000/api/auth/callback"
  },
  "Api": {
    "Key": "",
    "BaseUrl": "https://localhost:5000"
  }
}
```

Leave all secret fields empty here. They are populated at runtime via user secrets.

---

## Running Locally

```powershell
dotnet run
```

The API starts at `https://localhost:5000`. In Development mode, Swagger UI is available at:

```
https://localhost:5000/api/swagger
```

If your browser shows a certificate warning on first launch, run the following once to trust the development certificate, then restart the browser:

```powershell
dotnet dev-certs https --trust
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

### GET /api/github/commits/{owner}/{repo}

List commits for a repository.

**Request**

```
GET /api/github/commits/microsoft/vscode
```

**Response** `200 OK`

```json
[
  {
    "sha": "a1b2c3d4e5f6...",
    "message": "Fix terminal cursor regression",
    "authorName": "contributor",
    "authorEmail": "contributor@example.com",
    "gitHubLogin": "contributor",
    "committedAt": "2024-01-14T17:00:00Z",
    "htmlUrl": "https://github.com/microsoft/vscode/commit/a1b2c3d4e5f6"
  }
]
```

`gitHubLogin` is the GitHub username of the committer and may be `null` for commits made outside of GitHub.

---

### POST /api/github/pulls

Create a pull request in a repository.

**Request body**

```json
{
  "owner": "your-username",
  "repo": "your-repo",
  "title": "Add new feature",
  "body": "Description of the changes.",
  "head": "feature-branch",
  "base": "main",
  "draft": false
}
```

`body` and `draft` are optional. All other fields are required. `head` is the branch with your changes; `base` is the branch you want to merge into.

**Response** `201 Created`

```json
{
  "id": 1234567890,
  "number": 7,
  "title": "Add new feature",
  "body": "Description of the changes.",
  "state": "open",
  "htmlUrl": "https://github.com/your-username/your-repo/pull/7",
  "authorLogin": "your-username",
  "headBranch": "feature-branch",
  "baseBranch": "main",
  "isDraft": false,
  "isMerged": false,
  "createdAt": "2024-01-14T17:00:00Z",
  "updatedAt": "2024-01-14T17:00:00Z"
}
```

---

### GET /api/auth/login

Redirects the browser to GitHub's OAuth authorization page. Use this for the interactive login flow outside of Swagger.

---

### GET /api/auth/callback

GitHub redirects back here after the user authorizes the app. The authorization code is exchanged for an access token, which is stored in memory for the session. This endpoint is called automatically by GitHub — you do not call it directly.

---

### GET /api/auth/status

Check whether the API is currently authenticated and by which method.

**Response** `200 OK`

```json
{
  "message": "Authenticated via OAuth.",
  "scope": "repo"
}
```

---

## Testing with Swagger UI

The API ships with Swagger UI at `https://localhost:5000/api/swagger`. All endpoints are listed and executable from the browser — no external tools needed.

### Step 1 — Open Swagger

Start the app and navigate to:

```
https://localhost:5000/api/swagger
```

### Step 2 — Authorize

Click the **Authorize** button (lock icon, top-right of the Swagger page). You will see two authentication options:

#### Option A — Bearer (API Key or PAT)

Enter your API key in the `BearerAuth` field:

```
your-api-key-here
```

This is the value you set for `Api:Key` in user secrets. Alternatively you can enter a raw GitHub PAT here if you have not set up an API key.

Click **Authorize**, then **Close**.

#### Option B — GitHub OAuth 2.0

1. Under **GitHub OAuth2**, click **Authorize**
2. You will be redirected to GitHub's login and authorization page
3. After you approve, GitHub redirects back to Swagger and the token is stored automatically
4. Click **Close**

> OAuth 2.0 requires `GitHub:ClientId` and `GitHub:ClientSecret` to be set. See the Configuration section above.

### Step 3 — Call an endpoint

1. Expand any endpoint, for example `GET /api/github/repos`
2. Click **Try it out**
3. Fill in the parameters (e.g. `owner = torvalds`, `type = user`)
4. Click **Execute**
5. The response body and status code appear below

### Step 4 — Check auth status

To verify which authentication method is active, expand `GET /api/auth/status` and execute it. No authorization header is needed for this endpoint.

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
| 502    | GitHub returned a non-success response during OAuth token exchange     |

---

## Design Decisions

**Layered architecture** — Controllers handle HTTP binding and response shaping. Services coordinate business logic and mapping. The API client handles all raw GitHub communication. Each layer has a single, well-defined responsibility.

**Typed HttpClient** — `GitHubApiClient` is registered as a typed client via `IHttpClientFactory`. All HTTP configuration (base URL, auth header, versioning headers) is centralized in the registration. This keeps the client class clean and makes it straightforward to mock in tests.

**Options pattern** — `GitHubOptions` binds `GitHub:Token`, `GitHub:BaseUrl`, `GitHub:ClientId`, `GitHub:ClientSecret`, and `GitHub:RedirectUri` from configuration. `ApiOptions` binds `Api:Key` and `Api:BaseUrl`. Using `IOptions<T>` keeps credential access explicit and out of application logic.

**DelegatingHandler for outbound auth** — `AuthorizationHandler` is an `HttpMessageHandler` that intercepts every outbound request to the GitHub API and injects the `Authorization: Bearer` header automatically. It checks the `ITokenStore` for an OAuth token first and falls back to the PAT. Neither the client nor the service needs to know how authentication works.

**OAuth 2.0 Authorization Code flow** — `AuthController` implements the full GitHub OAuth flow: CSRF-protected redirect, code exchange (server-side, keeping `client_secret` out of the browser), and token storage. `InMemoryTokenStore` holds the live access token as a volatile field on a singleton. The Swagger `POST /api/auth/token` proxy endpoint is purpose-built so that Swagger's Authorize dialog can trigger the exchange without ever seeing the client secret.

**CSRF protection** — A cryptographically random base64url `state` value is generated on `/api/auth/login` and stored in `IMemoryCache` with a 5-minute TTL. The callback validates this value before proceeding, preventing cross-site request forgery on the OAuth flow.

**API key authentication with fallback** — `ApiKeyAuthenticationHandler` is a custom `AuthenticationHandler`. It first checks the Bearer token against the configured `Api:Key` using constant-time SHA-256 comparison (preventing timing attacks). If that fails, it validates the token against the GitHub `/user` endpoint via `IGitHubTokenValidator`, allowing a GitHub OAuth token to be used directly.

**Global exception middleware** — `GlobalExceptionMiddleware` is a single catch-all that translates `GitHubApiException` to the correct HTTP status code and ensures internal details are never exposed in responses. Unexpected exceptions produce a 500 with a safe generic message.

**FluentValidation** — `CreateIssueRequestValidator` and `CreatePullRequestRequestValidator` are registered from the assembly and injected into controllers explicitly. This keeps validation rules decoupled from model annotations and controller logic.

**PR filtering** — The GitHub Issues API returns pull requests in its results. The service layer filters them out by checking for the presence of the `pull_request` field, so callers always receive genuine issues only.

---

## Generating a GitHub PAT

1. Go to GitHub Settings > Developer settings > Personal access tokens > Tokens (classic)
2. Click "Generate new token"
3. Select the `repo` scope for full repository access, or `public_repo` for public repositories only
4. Copy the token immediately; it will not be shown again
5. Configure it via user secrets or an environment variable as described above
