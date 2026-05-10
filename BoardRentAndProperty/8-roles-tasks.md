# MVC Feature Owner Tasks

## Current Project State

The project is being migrated to ASP.NET Core MVC using this target architecture:

```text
MVC Controller
-> Proxy Service
-> API
-> Service Layer
-> Repository
-> Database
```

The MVC project is `GUI-BRAP`.

The API project is `BoardRentAndProperty.Api`.

The shared DTO project is `BoardRentAndProperty.Contracts`.

The MVC project currently contains scaffolded controllers and views for several entities. Some scaffolded controllers still directly inject `AppDbContext`. Those direct database paths are temporary and must be replaced inside each feature.

Each feature owner should migrate their assigned feature so the MVC website uses proxy services and API endpoints instead of direct database access.

This document describes the 8 feature slices that can be assigned in parallel.

## General Rules For All Feature Owners

The final MVC feature workflow should follow:

```text
MVC Controller
-> Feature Proxy Service
-> API Controller
-> API Service
-> Repository
-> Database
```

Feature owners should not use `AppDbContext` in final MVC controllers.

Feature owners should not use repositories directly in MVC.

Feature owners should not use `BoardRentAndProperty.Api.Models` in final MVC views.

Feature owners should use DTOs from `BoardRentAndProperty.Contracts` or MVC-specific view models.

Feature proxy services should be registered through the shared DI pattern prepared in the MVC project.

MVC controllers should receive dependencies through constructor injection.

Feature pages should use the shared authentication, authorization, UI, Bootstrap, and proxy error-handling conventions prepared by the setup tasks.

Each feature owner should verify that their MVC feature works by running the API project and the MVC project together.

## 1. Auth Feature

## Purpose

This feature handles the web authentication workflow.

It is responsible for allowing users to log in, log out, register if supported by the MVC app, and access forgot-password behavior if exposed in MVC.

## Current Related Code

Desktop reference:

```text
BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/RegisterPage.xaml
BoardRentAndProperty/BoardRentAndProperty/ViewModels/LoginViewModel.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/RegisterViewModel.cs
```

API:

```text
BoardRentAndProperty.Api/Controllers/AuthController.cs
BoardRentAndProperty.Api/Services/IAuthService.cs
BoardRentAndProperty.Api/Services/AuthService.cs
```

MVC current files:

```text
GUI-BRAP/Controllers/AuthController.cs
GUI-BRAP/Services/IAuthProxyService.cs
GUI-BRAP/Services/AuthProxyService.cs
GUI-BRAP/Models/Auth/LoginViewModel.cs
GUI-BRAP/Views/Auth/Login.cshtml
GUI-BRAP/Views/Auth/AccessDenied.cshtml
```

Shared DTOs:

```text
LoginDataTransferObject
RegisterDataTransferObject
AccountProfileDataTransferObject
```

## API Endpoints

Current API endpoints:

```text
POST /api/auth/login
POST /api/auth/register
POST /api/auth/logout
GET  /api/auth/forgot-password
```

## Remaining Work

The MVC login workflow already exists, but it must be verified and completed as a full feature.

The feature should make sure login calls the API through `AuthProxyService`, handles API errors, and creates the correct MVC authentication cookie.

Logout should clear the MVC authentication cookie.

Register should be added to MVC if the web app is expected to support registration.

Forgot-password can be exposed in MVC if required by the current project scope.

The feature should make sure failed login/register cases display useful validation errors.

The feature should make sure anonymous access is allowed only for login/register/access denied/forgot-password pages.

The feature should make sure authenticated users are redirected to the intended page after login.

## Expected Final MVC Shape

```text
AuthController
-> IAuthProxyService / AuthProxyService
-> API AuthController
-> IAuthService / AuthService
-> AccountRepository / FailedLoginRepository
-> Database
```

## Links With Other Feature Owners

This feature shares authentication claims and role data with every other feature.

It especially affects:

- Account/Profile, because profile uses the logged-in account id;
- Admin/Account Management, because admin access depends on the role claim;
- Games, Requests, Rentals, and Notifications, because they use the current logged-in user id;
- Architecture foundation, because the auth setup and role constants must remain consistent.

Shared files likely touched by this feature:

```text
GUI-BRAP/Controllers/AuthController.cs
GUI-BRAP/Services/IAuthProxyService.cs
GUI-BRAP/Services/AuthProxyService.cs
GUI-BRAP/Models/Auth/*
GUI-BRAP/Views/Auth/*
GUI-BRAP/Utilities/ClaimsPrincipalExtensions.cs
```

Changes to authentication claims should be coordinated before other feature owners depend on them.

## 2. Account / Profile Feature

## Purpose

This feature handles the normal logged-in user's own account/profile page.

It should not expose raw account administration or password hashes.

## Current Related Code

Desktop reference:

```text
BoardRentAndProperty/BoardRentAndProperty/Views/ProfilePage.xaml
BoardRentAndProperty/BoardRentAndProperty/ViewModels/ProfileViewModel.cs
```

API:

```text
BoardRentAndProperty.Api/Controllers/AccountsController.cs
BoardRentAndProperty.Api/Services/IAccountService.cs
BoardRentAndProperty.Api/Services/AccountService.cs
```

MVC scaffold currently exists:

```text
GUI-BRAP/AccountsController.cs
GUI-BRAP/Views/Accounts/*
```

The current scaffold uses `AppDbContext` and `BoardRentAndProperty.Api.Models.Account`. That is not final.

Shared DTOs:

```text
AccountProfileDataTransferObject
UpdateProfileDataTransferObject
ChangePasswordDataTransferObject
AvatarUploadResponseDataTransferObject
```

## API Endpoints

Current API endpoints:

```text
GET    /api/accounts/{accountId}
PUT    /api/accounts/{accountId}
PUT    /api/accounts/{accountId}/password
POST   /api/accounts/{accountId}/avatar
DELETE /api/accounts/{accountId}/avatar
```

## Remaining Work

The scaffolded MVC account CRUD should be replaced for normal profile usage.

The profile page should load the current user using the logged-in account id from the MVC claims.

The feature should create or adapt an account/profile proxy service.

Profile update should call the API instead of saving directly to the database.

Password change should call the API password endpoint.

Avatar upload/remove can be added if profile avatar support is required in the MVC scope.

The MVC UI should never display or edit `PasswordHash`.

The MVC UI should not allow a normal user to edit other users' accounts.

## Expected Final MVC Shape

```text
Profile/Accounts MVC Controller
-> IAccountProxyService / AccountProxyService
-> API AccountsController
-> IAccountService / AccountService
-> AccountRepository
-> Database
```

## Links With Other Feature Owners

This feature overlaps with Auth because the logged-in account id and claims come from login.

This feature overlaps with Admin/Account Management because both use account profile DTOs and account API behavior, but the normal profile feature should only manage the current user's own profile.

This feature can affect the navbar if a profile link is added later.

Shared files likely touched by this feature:

```text
GUI-BRAP/AccountsController.cs
GUI-BRAP/Views/Accounts/*
GUI-BRAP/Utilities/ClaimsPrincipalExtensions.cs
GUI-BRAP/Services/*
```

Coordinate with Admin/Account Management before changing shared account views or deleting scaffolded `Views/Accounts`.

## 3. Admin / Account Management Feature

## Purpose

This feature handles administrator account management.

It is separate from normal user profile editing.

## Current Related Code

Desktop reference:

```text
BoardRentAndProperty/BoardRentAndProperty/Views/AdminPage.xaml
BoardRentAndProperty/BoardRentAndProperty/ViewModels/AdminViewModel.cs
```

API:

```text
BoardRentAndProperty.Api/Controllers/AdminController.cs
BoardRentAndProperty.Api/Services/IAdminService.cs
BoardRentAndProperty.Api/Services/AdminService.cs
```

MVC scaffold that may be reused or replaced:

```text
GUI-BRAP/AccountsController.cs
GUI-BRAP/Views/Accounts/*
```

Shared DTOs:

```text
AccountProfileDataTransferObject
ResetPasswordDataTransferObject
```

## API Endpoints

Current API endpoints:

```text
GET /api/admin/accounts
PUT /api/admin/accounts/{accountId}/suspend
PUT /api/admin/accounts/{accountId}/unsuspend
PUT /api/admin/accounts/{accountId}/reset-password
PUT /api/admin/accounts/{accountId}/unlock
```

## Remaining Work

The feature should create an admin proxy service.

The MVC admin area should list accounts through the API.

The MVC admin area should allow suspend, unsuspend, unlock, and reset password actions through the API.

The MVC admin area should be protected with administrator-only authorization.

The administrator role name must match the seeded role name:

```text
Administrator
```

The MVC admin pages should not expose raw password hashes.

The MVC admin workflow should not use direct database access.

## Expected Final MVC Shape

```text
Admin MVC Controller
-> IAdminProxyService / AdminProxyService
-> API AdminController
-> IAdminService / AdminService
-> AccountRepository / FailedLoginRepository
-> Database
```

## Links With Other Feature Owners

This feature overlaps with Account/Profile because both deal with account data.

It overlaps with Auth because admin authorization depends on the role claim created at login.

It overlaps with UI preparation if admin navigation is shown in the shared layout.

Shared files likely touched by this feature:

```text
GUI-BRAP/AccountsController.cs
GUI-BRAP/Views/Accounts/*
GUI-BRAP/Views/Shared/_Layout.cshtml
GUI-BRAP/Services/*
```

Coordinate with Account/Profile before reusing, moving, or deleting scaffolded account views.

## 4. Games / Listings Feature

## Purpose

This feature handles board game listings.

It includes viewing games, creating games, editing games, deleting games, and showing game details.

## Current Related Code

Desktop reference:

```text
BoardRentAndProperty/BoardRentAndProperty/Views/ListingsPage.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/CreateGameView.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/EditGameView.xaml
BoardRentAndProperty/BoardRentAndProperty/ViewModels/ListingsViewModel.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/CreateGameViewModel.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/EditGameViewModel.cs
```

API:

```text
BoardRentAndProperty.Api/Controllers/GamesController.cs
BoardRentAndProperty.Api/Services/IGameService.cs
BoardRentAndProperty.Api/Services/GameService.cs
```

MVC current files:

```text
GUI-BRAP/Controllers/GamesController.cs
GUI-BRAP/Services/IGameProxyService.cs
GUI-BRAP/Services/GameProxyService.cs
GUI-BRAP/Views/Games/*
```

Shared DTOs:

```text
GameDTO
UserDTO
```

## API Endpoints

Current API endpoints:

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

## Remaining Work

The MVC Games feature is already partially migrated to the correct architecture.

The feature should verify the complete browser workflow for list, details, create, edit, and delete.

Create should use the logged-in user as the game owner.

Edit should preserve the original owner.

Edit/delete should be restricted to the game owner or an administrator, depending on the agreed rule.

API errors such as validation failures or delete conflicts should be shown in the MVC page using the shared error handling pattern.

The final Games workflow should not use `AppDbContext` from MVC.

## Expected Final MVC Shape

```text
GamesController
-> IGameProxyService / GameProxyService
-> API GamesController
-> IGameService / GameService
-> GameRepository
-> Database
```

## Links With Other Feature Owners

This feature overlaps with Requests because request creation depends on available games and booked dates.

This feature overlaps with Rentals because rentals are created for games and availability depends on existing rentals.

This feature overlaps with Auth because game ownership uses the logged-in account id.

Shared files likely touched by this feature:

```text
GUI-BRAP/Controllers/GamesController.cs
GUI-BRAP/Services/IGameProxyService.cs
GUI-BRAP/Services/GameProxyService.cs
GUI-BRAP/Views/Games/*
```

Coordinate with Requests and Rentals before changing game availability behavior or game DTO assumptions.

## 5. Requests - Renter Side Feature

## Purpose

This feature handles requests created by the current logged-in renter.

It includes creating a request, viewing outgoing requests, cancelling a request, and checking availability/booked dates if needed.

## Current Related Code

Desktop reference:

```text
BoardRentAndProperty/BoardRentAndProperty/Views/CreateRequestView.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/RequestsToOthersPage.xaml
BoardRentAndProperty/BoardRentAndProperty/ViewModels/CreateRequestViewModel.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/RequestsToOthersViewModel.cs
```

API:

```text
BoardRentAndProperty.Api/Controllers/RequestsController.cs
BoardRentAndProperty.Api/Services/IRequestService.cs
BoardRentAndProperty.Api/Services/RequestService.cs
```

MVC scaffold currently exists:

```text
GUI-BRAP/RequestsController.cs
GUI-BRAP/Views/Requests/*
```

The current scaffold uses `AppDbContext` and `BoardRentAndProperty.Api.Models.Request`. That is not final.

Shared DTOs:

```text
RequestDTO
CreateRequestDataTransferObject
RequestActionDataTransferObject
BookedDateRangeDataTransferObject
GameDTO
UserDTO
```

## API Endpoints

Relevant current API endpoints:

```text
GET  /api/requests/renter/{renterAccountId}
POST /api/requests
PUT  /api/requests/{requestId}/cancel
GET  /api/requests/games/{gameId}/booked-dates
GET  /api/requests/games/{gameId}/availability
```

## Remaining Work

The feature should create or share a request proxy service.

The MVC request creation workflow should use the logged-in account id as the renter id.

The owner id should come from the selected game.

The create request page should call the API instead of saving a scaffolded request directly to the database.

The outgoing requests page should show requests for the current renter.

Cancel should call the API cancel endpoint and should only apply to the current user's own requests.

Availability or booked-date checks should use the API endpoints when needed.

The final renter-side request workflow should not use `AppDbContext` from MVC.

## Expected Final MVC Shape

```text
Requests MVC Controller
-> IRequestProxyService / RequestProxyService
-> API RequestsController
-> IRequestService / RequestService
-> RequestRepository / RentalRepository / GameRepository
-> Database
```

## Links With Other Feature Owners

This feature strongly overlaps with Requests - Owner Side because both should use the same request proxy service if possible.

It overlaps with Games because request creation needs selected games and game owner data.

It overlaps with Notifications because request actions can produce notifications.

Shared files likely touched by this feature:

```text
GUI-BRAP/RequestsController.cs
GUI-BRAP/Views/Requests/*
GUI-BRAP/Services/IRequestProxyService.cs
GUI-BRAP/Services/RequestProxyService.cs
```

Coordinate with Requests - Owner Side before changing request proxy method names, request views, or request routing.

## 6. Requests - Owner Side Feature

## Purpose

This feature handles requests received by the current logged-in game owner.

It includes viewing incoming/open requests, approving requests, denying requests, and offering a game when the workflow supports it.

## Current Related Code

Desktop reference:

```text
BoardRentAndProperty/BoardRentAndProperty/Views/RequestsFromOthersPage.xaml
BoardRentAndProperty/BoardRentAndProperty/ViewModels/RequestsFromOthersViewModel.cs
```

API:

```text
BoardRentAndProperty.Api/Controllers/RequestsController.cs
BoardRentAndProperty.Api/Services/IRequestService.cs
BoardRentAndProperty.Api/Services/RequestService.cs
```

MVC scaffold currently exists:

```text
GUI-BRAP/RequestsController.cs
GUI-BRAP/Views/Requests/*
```

Shared DTOs:

```text
RequestDTO
RequestActionDataTransferObject
```

## API Endpoints

Relevant current API endpoints:

```text
GET /api/requests/owner/{ownerAccountId}
GET /api/requests/owner/{ownerAccountId}/open
PUT /api/requests/{requestId}/approve
PUT /api/requests/{requestId}/deny
PUT /api/requests/{requestId}/offer
```

## Remaining Work

The feature should reuse or extend the request proxy service.

The incoming requests page should load requests for the current logged-in owner.

The open requests page should load open requests for the current logged-in owner if this page is needed.

Approve should call the API approve endpoint.

Deny should call the API deny endpoint and include a reason when the UI supports it.

Offer should call the API offer endpoint when the feature is included in the MVC workflow.

Owner actions should only be available to the owner of the request's game, or to an administrator if that rule is agreed.

The final owner-side request workflow should not use `AppDbContext` from MVC.

## Expected Final MVC Shape

```text
Requests MVC Controller
-> IRequestProxyService / RequestProxyService
-> API RequestsController
-> IRequestService / RequestService
-> RequestRepository / RentalRepository / GameRepository / NotificationService
-> Database
```

## Links With Other Feature Owners

This feature strongly overlaps with Requests - Renter Side because both should use the same request proxy service if possible.

It overlaps with Rentals because approving a request can create a rental.

It overlaps with Notifications because approve, deny, cancel, and offer actions can produce notifications.

Shared files likely touched by this feature:

```text
GUI-BRAP/RequestsController.cs
GUI-BRAP/Views/Requests/*
GUI-BRAP/Services/IRequestProxyService.cs
GUI-BRAP/Services/RequestProxyService.cs
```

Coordinate with Requests - Renter Side before changing shared request routes, views, or proxy method names.

## 7. Rentals Feature

## Purpose

This feature handles confirmed rentals.

It includes rentals where the current user is the owner, rentals where the current user is the renter, creating confirmed rentals, and checking rental availability.

## Current Related Code

Desktop reference:

```text
BoardRentAndProperty/BoardRentAndProperty/Views/CreateRentalView.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/RentalsToOthersPage.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/RentalsFromOthersPage.xaml
BoardRentAndProperty/BoardRentAndProperty/ViewModels/CreateRentalViewModel.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/RentalsToOthersViewModel.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/RentalsFromOthersViewModel.cs
```

API:

```text
BoardRentAndProperty.Api/Controllers/RentalsController.cs
BoardRentAndProperty.Api/Services/IRentalService.cs
BoardRentAndProperty.Api/Services/RentalService.cs
```

MVC scaffold currently exists:

```text
GUI-BRAP/RentalsController.cs
GUI-BRAP/Views/Rentals/*
```

The current scaffold uses `AppDbContext` and `BoardRentAndProperty.Api.Models.Rental`. That is not final.

Shared DTOs:

```text
RentalDTO
CreateRentalDataTransferObject
GameDTO
UserDTO
```

## API Endpoints

Current API endpoints:

```text
GET  /api/rentals/owner/{ownerAccountId}
GET  /api/rentals/renter/{renterAccountId}
POST /api/rentals
GET  /api/rentals/games/{gameId}/availability
```

## Remaining Work

The feature should create a rental proxy service.

The MVC owner rentals page should show rentals where the current user is the owner.

The MVC renter rentals page should show rentals where the current user is the renter.

Create confirmed rental should call the API instead of saving directly to the database.

The current logged-in account id should be used as owner or renter according to the workflow.

Rental availability should be checked through the API endpoint when needed.

The final rental workflow should not use `AppDbContext` from MVC.

## Expected Final MVC Shape

```text
RentalsController
-> IRentalProxyService / RentalProxyService
-> API RentalsController
-> IRentalService / RentalService
-> RentalRepository / GameRepository
-> Database
```

## Links With Other Feature Owners

This feature overlaps with Games because rentals belong to games and depend on game ownership.

It overlaps with Requests - Owner Side because approving a request can create a rental.

It overlaps with Notifications if rental events should produce or display notifications.

Shared files likely touched by this feature:

```text
GUI-BRAP/RentalsController.cs
GUI-BRAP/Views/Rentals/*
GUI-BRAP/Services/IRentalProxyService.cs
GUI-BRAP/Services/RentalProxyService.cs
```

Coordinate with Games before changing assumptions about active games or game owner ids.

Coordinate with Requests - Owner Side before changing rental creation behavior that follows request approval.

## 8. Notifications Feature

## Purpose

This feature handles notifications for the current logged-in user.

It includes listing notifications, viewing details if needed, updating notification data if supported, and deleting notifications.

## Current Related Code

Desktop reference:

```text
BoardRentAndProperty/BoardRentAndProperty/Views/NotificationsPage.xaml
BoardRentAndProperty/BoardRentAndProperty/ViewModels/NotificationsViewModel.cs
```

API:

```text
BoardRentAndProperty.Api/Controllers/NotificationsController.cs
BoardRentAndProperty.Api/Services/INotificationService.cs
BoardRentAndProperty.Api/Services/NotificationService.cs
```

MVC scaffold currently exists:

```text
GUI-BRAP/NotificationsController.cs
GUI-BRAP/Views/Notifications/*
```

The current scaffold uses `AppDbContext` and `BoardRentAndProperty.Api.Models.Notification`. That is not final.

Shared DTOs:

```text
NotificationDTO
UserDTO
```

## API Endpoints

Current API endpoints:

```text
GET    /api/notifications/user/{accountId}
GET    /api/notifications/{notificationId}
PUT    /api/notifications/{notificationId}
DELETE /api/notifications/{notificationId}
```

## Remaining Work

The feature should create a notification proxy service.

The MVC notifications page should show notifications for the current logged-in user.

Details should load the notification through the API if details are included.

Update should call the API if editing/updating notifications is included.

Delete should call the API delete endpoint.

Users should not be able to access other users' notifications through the MVC UI.

The final notifications workflow should not use `AppDbContext` from MVC.

## Expected Final MVC Shape

```text
NotificationsController
-> INotificationProxyService / NotificationProxyService
-> API NotificationsController
-> INotificationService / NotificationService
-> NotificationRepository
-> Database
```

## Links With Other Feature Owners

This feature overlaps with Requests because request actions can generate notifications.

It overlaps with Rentals if rental actions generate notifications.

It overlaps with Auth because notifications are loaded for the current logged-in account id.

Shared files likely touched by this feature:

```text
GUI-BRAP/NotificationsController.cs
GUI-BRAP/Views/Notifications/*
GUI-BRAP/Services/INotificationProxyService.cs
GUI-BRAP/Services/NotificationProxyService.cs
```

Coordinate with Requests feature owners before changing expectations around request-generated notifications.

## Final Coordination Notes

The Requests feature is intentionally split into renter side and owner side so two people can work in parallel. They should still coordinate closely because both probably share the same MVC controller, views folder, and request proxy service.

Account/Profile and Admin/Account Management are separate features, but both may touch the scaffolded account MVC files. They should decide whether to split the views into separate folders or carefully share the existing `Views/Accounts` folder.

Games, Requests, and Rentals are connected through game ownership and date availability. Changes in one feature can affect the others.

Notifications is mostly a display feature, but notification data is often produced by actions in Requests and Rentals.

Auth is a foundation for all feature owners because all user-specific features depend on the current account id and role claim.

