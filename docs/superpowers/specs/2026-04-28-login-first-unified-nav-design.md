# Login-First Unified Nav — Design Spec

**Date:** 2026-04-28
**Target solution:** `BoardRentAndProperty/`
**Status:** Approved by user, ready for implementation plan

---

## 1. Goal

Restructure the existing `BoardRentAndProperty` merge so that the app boots into the BoardRent `LoginPage`, and after successful login (or registration) presents a single unified left-nav containing every feature from both PaM and BoardRent. The currently-logged-in `Account` drives the identity used by every feature, including PaM features that today key off a CLI-supplied `int CurrentUserId`.

This is an architectural change to the existing merge — not a re-merge. The existing merged sources stay; what changes is the lifecycle (boot path), the navigation surface (one combined nav), and the identity wiring (session-driven, not CLI-driven).

---

## 2. Decisions taken during brainstorming

| # | Question | Decision |
|---|---|---|
| 1 | Account ↔ PaM User bridge | Add `PamUserId INT NULL UNIQUE` column to `Account`. On register, create a matching `Users` row and store its `id` on the Account. |
| 2 | PaM seed data | Keep the seeded PaM Users (Darius=1, Mihai=2) and their content. Auto-seed two matching BoardRent Accounts (`darius`, `mihai`) with `PamUserId` set. |
| 3 | Logout location | Last entry in the unified nav. Click → clear session → navigate back to `LoginPage`. |
| 4 | Admin nav visibility | Build the nav dynamically from `ISessionContext.Role`. `Admin` only appears for `Administrator`. |
| 5 | After register | Treat as logged in. Land on `MenuBarPage` immediately. |
| 6 | Two-window dev mode | Keep the spawn behaviour. CLI arg becomes "process slot" for `AppUserModelId` + tray-icon Guid. Identity comes from login. |

---

## 3. Architecture

### 3.1 Identity bridge

Two domains stay parallel (PaM `User` int Id, BoardRent `Account` Guid Id) but a single column links them:

```sql
ALTER TABLE [BoardRentDb].[dbo].[Account]
    ADD PamUserId INT NULL;
-- Idempotent guard via IF NOT EXISTS in sys.columns.
-- Optional UNIQUE constraint added the same way.
```

The link is a logical FK. SQL Server cannot enforce a foreign key across databases (PaM data lives in `[BoardRent]` SQL Express; auth lives in `[BoardRentDb]` LocalDB), so the constraint is enforced in `AuthService` and on lazy-fix-up at login.

**Code touched:**
- `Models/Account.cs` — add `int? PamUserId`.
- `Mappers/AccountMapper.cs::FromReader` — read the new column.
- `Repositories/AccountRepository.cs` — `AddAsync` / `UpdateAsync` SQL include `PamUserId`. New helper `SetPamUserIdAsync(Guid, int)` for the post-insert update.
- `Data/AppDbContext.cs::EnsureCreated` — idempotent `ALTER TABLE` block to add `PamUserId` if missing.

### 3.2 Session-driven identity

`ISessionContext` becomes the single source of truth for "who is logged in". `ICurrentUserContext` becomes a thin getter over it:

```csharp
public interface ISessionContext
{
    Guid AccountId { get; }
    string Username { get; }
    string DisplayName { get; }
    string Role { get; }
    int PamUserId { get; }            // NEW
    bool IsLoggedIn { get; }

    void Populate(Account account, string roleName);  // populates PamUserId from account.PamUserId ?? 0
    void Clear();
}

public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly ISessionContext sessionContext;

    public CurrentUserContext(ISessionContext sessionContext)
    {
        this.sessionContext = sessionContext;
    }

    public int CurrentUserId => this.sessionContext.PamUserId;
}
```

Both are singletons. The DI registration changes from
`AddSingleton<ICurrentUserContext>(new CurrentUserContext(CurrentUserId))` to
`AddSingleton<ICurrentUserContext, CurrentUserContext>()` (resolved against the singleton `ISessionContext`).

### 3.3 Lifecycle (App.xaml.cs)

```
ctor:
    CurrentProcessSlot = GetSlotFromArgs()      (renamed from CurrentUserId; CLI arg is now slot id only)
    DatabaseInitializer.EnsureDatabaseInitialized()           (PaM DB)
    if (slot == 1 && IsTwoWindowsEnabled())   StartNotificationServer()
    AppUserModelId = $"BoardRentAndProperty -- slot-{slot}"
    notificationManager = new NotificationManager()
    SetupNotificationManager()
    EnsureSingleInstance(AppUserModelId)
    ConfigureServices()                                      (DI built; ICurrentUserContext bridges to ISessionContext)
    Services.GetRequiredService<AppDbContext>().EnsureCreated()  (BoardRent DB; also seeds darius/mihai accounts)
    InitializeServices()                                     (resolve singletons; do NOT subscribe to notifications yet)
    InitializeComponent()

OnLaunched:
    CreateAndShowMainWindow()
    rootGrid wraps RootFrame
    RootFrame.Navigate(typeof(LoginPage))                    (CHANGED — was MenuBarPage)
    CreateTrayIcon()
    if (slot == 1 && IsTwoWindowsEnabled())   LaunchSecondClient()

App.OnUserLoggedIn() — NEW static helper
    notificationService.StartListening()
    notificationService.SubscribeToServer(sessionContext.PamUserId)
    NavigateTo(typeof(MenuBarPage), gameService, clearBackStack: true)

App.OnUserLoggedOut() — NEW static helper
    notificationService.StopListening()
    sessionContext.Clear()
    NavigateTo(typeof(LoginPage), parameter: null, clearBackStack: true)
```

`InitializeServices` no longer calls `StartListening` / `SubscribeToServer` — those are deferred to `OnUserLoggedIn` so the server gets the right `PamUserId`. The notification client and the toast manager are still constructed at startup (they're cheap and stateless until subscribed).

### 3.4 Unified MenuBarPage

`AppPage` enum (`ViewModels/AppPage.cs`):

```csharp
public enum AppPage
{
    Listings,
    RequestsToOthers,
    RentalsFromOthers,
    RequestsFromOthers,
    RentalsToOthers,
    Notifications,
    Profile,        // NEW
    Admin,          // NEW
    Logout          // NEW
    // BoardRent — REMOVED
}
```

`MenuBarViewModel` takes `ISessionContext` in its constructor. Its dictionary is built lazily after login and reflects role:

```
My Games            → Listings
My Requests         → RequestsToOthers
My Rentals          → RentalsFromOthers
Others' Requests    → RequestsFromOthers
Others' Rentals     → RentalsToOthers
Notifications       → Notifications
Profile             → Profile
Admin               → Admin                (only when sessionContext.Role == "Administrator")
Logout              → Logout
```

(Item labels reuse the existing English. Order is grouped: "My", "Others'", system, account.)

`MenuBarPage.PageTypeMap` adds:
```csharp
{ AppPage.Profile, typeof(ProfilePage) },
{ AppPage.Admin,   typeof(AdminPage)   },
```

`MenuBarPage.OnViewModelRequestedNavigation` handles `Logout` specially: `if (page == AppPage.Logout) { App.OnUserLoggedOut(); return; }`. The "reset to My Games on returning from BoardRent" hack in `OnNavigatedTo` is removed — there is no separate BoardRent area to come back from.

`MenuBarViewModel` rebuild: `MenuBarViewModel` is currently a singleton. To pick up role changes (e.g. logout-then-login-as-different-role), expose a `Rebuild()` method that recreates `NavigationActionsByMenuLabel` from the current session and raises `PropertyChanged` on it. Called from `App.OnUserLoggedIn`. Alternative considered: make `MenuBarViewModel` transient — rejected because `MenuBarPage` resolves it once at construction and the side ListView's bindings would not survive replacement.

### 3.5 LoginPage / RegisterPage / ProfilePage

`LoginPage`:
- Remove the "← Back to Property & Management" `HyperlinkButton` and its click handler. Login is the entry point — there's no PaM area to go back to.
- After successful login, the existing `OnLoginSuccess` callback in `LoginPage.xaml.cs` changes from `App.NavigateTo(typeof(AdminPage|ProfilePage), true)` to `App.OnUserLoggedIn()`. The post-login destination is always `MenuBarPage`; whether a user lands on Admin or somewhere else is user-driven (they click in the nav).

`RegisterPage`:
- After successful register (which auto-populates session), call `App.OnUserLoggedIn()`. Same destination as login.

`ProfilePage`:
- No structural change. Already reads from `ISessionContext` for the current account. Any "Sign out" / "Back" button it currently has is removed in favour of the nav's `Logout` entry. (To verify during implementation: read `ProfilePage.xaml.cs` and remove navigation hooks that targeted the old standalone-BoardRent flow.)

`AdminPage`:
- No structural change. Reachable via the nav only when role is `Administrator`. (To verify during implementation: same as ProfilePage.)

### 3.6 Auth flow (register and login)

`AuthService` gains a constructor dependency on PaM's `IUserRepository` (already in the merged solution and already DI-registered). `IUserRepository.Add(User)` already does `INSERT INTO Users (display_name) VALUES (@display_name); SELECT SCOPE_IDENTITY();` and sets the new int back on the passed `User` model — exactly the bridge we need.

Register (`AuthService.RegisterAsync`):

```
1. Open BoardRent UnitOfWork.
2. Reject duplicate username.
3. INSERT INTO [Account] (..., PamUserId = NULL) — new Guid.
4. INSERT INTO [AccountRoles] linking new account to "Standard User".
5. Commit BoardRent UoW.
6. Call userRepository.Add(new User { DisplayName = registrationRequest.DisplayName }) — returns newPamUserId via the User model's mutated Id.
7. Call accountRepository.SetPamUserIdAsync(newAccountId, newPamUserId) — UPDATE on BoardRentDb.
8. Mutate the in-memory account.PamUserId so step 9 sees the correct value.
9. Populate ISessionContext with (account, "Standard User"); session reads PamUserId from the account.
10. Return ServiceResult<bool>.Ok(true).
```

If steps 6–7 fail after the BoardRent commit, the Account exists with `PamUserId = NULL`. The system recovers in `AuthService.LoginAsync`: when an account with `PamUserId == NULL` logs in, the service performs steps 6–7 lazily before populating the session. Net effect: registration is robust to a transient PaM failure; the next login completes the link.

Login (`AuthService.LoginAsync`):
- Existing flow (lookup, password check, suspended check, failed-login throttle) unchanged.
- After password verifies and before `sessionContext.Populate(...)`: if `accountEntity.PamUserId == null`, call `userRepository.Add(new User { DisplayName = accountEntity.DisplayName })`, then `accountRepository.SetPamUserIdAsync(...)`, then mutate `accountEntity.PamUserId` in memory.
- `sessionContext.Populate` extended to read `account.PamUserId` and copy it into its own `PamUserId` field.

### 3.7 Seed data

PaM DB (`DatabaseInitializer`):
- Unchanged. Still seeds `Users (1=Darius Turcu, 2=Mihai Tira)` and their 11 games + 11 requests + 10 rentals + 10 notifications.

BoardRent DB (`AppDbContext.EnsureCreated`):
- Existing seed: roles `Administrator` + `Standard User`, admin Account (`admin@boardrent.com`). The existing literal hash for `admin` is **replaced** with a hash of the documented dev password `password123` so all three seed accounts share one known credential. (Today's hash maps to a password that isn't documented anywhere; reusing one known dev password is friendlier and removes a post-change debugging trap.)
- **NEW** seed: two Accounts linked to the seeded PaM Users.
  - `darius` — Username `darius`, DisplayName `Darius Turcu`, Email `darius@boardrent.com`, Role `Standard User`, `PamUserId = 1`.
  - `mihai` — Username `mihai`, DisplayName `Mihai Tira`, Email `mihai@boardrent.com`, Role `Standard User`, `PamUserId = 2`.
- All three seed accounts (`admin`, `darius`, `mihai`) use password `password123`. Password hash is computed at seed time in C# (`PasswordHasher.HashPassword("password123")`) and bound as a `SqlParameter` — PBKDF2 has no T-SQL equivalent. Each `INSERT` is wrapped in `IF NOT EXISTS (SELECT 1 FROM [Account] WHERE Username = @username)` so the seed is idempotent and re-running on an existing DB doesn't duplicate rows.
- The admin account's `PamUserId` is left `NULL` in the seed; lazy fix-up on first admin login creates an `Administrator` row in PaM `Users`.

Order of seeding (already correct in `App.xaml.cs` ctor): `DatabaseInitializer` (PaM) runs before `AppDbContext.EnsureCreated` (BoardRent). PaM Users 1 and 2 exist by the time the BoardRent seed inserts the linked accounts.

### 3.8 Two-window dev mode

Behaviour preserved, role of CLI arg changed:

- The CLI int is renamed conceptually from `CurrentUserId` to `ProcessSlot` (the field, the constants `DefaultUserId`, `DevModePrimary/SecondaryUserIdentifier`). Used only for `AppUserModelId` and the per-process tray-icon Guid.
- `ICurrentUserContext` no longer takes the slot. PaM identity is `sessionContext.PamUserId`, set on login.
- `StartNotificationServer` still fires for slot 1 with `TWO_WINDOWS=true`. `LaunchSecondClient` still spawns slot 2 in `OnLaunched`.
- Both windows boot to LoginPage. The tester logs in as `darius` in window 1 and `mihai` in window 2 to exercise cross-window notifications.

### 3.9 ViewModels that captured user identity

`ListingsViewModel` is registered as `AddTransient(sp => new ListingsViewModel(sp.GetRequiredService<IGameService>(), sp.GetRequiredService<ICurrentUserContext>().CurrentUserId))`. It captures `CurrentUserId` at construction. Because navigations create a fresh instance each time and navigations only happen post-login, the captured value is correct. Keep as-is.

`NotificationsViewModel` is a singleton. Verify during implementation whether it captures user identity at construction; if it does, switch to reading `ICurrentUserContext.CurrentUserId` on demand inside its data-fetch methods.

Other VMs (`CreateGameViewModel`, `CreateRentalViewModel`, etc.) are transient and either don't capture identity or take it through method parameters from views. No change anticipated, but each VM will be eyeballed during implementation.

---

## 4. Coding convention

All new files and all edited regions use BoardRent style:

- File-scoped namespace OR braced namespace; **`using` directives go INSIDE the `namespace` block** (BoardRent's chosen form).
- `this.` qualifier on instance member access.
- `private readonly` for injected dependencies.
- `Async` suffix on async methods.
- Verbose, descriptive identifiers (`accountEntity`, not `acc`; `registrationRequest`, not `req`).
- `ServiceResult<T>.Ok / Fail` for service return values.
- `[ObservableProperty]` + `[RelayCommand]` (CommunityToolkit.Mvvm) for view-model state where it fits the existing pattern.
- StyleCop rules already enforced via `..\SE.ruleset`.

We do NOT mass-reformat untouched PaM files. Convention applies only to code we add or modify.

---

## 5. Components touched (summary)

**Schema:**
- `BoardRentDb.dbo.Account` — new `PamUserId INT NULL` column.

**Models / DTOs:**
- `Models/Account.cs` — `+ PamUserId`.

**Data layer:**
- `Data/AppDbContext.cs` — new ALTER TABLE block; new seed for `darius` / `mihai` accounts.

**Mappers:**
- `Mappers/AccountMapper.cs` — read `PamUserId`.

**Repositories:**
- `Repositories/AccountRepository.cs` — `AddAsync` / `UpdateAsync` SQL; new `SetPamUserIdAsync`.
- `Repositories/IAccountRepository.cs` — `+ SetPamUserIdAsync`.

**Services:**
- `Services/AuthService.cs` — constructor gains `IUserRepository` (PaM); register creates linked PaM user; login lazy-fixes missing link.
- `Services/IAuthService.cs` — unchanged signatures.

**Utilities:**
- `Utilities/ISessionContext.cs` — `+ PamUserId`.
- `Utilities/SessionContext.cs` — populate/clear `PamUserId`.
- `Utilities/CurrentUserContext.cs` (renamed from root-level) — bridges to `ISessionContext`.
- `Utilities/ICurrentUserContext.cs` — unchanged interface.

**ViewModels:**
- `ViewModels/AppPage.cs` — drop `BoardRent`; add `Profile`, `Admin`, `Logout`.
- `ViewModels/MenuBarViewModel.cs` — depends on `ISessionContext`; rebuilds nav based on role; new entries; new `Rebuild()`.
- `ViewModels/NotificationsViewModel.cs` — possibly read user from `ICurrentUserContext` lazily (verify during implementation).

**Views:**
- `Views/LoginPage.xaml` — drop the back HyperlinkButton.
- `Views/LoginPage.xaml.cs` — drop `OnBackToPropertyClicked`; route `OnLoginSuccess` through `App.OnUserLoggedIn`.
- `Views/RegisterPage.xaml.cs` — route post-register through `App.OnUserLoggedIn`.
- `Views/MenuBarPage.xaml.cs` — `PageTypeMap` adds `Profile` / `Admin`; handle `Logout`; remove the BoardRent-return reset.
- `Views/ProfilePage.xaml.cs` and `Views/AdminPage.xaml.cs` — remove standalone-flow navigation hooks if any; verify on implementation.

**App / lifecycle:**
- `App.xaml.cs` — `OnLaunched` navigates to `LoginPage`; new `OnUserLoggedIn` / `OnUserLoggedOut`; `InitializeServices` no longer subscribes; CLI arg renamed conceptually to `ProcessSlot`.

---

## 6. Acceptance criteria

A merged build passes when, in order:

1. `dotnet build BoardRentAndProperty.sln` succeeds with no errors and no new warnings.
2. Launching the app shows `LoginPage` first, with no left nav, no "Back to Property & Management" button.
3. Logging in with seeded credentials `darius` / `password123` lands on `MenuBarPage` showing "My Games" populated with Darius's seeded games (Catan Base Game, Ticket to Ride Europe, …). The user identity used by feature pages reflects PaM user 1.
4. Same flow with `mihai` / `password123` shows Mihai's seeded games. Identity reflects PaM user 2.
5. Same flow with `admin` / `password123` shows `Admin` in the nav. Logging in as `darius` does not show `Admin` in the nav.
6. Clicking "Logout" returns to `LoginPage`; the back stack does not allow navigating back into MenuBar.
7. Clicking "Create an account" → registering a fresh user `alice` / `password123` → lands on `MenuBarPage` with empty games / requests / rentals (alice has a fresh PaM Users row with no content yet).
8. With `.env` `TWO_WINDOWS=true`, launching the app produces two windows + `NotificationServer.exe`. Each window boots independently to its own `LoginPage`. Logging in as `darius` in one and `mihai` in the other lets a notification fired by darius appear as a toast in mihai's window.
9. Closing and reopening the app preserves nothing (no persistent login). Same `LoginPage` on every cold start.
10. Inspecting the database after registration of `alice`: `[BoardRentDb].dbo.Account` has a row with `Username='alice'` and `PamUserId` set to a non-null int, and `[BoardRent].dbo.Users` has a corresponding row with that int as `id`.

---

## 7. Out of scope

- Persistent "Remember me" — the existing `RememberMe` checkbox stays bound but does nothing. Out of scope; revisit later if needed.
- Forgot-password actual flow — the `ContentDialog` placeholder text on `LoginPage` stays as-is.
- Migration of existing in-flight users — there are none; the seed handles the only existing data.
- Tests — the merged solution has no test project; that's a separate brainstorm.
- WebApp / WebApplication / Documentation merging — out of scope for this design.
- Renaming PaM's `User` model to something else, or migrating PaM to Guid IDs — out of scope, explicitly.

---

## 8. Risks and mitigations

| Risk | Mitigation |
|---|---|
| Cross-DB transactions for register can leave Account orphaned (no PamUserId) if PaM INSERT fails. | Lazy fix-up in `AuthService.LoginAsync` repairs the link on next login. |
| `MenuBarViewModel` is a singleton; rebuilding its dictionary mid-session must invalidate the bound `ListView`. | `Rebuild()` raises `PropertyChanged` for `NavigationActionsByMenuLabel`. ListView re-binds. Verify with the smoke test. |
| Two-window mode previously assumed slot id == identity. Tests/scripts that hard-coded "user 1" must instead log in as `darius`. | Update `Scripts/demo-run-2-users.ps1` if it references identity. |
| `NotificationsViewModel` singleton may capture identity. | Convert to lazy `ICurrentUserContext` read inside its data fetch; flagged for verification during implementation. |
| Idempotent ALTER TABLE on `Account.PamUserId` must not break existing dev DBs. | Standard `IF NOT EXISTS (SELECT … FROM sys.columns)` guard. |

---

## 9. Plan complete

This spec is the contract. Next step is to invoke `superpowers:writing-plans` to produce a numbered, file-by-file implementation plan with verification gates.
