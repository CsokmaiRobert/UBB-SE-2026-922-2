# Task: Repair the merged `test` branch while preserving unified DB architecture

## Objective

Fix the current merged state of the `test` branch so that:

1. The application builds and runs correctly.
2. The already-merged feature work remains present, especially the Game Listings / Game Management review work and the compatible rental-page UI work.
3. The final code follows the unified database requirement:
   - one database only
   - EF Core code-first / migrations
   - no old dual-database bridge logic
   - no old PaM `User` / `IUserRepository` dependency inside the unified app layer
   - relationships modeled through objects in the domain layer, not old id-only cross-app patterns

## Branch context

Use this branch history as the source of truth for the repair:

- `test` was created as a copy of `main`.
- At the moment `test` was created, `main` already contained the reviewed and working feature set from the other teammates and it already followed the important database-unification direction.
- The Game Listings / Game Management review work from Cota was not yet integrated into that clean unified branch state.
- Cota's feature branch was then merged into `test`.
- During conflict resolution, the conflicting hunks were resolved by accepting the `test` side, because `test` already represented the correct unified architecture and database direction.
- As a result, the merged branch now contains Cota's new files and part of Cota's feature work, but the architecture that must remain authoritative is the one from `test`, not the older split-model assumptions from Cota's branch.

This means the job is not to rebuild the branch around the old feature branch architecture. The job is to adapt the merged feature work so that it runs correctly on top of the already-correct unified `test` architecture.

## Canonical database-unification requirement

This repair must preserve and respect the following requirement:

- Switch the data access layer to Entity Framework Core.
- Use code first and migrations to create and evolve the database.
- The entire project must use the same database, not two or more different databases.
- Saving or loading files is still allowed only for things that are not domain data.
- Raw SQL, stored procedures, and database views are not part of the domain-data solution anymore.
- Links between domain objects must be modeled through classes. If class `A` has class `B`, then class `A` must expose a property of type `B`, not only an `IdOfB` field as the primary representation of that relationship.

## How to apply that requirement in this branch

Use the requirement above as a decision filter during the merge repair:

- If a merged file tries to reintroduce `User`, `IUserRepository`, split user storage, bridge identifiers, or old cross-app patterns, that direction is wrong for this branch.
- If a merged file follows the current `Account` + `Guid` + EF Core + `UserDTO` model from `test`, that direction is correct.
- Keep Cota's useful feature behavior and UI where it is compatible with the unified branch, but rewrite the underlying dependencies if they still point to the old architecture.
- Port behavior, not obsolete architecture.
- Do not use `docs/superpowers/specs/2026-04-28-login-first-unified-nav-design.md` as the target architecture for this repair if it still implies bridge-style identity or old two-source assumptions.

## Non-negotiable constraints

- Follow Csokmai Code Rules.
- If Csokmai Code Rules are not present in the branch/repo, obtain the exact rules from Cota before making style/formatting decisions.
- Do not invent replacement rules if the real rules are missing.
- Do not add comments.
- Do not implement tests.
- Do not change the architecture back toward the old two-database / bridge model.
- Do not use the obsolete `PamUserId` / cross-database bridge direction from `docs/superpowers/specs/2026-04-28-login-first-unified-nav-design.md` as the implementation target for this branch.
- Treat the current unified `test` architecture as the source of truth for identity, DTOs, repositories, and services.
- Keep the teammate's useful Game Listings / Game Management and rental UI work only when it is compatible with the unified architecture or can be adapted to it cleanly.
- Do not force the unified branch to imitate the old feature branch internals.

## Current broken state

The current build failure is caused by old-model files surviving the merge:

- `BoardRentAndProperty/BoardRentAndProperty/Services/DirectoryService.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Services/IDirectoryService.cs`

These files still use:

- `IUserRepository`
- `User`
- `IMapper<User, UserDTO>` with the wrong generic arity
- `int`-based user exclusion

Those types and patterns do not belong to the unified branch anymore.

The visible compiler errors are:

- `CS0246` for `IUserRepository`
- `CS0246` for `User`
- `CS0305` for wrong `IMapper<TDomainModel, TDTO, TId>` usage

The XAML compiler error is secondary and should disappear after the C# compile errors are fixed.

There are also hidden merge mistakes in `CreateRentalViewModel.cs` that will surface after the first errors are fixed:

- property getter/setter uses `selectedRenterUser`, but the field is `selectedRenter`
- `LoadRentalFormData()` contains `await` even though the method is not `async`

These are merge-repair issues, not reasons to undo the unified branch model. Fix them by bringing the merged files into consistency with the `test` architecture.

## Architectural decisions that must remain true after the fix

- Use `Account` as the domain identity object.
- Use `Guid` as the active user/account identifier in the unified branch.
- Use `UserDTO` in DTO and view-model layers when user display data is needed.
- Use `IUserService.GetUsersExcept(Guid excludeAccountId)` for renter lookup.
- Use `ICurrentUserContext.CurrentUserId` as the current identity source.
- Keep the current unified EF Core repository model.
- Keep code first and migrations as the database strategy.
- Keep one application database for domain data.
- Keep domain relationships modeled through navigation/object properties in the domain model.
- Do not use raw SQL, stored procedures, or DB views as the domain-data implementation strategy.
- Do not reintroduce old PaM data access abstractions into the unified branch.

## Files that require action

### 1. `BoardRentAndProperty/BoardRentAndProperty/Services/DirectoryService.cs`

Preferred outcome:

- Remove this file from the final merged branch.

Reason:

- It is not referenced by the unified branch flow.
- It depends on old architecture types that no longer exist in the current merged model.
- The renter lookup responsibility already exists in `IUserService` / `UserService`.

If it is intentionally kept instead of removed, it must be fully rewritten to the unified architecture:

- use `IAccountRepository`
- use `Account`
- use `IMapper<Account, UserDTO, Guid>`
- use `Guid excludeAccountId`
- do not reference `IUserRepository`
- do not reference `User`
- do not use 2-generic `IMapper`

Deletion is preferred over rewriting unless a real active caller requires this abstraction.

### 2. `BoardRentAndProperty/BoardRentAndProperty/Services/IDirectoryService.cs`

Preferred outcome:

- Remove this file together with `DirectoryService.cs`.

If it is intentionally kept, the signature must match the unified model:

- `ImmutableList<UserDTO> GetUsersExcept(Guid excludeAccountId);`

No `int excludeUserId` signature must remain.

### 3. `BoardRentAndProperty/BoardRentAndProperty/ViewModels/CreateRentalViewModel.cs`

This file must be manually normalized. It cannot be fixed safely by blindly accepting one side of the merge.

Required final behavior:

- Inject:
  - `IGameService`
  - `IRentalService`
  - `IUserService`
  - `ICurrentUserContext`
- Keep `CurrentUserId` as `Guid`.
- Keep `OwnedActiveGames` as `ObservableCollection<GameDTO>`.
- Keep `AvailableRenters` as `ObservableCollection<UserDTO>`.
- Keep `SelectedGameToRent` as `GameDTO`.
- Keep `SelectedRenter` as `UserDTO`.

Required consistency fixes:

- Use one renter field name only.
- The property must use the actual field that exists in the file.
- Remove all leftover names from the old branch such as:
  - `selectedRenterUser`
  - `userLookupService`
  - `IDirectoryService`

Required data-loading behavior:

- `LoadRentalFormData()` loads owned active games using:
  - `gameListingService.GetActiveGamesForOwner(CurrentUserId)`
- `LoadRentalFormData()` loads renters using:
  - `userService.GetUsersExcept(CurrentUserId)`
- `LoadRentalFormData()` must not contain stray `await` unless the whole method is intentionally converted to a real async flow and all call sites are updated consistently.
- Preferred result: keep the method synchronous and remove the stray `await`.

Required rental-creation behavior:

- `CreateRental()` must call:
  - `rentalCreationService.CreateConfirmedRental(SelectedGameToRent.Id, SelectedRenter.Id, CurrentUserId, StartDate.Value.DateTime, EndDate.Value.DateTime);`
- Do not convert renter/owner ids back to `int`.

Validation:

- Validate selected game, selected renter, start date, and end date.
- Keep the current unified validation flow and dialog handling.

### 4. `BoardRentAndProperty/BoardRentAndProperty/Services/RentalService.cs`

Keep the unified service shape as the base:

- `IRentalRepository`
- `IGameRepository`
- `IMapper<Rental, RentalDTO, int>`
- `Guid renterAccountId`
- `Guid ownerAccountId`
- `Account` stubs when creating a `Rental`

Do not pull in old incompatible constructor dependencies from the obsolete branch unless they are rewritten fully against the unified branch interfaces and actually needed.

Important rule:

- If old branch logic was added here for request overlap or notifications, port it only if it is rewritten on top of the unified architecture.
- Do not reintroduce old repository/service shapes just to preserve branch code.

The existing unified behavior that must remain valid:

- `IsSlotAvailable(int gameId, DateTime startDate, DateTime endDate)`
- `CreateConfirmedRental(int gameId, Guid renterAccountId, Guid ownerAccountId, DateTime startDate, DateTime endDate)`
- `GetRentalsForRenter(Guid renterAccountId)`
- `GetRentalsForOwner(Guid ownerAccountId)`

### 5. `BoardRentAndProperty/BoardRentAndProperty/Views/RentalsFromOthersPage.xaml`

Preserve the teammate’s UI work if it matches the current DTOs.

Verify that these bindings remain valid against `RentalDTO`:

- `Game.Name`
- `Owner.DisplayName`
- `StartDateDisplayLong`
- `EndDateDisplayLong`
- `IsExpired`

This page is compatible with the unified DTO model as long as `RentalDTO` remains:

- `Game : GameDTO`
- `Owner : UserDTO?`

### 6. `BoardRentAndProperty/BoardRentAndProperty/Views/RentalsToOthersPage.xaml`

Preserve the teammate’s UI work if it matches the current DTOs.

Verify that these bindings remain valid against `RentalDTO`:

- `Game.Name`
- `Renter.DisplayName`
- `StartDateDisplayLong`
- `EndDateDisplayLong`
- `IsExpired`

Verify that the Create Rental button still opens the correct page:

- `CreateRentalView`

### 7. `BoardRentAndProperty/BoardRentAndProperty/Views/CreateRentalView.xaml.cs`

Verify that the code-behind remains aligned with the unified DTO model:

- `GamePicker.SelectedItem as GameDTO`
- `RenterPicker.SelectedItem as UserDTO`

Do not let this page drift back to old `User` or `int`-based assumptions.

### 8. `BoardRentAndProperty/BoardRentAndProperty/ViewModels/RentalsFromOthersViewModel.cs`

Do not regress this file.

It must continue to:

- read current user from `ICurrentUserContext`
- use `Guid`
- call `IRentalService.GetRentalsForRenter(Guid renterAccountId)`

### 9. `BoardRentAndProperty/BoardRentAndProperty/ViewModels/RentalsToOthersViewModel.cs`

Do not regress this file.

It must continue to:

- read current user from `ICurrentUserContext`
- use `Guid`
- call `IRentalService.GetRentalsForOwner(Guid ownerAccountId)`

### 10. `BoardRentAndProperty/BoardRentAndProperty/App.xaml.cs`

Verify that service registration remains aligned with the unified architecture:

- `IUserService` is registered
- `UserService` is registered
- `IDirectoryService` and `DirectoryService` must not be added back into DI unless they are intentionally kept and correctly rewritten

### 11. Game Listings / Game Management files

These files are already aligned with the unified branch and must not be regressed:

- `BoardRentAndProperty/BoardRentAndProperty/ViewModels/CreateGameViewModel.cs`
- `BoardRentAndProperty/BoardRentAndProperty/ViewModels/EditGameViewModel.cs`
- `BoardRentAndProperty/BoardRentAndProperty/ViewModels/ListingsViewModel.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Services/GameService.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Repositories/GameRepository.cs`
- `BoardRentAndProperty/BoardRentAndProperty/DataTransferObjects/GameDTO.cs`
- `BoardRentAndProperty/BoardRentAndProperty/Mappers/GameMapper.cs`

Keep these files on the unified model:

- `Guid` owner identity
- `UserDTO` as DTO-level owner
- `Account` as domain model owner
- EF Core repository usage

Do not modify these files unless a real merge regression requires it.

## Explicitly forbidden changes

- Do not reintroduce `PamUserId`.
- Do not reintroduce two separate databases.
- Do not reintroduce cross-database identity bridging.
- Do not reintroduce `IUserRepository`.
- Do not reintroduce `User` as an active domain dependency in the unified app layer.
- Do not reintroduce raw SQL-based directory/user lookup services.
- Do not introduce raw SQL repositories for domain data.
- Do not introduce stored procedures for domain data.
- Do not introduce DB views as a substitute for correct EF Core modeling.
- Do not model domain relationships primarily through `IdOfB`-style fields when the unified domain model should expose object relationships.
- Do not add comments.
- Do not add tests.

## Build and verification requirements

The work is complete only when all of the following are true:

1. The solution builds with no compile errors.
2. `DirectoryService.cs` and `IDirectoryService.cs` are either removed or fully rewritten to the unified model.
3. No references remain to:
   - `IUserRepository`
   - `User`
   - `IDirectoryService`
   - `DirectoryService`
   - `PamUserId`
4. `CreateRentalViewModel.cs` has no leftover old-branch names and no `await` misuse.
5. The Create Rental flow uses `Guid` identities end to end.
6. The Rentals pages render with the teammate’s UI work intact.
7. Game Listings / Game Management continues to work on the unified architecture.
8. The branch still respects the unified DB requirement.
9. Domain-data repositories remain EF Core based.
10. No active domain flow depends on split-database assumptions or old `User`-model runtime assumptions.

## Required manual checks after the code fix

- Open the app and confirm the solution runs.
- Open the rentals pages and confirm they display correctly.
- Open Create Rental and confirm:
  - owner’s active games load
  - renter list loads
  - current user is excluded from renter list
  - rental creation path uses the merged service successfully
- Open Listings / Game Management and confirm:
  - listing load still works
  - create game still works
  - edit game still works
  - delete game flow still works

## CLI verification command

Use the solution build that matches the repository setup:

```powershell
$env:DOTNET_CLI_HOME=(Resolve-Path '.dotnet_home').Path
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'
dotnet msbuild BoardRentAndProperty\BoardRentAndProperty.sln /t:Build /p:Configuration=Debug /p:Platform=x64 /m:1 /v:minimal
```

Do not stop after removing the first visible error. Continue until the full solution builds successfully and the merged feature behavior remains compatible with the unified database architecture.
