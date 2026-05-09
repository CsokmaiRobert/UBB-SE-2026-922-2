# ASP.NET Core MVC Option 3 Migration Notes

## Requirement

The project already has a desktop GUI and an API/database backend. The assignment asks for an ASP.NET Core Web App using MVC, not Razor Pages. The migration should mainly affect the GUI: the application should become usable from a browser on localhost through MVC controllers and Razor views, while the existing backend architecture remains the source of business rules and database access.

The chosen architecture is option 3 from the assignment:

```text
MVC Controller
    -> Proxy Service
    -> API
    -> Service Layer
    -> Repository
    -> Database
```

The important idea is that the MVC web project must not use `AppDbContext` directly. MVC controllers should call web proxy services. Those proxy services should call the API over HTTP. The API controllers should call service-layer interfaces. The service layer should call repositories. Repositories should be the only code that talks directly to the database.

## Current Project State

The relevant solution is `BoardRentAndProperty/BoardRentAndProperty.sln`.

Important projects:

```text
BoardRentAndProperty
```

WinUI desktop client. It already uses proxy-style services such as `GameService`, `RequestService`, `RentalService`, `NotificationService`, `AuthService`, `AccountService`, and `AdminService`. These services use `HttpClient` and call API routes like `api/games`, `api/requests`, `api/auth`, etc.

```text
BoardRentAndProperty.Api
```

ASP.NET Core API project. It owns `AppDbContext`, EF models, repositories, services, API controllers, mappers, and migrations.

```text
BoardRentAndProperty.Contracts
```

Shared DTO project. It contains DTOs such as `GameDTO`, `RequestDTO`, `RentalDTO`, `NotificationDTO`, `AccountProfileDataTransferObject`, `LoginDataTransferObject`, and `RegisterDataTransferObject`.

```text
GUI-BRAP
```

ASP.NET Core MVC web project. This is the new web GUI. It currently has scaffolded MVC controllers and views for `Accounts` and `Requests`.

The current workspace also contains scaffolded MVC controllers/views for `Games`, `Rentals`, `Notifications`, and `Roles`. These follow the same direct-`AppDbContext` scaffold pattern and should also be refactored to proxy services.

The current MVC scaffolded architecture is:

```text
GUI-BRAP/AccountsController or GUI-BRAP/RequestsController
    -> AppDbContext
    -> Database
```

This is useful for quick scaffolding, but it is not the desired final architecture. The scaffolded MVC controllers currently inject `AppDbContext` and use EF queries directly. For example, `AccountsController` calls `_context.Accounts.ToListAsync()`, `RequestsController` calls `_context.Requests.ToListAsync()`, `GamesController` calls `_context.Games.ToListAsync()`, `RentalsController` calls `_context.Rentals.ToListAsync()`, `NotificationsController` calls `_context.Notifications.ToListAsync()`, and `RolesController` calls `_context.Roles.ToListAsync()`.

## What Is Already Correct

The API project mostly already follows the correct backend shape:

```text
API Controller
    -> API Service Interface
    -> Repository
    -> Database
```

Examples:

```text
BoardRentAndProperty.Api/Controllers/GamesController
    -> IGameService

BoardRentAndProperty.Api/Controllers/RequestsController
    -> IRequestService

BoardRentAndProperty.Api/Controllers/RentalsController
    -> IRentalService

BoardRentAndProperty.Api/Controllers/NotificationsController
    -> INotificationService

BoardRentAndProperty.Api/Controllers/AdminController
    -> IAdminService

BoardRentAndProperty.Api/Controllers/AccountsController
    -> IAccountService
```

The API also registers its services and repositories in dependency injection in `BoardRentAndProperty.Api/Program.cs`.

This means the main architectural problem is not that the API controllers use repositories directly. They generally do not. The main problem is that the MVC project bypasses the API and service layer by injecting `AppDbContext` directly.

## API Side Must Be Ready First

Before refactoring the MVC website to call the API, the API project must already be able to do the required work by itself. The MVC project should not be used to compensate for missing API behavior. If an MVC page needs to list, view, create, edit, or delete an entity, the API must expose working endpoints for those actions first.

For a scaffold-style CRUD page, "API side is ready" usually means the API can be run alone and exposes working URLs like this:

```text
GET    /api/requests
GET    /api/requests/{requestId}
POST   /api/requests
PUT    /api/requests/{requestId}
DELETE /api/requests/{requestId}
```

The same readiness check applies to each scaffolded entity. For example, before converting `GUI-BRAP/RequestsController` to a proxy-based controller, the API request flow should already work like this:

```text
BoardRentAndProperty.Api/Controllers/RequestsController
    -> IRequestService / RequestService
    -> IRequestRepository / RequestRepository
    -> AppDbContext
    -> Database
```

This check is important because the target architecture is:

```text
MVC Controller
    -> Proxy Service
    -> API
    -> Service Layer
    -> Repository
    -> Database
```

If the API does not yet expose an operation, the fix should start in the API layer:

- add or update the service interface method;
- implement the method in the service class;
- call the repository from the service;
- expose the method through the API controller;
- verify it through Swagger, browser, Postman, or another HTTP client;
- only then call it from a `GUI-BRAP` proxy service.

The API controller should not call the repository directly just to make an endpoint quickly. Keep the API chain as controller -> service -> repository -> database.

## Main Work Needed

The migration should replace this:

```text
GUI-BRAP Controller
    -> AppDbContext
    -> Database
```

with this:

```text
GUI-BRAP Controller
    -> GUI-BRAP proxy service
    -> BoardRentAndProperty.Api HTTP endpoint
    -> BoardRentAndProperty.Api service
    -> BoardRentAndProperty.Api repository
    -> Database
```

The MVC controllers should become thin. They should mostly:

- receive browser requests;
- call an injected proxy service;
- pass DTO/view-model data to Razor views;
- redirect after create/edit/delete/action operations;
- display validation/API errors.

The MVC controllers should not:

- inject `AppDbContext`;
- use `DbSet`;
- call `ToListAsync()` on EF entities;
- include repository logic;
- expose raw database-only fields such as `PasswordHash`.

## Project Reference Direction

`GUI-BRAP` currently references `BoardRentAndProperty.Api`. That is why the scaffolded MVC controllers can use `AppDbContext` and API model classes directly.

For the target architecture, `GUI-BRAP` should reference the shared contracts project and communicate with the API over HTTP.

Preferred target references for `GUI-BRAP`:

```text
GUI-BRAP
    -> BoardRentAndProperty.Contracts
```

Avoid relying on this in the final MVC code:

```text
GUI-BRAP
    -> BoardRentAndProperty.Api
```

The web project should use DTOs from `BoardRentAndProperty.Contracts`, not EF models from `BoardRentAndProperty.Api.Models`.

## Web Proxy Services Needed In GUI-BRAP

Create proxy service interfaces and implementations inside the MVC project, for example:

```text
GUI-BRAP/Services/IGameProxyService.cs
GUI-BRAP/Services/GameProxyService.cs
GUI-BRAP/Services/IRequestProxyService.cs
GUI-BRAP/Services/RequestProxyService.cs
GUI-BRAP/Services/IRentalProxyService.cs
GUI-BRAP/Services/RentalProxyService.cs
GUI-BRAP/Services/INotificationProxyService.cs
GUI-BRAP/Services/NotificationProxyService.cs
GUI-BRAP/Services/IAccountProxyService.cs
GUI-BRAP/Services/AccountProxyService.cs
GUI-BRAP/Services/IAuthProxyService.cs
GUI-BRAP/Services/AuthProxyService.cs
GUI-BRAP/Services/IAdminProxyService.cs
GUI-BRAP/Services/AdminProxyService.cs
```

These services should use `HttpClient` to call the API. Their method names can mirror the existing desktop proxy services, but they should return async `Task` results because MVC controllers are naturally async.

Example shape:

```csharp
public interface IGameProxyService
{
    Task<IReadOnlyList<GameDTO>> GetAllGamesAsync();
    Task<GameDTO?> GetGameByIdAsync(int id);
    Task CreateGameAsync(GameDTO game);
    Task UpdateGameAsync(int id, GameDTO game);
    Task DeleteGameAsync(int id);
}
```

Register these services in `GUI-BRAP/Program.cs`:

```csharp
builder.Services.AddHttpClient("BoardRentAndPropertyApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
});

builder.Services.AddScoped<IGameProxyService, GameProxyService>();
builder.Services.AddScoped<IRequestProxyService, RequestProxyService>();
builder.Services.AddScoped<IRentalProxyService, RentalProxyService>();
builder.Services.AddScoped<INotificationProxyService, NotificationProxyService>();
builder.Services.AddScoped<IAccountProxyService, AccountProxyService>();
builder.Services.AddScoped<IAuthProxyService, AuthProxyService>();
builder.Services.AddScoped<IAdminProxyService, AdminProxyService>();
```

Add an API base URL to `GUI-BRAP/appsettings.json`, for example:

```json
{
  "ApiBaseUrl": "https://localhost:7269"
}
```

The current API launch settings expose `https://localhost:7269` and `http://localhost:5114` from `BoardRentAndProperty.Api/Properties/launchSettings.json`.

## MVC Controllers To Refactor

The scaffolded controllers should be refactored after scaffolding is complete.

Current MVC controllers:

```text
GUI-BRAP/AccountsController.cs
GUI-BRAP/GamesController.cs
GUI-BRAP/NotificationsController.cs
GUI-BRAP/RentalsController.cs
GUI-BRAP/RequestsController.cs
GUI-BRAP/RolesController.cs
```

Expected scaffolded/refactored MVC controllers:

```text
GUI-BRAP/AccountsController.cs
GUI-BRAP/GamesController.cs
GUI-BRAP/RequestsController.cs
GUI-BRAP/RentalsController.cs
GUI-BRAP/NotificationsController.cs
GUI-BRAP/RolesController.cs, optional
```

Each controller should stop injecting `AppDbContext` and should inject the correct proxy service instead.

Example target shape:

```csharp
public class GamesController : Controller
{
    private readonly IGameProxyService gameProxyService;

    public GamesController(IGameProxyService gameProxyService)
    {
        this.gameProxyService = gameProxyService;
    }

    public async Task<IActionResult> Index()
    {
        var games = await this.gameProxyService.GetAllGamesAsync();
        return View(games);
    }
}
```

The Razor views should also change their model types from EF entities to DTOs or MVC-specific view models.

Example:

```cshtml
@model IEnumerable<BoardRentAndProperty.Contracts.DataTransferObjects.GameDTO>
```

instead of:

```cshtml
@model IEnumerable<BoardRentAndProperty.Api.Models.Game>
```

## Entity And Endpoint Gap Analysis

### Games

This is the easiest entity to migrate first.

The API already has useful CRUD-like endpoints:

```text
GET    /api/games
GET    /api/games/{gameId}
GET    /api/games/owner/{ownerAccountId}
GET    /api/games/owner/{ownerAccountId}/active
GET    /api/games/renter/{renterAccountId}/available
POST   /api/games
PUT    /api/games/{gameId}
DELETE /api/games/{gameId}
```

The API already routes these through `IGameService`. `GameService` contains validation and deletion rules. The MVC `GamesController` can be converted to use a `GameProxyService` without adding many backend methods.

Recommended first completed migration:

```text
GUI-BRAP/GamesController
    -> IGameProxyService
    -> /api/games
    -> IGameService
    -> IGameRepository
    -> Database
```

### Accounts

The scaffolded `AccountsController` currently exposes raw `Account` data and includes `PasswordHash`. That should not be shown in the web UI.

The API does not expose raw account CRUD. It exposes account/profile/admin operations:

```text
GET    /api/accounts/{accountId}
PUT    /api/accounts/{accountId}
PUT    /api/accounts/{accountId}/password
POST   /api/accounts/{accountId}/avatar
DELETE /api/accounts/{accountId}/avatar
GET    /api/admin/accounts
PUT    /api/admin/accounts/{accountId}/suspend
PUT    /api/admin/accounts/{accountId}/unsuspend
PUT    /api/admin/accounts/{accountId}/reset-password
PUT    /api/admin/accounts/{accountId}/unlock
POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/logout
```

For MVC, account pages should use `AccountProfileDataTransferObject`, not `Account`.

Recommended MVC behavior:

- `Accounts/Index`: use `AdminProxyService.GetAccountsAsync()`.
- `Accounts/Details/{id}`: use `AccountProxyService.GetProfileAsync(id)`.
- `Accounts/Edit/{id}`: use `AccountProxyService.UpdateProfileAsync(id, dto)`.
- `Accounts/Create`: should probably call `AuthProxyService.RegisterAsync()`, not raw account insertion.
- `Accounts/Delete`: should probably be removed or replaced by suspend/unsuspend, because the API supports account suspension rather than account deletion.

Before presenting, remove `PasswordHash` from all account views.

### Requests

The scaffolded `RequestsController` currently uses `AppDbContext.Requests` directly.

The API exposes request workflow endpoints, not generic scaffold CRUD:

```text
GET  /api/requests/owner/{ownerAccountId}
GET  /api/requests/renter/{renterAccountId}
GET  /api/requests/owner/{ownerAccountId}/open
POST /api/requests
PUT  /api/requests/{requestId}/approve
PUT  /api/requests/{requestId}/deny
PUT  /api/requests/{requestId}/cancel
PUT  /api/requests/{requestId}/offer
GET  /api/requests/games/{gameId}/booked-dates
GET  /api/requests/games/{gameId}/availability
```

This matches the real business workflow: users do not normally edit arbitrary requests. They create, approve, deny, cancel, or offer requests.

For a scaffold-style web listing, the API is missing:

```text
GET /api/requests
GET /api/requests/{requestId}
```

If the MVC scaffold needs a general `Index` and `Details`, add service-layer methods such as:

```csharp
ImmutableList<RequestDTO> GetAllRequests();
RequestDTO GetRequestByIdentifier(int requestId);
```

Then expose them from the API controller:

```text
GET /api/requests
GET /api/requests/{requestId}
```

Avoid adding arbitrary edit/delete behavior unless the business requirement really needs it. It is better for MVC request actions to call approve/deny/cancel/offer endpoints.

### Rentals

The API currently exposes:

```text
GET  /api/rentals/owner/{ownerAccountId}
GET  /api/rentals/renter/{renterAccountId}
POST /api/rentals
GET  /api/rentals/games/{gameId}/availability
```

For scaffold-style MVC pages, the API is missing:

```text
GET    /api/rentals
GET    /api/rentals/{rentalId}
PUT    /api/rentals/{rentalId}
DELETE /api/rentals/{rentalId}
```

The repository already has generic methods such as `GetAll`, `Get`, `Update`, and `Delete`, but the service interface does not expose all of them yet.

If the web UI only needs to display rentals, add:

```csharp
ImmutableList<RentalDTO> GetAllRentals();
RentalDTO GetRentalByIdentifier(int rentalId);
```

If the scaffolded MVC edit/delete pages must work, add service-layer methods for update/delete and enforce business rules there. Do not let the MVC project call `IRentalRepository` or `AppDbContext`.

### Notifications

The API currently exposes:

```text
GET    /api/notifications/user/{accountId}
GET    /api/notifications/{notificationId}
PUT    /api/notifications/{notificationId}
DELETE /api/notifications/{notificationId}
```

The service also has `SendNotificationToUser`, but the API does not expose it clearly as a `POST`.

For scaffold-style MVC pages, the API may need:

```text
GET  /api/notifications
POST /api/notifications
```

Recommended cleaner API shape:

```text
GET    /api/notifications
GET    /api/notifications/user/{accountId}
GET    /api/notifications/{notificationId}
POST   /api/notifications
PUT    /api/notifications/{notificationId}
DELETE /api/notifications/{notificationId}
```

The web UI should usually show notifications for the logged-in user, not every notification in the system, unless the page is an admin page.

### Roles

`Role` can be scaffolded for simple display, but the current API does not have a dedicated `RolesController` or `IRoleService`.

Because roles are mostly fixed seed data, this is optional. If roles must appear in MVC through option 3, add:

```text
IRoleService
RoleService
IRoleRepository, or role methods on an existing admin service
RolesController in the API
RoleProxyService in GUI-BRAP
RolesController in GUI-BRAP
```

This is lower priority than games, accounts, requests, rentals, and notifications.

### AccountRole And FailedLoginAttempt

Do not build normal MVC CRUD pages for these.

`AccountRole` is a join table. It should be managed through account/admin role features, not raw CRUD.

`FailedLoginAttempt` is security/internal state. It should not be exposed as a normal web CRUD page.


## Dependency Injection

Dependency injection is a separate cross-cutting task.
Until the DI structure is finalized, feature code should use constructor injection and interfaces, and avoid direct instantiation.

MVC controllers should receive dependencies through constructor injection.

## Final Expected Shape

After migration, a request to a web page should flow like this:

```text
Browser opens /Games
    -> GUI-BRAP/GamesController.Index
    -> IGameProxyService.GetAllGamesAsync()
    -> HTTP GET https://localhost:<api-port>/api/games
    -> BoardRentAndProperty.Api/GamesController.GetAll()
    -> IGameService.GetAllGames()
    -> IGameRepository.GetAll()
    -> AppDbContext
    -> SQL Server LocalDB
```

The same pattern should apply to accounts, requests, rentals, and notifications.

The MVC project is then only a web GUI. The API remains responsible for business rules. The repositories remain responsible for database access.
