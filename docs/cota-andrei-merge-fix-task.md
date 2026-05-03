# BoardRentAndProperty ASP.NET Core Web API Refactor Task

## 1. What this task actually means

This task does **not** mean:

- rewrite the WinUI app into a browser website
- deploy the project
- redesign the UI

This task **does** mean:

- keep the existing WinUI desktop client
- add a separate ASP.NET Core Web API project to the solution
- move database access out of the WinUI client and into that API
- move server-side file storage out of the WinUI client and into that API
- make the WinUI app call HTTP endpoints instead of calling repositories / `DbContext` directly

After this refactor, the app becomes a small full-stack system:

- `BoardRentAndProperty` = desktop client
- `BoardRentAndProperty.Api` = backend
- SQL Server database = used only by the API

## 2. Current project situation

Right now the WinUI app still directly owns the persistence layer:

- `BoardRentAndProperty/BoardRentAndProperty/Data/AppDbContext.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Repositories/*`
- `BoardRentAndProperty/BoardRentAndProperty/Migrations/*`
- `BoardRentAndProperty/App.config`
- `BoardRentAndProperty/BoardRentAndProperty/App.xaml.cs`

The WinUI app also stores avatar files locally right now:

- `BoardRentAndProperty/BoardRentAndProperty/Services/AccountService.cs`

That is exactly what must change.

## 3. Target architecture

### Keep in the WinUI client

- `Views/*`
- `ViewModels/*`
- UI-only services like `FilePickerService`
- session / navigation / local notification display logic
- DTOs if needed by the client

### Move to the new API

- EF Core `AppDbContext`
- connection string ownership
- migrations
- repositories
- business services that currently depend on repositories
- avatar file storage
- all CRUD / business endpoints

### Important rule

The WinUI client must no longer:

- create or migrate the database
- reference `DbContext`
- reference repository implementations
- write uploaded files to `%LocalAppData%`

## 4. Recommended project structure

Minimum acceptable structure:

- `BoardRentAndProperty` existing WinUI project
- `BoardRentAndProperty.Api` new ASP.NET Core Web API project

Recommended structure if the AI can do it cleanly:

- `BoardRentAndProperty` existing WinUI project
- `BoardRentAndProperty.Api` new ASP.NET Core Web API project
- `BoardRentAndProperty.Persistence` optional class library for `AppDbContext`, repositories, and migrations

If time is short, it is completely acceptable to keep `DbContext`, repositories, and controllers directly inside `BoardRentAndProperty.Api`.

## 5. What must be done manually in Visual Studio

These are the steps I would ask the teammate to do manually first because they are faster and less error-prone in Visual Studio:

1. Install the Visual Studio workload `ASP.NET and web development`.
2. Open `BoardRentAndProperty/BoardRentAndProperty.sln`.
3. Add a new project:
   `Add -> New Project -> ASP.NET Core Web API`
4. Name it `BoardRentAndProperty.Api`.
5. Optional but recommended:
   add a second project `Class Library` named `BoardRentAndProperty.Persistence`.
6. Add project references:
   - `BoardRentAndProperty.Api` -> reference `BoardRentAndProperty.Persistence` if that library exists
   - the WinUI project must **not** reference the API project
7. Set multiple startup projects for local testing:
   - `BoardRentAndProperty.Api`
   - `BoardRentAndProperty`
8. Put the API connection string in `BoardRentAndProperty.Api/appsettings.json`.
9. If using Package Manager Console, make sure the default project for migrations is the API project or the persistence project that owns the `DbContext`.

## 6. After Create: exact next steps in order

The moment he presses `Create`, he should follow this exact order.

### Step 1. Verify the new project name

Make sure the project is named:

- `BoardRentAndProperty.Api`

If Visual Studio created a different name, rename it immediately before doing anything else.

### Step 2. Verify the target framework

Open `BoardRentAndProperty.Api.csproj`.

The safest target framework for this solution is:

```xml
<TargetFramework>net8.0</TargetFramework>
```

Why:

- the WinUI client currently targets `.NET 8`
- EF Core in the current app is already `8.0.0`
- using `.NET 10` for the API is possible, but it adds unnecessary mismatch and risk for a university refactor

If the template created `net10.0`, change it to `net8.0`.

### Step 3. Delete the template sample files

Inside the API project, delete:

- `WeatherForecast.cs`
- `Controllers/WeatherForecastController.cs`

Keep:

- `Program.cs`
- `appsettings.json`
- `appsettings.Development.json`
- `Properties/launchSettings.json`

### Step 4. Install the required NuGet packages

In `BoardRentAndProperty.Api`, install these packages with `8.x` versions:

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.EntityFrameworkCore.Design`

Swagger requirement note:

- follow the course requirement and make sure Swagger support exists
- if the template already included Swagger support, keep it
- if it did not, add the required Swagger package support so these lines can be used in `Program.cs`:
  - `builder.Services.AddSwaggerGen();`
  - `app.UseSwagger();`
  - `app.UseSwaggerUI();`

Recommended extra package / note for file upload and static file serving if needed later:

- keep the default ASP.NET Core packages from the template

### Step 5. Add the initial API folders

Create these folders in `BoardRentAndProperty.Api`:

- `Controllers`
- `Data`
- `Repositories`
- `Services`
- `Dtos`
- `Models`
- `Mappers`
- `Uploads`

Notes:

- `Uploads` is where server-side files like avatars can be stored
- if he wants cleaner architecture, he can also add `Interfaces`

### Step 6. Move the connection string to the API

In `BoardRentAndProperty.Api/appsettings.json`, add:

```json
{
  "ConnectionStrings": {
    "BoardRentAndProperty": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BoardRentAndProperty;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
  }
}
```

Optional extra settings:

```json
{
  "Storage": {
    "AvatarFolder": "Uploads/Avatars"
  }
}
```

Important:

- the connection string must stop being owned by `BoardRentAndProperty/App.config`
- `App.config` will no longer be used for database access after the refactor

### Step 7. Configure the API startup

Edit `Program.cs` so the API is ready for controllers, Swagger, EF Core, and dependency injection.

At minimum it should:

- add controllers
- add Swagger
- register `AppDbContext`
- register repositories
- register server-side business services
- enable static file access only if needed for uploaded files

The important Swagger lines that should exist are:

- `builder.Services.AddSwaggerGen();`
- `app.UseSwagger();`
- `app.UseSwaggerUI();`

Minimum shape:

```csharp
using BoardRentAndProperty.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("BoardRentAndProperty")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
```

### Step 8. Move persistence code from the client to the API

The first real work is moving persistence ownership to the API.

He should move or copy from the WinUI project into the API project:

- `Data/AppDbContext.cs`
- `Repositories/*`
- `Migrations/*`
- models that are needed by EF Core
- mappers and DTOs needed by controllers and services

Best practical rule:

- if a class touches EF Core, SQL Server, repositories, or migrations, it belongs on the API side

### Step 9. Create server-side business services

Move or recreate the business logic on the API side for:

- authentication
- accounts / profile
- admin operations
- games
- requests
- rentals
- notifications CRUD

These services must run on the API and use repositories there.

### Step 10. Create the controllers and endpoints

This is the step that gives him the actual Web API required by the assignment.

He must implement controllers such as:

- `AuthController`
- `AccountsController`
- `AdminController`
- `GamesController`
- `RequestsController`
- `RentalsController`
- `NotificationsController`

Each controller must expose endpoints that the WinUI app can call.

Minimum endpoint set:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/accounts/{accountId}`
- `PUT /api/accounts/{accountId}`
- `PUT /api/accounts/{accountId}/password`
- `POST /api/accounts/{accountId}/avatar`
- `DELETE /api/accounts/{accountId}/avatar`
- `GET /api/admin/accounts`
- `PUT /api/admin/accounts/{accountId}/suspend`
- `PUT /api/admin/accounts/{accountId}/unsuspend`
- `PUT /api/admin/accounts/{accountId}/reset-password`
- `PUT /api/admin/accounts/{accountId}/unlock`
- `GET /api/games`
- `GET /api/games/{gameId}`
- `POST /api/games`
- `PUT /api/games/{gameId}`
- `DELETE /api/games/{gameId}`
- `GET /api/requests/owner/{ownerAccountId}`
- `GET /api/requests/renter/{renterAccountId}`
- `POST /api/requests`
- `PUT /api/requests/{requestId}/approve`
- `PUT /api/requests/{requestId}/deny`
- `PUT /api/requests/{requestId}/cancel`
- `GET /api/rentals/owner/{ownerAccountId}`
- `GET /api/rentals/renter/{renterAccountId}`
- `GET /api/notifications/user/{accountId}`

### Step 11. Set up migrations on the API side

After the API owns `AppDbContext`, run migrations from the API project.

Package Manager Console example:

```powershell
Add-Migration InitialApiSetup
Update-Database
```

Two valid approaches:

1. move the existing migration files to the API side
2. delete the old migration ownership from the client and create a fresh initial migration in the API

For a university task, option 2 is acceptable if it works correctly and the schema matches the app needs.

### Step 12. Run and test the API by itself

Before touching the WinUI client, the API must run independently.

He should:

1. start only `BoardRentAndProperty.Api`
2. open Swagger / Swagger UI
3. confirm the endpoints appear
4. test at least login, games listing, and one write endpoint

If Swagger works and the endpoints respond, the backend part of the requirement exists.

### Step 13. Only then refactor the WinUI client

This must happen after the API already works.

In the WinUI project, he or the AI should:

- remove EF Core setup from `App.xaml.cs`
- remove `Database.Migrate()` from startup
- remove repository registrations from dependency injection
- keep the existing view models if possible
- replace local service implementations with `HttpClient` implementations

Meaning:

- current `AuthService`, `AccountService`, `GameService`, `RequestService`, `RentalService`, `AdminService`, and notification CRUD calls should stop querying repositories directly
- instead, they should call `https://.../api/...`

### Step 14. Implement server-side file storage

For avatars, the correct flow is:

1. the WinUI app lets the user pick a file
2. the WinUI app uploads the file to the API
3. the API saves the file in a server folder such as `Uploads/Avatars`
4. the API returns the stored path or URL
5. the WinUI app stores and displays that returned value

This is required because the assignment says:

> All non-domain related files must be stored on this server as well.

So avatar storage must no longer happen in the client machine local app data folder.

### Step 15. Run both projects together

In Visual Studio, set multiple startup projects:

- `BoardRentAndProperty.Api`
- `BoardRentAndProperty`

This is done by right clicking the solution and selecting:

- `Set Startup Projects...`
- `Multiple startup projects`

Then verify:

- API starts
- WinUI starts
- login works
- game data loads
- profile updates work
- avatar upload works through the API

If this works, the application is ready to be deployed later.

## 7. What the AI should do

The AI should do the actual refactor work.

### A. Create the backend

- create controllers
- register EF Core in API `Program.cs`
- move / copy `AppDbContext` into the API side
- move / copy repository logic into the API side
- move / copy business logic into the API side

### B. Remove direct database access from the client

- remove EF Core setup from `BoardRentAndProperty/BoardRentAndProperty/App.xaml.cs`
- remove `Database.Migrate()` from the WinUI startup
- remove repository registrations from WinUI dependency injection
- replace current service implementations with `HttpClient`-based API services

### C. Move server-side file storage

- avatar upload must be saved by the API on the server machine, not by the desktop app
- the client may still use a file picker to choose the file locally
- after selection, the file must be uploaded to the API with `multipart/form-data`
- the API stores the file and returns a public or retrievable path / URL

### D. Keep the UI as unchanged as possible

- do not rewrite pages
- do not rewrite view models unless required by API contract changes
- preserve current behavior

## 8. Concrete files that must be considered

### Persistence currently in client

- `BoardRentAndProperty/BoardRentAndProperty/Data/AppDbContext.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Repositories/AccountRepository.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Repositories/FailedLoginRepository.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Repositories/GameRepository.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Repositories/NotificationRepository.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Repositories/RentalRepository.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Repositories/RequestRepository.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Migrations/*`

### Services whose logic must become server-backed

- `BoardRentAndProperty/BoardRentAndProperty/Services/AuthService.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Services/AccountService.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Services/AdminService.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Services/GameService.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Services/RequestService.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Services/RentalService.cs`
- notification persistence logic from `BoardRentAndProperty/BoardRentAndProperty/Services/NotificationService.cs`

### Startup/config that must change

- `BoardRentAndProperty/BoardRentAndProperty/App.xaml.cs`
- `BoardRentAndProperty/App.config`

## 9. Minimum endpoint checklist

The API should expose enough endpoints to cover the current services.

### Auth

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/forgot-password`

### Accounts / profile

- `GET /api/accounts/{accountId}`
- `PUT /api/accounts/{accountId}`
- `PUT /api/accounts/{accountId}/password`
- `POST /api/accounts/{accountId}/avatar`
- `DELETE /api/accounts/{accountId}/avatar`

### Admin

- `GET /api/admin/accounts?pageNumber=1&pageSize=20`
- `PUT /api/admin/accounts/{accountId}/suspend`
- `PUT /api/admin/accounts/{accountId}/unsuspend`
- `PUT /api/admin/accounts/{accountId}/reset-password`
- `PUT /api/admin/accounts/{accountId}/unlock`

### Games

- `GET /api/games`
- `GET /api/games/{gameId}`
- `GET /api/games/owner/{ownerAccountId}`
- `GET /api/games/owner/{ownerAccountId}/active`
- `GET /api/games/renter/{renterAccountId}/available`
- `POST /api/games`
- `PUT /api/games/{gameId}`
- `DELETE /api/games/{gameId}`

### Requests

- `GET /api/requests/owner/{ownerAccountId}`
- `GET /api/requests/renter/{renterAccountId}`
- `GET /api/requests/owner/{ownerAccountId}/open`
- `POST /api/requests`
- `PUT /api/requests/{requestId}/approve`
- `PUT /api/requests/{requestId}/deny`
- `PUT /api/requests/{requestId}/cancel`
- `PUT /api/requests/{requestId}/offer`
- `GET /api/requests/games/{gameId}/booked-dates`
- `GET /api/requests/games/{gameId}/availability`

### Rentals

- `GET /api/rentals/owner/{ownerAccountId}`
- `GET /api/rentals/renter/{renterAccountId}`
- `POST /api/rentals`
- `GET /api/rentals/games/{gameId}/availability`

### Notifications

- `GET /api/notifications/user/{accountId}`
- `GET /api/notifications/{notificationId}`
- `DELETE /api/notifications/{notificationId}`
- `PUT /api/notifications/{notificationId}`

## 10. Important implementation notes

### Database ownership

The API must own the database connection string in `appsettings.json`.

In API `Program.cs`, register EF Core with something like:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BoardRentAndProperty")));
```

### Migrations

The API must own migrations.

Two acceptable options:

1. Move the current migration files from the WinUI project to the API side.
2. Create a fresh initial migration from the API if keeping old migration history is not important.

After that, use Package Manager Console:

```powershell
Add-Migration InitialApiSetup
Update-Database
```

### Authentication / authorization scope

This course task is mainly about architecture separation, not production-grade security.

So the minimum acceptable approach is:

- login endpoint validates credentials on the server
- client stores the logged-in user locally as it does now
- later requests send the acting `accountId`
- admin endpoints must still validate admin role on the server

If the teammate wants to add JWT authentication, that is fine, but it is not required for this task unless explicitly requested.

### Notifications

The current solution already has a separate `NotificationServer` project for real-time messages.

For this task:

- keep the notification server if it already works
- move database-backed notification CRUD behind the API
- do not spend the entire refactor trying to redesign the notification architecture

### Files that must move to server storage

At minimum:

- uploaded avatar files

Optional improvement:

- other user-uploaded files if introduced later

Do **not** waste time moving packaged client assets like:

- `Assets/default-game-placeholder.jpg`

That is a client asset, not user-generated server storage.

## 11. Definition of done

The task is complete when all of the following are true:

1. The solution contains a separate ASP.NET Core Web API project.
2. The API can start independently from the WinUI app.
3. The API is the only project that talks directly to SQL Server.
4. The WinUI app no longer references `AppDbContext` or repository implementations.
5. The WinUI app uses HTTP calls for auth, accounts, games, requests, rentals, and notifications.
6. Avatar upload is handled by the API and stored on the server side.
7. The app still supports login, register, profile update, admin user management, game CRUD, request flow, rental flow, and notifications.
8. The solution builds and runs locally with the API plus desktop app.
9. The result is ready for deployment, even if deployment itself is not part of this task.

## 12. Short explanation to tell the teammate

You can explain it like this:

> Keep the current desktop app, but insert a backend between the app and the database. Create a separate ASP.NET Core Web API project, move EF Core, repositories, migrations, and server-side file storage there, then replace the current client services so they call HTTP endpoints instead of touching the database directly. Do not deploy it, just make the solution ready to deploy.

## 13. What to give the AI after clicking Create

Once the empty API project already exists, this is the best prompt to give to AI.

Use this as the exact task specification:

> The ASP.NET Core Web API project `BoardRentAndProperty.Api` has already been created inside my existing solution `BoardRentAndProperty/BoardRentAndProperty.sln`. I do not need you to create the project template. I need you to implement the backend and refactor the WinUI client to use it.  
>  
> Goal: keep the WinUI desktop client, use the already-created `BoardRentAndProperty.Api` project as a separate backend, move all direct database access into that API, and make the desktop client consume HTTP endpoints instead of repositories / `DbContext`. This task is only about making the application ready for deployment, not deploying it.  
>  
> Current client-side persistence files include:  
> - `BoardRentAndProperty/BoardRentAndProperty/Data/AppDbContext.cs`  
> - `BoardRentAndProperty/BoardRentAndProperty/Repositories/*`  
> - `BoardRentAndProperty/BoardRentAndProperty/Migrations/*`  
> - `BoardRentAndProperty/App.config`  
> - `BoardRentAndProperty/BoardRentAndProperty/App.xaml.cs`  
>  
> Current business services include:  
> - `Services/AuthService.cs`  
> - `Services/AccountService.cs`  
> - `Services/AdminService.cs`  
> - `Services/GameService.cs`  
> - `Services/RequestService.cs`  
> - `Services/RentalService.cs`  
> - notification persistence logic from `Services/NotificationService.cs`  
>  
> What I need implemented in the API project:  
> 1. Configure `Program.cs` with controllers, Swagger, EF Core SQL Server, and dependency injection.  
> 2. Move or recreate `AppDbContext`, repository logic, migrations ownership, and database-related business services inside `BoardRentAndProperty.Api`.  
> 3. Add controllers with endpoints for auth, accounts, admin, games, requests, rentals, and notifications.  
> 4. Implement avatar upload in the API using `multipart/form-data`, storing files on the server side, not in the WinUI client local app data folder.  
> 5. Move the SQL Server connection string from `BoardRentAndProperty/App.config` to API `appsettings.json`.  
>  
> What I need changed in the WinUI project:  
> 6. Remove EF Core setup and `Database.Migrate()` from `App.xaml.cs`.  
> 7. Remove repository registrations and direct database access from the WinUI client.  
> 8. Replace current service implementations with `HttpClient`-based implementations that call the new API endpoints. Try to preserve existing service interfaces used by the view models so UI changes stay minimal.  
> 9. Keep the current WinUI pages and view models as intact as possible.  
>  
> Minimum endpoints that must exist in the API:  
> - auth: register, login, logout, forgot-password  
> - accounts: get profile, update profile, change password, upload avatar, delete avatar  
> - admin: list accounts, suspend, unsuspend, reset password, unlock  
> - games: list, get by id, create, update, delete, get by owner, get active by owner, get available for renter  
> - requests: create, approve, deny, cancel, offer, get by owner, get by renter, get booked dates, check availability  
> - rentals: get by owner, get by renter, create confirmed rental, check availability  
> - notifications: list by user, get by id, update, delete  
>  
> Constraints:  
> - do not rewrite the project into a browser web app  
> - do not deploy anything  
> - do not redesign the UI  
> - keep the existing notification server architecture unless changes are required for compilation  
> - working refactor is more important than perfect architecture  
>  
> Definition of done:  
> - the API is a separate runnable project  
> - the API is the only layer that accesses SQL Server  
> - the WinUI app uses HTTP calls to the API  
> - avatar storage is server-side  
> - the solution is ready to deploy later

## 14. What work proves the requirement is satisfied

The requirement is satisfied only if the delivered work includes all of these:

1. A separate project `BoardRentAndProperty.Api` exists in the solution.
2. The API runs independently from the desktop application.
3. `AppDbContext` and repositories are no longer owned by the WinUI client.
4. The database connection string is owned by the API, not `App.config`.
5. The WinUI app no longer calls the database directly.
6. The WinUI app calls API endpoints through `HttpClient`.
7. The API has real controllers and endpoints, not just an empty template.
8. Avatar or other non-domain uploaded files are stored on the server side.
9. The solution can be started as two processes:
   - `BoardRentAndProperty.Api`
   - `BoardRentAndProperty`
10. Core flows still work:
   - login
   - register
   - game listing / CRUD
   - request flow
   - rental flow
   - profile update
   - admin account actions
