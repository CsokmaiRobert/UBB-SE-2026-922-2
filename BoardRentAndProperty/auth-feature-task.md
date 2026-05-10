# Auth Feature Research And Task Notes

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

The MVC project is:

```text
BoardRentAndProperty/GUI-BRAP
```

The API project is:

```text
BoardRentAndProperty/BoardRentAndProperty.Api
```

The shared DTO project is:

```text
BoardRentAndProperty/BoardRentAndProperty.Contracts
```

The auth feature is partially present in MVC already. Login exists, but the web auth workflow is not complete yet.

Current MVC auth files:

```text
GUI-BRAP/Controllers/AuthController.cs
GUI-BRAP/Services/IAuthProxyService.cs
GUI-BRAP/Services/AuthProxyService.cs
GUI-BRAP/Models/Auth/LoginViewModel.cs
GUI-BRAP/Views/Auth/Login.cshtml
GUI-BRAP/Views/Auth/AccessDenied.cshtml
```

Current API auth files:

```text
BoardRentAndProperty.Api/Controllers/AuthController.cs
BoardRentAndProperty.Api/Services/IAuthService.cs
BoardRentAndProperty.Api/Services/AuthService.cs
```

Current desktop auth reference files:

```text
BoardRentAndProperty/Views/LoginPage.xaml
BoardRentAndProperty/Views/RegisterPage.xaml
BoardRentAndProperty/ViewModels/LoginViewModel.cs
BoardRentAndProperty/ViewModels/RegisterViewModel.cs
BoardRentAndProperty/Services/AuthService.cs
```

The desktop project already uses the API for login, register, logout, and forgot password. The MVC auth feature should follow the same general API-based direction, adapted to MVC cookie authentication.

## Relationship With Architecture Preparation

This feature depends on the shared foundation described in:

```text
BoardRentAndProperty/architecture-task.md
```

At the moment this auth task was written, that architecture foundation may not yet be implemented in the codebase. By the time active auth feature work starts, the MVC project is expected to have the shared pieces from `architecture-task.md`, such as:

- role constants;
- current-user claim helpers;
- proxy service DI registration convention;
- configured API `HttpClient`;
- shared proxy error handling;
- authentication and authorization conventions.

The auth feature should reuse those shared pieces instead of duplicating them.

For example, if the architecture preparation adds:

```text
AppRoles
ClaimsPrincipalExtensions
AddProxyServices()
ApiClientNames
ProxyServiceException
```

then this feature should use those existing names and patterns.

## Purpose Of The Auth Feature

The auth feature is responsible for the MVC web authentication workflow.

It should allow the MVC website to:

- show a login page;
- authenticate against the API;
- create the MVC authentication cookie;
- store useful claims for the logged-in user;
- log out;
- show access denied when the user is logged in but not allowed;
- support registration if web registration is required;
- support forgot-password behavior if web forgot-password is required.

The assignment requires:

```text
All functionalities need to be guarded from unauthorized users.
```

For the MVC website, the minimum expected behavior is:

- users who are not logged in cannot access normal application pages;
- users who are not logged in are redirected to login;
- public pages are explicitly marked with `[AllowAnonymous]`;
- logged-in users receive claims that other features can use for authorization and user-specific data.

Authentication answers:

```text
Who is this user?
```

Authorization answers:

```text
Is this user allowed to perform this action?
```

This feature mainly prepares and completes authentication. It also provides the claims needed by later authorization checks.

## Existing API Behavior

The API auth controller currently exposes:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/logout
GET  /api/auth/forgot-password
```

The API controller calls `IAuthService`.

The API service calls:

```text
IAccountRepository
IFailedLoginRepository
```

So the API side already follows the required direction:

```text
API AuthController
-> IAuthService / AuthService
-> Repositories
-> Database
```

The MVC feature should call this API through `AuthProxyService`.

## Existing API Login Behavior

API login accepts:

```text
LoginDataTransferObject
```

The DTO contains:

```text
UsernameOrEmail
Password
RememberMe
```

The API accepts either username or email.

If the account does not exist, the API returns an error:

```text
Invalid username or password.
```

If the account is suspended, the API returns an error:

```text
This account has been suspended.
```

If the password is wrong, the API increments failed login count and returns:

```text
Invalid username or password.
```

If login succeeds, the API returns:

```text
AccountProfileDataTransferObject
```

That profile includes:

```text
Id
Username
DisplayName
Email
PhoneNumber
AvatarUrl
Role
IsSuspended
IsLocked
Country
City
StreetName
StreetNumber
```

The role is important for MVC authorization. The seeded role names are:

```text
Administrator
Standard User
```

The code should not use `"Admin"` as the role name.

## Existing API Register Behavior

API register accepts:

```text
RegisterDataTransferObject
```

The DTO contains:

```text
DisplayName
Username
Email
Password
ConfirmPassword
PhoneNumber
Country
City
StreetName
StreetNumber
```

The API currently checks whether the username already exists.

If the username already exists, the API returns an error formatted like:

```text
Username|Username is already taken.
```

The `Field|Message` format is useful because MVC can display that error on a specific field.

If registration succeeds, the API creates a new account and assigns the default role:

```text
Standard User
```

The desktop auth service registers and then immediately logs in with the new credentials. The MVC behavior can follow the same user experience if that fits the web flow.

## Existing API Logout Behavior

API logout currently returns success.

For MVC, the important logout action is clearing the MVC cookie:

```text
HttpContext.SignOutAsync(...)
```

The MVC proxy can still call the API logout endpoint for consistency, but the web user is not actually logged out unless the MVC cookie is removed.

## Existing API Forgot-Password Behavior

API forgot-password currently returns a simple message:

```text
Please contact the Administrator at admin@boardrent.com.
```

If MVC exposes forgot password, the page can show this message after calling the API.

This is not a full email reset flow.

## Current MVC Auth Behavior

MVC currently has cookie authentication in `Program.cs`.

The login path is:

```text
/Auth/Login
```

The logout path is:

```text
/Auth/Logout
```

The access denied path is:

```text
/Auth/AccessDenied
```

MVC also has a fallback policy that requires authenticated users by default.

That means normal pages are guarded unless explicitly marked:

```csharp
[AllowAnonymous]
```

The MVC login action currently:

- validates the login view model;
- calls `AuthProxyService.LoginAsync`;
- receives `AccountProfileDataTransferObject`;
- builds a `ClaimsIdentity`;
- signs in with cookie authentication;
- redirects to the local return URL when present;
- otherwise redirects to Games.

The current claims are:

```text
ClaimTypes.NameIdentifier -> profile.Id
ClaimTypes.Name           -> profile.Username
DisplayName               -> profile.DisplayName
ClaimTypes.Role           -> profile.Role.Name
```

This is a good base and should be preserved.

## Target Auth Architecture

The expected final MVC auth direction is:

```text
MVC AuthController
-> IAuthProxyService / AuthProxyService
-> API AuthController
-> IAuthService / AuthService
-> AccountRepository / FailedLoginRepository
-> Database
```

MVC should not call `AppDbContext`.

MVC should not call `IAccountRepository`.

MVC should not duplicate password hashing or login business logic.

The API remains responsible for validating credentials and creating accounts.

The MVC project remains responsible for the web session cookie and MVC claims.

## Work That Needs To Be Understood Before Implementation

## Login

Login already exists in MVC, but it should be verified carefully.

The login page should accept:

```text
username or email
password
remember me
return URL
```

The login POST should:

- call `IAuthProxyService.LoginAsync`;
- handle API errors through the shared proxy error handling;
- create the MVC cookie only when the API login succeeds;
- include account id, username, display name, and role claims;
- redirect only to local return URLs;
- redirect to a safe default page when no return URL exists.

The default redirect currently goes to:

```text
Games/Index
```

That is acceptable unless the team decides on a different authenticated landing page.

## Logout

Logout currently clears the MVC cookie.

The auth feature should decide whether `AuthProxyService` should also expose `LogoutAsync` and call:

```text
POST /api/auth/logout
```

The desktop service already calls the API logout endpoint, but the API logout is currently lightweight.

The MVC logout must always remove the cookie, even if the API logout call is skipped or fails.

Logout should use POST and anti-forgery validation.

## Register

MVC register is not currently implemented.

If web registration is part of the expected MVC workflow, the auth feature should add it.

Register should use a MVC view model instead of binding directly to the DTO in the Razor page.

The view model should include validation attributes for the fields that the form collects.

The MVC register flow should call:

```text
POST /api/auth/register
```

through `AuthProxyService`.

After successful registration, the MVC app can either:

- redirect to login with a success message;
- or automatically log in the new user, matching the desktop behavior.

The desktop service auto-logs in after registration. If MVC does the same, it should call login after successful register and then create the same cookie claims used by normal login.

Registration errors should be displayed clearly.

The API may return field-style errors such as:

```text
Username|Username is already taken.
```

MVC can map that to:

```text
ModelState["Username"]
```

instead of only showing a page-level error.

## Forgot Password

MVC forgot password is not currently implemented.

The API endpoint currently returns a contact message, not a real password reset token.

If exposed in MVC, forgot password can be a simple page or action that calls:

```text
GET /api/auth/forgot-password
```

and displays the returned message.

This should be marked anonymous because users who forgot their password cannot be expected to already be logged in.

## Access Denied

Access denied already exists as a MVC action and view.

It should remain accessible without login:

```csharp
[AllowAnonymous]
```

The access denied page should be used when a logged-in user does not have permission for a feature.

The auth feature should not hardcode all feature authorization rules, but it should make sure the auth flow supports those rules through role claims.

## Role Claims

The login cookie must contain the user's role claim.

The API returns the primary role in:

```text
profile.Role.Name
```

The MVC cookie should store that value as:

```text
ClaimTypes.Role
```

The expected role values are:

```text
Administrator
Standard User
```

Other feature owners will later use:

```csharp
[Authorize(Roles = AppRoles.Administrator)]
```

or helper methods such as:

```csharp
User.IsAdministrator()
```

The auth feature must not accidentally remove or rename the role claim.

## Proxy Service Expectations

`IAuthProxyService` currently has only:

```text
LoginAsync
```

The completed auth feature will likely need proxy methods for:

```text
LoginAsync
RegisterAsync
LogoutAsync
ForgotPasswordAsync
```

The proxy service should use the shared configured API client.

Expected direction:

```csharp
HttpClient client = httpClientFactory.CreateClient(ApiClientNames.BoardRentAndPropertyApi);
```

If the architecture preparation keeps the client name as a string, use the existing shared convention and do not invent a new name.

The proxy should not create `HttpClient` manually.

The proxy should not know about MVC cookies.

The proxy should only call the API and translate API failure into `ProxyServiceException` or a shared result pattern if the architecture preparation provides one.

## Error Handling Expectations

The API error response uses this shape:

```text
Code
Error
Status
```

The existing MVC helper reads at least:

```text
Error
```

The auth feature should use the shared helper instead of parsing every response separately.

Important auth error cases:

```text
invalid username or password
suspended account
username already taken
empty login response
server unavailable
```

Login errors can be shown as page-level errors.

Registration errors should be shown as field-level errors when the API error identifies a field.

The project already has examples of API errors formatted like:

```text
FieldName|Error message
```

Implementation should preserve useful validation messages instead of replacing everything with a generic error.

## MVC View Model Expectations

`LoginViewModel` already exists.

Register should have its own MVC view model if registration is implemented.

A MVC register view model can mirror the registration fields but should contain web validation attributes.

Fields likely needed:

```text
DisplayName
Username
Email
Password
ConfirmPassword
PhoneNumber
Country
City
StreetName
StreetNumber
ReturnUrl if needed
```

Password and confirm password should be checked in MVC before calling the API.

The API currently receives `ConfirmPassword`, but `AuthService.RegisterAsync` does not visibly use it in the current code. The MVC side should still validate matching passwords for a good user experience.

## Authorization Attributes

Public auth pages should be anonymous:

```csharp
[AllowAnonymous]
```

Expected anonymous pages:

```text
Login GET
Login POST
Register GET if implemented
Register POST if implemented
ForgotPassword GET/POST if implemented
AccessDenied GET
```

Logout should require an authenticated user through the fallback policy or an explicit:

```csharp
[Authorize]
```

Logout should not be a GET action.

Normal feature pages should not be marked anonymous.

## UI Expectations

The auth pages should follow the shared UI conventions from:

```text
BoardRentAndProperty/UI-task.md
```

The auth feature owner should not introduce extra frontend frameworks.

The login/register/forgot-password pages should use Bootstrap and MVC validation.

The auth feature should not redesign the whole layout.

Navbar links may depend on the shared layout and final review stage. The auth feature should keep its changes compatible with the layout, but it does not need to take ownership of all navigation.

## Files Likely To Be Touched

Likely MVC files:

```text
GUI-BRAP/Controllers/AuthController.cs
GUI-BRAP/Services/IAuthProxyService.cs
GUI-BRAP/Services/AuthProxyService.cs
GUI-BRAP/Models/Auth/LoginViewModel.cs
GUI-BRAP/Models/Auth/RegisterViewModel.cs
GUI-BRAP/Models/Auth/ForgotPasswordViewModel.cs
GUI-BRAP/Views/Auth/Login.cshtml
GUI-BRAP/Views/Auth/Register.cshtml
GUI-BRAP/Views/Auth/ForgotPassword.cshtml
GUI-BRAP/Views/Auth/AccessDenied.cshtml
```

Some of these files may not exist yet and would only be needed if the corresponding page is implemented.

Possible shared files, depending on the architecture foundation:

```text
GUI-BRAP/Utilities/ClaimsPrincipalExtensions.cs
GUI-BRAP/Constants/AppRoles.cs
GUI-BRAP/Services/ServiceCollectionExtensions.cs
GUI-BRAP/Services/ApiClientNames.cs
```

The exact filenames for shared foundation may differ depending on how `architecture-task.md` is implemented.

## Files That Should Usually Not Be Touched

The auth feature should not normally need to edit:

```text
GUI-BRAP/RequestsController.cs
GUI-BRAP/RentalsController.cs
GUI-BRAP/NotificationsController.cs
GUI-BRAP/AccountsController.cs
GUI-BRAP/RolesController.cs
```

Those are feature-specific scaffold controllers.

The auth feature should not directly edit repositories unless a real API auth bug is discovered.

The auth feature should not change the database schema unless the team explicitly decides that registration/login data is missing something essential.

## Links With Other Feature Owners

This feature is connected to every other feature because every feature depends on logged-in user identity.

Important shared data:

```text
current account id
username
display name
role name
authentication cookie
return URL behavior
logout behavior
```

Account/Profile depends on the auth claims to know which profile to load.

Admin/Account Management depends on the role claim to protect administrator-only pages.

Games depends on the account id to set or check game ownership.

Requests depends on the account id for renter-side and owner-side workflows.

Rentals depends on the account id for renter and owner rental lists.

Notifications depends on the account id to show only the current user's notifications.

UI preparation may touch the login/logout area in the navbar. Auth changes should stay compatible with that shared layout.

## Implementation Hints

The current MVC login flow already contains the most important cookie sign-in code.

The existing `BuildIdentity` method creates the correct claim types:

```csharp
ClaimTypes.NameIdentifier
ClaimTypes.Name
ClaimTypes.Role
```

This pattern should be preserved.

If registration auto-login is implemented, the cookie creation should reuse the same identity-building logic as login.

If the architecture preparation moves identity building or role helpers somewhere shared, use the shared version rather than duplicating claim-building logic.

`AuthProxyService` can follow the same style as `GameProxyService`:

```text
create named API client
call endpoint
ensure success with shared helper
read response DTO
return typed result
```

For registration:

```text
POST api/auth/register
```

For API logout:

```text
POST api/auth/logout
```

For forgot password:

```text
GET api/auth/forgot-password
```

The desktop `BoardRentAndProperty/Services/AuthService.cs` is a useful reference for register, logout, and forgot-password behavior, but the MVC implementation should use async MVC actions and cookie auth instead of desktop `SessionContext`.

Return URL handling should keep the existing safety check:

```csharp
Url.IsLocalUrl(returnUrl)
```

Do not redirect to arbitrary external URLs from login.

Registration password confirmation can be validated in the MVC view model using normal data annotations.

API field errors like:

```text
Username|Username is already taken.
```

can be split into field name and message before adding to `ModelState`.

## Verification Scenarios

The auth feature should be considered ready when these scenarios work in the MVC website:

```text
Unauthenticated user opens /Games -> redirected to /Auth/Login
Unauthenticated user opens /Auth/Login -> page loads
User logs in with valid username/password -> cookie is created and user reaches authenticated page
User logs in with email/password -> login succeeds
User logs in with wrong password -> useful error is displayed
Suspended user cannot log in -> useful error is displayed
Logged-in user logs out -> cookie is cleared and protected pages require login again
AccessDenied page can be displayed
Role claim is present after login
Current account id is present after login
Register creates an account if MVC registration is implemented
Forgot password shows the API message if MVC forgot-password is implemented
```

## What Should Not Be Done In This Feature

Do not make MVC call the database directly.

Do not create accounts directly from MVC using `AppDbContext`.

Do not hash passwords in MVC.

Do not duplicate the API's auth business logic in MVC.

Do not hardcode `"Admin"` as a role.

Do not remove the role claim from the login cookie.

Do not mark normal application pages with `[AllowAnonymous]`.

Do not implement feature-specific authorization rules for Games, Requests, Rentals, Notifications, or Admin pages inside this feature.

Do not introduce new frontend frameworks or libraries.

Do not redesign the shared layout as part of auth.

Do not remove the temporary old scaffold controllers as part of auth.

## Completion Criteria

This feature is ready when:

- MVC login works through the API;
- MVC logout clears the cookie;
- auth errors are displayed clearly;
- login stores account id, username, display name, and role claims;
- unauthenticated users cannot access protected MVC pages;
- public auth pages are explicitly anonymous;
- `IAuthProxyService` and `AuthProxyService` cover the auth endpoints needed by the MVC scope;
- register works if MVC registration is included;
- forgot password works if MVC forgot-password is included;
- the feature follows the shared architecture and DI conventions;
- the MVC project builds;
- the API and MVC projects run together for the tested auth flows.

