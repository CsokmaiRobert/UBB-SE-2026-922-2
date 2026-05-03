# BoardRentAndProperty Claude Handoff

## 1. Purpose of this file

This file is the only handoff that needs to be given to the teammate for this phase.

It separates clearly:

- what must be done manually in Visual Studio
- what Claude must implement in the codebase
- what the final result must look like before deployment

This is important because the university requirement is not just "create a web API project".

The real requirement is:

- the application must use a separate ASP.NET Core Web API project
- the desktop app must no longer connect directly to the database
- the desktop app must access data through API endpoints
- non-domain related files must be stored on the server side as well

So creating the Web API template is only the start. The real work is the refactor after that.

## 2. Scope boundary of this phase

This phase is the step between the current version of the application and the future deployment.

So this phase must end with an application that is:

- refactored
- runnable locally
- split into client plus backend
- ready to be deployed later

This phase does **not** include:

- publishing the application to a server
- configuring the production hosting environment
- reverse proxy setup
- cloud deployment
- production secrets setup

So his responsibility ends when the solution is technically ready for deployment, not when it is actually deployed.

## 3. Why this work is needed

The requirement says, in practice:

1. there must be a separate ASP.NET Core Web API project
2. the main application must stop talking directly to SQL Server
3. database access must go through the API
4. files like uploaded avatars must be stored on the server side

That means the final architecture should be:

- `BoardRentAndProperty` = WinUI desktop client
- `BoardRentAndProperty.Api` = ASP.NET Core Web API backend
- SQL Server database = used only by the API

So this is not a browser website rewrite.

It is a backend split:

- persistence moves to the API
- the client becomes an HTTP client

## 4. Short version of what he must understand

He is not building a website and he is not deploying anything yet.

He is taking the current WinUI app, adding a separate ASP.NET Core Web API backend, moving database and server-side file responsibilities there, and making the WinUI app communicate with that backend through API endpoints so the project is ready to deploy afterward.

## 5. What must be done manually before using Claude

These steps should be done manually in Visual Studio first.

### Manual step 1. Create the API project

Inside the existing solution:

- `Add -> New Project -> ASP.NET Core Web API`

Project name:

- `BoardRentAndProperty.Api`

Recommended creation settings:

- framework: `.NET 8`
- authentication: `None`
- `Configure for HTTPS`: checked
- `Use controllers`: checked
- Swagger support enabled

Optional but recommended:

- if he wants a cleaner architecture, he may also create a class library later such as `BoardRentAndProperty.Persistence`

### Manual step 2. Clean the template

Delete template sample files:

- `WeatherForecast.cs`
- `Controllers/WeatherForecastController.cs`

Keep:

- `Program.cs`
- `appsettings.json`
- `appsettings.Development.json`
- `Properties/launchSettings.json`

### Manual step 3. Install packages

In the API project install:

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.EntityFrameworkCore.Design`

If Swagger support is not already present, make sure Swagger support is added so these lines can exist:

- `builder.Services.AddSwaggerGen();`
- `app.UseSwagger();`
- `app.UseSwaggerUI();`

### Manual step 4. Check the target framework

Open `BoardRentAndProperty.Api.csproj` and make sure it uses:

```xml
<TargetFramework>net8.0</TargetFramework>
```

### Manual step 5. Add the API connection string

In `BoardRentAndProperty.Api/appsettings.json`, the API must own the database connection string.

Example:

```json
{
  "ConnectionStrings": {
    "BoardRentAndProperty": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BoardRentAndProperty;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
  },
  "Storage": {
    "AvatarFolder": "Uploads/Avatars"
  }
}
```

Important:

- the API must own the connection string
- `BoardRentAndProperty/App.config` must stop being the source of database access for the WinUI app

### Manual step 6. Set startup projects

Right click the solution:

- `Set Startup Projects...`
- `Multiple startup projects`

Set these two to start:

- `BoardRentAndProperty.Api`
- `BoardRentAndProperty`

### Manual step 7. Optional preparation before Claude

He can also create these folders in the API project, although Claude can do this too:

- `Controllers`
- `Data`
- `Repositories`
- `Services`
- `Dtos`
- `Models`
- `Mappers`
- `Uploads`

## 6. What Claude should implement

Claude should do the refactor work, not just generate an empty API.

### Part A. Move persistence to the API

Claude should move or recreate on the API side:

- `AppDbContext`
- repositories
- migrations ownership
- EF Core configuration
- database-related business logic

The WinUI project must stop owning:

- direct `DbContext` access
- repository implementations
- database migration logic
- the SQL Server connection string for data access

### Part B. Create the actual Web API

Claude must implement real controllers and endpoints, not just leave the template.

Minimum areas:

- auth
- accounts / profile
- admin
- games
- requests
- rentals
- notifications

This is necessary because the client must communicate with the server through HTTP endpoints.

### Part C. Refactor the WinUI app to use HTTP

Claude must replace the current direct database access flow with this:

- WinUI view model
- WinUI service using `HttpClient`
- API controller
- API service / repository
- SQL Server

That means services in the WinUI app should stop calling repositories directly and start calling the API.

### Part D. Move file storage to the server

Claude must implement avatar upload so that:

1. the user selects a file in the WinUI app
2. the WinUI app uploads it to the API with `multipart/form-data`
3. the API saves it in a server folder such as `Uploads/Avatars`
4. the API returns the stored path or URL

This matters because the requirement says non-domain related files must also be stored on the server.

### Part E. Own migrations on the API side

Claude should make sure migrations belong to the API side after the refactor.

Two acceptable ways:

1. move the existing migrations to the API side
2. create a fresh initial migration owned by the API

For this assignment, either is acceptable if the result works and the API becomes the only project responsible for the schema.

### Part F. Preserve the current app behavior

Claude should preserve these flows as much as possible:

- login
- register
- profile view and update
- admin user actions
- game CRUD
- request flow
- rental flow
- notifications

The goal is architectural change, not functional redesign.



## 7. Recommended work order for Claude

The best implementation order is:

1. configure the API project
2. move persistence and business logic to the API
3. implement controllers and endpoints
4. verify the API runs by itself and Swagger works
5. refactor the WinUI services to use `HttpClient`
6. test both projects together

This order matters because the backend should exist first, and only then should the client be switched to it.

## 8. Prompt to paste into Claude

Copy and paste the text below into Claude.

```text
I already created an ASP.NET Core Web API project named BoardRentAndProperty.Api inside my existing solution BoardRentAndProperty/BoardRentAndProperty.sln. I do not need help creating the template. I need help implementing the actual refactor required by the assignment.

The goal is to keep the current WinUI desktop client, use BoardRentAndProperty.Api as a separate backend, move all direct database access into that API, and make the desktop client consume HTTP endpoints instead of repositories or DbContext. This task is only about making the application ready for deployment later. It is not about deploying it now, and it is not about rewriting the app into a browser website.

The requirement I am trying to satisfy is this in practical terms:
- there must be a separate ASP.NET Core Web API project
- the main application must no longer connect directly to the database
- database access must happen through the API
- non-domain related files must also be stored on the server side

This work is the step before deployment. I do not need deployment itself. I need the application to be left in a state where it is ready to be deployed afterward.

Right now the WinUI project still owns persistence and direct database access. Important current files are:
- BoardRentAndProperty/BoardRentAndProperty/Data/AppDbContext.cs
- BoardRentAndProperty/BoardRentAndProperty/Repositories/*
- BoardRentAndProperty/BoardRentAndProperty/Migrations/*
- BoardRentAndProperty/App.config
- BoardRentAndProperty/BoardRentAndProperty/App.xaml.cs

Important current business services are:
- BoardRentAndProperty/BoardRentAndProperty/Services/AuthService.cs
- BoardRentAndProperty/BoardRentAndProperty/Services/AccountService.cs
- BoardRentAndProperty/BoardRentAndProperty/Services/AdminService.cs
- BoardRentAndProperty/BoardRentAndProperty/Services/GameService.cs
- BoardRentAndProperty/BoardRentAndProperty/Services/RequestService.cs
- BoardRentAndProperty/BoardRentAndProperty/Services/RentalService.cs
- BoardRentAndProperty/BoardRentAndProperty/Services/NotificationService.cs for notification CRUD persistence logic

The current notification server architecture already exists in a separate project. Keep it unless changes are required for compilation, but move notification database CRUD behind the API.

What I need implemented in BoardRentAndProperty.Api:

1. Configure Program.cs with controllers, Swagger, EF Core SQL Server, and dependency injection.
2. Move or recreate AppDbContext, repository logic, migrations ownership, and database-related business services inside BoardRentAndProperty.Api.
3. Move the SQL Server connection string from BoardRentAndProperty/App.config to BoardRentAndProperty.Api/appsettings.json.
4. Add real controllers and endpoints for auth, accounts, admin, games, requests, rentals, and notifications.
5. Implement avatar upload in the API using multipart/form-data and store uploaded files on the server side, not in the WinUI client local app data folder.

What I need changed in the WinUI project:

6. Remove EF Core setup and Database.Migrate() from App.xaml.cs.
7. Remove repository registrations and direct database access from the WinUI client.
8. Replace current service implementations with HttpClient-based implementations that call the new API endpoints.
9. Keep the current WinUI pages and view models as intact as possible so the UI changes stay minimal.
10. Preserve existing service interfaces where practical so the UI layer needs as few changes as possible.

Minimum API endpoints that should exist:

- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/logout
- GET /api/auth/forgot-password
- GET /api/accounts/{accountId}
- PUT /api/accounts/{accountId}
- PUT /api/accounts/{accountId}/password
- POST /api/accounts/{accountId}/avatar
- DELETE /api/accounts/{accountId}/avatar
- GET /api/admin/accounts
- PUT /api/admin/accounts/{accountId}/suspend
- PUT /api/admin/accounts/{accountId}/unsuspend
- PUT /api/admin/accounts/{accountId}/reset-password
- PUT /api/admin/accounts/{accountId}/unlock
- GET /api/games
- GET /api/games/{gameId}
- GET /api/games/owner/{ownerAccountId}
- GET /api/games/owner/{ownerAccountId}/active
- GET /api/games/renter/{renterAccountId}/available
- POST /api/games
- PUT /api/games/{gameId}
- DELETE /api/games/{gameId}
- GET /api/requests/owner/{ownerAccountId}
- GET /api/requests/renter/{renterAccountId}
- GET /api/requests/owner/{ownerAccountId}/open
- POST /api/requests
- PUT /api/requests/{requestId}/approve
- PUT /api/requests/{requestId}/deny
- PUT /api/requests/{requestId}/cancel
- PUT /api/requests/{requestId}/offer
- GET /api/requests/games/{gameId}/booked-dates
- GET /api/requests/games/{gameId}/availability
- GET /api/rentals/owner/{ownerAccountId}
- GET /api/rentals/renter/{renterAccountId}
- POST /api/rentals
- GET /api/rentals/games/{gameId}/availability
- GET /api/notifications/user/{accountId}
- GET /api/notifications/{notificationId}
- PUT /api/notifications/{notificationId}
- DELETE /api/notifications/{notificationId}

Important constraints:
- do not rewrite the app into a browser-based web application
- do not deploy anything
- do not redesign the UI
- keep the current notification server architecture unless changes are needed for compilation
- if needed, it is acceptable to duplicate DTOs between client and API to reduce refactor risk
- a working refactor is more important than perfect architecture
- follow the Csokmai CodeRules(if you do not have them, ask for them)
- do not use single character variables names, use very specific names
- do not write comments, no comments accepted
- do not commit the changes with "Claude" contribution

Definition of done:
- BoardRentAndProperty.Api is a separate runnable project
- the API is the only layer that talks directly to SQL Server
- the WinUI app uses HTTP calls to the API
- avatar storage is server-side
- the main user flows still work
- the solution is ready to be deployed later
```

## 9. What he should hand back after this phase

Before you take over for deployment, his finished work should include all of these:

1. a real `BoardRentAndProperty.Api` project inside the solution
2. `Program.cs` configured with controllers, Swagger, EF Core SQL Server, and dependency injection
3. `appsettings.json` in the API project containing the database connection string
4. `AppDbContext`, repositories, and migration ownership moved to the API side
5. controllers with real endpoints for auth, accounts, admin, games, requests, rentals, and notifications
6. avatar upload handled by the API and stored in a server-side folder
7. WinUI service implementations changed to use `HttpClient`
8. direct database access removed from the WinUI app
9. `Database.Migrate()` removed from the WinUI startup
10. both projects runnable together from Visual Studio
11. the main flows still working locally

If he hands this back, then your part can start: deployment.

