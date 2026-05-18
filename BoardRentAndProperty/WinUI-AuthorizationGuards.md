# WinUI Authorization Guards Task

## Current Project State

The solution contains both the original WinUI desktop application and the ASP.NET Core MVC web application.

The relevant projects are:

```text
BoardRentAndProperty/BoardRentAndProperty
BoardRentAndProperty/GUI-BRAP
BoardRentAndProperty/BoardRentAndProperty.Api
BoardRentAndProperty/BoardRentAndProperty.Contracts
```

`BoardRentAndProperty/BoardRentAndProperty` is the WinUI desktop application.

`GUI-BRAP` is the ASP.NET Core MVC web GUI.

The assignment requirement says:

```text
All functionalities need to be guarded from unauthorized users.
```

This requirement applies to the whole solution, not only to the MVC web application. The desktop application also needs explicit protection so a user who is not logged in cannot access application functionality.

The MVC web application is being prepared with authentication, authorization, dependency injection, proxy/API architecture, and role-based behavior. The WinUI desktop app should behave consistently with the web app for the same authentication and authorization meaning.

## Reason For This Task

The desktop application already has login, registration, session storage, and an administrator page, but the protection is incomplete.

The app currently starts on `LoginPage` and navigates to `MenuBarPage` after successful login. This makes normal usage look guarded, but most WinUI feature pages do not check authorization themselves. They trust that the user reached them through the expected navigation path.

That is not enough for the assignment.

The final desktop application should make the authorization rule explicit:

```text
public pages are only login/register/forgot-password;
every real feature page requires a logged-in session;
administrator-only pages and actions require the Administrator role;
normal users can only see and modify their own data unless admin behavior is explicitly allowed.
```

The goal is not to redesign the desktop application. The goal is to add the missing guards and align desktop behavior with the MVC web behavior expected by the final project.

## Meaning Of Authentication, Authorization, And Roles

Authentication means the app knows who the current user is.

In the WinUI desktop app, the current authenticated user is stored in:

```text
BoardRentAndProperty/BoardRentAndProperty/Utilities/ISessionContext.cs
BoardRentAndProperty/BoardRentAndProperty/Utilities/SessionContext.cs
BoardRentAndProperty/BoardRentAndProperty/Utilities/CurrentUserContext.cs
```

`SessionContext` stores:

```text
AccountId
Username
DisplayName
Role
PamUserId
IsLoggedIn
Email
PhoneNumber
Country
City
StreetName
StreetNumber
```

Authorization means deciding what the current authenticated user is allowed to open, see, or modify.

The current project has two real role names:

```text
Administrator
Standard User
```

These are created by the database initialization in:

```text
BoardRentAndProperty/BoardRentAndProperty/Data/AppDbContext.cs
```

New registered users receive:

```text
Standard User
```

The administrator role name is exactly:

```text
Administrator
```

The implementation should not use `Admin` as the role name.

The desktop app and the web app should use the same role meaning:

```text
Standard User -> normal logged-in user behavior
Administrator -> normal logged-in behavior plus administrator-only features
```

## Current Related WinUI Code

Authentication and session files:

```text
BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml.cs
BoardRentAndProperty/BoardRentAndProperty/Views/RegisterPage.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/RegisterPage.xaml.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/LoginViewModel.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/RegisterViewModel.cs
BoardRentAndProperty/BoardRentAndProperty/Services/IAuthService.cs
BoardRentAndProperty/BoardRentAndProperty/Services/AuthService.cs
BoardRentAndProperty/BoardRentAndProperty/Utilities/ISessionContext.cs
BoardRentAndProperty/BoardRentAndProperty/Utilities/SessionContext.cs
BoardRentAndProperty/BoardRentAndProperty/Utilities/CurrentUserContext.cs
```

Navigation and shell files:

```text
BoardRentAndProperty/BoardRentAndProperty/App.xaml.cs
BoardRentAndProperty/BoardRentAndProperty/Views/MenuBarPage.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/MenuBarPage.xaml.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/MenuBarViewModel.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/AppPage.cs
```

Feature pages that need logged-in protection:

```text
BoardRentAndProperty/BoardRentAndProperty/Views/ListingsPage.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/CreateGameView.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/EditGameView.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/RequestsToOthersPage.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/CreateRequestView.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/RequestsFromOthersPage.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/RentalsFromOthersPage.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/RentalsToOthersPage.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/CreateRentalView.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/NotificationsPage.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/ProfilePage.xaml
```

Administrator files:

```text
BoardRentAndProperty/BoardRentAndProperty/Views/AdminPage.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/AdminPage.xaml.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/AdminViewModel.cs
BoardRentAndProperty/BoardRentAndProperty/Services/IAdminService.cs
BoardRentAndProperty/BoardRentAndProperty/Services/AdminService.cs
```

Games files where desktop/web admin behavior needs alignment:

```text
BoardRentAndProperty/BoardRentAndProperty/Views/ListingsPage.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/ListingsPage.xaml.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/ListingsViewModel.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/EditGameViewModel.cs
BoardRentAndProperty/BoardRentAndProperty/Services/IGameService.cs
BoardRentAndProperty/BoardRentAndProperty/Services/GameService.cs
```

## Current Issues

The current WinUI app has a login flow, but most pages do not explicitly guard themselves.

The app launches into `LoginPage`, and after login it navigates to `MenuBarPage`. This is good, but it is not the same as every feature being guarded.

The current `MenuBarViewModel` hides the Admin link unless `SessionContext.Role` is `Administrator`. This is good, but hiding a link is not enough by itself.

The current `AdminPage` checks whether the user is logged in and whether the role is `Administrator`. It hides the admin content and shows an unauthorized message. This is partially correct.

The current `AdminService` also checks whether the current session is logged in and administrator before admin operations. This is also correct and should be preserved.

The missing part is that normal feature pages such as listings, requests, rentals, notifications, profile, create game, edit game, create request, and create rental do not have a shared page-level guard that blocks unauthenticated access.

Some action-level ownership rules also rely mostly on the UI and current-user filtering. The final project should be clear that a normal user can only work with their own data, and administrator behavior should match the web app where the feature requires it.

The desktop Games/Listings behavior also does not fully match the web requirement. The web task expects:

```text
normal user -> My Games shows only the user's own games
administrator -> Games shows all games and can manage any game
```

The current desktop `ListingsViewModel` loads games only for the current owner. That is correct for normal users, but it does not give administrators the same all-games management behavior expected in the web application.

## Required End State

The WinUI desktop application should have the same authorization meaning as the MVC web application.

Unauthenticated users should only be able to access:

```text
LoginPage
RegisterPage
Forgot password dialog/message
```

Unauthenticated users should not be able to access:

```text
MenuBarPage
ListingsPage
CreateGameView
EditGameView
RequestsToOthersPage
CreateRequestView
RequestsFromOthersPage
RentalsFromOthersPage
RentalsToOthersPage
CreateRentalView
NotificationsPage
ProfilePage
AdminPage
```

If an unauthenticated user reaches a guarded page or guarded action, the app should send the user back to `LoginPage` and clear the navigation state so the protected page cannot be reached by going back.

Logged-in normal users should be able to access the normal application features:

```text
My Games
My Requests
My Rentals
Others' Requests
Others' Rentals
Notifications
Profile
Logout
```

Logged-in normal users should not be able to access administrator-only functionality:

```text
AdminPage
Account Administration
account list
suspend account
unsuspend account
reset password for another account
unlock account
administrator all-games management behavior, unless explicitly allowed through the admin role
```

Logged-in administrators should be able to access normal user features and administrator-only features.

The administrator should be able to access:

```text
AdminPage
Account Administration
Suspend Account
Unsuspend Account
Reset Password
Unlock
Games all-list management behavior
```

The administrator-only restriction should be enforced by the actual page/action logic, not only by hiding a menu item.

## Expected Desktop Authorization Shape

The WinUI app should have a reusable authorization approach instead of scattered duplicated checks.

The implementation should introduce or reuse a small shared authorization/navigation guard that can answer:

```text
is the current user logged in?
is the current user an administrator?
should this page be accessible?
what should happen when access is denied?
```

The guard should use `ISessionContext`.

The guard should not create a second authentication system.

The guard should not store a separate current user outside `SessionContext`.

The guard should not hardcode many role strings across the project. If role names are needed in more than one place, use a shared constant or helper so `Administrator` and `Standard User` stay consistent.

The guarded navigation behavior should be consistent:

```text
not logged in -> navigate to LoginPage and clear back stack
logged in but not administrator for admin page/action -> show access denied/unauthorized message or block the navigation
logged in and allowed -> continue normally
```

## Page Guard Requirements

All protected WinUI pages should check login state when they are opened.

The required public pages are:

```text
LoginPage
RegisterPage
Forgot password dialog/message
```

All other pages should be treated as protected.

The protected pages should not load data or execute view model actions if `SessionContext.IsLoggedIn` is false.

Examples of behavior that should be blocked for unauthenticated users:

```text
loading games
creating games
editing games
deleting games
loading requests
creating requests
offering or denying requests
loading rentals
creating rentals
loading notifications
deleting notifications
loading profile data
saving profile changes
changing password
opening account administration
running administrator actions
```

If the implementation uses a base page, navigation helper, or shared method, it should keep the change simple and focused. The goal is consistent guarding, not a full navigation framework rewrite.

## Menu And Navigation Requirements

`MenuBarPage` should only be reachable after login.

`MenuBarViewModel` should continue to build normal navigation links for logged-in users.

The Admin link should continue to appear only for users with the `Administrator` role.

The menu should not show protected links before login.

Logout should continue to clear the session and return to `LoginPage`.

After logout, the user should not be able to navigate back into the menu or any protected page.

Notification click navigation should also respect the login guard. If a notification tries to open `NotificationsPage` while the user is not logged in, the app should route back to login instead of opening the protected page.

## Admin Requirements

Administrator behavior should match the web app meaning.

The administrator role is:

```text
Administrator
```

The admin page should remain administrator-only.

The following admin operations should remain administrator-only:

```text
view all accounts
suspend account
unsuspend account
reset password
unlock account
```

The existing `AdminService` authorization checks should be preserved.

The `AdminPage` should not only hide controls. It should also prevent admin data loading and admin actions when the user is not an administrator.

If a logged-in normal user somehow reaches the admin page, the user should see an unauthorized/access denied message or be redirected away. The normal user must not be able to execute admin actions.

## Games And Administrator Parity

The desktop Games/Listings behavior should be aligned with the web app behavior.

For a logged-in normal user:

```text
Listings/My Games should show only games owned by that user.
The normal user should be able to create games for their own account.
The normal user should be able to edit only their own games.
The normal user should be able to delete only their own games.
The normal user should not see or manage games owned by other users.
```

For a logged-in administrator:

```text
Games/Listings should allow the administrator to see all games.
The administrator should be able to edit any game.
The administrator should be able to delete any game.
The administrator should still be able to create games for the administrator account unless the team explicitly adds owner selection.
```

The desktop implementation should not make normal users see all games.

The desktop implementation should not give all-games management to users whose role is not `Administrator`.

If the desktop page title stays visually close to the old WinUI layout, that is acceptable, but the behavior should match the web rules.

## User-Owned Feature Rules

For normal user features, the desktop application should use the current logged-in user from `SessionContext` / `CurrentUserContext`.

User-owned pages should stay user-owned:

```text
My Requests -> requests created by the current user
Others' Requests -> open requests for games owned by the current user
My Rentals -> rentals where the current user is the renter
Others' Rentals -> rentals where the current user owns the rented game
Notifications -> notifications received by the current user
Profile -> current user's own account profile
```

For these features, administrators behave like normal users unless an administrator-specific behavior is explicitly required.

This means:

```text
administrator My Requests -> requests created by the administrator account
administrator Others' Requests -> requests for games owned by the administrator account
administrator My Rentals -> rentals where the administrator is the renter
administrator Others' Rentals -> rentals where the administrator owns the game
administrator Notifications -> notifications received by the administrator account
administrator Profile -> administrator's own profile
```

The special administrator expansion currently required by the web feature set is Games/Listings all-games management and Account Administration.

## Authentication Service Expectations

The existing `AuthService` should continue to handle:

```text
login
register
logout
forgot password message
suspended account rejection
failed login tracking
session population
session clearing
```

Suspended accounts should not be allowed to log in.

Failed login locking should be checked carefully. The repository already stores `FailedAttempts` and `LockedUntil`, and the admin feature can unlock accounts. Login should not allow an account to log in while it is actively locked.

Register should continue to create a `Standard User` account.

Forgot password should continue to show the existing meaning:

```text
Please contact the Administrator at admin@boardrent.com.
```

The implementation should not add a new password reset email/token system unless explicitly required separately.

## Dependency Injection And Code Quality Rules

No code comments should be added.

Code should follow the existing StyleCop and project style conventions.

Variable names should be descriptive. One-character variable names should not be introduced.

Edits should stay scoped to WinUI authorization guarding and the small behavior fixes needed for parity.

Unrelated refactors should not be included.

The existing dependency injection setup in `App.xaml.cs` should be reused.

Dependencies should be resolved through the existing DI setup instead of manually creating services.

The implementation should not create a parallel service container.

The implementation should not remove the existing `SessionContext`, `CurrentUserContext`, `AuthService`, or `AdminService` patterns unless replacing them with an equivalent project-wide pattern is genuinely necessary.

## What Should Not Be Changed

Do not rewrite the whole WinUI navigation system unless a smaller guarded navigation change cannot solve the problem.

Do not change the role names.

Do not introduce new roles unless the team explicitly decides to do so.

Do not make `AdminPage` available to normal users.

Do not make normal users see or manage all games.

Do not remove the existing login/register flow.

Do not remove the existing forgot-password message behavior.

Do not move business logic into views.

Do not add a new frontend framework or UI library for this task.

Do not touch MVC feature implementation unless a shared role/authorization constant must be aligned across projects.

## Links With Other Work

This task overlaps with the Auth feature because login, logout, register, roles, failed login attempts, and session state are involved.

This task overlaps with Admin / Account Management because the administrator page and admin actions must remain administrator-only.

This task overlaps with Games / Listings because desktop administrator game behavior should match the web behavior.

This task overlaps lightly with Requests, Rentals, Notifications, and Profile because those pages must be protected from unauthenticated users and must continue using the current logged-in user.

Shared files likely touched by this task:

```text
BoardRentAndProperty/BoardRentAndProperty/App.xaml.cs
BoardRentAndProperty/BoardRentAndProperty/Views/MenuBarPage.xaml.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/MenuBarViewModel.cs
BoardRentAndProperty/BoardRentAndProperty/Utilities/ISessionContext.cs
BoardRentAndProperty/BoardRentAndProperty/Utilities/SessionContext.cs
BoardRentAndProperty/BoardRentAndProperty/Services/AuthService.cs
BoardRentAndProperty/BoardRentAndProperty/Views/AdminPage.xaml.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/ListingsViewModel.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/EditGameViewModel.cs
BoardRentAndProperty/BoardRentAndProperty/Services/GameService.cs
```

Feature pages may need small guard calls, but the implementation should avoid making every page contain a different hand-written authorization pattern.

## What Counts As Done

The desktop app starts on login and public registration still works.

A user who is not logged in cannot access any feature page.

After logout, the user cannot go back into a protected page.

Normal users can open normal feature pages after login.

Normal users cannot open the admin page or execute admin actions.

Administrators can open the admin page and execute admin actions.

The Admin link is visible only for administrators.

Normal users see and manage only their own games.

Administrators can see and manage all games in the desktop app, matching the web app behavior.

Requests, rentals, notifications, and profile still use the current logged-in user's identity.

Suspended users cannot log in.

Actively locked users cannot log in until unlocked or until the lock expires.

The app builds after the changes.

The final behavior can be manually verified with:

```text
not logged in user
logged-in Standard User
logged-in Administrator
logout then back-navigation attempt
normal user trying to reach admin page
administrator opening account administration
normal user games page
administrator games page
```

