# MVC Architecture Preparation Task

## Current Project State

The solution is being migrated to ASP.NET Core MVC using the selected option 3 architecture:

```text
MVC Controller
-> Proxy Service
-> API
-> Service Layer
-> Repository
-> Database
```

The MVC project is `GUI-BRAP`.

Some shared foundation already exists:

- `GUI-BRAP` has cookie authentication configured.
- `GUI-BRAP` has a fallback authorization policy that requires users to be logged in by default.
- `AuthController` supports login, logout, and access denied pages.
- `AuthProxyService` calls the API login endpoint.
- `GameProxyService` calls the API games endpoints.
- `ProxyServiceException` and `HttpResponseEnsurer` exist for API error handling.
- `ClaimsPrincipalExtensions` already provides helpers for reading the logged-in user's account id and display name.
- `GamesController` is the current example of the desired MVC-to-API direction.

There are still old scaffolded MVC controllers that directly inject `AppDbContext`. They are temporary scaffold output and should not be treated as the final architectural pattern.

The purpose of this task is to finish the shared MVC foundation so feature owners can safely migrate their own pages without inventing different authentication, authorization, proxy, or DI patterns.

## Reason For This Task

The assignment requires:

```text
All functionalities need to be guarded from unauthorized users.
Use a dependency injection framework for your project, have it do all the instantiations.
```

This means the MVC application needs a consistent foundation before individual features are migrated.

Every feature owner should be able to follow the same rules:

- MVC controllers do not create services manually.
- MVC controllers do not create `HttpClient` manually.
- MVC controllers do not use repositories.
- MVC controllers do not use `AppDbContext` for final feature workflows.
- MVC controllers call API endpoints through proxy services.
- Access to MVC functionality is blocked for users who are not logged in.
- Role-based access uses the real role names from the database seed data.
- API errors are displayed consistently in MVC pages.

Without this preparation, each feature can end up using a different style, different role strings, different error handling, or different DI registration patterns.

## Required End State

After this task, the MVC project should have a clear shared setup for:

- authentication;
- authorization conventions;
- role names;
- current-user helpers;
- proxy service registration;
- API error handling.

The project should still build successfully after the changes.

## Authentication Foundation

The existing cookie authentication setup should be preserved and verified.

The MVC application should redirect unauthenticated users to:

```text
/Auth/Login
```

Logout should clear the MVC authentication cookie and return the user to the login flow.

The login process should keep storing at least these claims:

```text
NameIdentifier -> account id
Name           -> username
DisplayName    -> display name
Role           -> primary role name
```

The role claim is important because MVC authorization checks will use it later for admin-only pages.

The login and access denied pages must remain accessible without being logged in:

```csharp
[AllowAnonymous]
```

If registration is added later, its GET and POST actions should also be anonymous. Registration does not need to be implemented as part of this foundation unless it is already in progress.

## Authorization Foundation

The MVC fallback authorization policy should remain active.

Expected behavior:

- A user who is not logged in cannot access normal application pages.
- A user who is not logged in is redirected to `/Auth/Login`.
- Public pages must be explicitly marked with `[AllowAnonymous]`.
- Normal functionality should use `[Authorize]` either directly or through the fallback policy.
- Admin-only functionality should use role authorization.

The current fallback policy already has the correct idea:

```csharp
options.FallbackPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();
```

This should not be removed.

The convention for public pages should be explicit:

```csharp
[AllowAnonymous]
```

The convention for logged-in pages should be:

```csharp
[Authorize]
```

The convention for administrator-only pages should be:

```csharp
[Authorize(Roles = AppRoles.Administrator)]
```

The exact feature-level rules will be applied inside each feature, but the shared constants and examples must exist so everyone uses the same role names.

## Role Constants

Role names must be centralized.

The database seed data uses these role names:

```text
Administrator
Standard User
```

The code should not use loose string values like:

```csharp
"Admin"
"administrator"
"StandardUser"
```

Those values can silently break authorization because role matching depends on the exact role claim value.

Create a shared place for role constants inside the MVC project, for example:

```csharp
public static class AppRoles
{
    public const string Administrator = "Administrator";
    public const string StandardUser = "Standard User";
}
```

Any MVC role check should use these constants.

Expected usage:

```csharp
[Authorize(Roles = AppRoles.Administrator)]
```

Feature owners should not need to remember the exact database strings.

## Current User Helpers

The existing `ClaimsPrincipalExtensions` should remain the common way to read logged-in user data.

It already supports:

```csharp
User.GetAccountId()
User.GetDisplayNameOrUsername()
```

Extend or confirm this helper area so feature owners have a simple way to read:

- the current account id;
- the current display name or username;
- the current role name;
- whether the current user is an administrator.

The intended result is that feature controllers can avoid manually searching claims everywhere.

Example of the desired style:

```csharp
Guid accountId = User.GetAccountId();
bool isAdministrator = User.IsAdministrator();
```

This helps feature owners implement workflows like:

- show current user's notifications;
- create a game owned by the current user;
- load rentals for the current renter;
- load requests for the current owner;
- guard admin pages.

## Dependency Injection Foundation

The MVC project should have a clear DI pattern for proxy services.

Currently, proxy services are registered directly in `Program.cs`, for example:

```csharp
builder.Services.AddScoped<IAuthProxyService, AuthProxyService>();
builder.Services.AddScoped<IGameProxyService, GameProxyService>();
```

This works, but the project should provide one obvious place where future feature proxy services are registered.

The preferred end state is a small extension method or equivalent shared registration area, for example:

```csharp
builder.Services.AddProxyServices();
```

That registration area should include the currently existing proxy services:

```csharp
IAuthProxyService -> AuthProxyService
IGameProxyService -> GameProxyService
```

Later proxy services should be added in the same place by feature owners:

```csharp
IRequestProxyService -> RequestProxyService
IRentalProxyService -> RentalProxyService
INotificationProxyService -> NotificationProxyService
```

The exact future proxies do not need to be fully implemented now. The important part is that the pattern is clear and consistent.

MVC controllers should receive dependencies through constructor injection.

Expected style:

```csharp
public GamesController(IGameProxyService gameProxyService)
{
    this.gameProxyService = gameProxyService;
}
```

Avoid manual instantiation like:

```csharp
new GameProxyService(...)
new HttpClient()
```

## API Client Configuration

The existing named API `HttpClient` should remain the shared way for MVC proxy services to call the API.

Current concept:

```csharp
builder.Services.AddHttpClient("BoardRentAndPropertyApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
```

All MVC proxy services should use this configured client through `IHttpClientFactory`.

Expected proxy style:

```csharp
HttpClient client = httpClientFactory.CreateClient("BoardRentAndPropertyApi");
```

If a shared constant is added for the client name, all proxies should use it. This is useful because a typo in the client name will only fail at runtime.

The API base URL should continue to come from configuration:

```json
"ApiBaseUrl": "https://localhost:7269"
```

Feature owners should not hardcode API hosts or ports inside their controllers or proxy services.

## Shared Proxy Error Handling

The MVC project already has:

- `ProxyServiceException`;
- `HttpResponseEnsurer`;
- `ApiErrorEnvelope`.

These should remain the common API error handling mechanism for all proxy services.

The API returns errors in an envelope with at least:

```text
Code
Error
```

The MVC proxy layer should read the API error message and throw a `ProxyServiceException` that contains enough information for controllers to display a useful validation or error message.

The current minimum useful data is:

```text
message
HTTP status code
```

It is also useful to preserve the API error code if possible:

```text
message
HTTP status code
API error code
```

Feature controllers should not each invent their own `HttpResponseMessage` parsing. They should call the shared helper and handle `ProxyServiceException`.

Expected controller style:

```csharp
try
{
    await proxy.DoSomethingAsync(model);
}
catch (ProxyServiceException ex)
{
    ModelState.AddModelError(string.Empty, ex.Message);
    return View(model);
}
```

## Expected Rules For Feature Owners After This Preparation

After this foundation is finished, feature owners should be able to follow these rules:

- Add their own proxy interface and proxy implementation.
- Register the proxy in the shared DI registration area.
- Inject the proxy into their MVC controller.
- Call the API through the proxy.
- Use `User.GetAccountId()` when the current user id is needed.
- Use role constants for administrator checks.
- Catch `ProxyServiceException` for API errors.
- Use DTOs or MVC view models, not API database models, for final MVC views.

## What Should Not Be Changed In This Preparation

Do not convert every scaffolded feature controller during this foundation task.

Do not remove the temporary `AppDbContext` registration from the MVC project yet.

Do not remove the temporary API project reference from `GUI-BRAP` yet.

Do not remove Entity Framework or scaffolding packages from the MVC project yet.

Do not delete old scaffolded views yet.

Do not change the database schema for this task.

Do not replace the selected option 3 architecture with direct service calls from MVC to the service layer.

Do not make MVC controllers call repositories.

Do not make MVC controllers call `AppDbContext` for final feature workflows.

Do not hardcode role names directly in feature controllers.

Do not hardcode API URLs inside proxy services.

## Completion Criteria

This preparation is complete when:

- the MVC project builds;
- login still works;
- logout still works;
- unauthenticated users are redirected to login for normal MVC pages;
- anonymous access is only used for public pages such as login and access denied;
- role constants exist and match the seeded role names exactly;
- current-user helper methods exist for account id, display name/username, role, and administrator check;
- proxy services have one clear DI registration pattern;
- existing auth and game proxy services are registered through that pattern;
- proxy services use the configured API client;
- shared proxy error handling is ready for feature owners to reuse.

