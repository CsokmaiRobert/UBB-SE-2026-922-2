# WinUI Login Debugging Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:systematic-debugging first, then use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Find and fix the WinUI login failure without guessing, while preserving the working deployed API and MVC web auth flow.

**Architecture:** MVC and WinUI both use `BoardRentAndProperty.ApiClient` to call `POST api/auth/login`. The web flow succeeds by creating an MVC cookie after `IAuthProxyService.LoginAsync`; the WinUI flow should call the same API client, populate `ISessionContext`, then navigate to `MenuBarPage`. The plan instruments each boundary so the permanent fix lands at the failing boundary only.

**Tech Stack:** .NET 10, WinUI 3, ASP.NET Core MVC, shared `BoardRentAndProperty.ApiClient`, NUnit tests, deployed API at the configured `ApiBaseUrl`.

---

## Codebase Findings

The assignment relevant to this bug is mainly `BoardRentAndProperty/WinUI-AuthorizationGuards.md`: WinUI should start on `LoginPage`, populate `SessionContext` after successful login, allow normal users into normal pages, allow administrators into admin pages, and keep unauthenticated users out of protected pages.

The web login path is:

```text
GUI-BRAP/Views/Auth/Login.cshtml
-> GUI-BRAP/Controllers/AuthController.cs
-> GUI-BRAP/ProxyServices/IAuthProxyService.cs
-> GUI-BRAP/Infrastructure/AuthProxyServiceAdapter.cs
-> BoardRentAndProperty.ApiClient/AuthService.cs
-> POST api/auth/login
-> BoardRentAndProperty.Api/Controllers/AuthController.cs
-> BoardRentAndProperty.Api/Services/AuthService.cs
```

The WinUI login path is:

```text
BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml
-> BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml.cs
-> BoardRentAndProperty/BoardRentAndProperty/ViewModels/LoginViewModel.cs
-> BoardRentAndProperty.ApiClient/AuthService.cs
-> POST api/auth/login
-> SessionContext.Populate(...)
-> App.OnUserLoggedIn()
-> MenuBarPage
```

Evidence already gathered:

```text
ApiBaseUrl in GUI-BRAP/appsettings.json: http://172.30.254.241:80
ApiBaseUrl in App.config: http://172.30.254.241:80
WinUI output config contains the same ApiBaseUrl.
GET http://172.30.254.241:80/api/auth/forgot-password returned 200.
POST http://172.30.254.241:80/api/auth/login with fake credentials returned the expected API 401 envelope.
dotnet test --filter FullyQualifiedName~LoginViewModelTests passed 5/5.
There is an existing uncommitted change in LoginViewModel.cs adding a broad catch to show unexpected login/navigation errors.
```

Current strongest hypothesis: the shared API client and deployed API are reachable, so the failure is probably in one of the WinUI-only runtime boundaries: input binding, post-login session/navigation, notification startup, or a protected page load immediately after login. The plan below is diagnostic-first because the exact boundary still needs a real valid-account reproduction.

## File Structure

Files to inspect or modify during execution:

```text
BoardRentAndProperty/App.config
BoardRentAndProperty/BoardRentAndProperty/App.xaml.cs
BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml
BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/LoginViewModel.cs
BoardRentAndProperty/BoardRentAndProperty/Utilities/SessionContext.cs
BoardRentAndProperty/BoardRentAndProperty/Services/DesktopNotificationService.cs
BoardRentAndProperty/BoardRentAndProperty/Services/Listeners/NotificationClient.cs
BoardRentAndProperty/BoardRentAndProperty/ViewModels/MenuBarViewModel.cs
BoardRentAndProperty/BoardRentAndProperty.Tests/ViewModels/LoginViewModelTests.cs
BoardRentAndProperty/BoardRentAndProperty.ApiClient/AuthService.cs
BoardRentAndProperty/BoardRentAndProperty.ApiClient/ApiResponseReader.cs
```

Do not modify MVC files unless the investigation proves the shared API client contract is wrong. The web path is the working reference.

### Task 1: Lock Down Baseline Evidence

**Files:**
- Read: `BoardRentAndProperty/App.config`
- Read: `BoardRentAndProperty/GUI-BRAP/appsettings.json`
- Read: `BoardRentAndProperty/BoardRentAndProperty/bin/x64/Debug/net10.0-windows10.0.19041.0/win-x64/BoardRentAndProperty.dll.config`
- Read: `BoardRentAndProperty/BoardRentAndProperty/ViewModels/LoginViewModel.cs`
- Read: `BoardRentAndProperty/BoardRentAndProperty.ApiClient/AuthService.cs`

- [ ] **Step 1: Confirm the working tree before editing**

Run:

```powershell
git status --short
git diff -- BoardRentAndProperty/BoardRentAndProperty/ViewModels/LoginViewModel.cs
```

Expected: `LoginViewModel.cs` may already be modified with exception handling. Preserve that change unless the user explicitly asks to remove it.

- [ ] **Step 2: Confirm API configuration parity**

Run:

```powershell
Get-Content -Raw BoardRentAndProperty/App.config
Get-Content -Raw BoardRentAndProperty/GUI-BRAP/appsettings.json
Get-Content -Raw BoardRentAndProperty/BoardRentAndProperty/bin/x64/Debug/net10.0-windows10.0.19041.0/win-x64/BoardRentAndProperty.dll.config
```

Expected: all WinUI and MVC runtime configs point to the same deployed API base URL.

- [ ] **Step 3: Confirm deployed API reachability from the desktop machine**

Run:

```powershell
Invoke-WebRequest -UseBasicParsing -Method GET 'http://172.30.254.241:80/api/auth/forgot-password' |
    Select-Object StatusCode,StatusDescription,Content
```

Expected: `200 OK` and the administrator contact message. If this fails, the WinUI login failure is a network/config problem, not a UI login problem.

- [ ] **Step 4: Confirm API auth error envelope**

Run:

```powershell
$body = '{"usernameOrEmail":"__probe__","password":"__probe__","rememberMe":false}'
try {
    Invoke-WebRequest -UseBasicParsing -Method POST 'http://172.30.254.241:80/api/auth/login' -ContentType 'application/json' -Body $body |
        Select-Object StatusCode,StatusDescription,Content
} catch {
    $response = $_.Exception.Response
    $reader = New-Object System.IO.StreamReader($response.GetResponseStream())
    [pscustomobject]@{
        StatusCode = [int]$response.StatusCode
        StatusDescription = $response.StatusDescription
        Content = $reader.ReadToEnd()
    }
}
```

Expected: `401 Unauthorized` with JSON containing `invalid_credentials`. This proves WinUI should be able to show a useful API error when credentials are rejected.

### Task 2: Add Temporary Login Boundary Diagnostics

**Files:**
- Modify: `BoardRentAndProperty/BoardRentAndProperty/ViewModels/LoginViewModel.cs`
- Test: `BoardRentAndProperty/BoardRentAndProperty.Tests/ViewModels/LoginViewModelTests.cs`

- [ ] **Step 1: Add a debug-only trace helper**

In `LoginViewModel.cs`, keep `using System.Diagnostics;` and add this method inside `LoginViewModel`:

```csharp
[Conditional("DEBUG")]
private static void TraceLogin(string message)
{
    Debug.WriteLine($"WinUI login: {message}");
}
```

- [ ] **Step 2: Trace each login boundary without logging secrets**

Inside `LoginAsync`, add trace calls around the existing flow. The important trace points are:

```csharp
TraceLogin(
    $"attempt hasIdentifier={!string.IsNullOrWhiteSpace(this.UsernameOrEmail)}, " +
    $"identifierLength={this.UsernameOrEmail?.Length ?? 0}, " +
    $"passwordLength={this.Password?.Length ?? 0}, rememberMe={this.RememberMe}");
```

Immediately after `authService.LoginAsync` returns:

```csharp
TraceLogin(
    $"api result success={loginResult.Success}, " +
    $"status={(int?)loginResult.StatusCode}, " +
    $"errorCode={loginResult.ErrorCode ?? string.Empty}, " +
    $"hasData={loginResult.Data is not null}");
```

Immediately after `sessionContext.Populate(loginResult.Data)`:

```csharp
TraceLogin(
    $"session populated isLoggedIn={this.sessionContext.IsLoggedIn}, " +
    $"accountId={this.sessionContext.AccountId}, " +
    $"role={this.sessionContext.Role}");
```

Immediately before and after the success callback:

```csharp
TraceLogin("invoking success callback");
this.OnLoginSuccess?.Invoke(userRole);
TraceLogin("success callback returned");
```

Inside the existing exception catch:

```csharp
TraceLogin($"exception {exception.GetType().FullName}: {exception.Message}");
```

Expected: the next WinUI login attempt will tell us whether the app fails before the command, during API login, during session population, inside `App.OnUserLoggedIn`, or after navigation.

- [ ] **Step 3: Add a regression test for hidden post-login exceptions**

Add this test to `LoginViewModelTests.cs`:

```csharp
[Test]
public async Task LoginAsync_SuccessCallbackThrows_SetsErrorMessageAndStopsLoading()
{
    this.systemUnderTest.UsernameOrEmail = "user";
    this.systemUnderTest.Password = "ValidPassword123!";
    this.systemUnderTest.OnLoginSuccess = _ => throw new InvalidOperationException("Navigation failed.");

    var profile = new AccountProfileDataTransferObject
    {
        Username = "user",
        Role = new RoleDataTransferObject { Name = "Standard User" },
    };

    this.authService.LoginResult = ServiceResult<AccountProfileDataTransferObject>.Ok(profile);

    await this.systemUnderTest.LoginCommand.ExecuteAsync(null);

    Assert.That(this.systemUnderTest.ErrorMessage, Does.Contain("Navigation failed."));
    Assert.That(this.systemUnderTest.IsLoading, Is.False);
}
```

- [ ] **Step 4: Run the focused tests**

Run:

```powershell
dotnet test BoardRentAndProperty/BoardRentAndProperty.Tests/BoardRentAndProperty.Tests.csproj --filter FullyQualifiedName~LoginViewModelTests --no-restore
```

Expected: all login view-model tests pass. If the new test fails, finish the catch/finally behavior in `LoginViewModel.cs` before running the app.

### Task 3: Reproduce With a Valid Account and Classify the Failure

**Files:**
- Read: `BoardRentAndProperty/BoardRentAndProperty/ViewModels/LoginViewModel.cs`
- Read: `BoardRentAndProperty/BoardRentAndProperty/App.xaml.cs`
- Read: `BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml`
- Read: `BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml.cs`

- [ ] **Step 1: Start WinUI from the project**

Run:

```powershell
dotnet run --project BoardRentAndProperty/BoardRentAndProperty/BoardRentAndProperty.csproj -p:SkipNotificationServerBuild=true
```

Expected: WinUI opens on `LoginPage`.

- [ ] **Step 2: Log in with an account that succeeds in the MVC web app**

Use the same username/email and password that succeeds on the deployed web side. Watch the Visual Studio Output window, DebugView, or terminal debug output for `WinUI login:` lines.

- [ ] **Step 3: Classify using the trace**

Use this decision table:

```text
No "attempt" trace:
  The button command or DataContext is not connected. Inspect LoginPage.xaml Command binding and LoginPage.xaml.cs ViewModel resolution.

attempt trace has passwordLength=0 after typing a password:
  The PasswordBox binding is not updating LoginViewModel.Password. Fix the LoginPage PasswordBox boundary.

api result success=false:
  The API rejected the credentials or account state. Use status/errorCode/error to distinguish invalid credentials, suspended account, locked account, timeout, or service unavailable.

api result success=true and session populated isLoggedIn=true, but no "success callback returned":
  Login succeeded and the failure is inside App.OnUserLoggedIn.

"success callback returned" appears, but the app stays on login or jumps back:
  Login succeeded and the failure is after navigation, probably MenuBarPage, a guarded page, or initial protected data load.
```

Expected: one branch is proven. Do not implement fixes from multiple branches.

### Task 4: Apply the Single Confirmed Fix

**Files depend on the branch proven in Task 3. Choose exactly one branch.**

- [ ] **Branch A: Fix disconnected command or DataContext**

Modify `BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml.cs` to fail loudly if DI returns null:

```csharp
this.ViewModel = Ioc.Default.GetService<LoginViewModel>()
    ?? throw new InvalidOperationException("LoginViewModel is not registered.");
```

Expected: if DI is broken, startup now reports the real registration failure instead of silently leaving a broken page.

- [ ] **Branch B: Fix PasswordBox not updating the view model**

Modify `BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml`:

```xml
<PasswordBox x:Name="PasswordInput"
             Header="Password"
             PasswordChanged="PasswordInput_PasswordChanged" />
```

Modify `BoardRentAndProperty/BoardRentAndProperty/Views/LoginPage.xaml.cs`:

```csharp
private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs eventArgs)
{
    this.ViewModel.Password = ((PasswordBox)sender).Password;
}
```

If registration has the same issue, apply the same pattern to `RegisterPage.xaml` and `RegisterPage.xaml.cs` for `Password` and `ConfirmPassword`.

- [ ] **Branch C: Fix API rejection or account state display**

If the trace shows `success=false`, keep the API as source of truth and ensure the message is visible in WinUI. In `LoginViewModel.cs`, preserve:

```csharp
this.ErrorMessage = loginResult.Error ?? "Login failed.";
```

Then manually test these API outcomes in WinUI:

```text
401 invalid_credentials -> shows "Invalid username or password."
403 account_suspended -> shows "This account has been suspended."
403 account_locked -> shows "This account is locked. Please contact an administrator or try again later."
503 service unavailable -> shows "Cannot connect to the API. Check that the API server is running and reachable."
```

If the message is not visible, inspect `LoginPage.xaml` and keep the existing error binding:

```xml
<TextBlock Text="{Binding ErrorMessage}" Foreground="Red" HorizontalAlignment="Center" TextWrapping="Wrap" />
```

- [ ] **Branch D: Fix post-login failure inside App.OnUserLoggedIn**

Modify `BoardRentAndProperty/BoardRentAndProperty/App.xaml.cs` so nonessential notification work cannot block successful login navigation:

```csharp
public static void OnUserLoggedIn()
{
    if (Application.Current is not App appInstance || appInstance.RootFrame == null)
    {
        return;
    }

    var resolvedSessionContext = Services.GetRequiredService<ISessionContext>();
    var resolvedMenuBarViewModel = Services.GetRequiredService<MenuBarViewModel>();
    resolvedMenuBarViewModel.Rebuild();

    NavigateTo(typeof(MenuBarPage), clearBackStack: true);

    try
    {
        var resolvedNotificationService = Services.GetRequiredService<IDesktopNotificationService>();
        var resolvedNotificationsViewModel = Services.GetRequiredService<NotificationsViewModel>();
        resolvedNotificationService.SubscribeToServer(resolvedSessionContext.AccountId);
        _ = resolvedNotificationsViewModel.LoadNotificationsForUserAsync(resolvedSessionContext.AccountId);
    }
    catch (InvalidOperationException exception)
    {
        Debug.WriteLine($"Post-login notification startup failed: {exception}");
    }
}
```

Expected: a successful API login reaches `MenuBarPage` even if notification startup has a runtime problem. If the caught exception type in the trace is different, catch that exact expected exception type instead of broadening permanently.

- [ ] **Branch E: Fix immediate protected-page navigation after success**

If the trace reaches `success callback returned`, inspect `MenuBarPage` and the first loaded page. Keep the authorization rule from `WinUI-AuthorizationGuards.md`:

```text
not logged in -> LoginPage and clear back stack
logged in and allowed -> continue normally
logged in but not administrator for admin -> block or show unauthorized
```

The expected code path is that `SessionContext.IsLoggedIn` is true before `NavigateTo(typeof(MenuBarPage), clearBackStack: true)`. If a protected page checks a different source of truth, change it to use `ISessionContext` or `IDesktopAuthorizationService`.

### Task 5: Verify the Chosen Fix

**Files:**
- Test: `BoardRentAndProperty/BoardRentAndProperty.Tests/ViewModels/LoginViewModelTests.cs`
- Test manually: WinUI login page

- [ ] **Step 1: Run focused tests**

Run:

```powershell
dotnet test BoardRentAndProperty/BoardRentAndProperty.Tests/BoardRentAndProperty.Tests.csproj --filter FullyQualifiedName~LoginViewModelTests --no-restore
```

Expected: all login tests pass.

- [ ] **Step 2: Run related WinUI tests**

Run:

```powershell
dotnet test BoardRentAndProperty/BoardRentAndProperty.Tests/BoardRentAndProperty.Tests.csproj --filter "FullyQualifiedName~RegisterViewModelTests|FullyQualifiedName~MenuBarViewModelTests|FullyQualifiedName~AdminViewModelTests" --no-restore
```

Expected: related auth/navigation tests pass.

- [ ] **Step 3: Run the WinUI manual scenarios**

Manual checks:

```text
Wrong password shows a visible API error and stays on LoginPage.
Valid standard user logs in and reaches MenuBarPage.
Valid administrator logs in and sees the Admin menu entry.
Logout clears SessionContext and returns to LoginPage.
After logout, back navigation does not reopen protected pages.
```

- [ ] **Step 4: Remove temporary traces only after verification**

Remove the `TraceLogin` helper and trace calls if they are no longer needed. Keep the user-visible exception handling only if the root cause showed hidden post-login failures.

- [ ] **Step 5: Run final build**

Run:

```powershell
dotnet build BoardRentAndProperty/BoardRentAndProperty.sln --no-restore
```

Expected: build succeeds. Existing warnings can be recorded, but no new errors should be introduced.

## Self-Review

Spec coverage:

```text
WinUI login starts on LoginPage: covered by manual verification.
SessionContext is source of truth: covered in Task 3 and Branch E.
API remains source of auth business logic: covered by shared ApiClient path and Branch C.
Normal/admin role behavior: covered by manual standard-user/admin checks.
No MVC regression: MVC files are not touched unless shared API behavior is proven wrong.
```

Placeholder scan:

```text
The plan intentionally has diagnostic branches because root cause is not proven yet.
Each branch has concrete files and concrete edits.
The executor must choose exactly one branch after Task 3 evidence.
```

Type consistency:

```text
IAuthService is BoardRentAndProperty.ApiClient.IAuthService.
Session state is ISessionContext / SessionContext.
Authorization checks use IDesktopAuthorizationService.
Roles remain "Administrator" and "Standard User".
```

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-05-21-winui-login-debugging.md`.

Recommended execution: run Tasks 1-3 first and stop after the failure branch is proven. Then implement exactly one branch from Task 4 and run Task 5 verification.
