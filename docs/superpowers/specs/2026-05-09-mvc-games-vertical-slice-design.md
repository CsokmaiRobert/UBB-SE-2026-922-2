# MVC Web Foundation + Games Vertical Slice (Assignment 5)

Date: 2026-05-09
Branch: `seminar` (in-place)
Solution: `BoardRentAndProperty/BoardRentAndProperty.sln`

## Goal

Set up the option-3 architecture for the `GUI-BRAP` ASP.NET Core MVC project and prove it works end-to-end through the `Games` entity, with cookie-based authentication so the assignment's "all functionalities guarded from unauthorized users" rule is satisfied for the entire site, not just for `Games`.

The architecture is fixed by `BoardRentAndProperty/mvc-option-3-migration.md`:

```
Browser
    -> GUI-BRAP/Controllers/<X>Controller
    -> GUI-BRAP/Services/I<X>ProxyService     (HttpClient)
    -> /api/<x>                               (BoardRentAndProperty.Api)
    -> I<X>Service
    -> I<X>Repository
    -> AppDbContext
    -> SQL Server LocalDB
```

This document covers the foundation work and the `Games` migration. The other entity controllers (`Accounts`, `Rentals`, `Requests`, `Notifications`, `Roles`) stay in their scaffolded state for their per-entity owners to convert later, but they get authentication coverage automatically via the global authorization fallback.

## API readiness check

`mvc-option-3-migration.md` says the API side must be ready before any MVC controller is converted. For this slice that condition is already met:

- **Games:** `BoardRentAndProperty.Api/Controllers/GamesController.cs` already exposes `GET /api/games`, `GET /api/games/{gameId}`, `POST /api/games`, `PUT /api/games/{gameId}`, `DELETE /api/games/{gameId}` (plus owner/active/available variants we do not need this session). `IGameService` has matching methods including validation and the rental-active deletion guard. No API work required.
- **Auth:** `BoardRentAndProperty.Api/Controllers/AuthController.cs` already exposes `POST /api/auth/login`, `POST /api/auth/logout`, `POST /api/auth/register`. We only consume `/api/auth/login` from the MVC side this session. No API work required.

The other entities the team will migrate next have known gaps documented in `mvc-option-3-migration.md` "Entity And Endpoint Gap Analysis". Each entity owner is responsible for closing their own gaps in the API before converting their controller. Examples for reference:

- Requests: missing `GET /api/requests` and `GET /api/requests/{id}`. The workflow endpoints (approve/deny/cancel/offer) already exist.
- Rentals: missing `GET /api/rentals`, `GET /api/rentals/{id}`, `PUT`, `DELETE`. Repository methods exist; service interface needs to expose them.
- Notifications: missing a top-level `GET /api/notifications` and an exposed `POST /api/notifications`.
- Accounts: no raw account CRUD on the API by design. Owner uses profile/admin/auth endpoints and removes `PasswordHash` from views.

This slice does not touch the API, so any API work for those entities is decoupled from this work.

## In scope

- Cookie authentication: login form, logout, claims-based principal, app-wide `[Authorize]` fallback policy.
- One `AuthController` for `GET/POST /Auth/Login`, `POST /Auth/Logout`, optional `AccessDenied`.
- One `IAuthProxyService` + `AuthProxyService` calling `POST /api/auth/login`.
- One `IGameProxyService` + `GameProxyService` calling the existing `/api/games` endpoints.
- A common error-envelope deserializer + `ProxyServiceException` shared between proxies.
- Refactor `GamesController` to use `IGameProxyService`; rebind `Views/Games/*.cshtml` from `Api.Models.Game` to `Contracts.DataTransferObjects.GameDTO`.
- Move `GamesController.cs` from project root into `Controllers/`.
- DI registration for `HttpClient`, both proxies, and the cookie auth scheme in `Program.cs`.
- `ApiBaseUrl` setting in `appsettings.json` and `appsettings.Development.json`.
- `_Layout.cshtml` nav: Home, Games, Login/Logout (Logout shown only when authenticated).
- Add `BoardRentAndProperty.Contracts` project reference to `GUI-BRAP`.
- Delete the `GUI-BRAP/GUI-BRAP.csproj.Backup.tmp` scaffolder leftover.

## Out of scope (deferred to entity owners or later sessions)

- Refactoring `AccountsController`, `RentalsController`, `RequestsController`, `NotificationsController`, `RolesController`. Their owners do these following the same template Games sets up.
- Removing the `BoardRentAndProperty.Api` project reference from `GUI-BRAP`. Only safe once the still-scaffolded controllers stop using `AppDbContext` directly.
- Removing `AddDbContext<AppDbContext>` from `GUI-BRAP/Program.cs` (same reason).
- Registration form, profile page, admin pages.
- API-side authentication. The API stays open within the local network for this slice.
- Memory caching.
- Sequence diagrams (separate per-student deliverable).
- Automated tests for the new proxies.

## File layout after this work

```
GUI-BRAP/
  Controllers/
    HomeController.cs                (existing, gets [AllowAnonymous] on Index/Privacy)
    AuthController.cs                (new)
    GamesController.cs               (moved here from project root, fully rewritten)
  Services/
    ApiErrorEnvelope.cs              (new, record + ensurer)
    ProxyServiceException.cs         (new)
    IAuthProxyService.cs             (new)
    AuthProxyService.cs              (new)
    IGameProxyService.cs             (new)
    GameProxyService.cs              (new)
  Models/
    ErrorViewModel.cs                (existing)
    Auth/
      LoginViewModel.cs              (new)
  Utilities/
    ClaimsPrincipalExtensions.cs     (new, GetAccountId helper)
  Views/
    _ViewImports.cshtml              (add Contracts namespace)
    Auth/
      Login.cshtml                   (new)
      AccessDenied.cshtml            (new, minimal)
    Games/
      Index.cshtml, Details.cshtml, Create.cshtml, Edit.cshtml, Delete.cshtml   (all rebound to GameDTO)
    Shared/
      _Layout.cshtml                 (nav additions, login/logout link)
  Program.cs                         (rewritten for HttpClient + auth + proxy DI)
  appsettings.json                   (ApiBaseUrl added)
  appsettings.Development.json       (ApiBaseUrl override if needed)
  GUI-BRAP.csproj                    (add Contracts project reference)

GUI-BRAP/AccountsController.cs       (untouched, Accounts owner)
GUI-BRAP/NotificationsController.cs  (untouched, Notifications owner)
GUI-BRAP/RentalsController.cs        (untouched, Rentals owner)
GUI-BRAP/RequestsController.cs       (untouched, Requests owner)
GUI-BRAP/RolesController.cs          (untouched, optional owner)
GUI-BRAP/GUI-BRAP.csproj.Backup.tmp  (deleted)
```

## Cookie authentication

`Program.cs` registers the cookie scheme as the default:

- `AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => …)` with:
  - `LoginPath = "/Auth/Login"`
  - `LogoutPath = "/Auth/Logout"`
  - `AccessDeniedPath = "/Auth/AccessDenied"`
  - `ExpireTimeSpan = TimeSpan.FromHours(8)`
  - `SlidingExpiration = true`
  - `Cookie.HttpOnly = true`
  - `Cookie.SameSite = SameSiteMode.Lax`
- `AddAuthorization(options => options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())`.
- `app.UseAuthentication(); app.UseAuthorization();` in that order, before `MapControllerRoute`.

`HomeController.Index`, `HomeController.Privacy`, `AuthController.Login`, and `AuthController.AccessDenied` get `[AllowAnonymous]`. Everything else is protected by the fallback policy without per-controller attributes.

`AuthController.Login` flow:

1. `GET /Auth/Login` returns the form bound to `LoginViewModel { UsernameOrEmail, Password, ReturnUrl }`.
2. `POST /Auth/Login` calls `IAuthProxyService.LoginAsync(LoginDataTransferObject)` which posts to `/api/auth/login` and returns the `AccountProfileDataTransferObject`.
3. On success, build `ClaimsIdentity` with claims:
   - `ClaimTypes.NameIdentifier` = `AccountId.ToString()`
   - `ClaimTypes.Name` = `Username`
   - `"DisplayName"` = `DisplayName`
   - one `ClaimTypes.Role` per role on the profile DTO
4. `await HttpContext.SignInAsync(scheme, new ClaimsPrincipal(identity))`.
5. If `ReturnUrl` is non-empty and `Url.IsLocalUrl(ReturnUrl)`, redirect there; otherwise redirect to `/`.
6. On failure, `ModelState.AddModelError(string.Empty, error)` and re-render the form.

`AuthController.Logout`:

- `[HttpPost]`, anti-forgery token required, signs out the cookie scheme, redirects to `/Auth/Login`.

`ClaimsPrincipalExtensions.GetAccountId(this ClaimsPrincipal user)` parses the `NameIdentifier` claim into a `Guid`; returns `Guid.Empty` if missing/invalid. Controllers that need the current user's id (e.g., later "my games" filtering) call this helper.

## HttpClient and proxy services

`Program.cs`:

```csharp
builder.Services.AddHttpClient("BoardRentAndPropertyApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
});

builder.Services.AddScoped<IAuthProxyService, AuthProxyService>();
builder.Services.AddScoped<IGameProxyService, GameProxyService>();
```

`appsettings.json` adds `"ApiBaseUrl": "https://localhost:7269"` to match `BoardRentAndProperty.Api/Properties/launchSettings.json`. Development override goes in `appsettings.Development.json` if needed for HTTPS dev cert quirks.

Proxies receive `IHttpClientFactory` and call `factory.CreateClient("BoardRentAndPropertyApi")` per request. Scoped lifetime for the proxy itself.

### Common error handling

```csharp
internal sealed record ApiErrorEnvelope(string? Error);

public sealed class ProxyServiceException : Exception
{
    public int StatusCode { get; }
    public ProxyServiceException(string message, int statusCode) : base(message) { StatusCode = statusCode; }
}

internal static class HttpResponseEnsurer
{
    public static async Task EnsureSuccessAsync(HttpResponseMessage response, string fallback)
    {
        if (response.IsSuccessStatusCode) return;
        string? message = null;
        try { message = (await response.Content.ReadFromJsonAsync<ApiErrorEnvelope>())?.Error; }
        catch { }
        throw new ProxyServiceException(message ?? fallback, (int)response.StatusCode);
    }
}
```

### `IAuthProxyService`

```csharp
public interface IAuthProxyService
{
    Task<AccountProfileDataTransferObject> LoginAsync(LoginDataTransferObject body);
}
```

Implementation: `POST /api/auth/login`, throw `ProxyServiceException` on non-success (controller catches and pushes into `ModelState`), otherwise return the deserialized profile. No proxy logout call. Sign-out is purely the cookie.

### `IGameProxyService`

```csharp
public interface IGameProxyService
{
    Task<IReadOnlyList<GameDTO>> GetAllGamesAsync();
    Task<GameDTO?> GetGameByIdAsync(int gameId);
    Task CreateGameAsync(GameDTO body);
    Task UpdateGameAsync(int gameId, GameDTO body);
    Task DeleteGameAsync(int gameId);
}
```

Implementation maps to `/api/games`, `/api/games/{id}`, `POST /api/games`, `PUT /api/games/{id}`, `DELETE /api/games/{id}`. `GetGameByIdAsync` returns `null` on 404; other methods throw `ProxyServiceException` on non-success.

## Games controller and views

`GamesController` moves into `Controllers/` and is rewritten to inject `IGameProxyService`. Actions:

- `Index` → `GetAllGamesAsync`
- `Details(int id)` → `GetGameByIdAsync(id)`, `NotFound()` if `null`
- `Create` GET returns empty form; POST validates `ModelState`, calls `CreateGameAsync`, redirects on success, re-renders with `ModelState` error on `ProxyServiceException`
- `Edit(int id)` GET pre-fills from `GetGameByIdAsync(id)`; POST same shape as `Create`
- `Delete(int id)` GET shows confirmation; `DeleteConfirmed(int id)` POST calls `DeleteGameAsync`, on `ProxyServiceException` puts the message into `TempData["DeleteError"]` and redirects to `Index` (so business errors like "game has active rentals" surface as a flash message rather than a YSOD)

Views in `Views/Games/`:

- `@model` changes from `BoardRentAndProperty.Api.Models.Game` to `BoardRentAndProperty.Contracts.DataTransferObjects.GameDTO`.
- Verify property parity between `Game.cs` and `GameDTO.cs` first; rebind only fields that exist on the DTO.
- `Index.cshtml` displays `TempData["DeleteError"]` as a Bootstrap alert when present.

`_ViewImports.cshtml` adds `@using BoardRentAndProperty.Contracts.DataTransferObjects` so DTOs don't need fully-qualified types in views.

## Layout and navigation

`Views/Shared/_Layout.cshtml` nav additions:

- `Home` (existing)
- `Games` (new, links to `/Games`)
- `Login` link visible when `User.Identity?.IsAuthenticated == false`
- `Logout` form-button visible when authenticated, posts to `/Auth/Logout` with anti-forgery token. Shows `DisplayName` (or `Username`) before the logout button.
- `Privacy` (existing) stays.

## DI summary (Program.cs after this work)

In order:

1. `AddControllersWithViews()`
2. `AddDbContext<AppDbContext>(...)` (kept for the still-scaffolded controllers; remove later)
3. `AddHttpClient("BoardRentAndPropertyApi", ...)`
4. `AddScoped<IAuthProxyService, AuthProxyService>()`, `AddScoped<IGameProxyService, GameProxyService>()`
5. `AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(...)`
6. `AddAuthorization(o => o.FallbackPolicy = ...)`
7. After `Build()`: exception handler / HSTS in non-dev, `UseHttpsRedirection`, `UseStaticFiles`, `UseRouting`, `UseAuthentication`, `UseAuthorization`, `MapControllerRoute(default: Home/Index/{id?})`, `app.Run()`.

## Verification (manual smoke test after implementation)

1. `dotnet build BoardRentAndProperty.sln` produces a clean build for Api and GUI-BRAP. The other still-scaffolded GUI-BRAP controllers continue to compile because we keep `AppDbContext` and the Api project reference.
2. Start `BoardRentAndProperty.Api` (https://localhost:7269) and `GUI-BRAP` (https://localhost:7002).
3. Hit `/Games` while not logged in. Expect redirect to `/Auth/Login?ReturnUrl=%2FGames`.
4. Log in with seeded admin credentials (the API seeds an admin via `AppDbContext`/Auth flow). Expected: redirect to `/Games`, list renders from `GET /api/games`.
5. Create a game → redirected to `Index`, new row visible. Edit it → updated. Delete a game with no rentals → row disappears.
6. Try to delete a game with an active rental → `Index` shows the red TempData alert, no exception page (the API returns 409 with `error` envelope; `EnsureSuccessAsync` raises `ProxyServiceException`; controller catches and stores in `TempData`).
7. Click Logout → cookie cleared, hitting `/Games` again redirects to login.
8. While logged in, `/Rentals`, `/Requests`, `/Notifications`, `/Accounts`, `/Roles` still load using their existing scaffolded `AppDbContext` paths. This is by design; those owners convert their own controllers later.
9. While logged out, the same pages all redirect to `/Auth/Login` (fallback policy at work).

## Known follow-ups (not this session)

- Each entity owner converts their controller and views to a proxy service following the Games template. They register their proxy in `Program.cs`.
- After all entity owners are done: drop `AddDbContext<AppDbContext>` from `Program.cs`, remove the `BoardRentAndProperty.Api` project reference from `GUI-BRAP.csproj`, remove `Microsoft.EntityFrameworkCore.*` packages.
- Remove `AccountsController`'s `PasswordHash` exposure when the Accounts owner refactors that controller.
- Optional: protect the API itself (cookie/JWT) once the MVC app is the sole client.
- Optional: memory cache for `GET /api/games` if listing performance becomes an issue.
