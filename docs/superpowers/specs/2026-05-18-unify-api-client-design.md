# Unificare strat client API — un singur ApiClient pentru web și desktop

Data: 2026-05-18
Branch: `unify-api-client`
Status: design aprobat, urmează planul de implementare

## Context

Avem două frontend-uri peste același API (`BoardRentAndProperty.Api`, server independent, deja deployed):

- **Desktop** WinUI 3 / MVVM — apelează API-ul prin `BoardRentAndProperty/Services/*Service.cs`
- **Web** ASP.NET MVC — apelează API-ul prin `GUI-BRAP/ProxyServices/*ProxyService.cs`

Afirmația de pornire („avem cod duplicat, cele două straturi sunt identice, mutați-le în librărie") s-a verificat și e **pe jumătate adevărată**:

- Adevărat: ambele lovesc același API, aceleași endpoint-uri, aceleași verbe/path-uri, aceleași DTO-uri — DTO-urile sunt deja partajate în `BoardRentAndProperty.Contracts`. Contractul pe sârmă e deja unic. Nu există decalaj de framework: toate proiectele sunt `net10.0` (desktop `net10.0-windows`, superset al lui `net10.0`).
- Fals: cele două straturi **nu sunt cod duplicat identic**. Divergențe reale: sync (desktop) vs async (web); 4 modele de erori pe desktop (`ServiceResult<T>` la auth/account/admin, throw `InvalidOperationException`/`ArgumentException` la game/rental, `Result<int,TError>` + `RequestServiceErrors` la request, swallow-to-empty la liste) vs unul singur pe web (throw `ProxyServiceException`); tipuri de retur diferite (`ImmutableList<T>` vs `IReadOnlyList<T>`); web-ului îi lipsesc metode întregi (n-are deloc `UserService`, lipsesc `IsSlotAvailable`, `CreateConfirmedRental`, `ApproveRequest`, `GetActiveGamesForOwner`, `GetBookedDates`, jumătate din notificări); side-effects înfipte în serviciile desktop (sesiune, rebase avatar, gate admin, `ValidateGame`, auto-login după register).

Concluzie: ținta (un singur strat de API, partajat, frontend-ul singura diferență) e corectă și realizabilă, dar e un refactor real, nu un copy-paste.

## Obiectiv

Codul care vorbește cu API-ul devine **o singură copie per domeniu**, într-o librărie nouă referită de ambele app-uri. Web și desktop diferă **doar** în frontend (controllere/view-uri MVC vs viewmodels/view-uri MVVM), plus învelișul realtime desktop-only la notificări (infra, nu logică de API) și serviciile local-only desktop.

## Decizii (luate cu utilizatorul)

1. **Scope**: toate cele 8 domenii, într-un singur efort (nu pilot, nu incremental).
2. **Model de erori**: clientul comun întoarce mereu `ServiceResult<T>`, nu aruncă pentru erori așteptate de la API. Web-ul primește adaptoare subțiri care aruncă `ProxyServiceException` ca să **nu** rescriem controllerele.
3. **Sync/async**: async până la capăt. Clientul comun e `async`. Viewmodel-urile desktop devin async (`AsyncRelayCommand`/`await`).
4. **Goluri web**: doar unificăm stratul de servicii. Clientul comun expune superset-ul; web-ul cheamă doar ce cheamă azi. **Nu** adăugăm controllere/view-uri web noi.

## Arhitectură

```
DESKTOP (MVVM)                         WEB (MVC)
  ViewModels (await)                     Controllers (neatinse)
     │                                      │
     │                              adaptoare throwing (glue subțire)
     └──► BoardRentAndProperty.ApiClient ◄──┘   ← UN singur strat HTTP, partajat
                       │
                       ▼ HTTP
              BoardRentAndProperty.Api   (server independent — NESCHIMBAT)
                       │
                      DB
```

### A. Proiect nou: `BoardRentAndProperty.ApiClient`

- Class library, `<TargetFramework>net10.0</TargetFramework>` (simplu, fără `-windows`, fără multi-targeting).
- Referințe: `ProjectReference` → `BoardRentAndProperty.Contracts`; `PackageReference` → `Microsoft.Extensions.Http` 10.0.5, `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.5.
- **Interdicție**: zero referințe spre WinUI / `Microsoft.WindowsAppSDK` / ASP.NET / API. Altfel nu mai e referabil de ambele frontend-uri.
- Adăugat în `BoardRentAndProperty.sln`.

Conținut:
- Cele 8 interfețe `I{Domain}Service` + implementări (Auth, Account, Admin, User, Game, Rental, Request, Notification), în namespace `BoardRentAndProperty.ApiClient`.
- `ServiceResult` / `ServiceResult<T>` consolidat.
- Parser de envelope `{ "Error": ..., "Code": ... }` (logica din `HttpResponseEnsurer` de azi), folosit uniform.
- `ApiUrlHelper` (rebase avatar URL — are nevoie doar de `HttpClient.BaseAddress`).
- `ApiClientNames` (named client `"BoardRentAndPropertyApi"`).
- `ApiClientOptions` + extensia DI `AddBoardRentApiClient`.

### B. Contractul unificat

- Toate metodele `async`, întorc `Task<ServiceResult<T>>` sau `Task<ServiceResult>` (operații fără payload).
- Un singur tip rezultat:
  - `ServiceResult` : `bool Success`, `string? Error`, `HttpStatusCode? StatusCode`, `string? ErrorCode`.
  - `ServiceResult<T>` : în plus `T? Data`.
  - Factory: `Ok(data)`, `Fail(error, statusCode?, errorCode?)`.
- Niciodată nu aruncă pentru erori de la API. `HttpRequestException`/`TaskCanceledException` (conexiune/timeout) → `ServiceResult.Fail(...)` uniform (azi doar `AuthService` face asta).
- Nume interfețe: `I{Domain}Service` (minimizează diff-ul pe desktop — doar `using` + async; web-ul oricum se schimbă).
- Superset: clientul expune toate metodele din ambele părți; web-ul folosește doar ce folosește azi.
- Deserializare JSON: case-insensitive uniform (azi web e case-insensitive, desktop e case-sensitive — aliniem pe case-insensitive).

### C. HttpClient + DI

- O extensie în librărie: `AddBoardRentApiClient(this IServiceCollection, Action<ApiClientOptions>)`, cu `ApiClientOptions { Uri BaseAddress; TimeSpan? Timeout }`.
- Înregistrează named client `"BoardRentAndPropertyApi"` (`BaseAddress`, `Timeout` din opțiuni) + cele 8 servicii.
- Toate serviciile injectează `IHttpClientFactory` și `CreateClient(ApiClientNames.BoardRentApi)`.
- Librăria **nu** citește config singură.
- **Desktop** `App.xaml.cs`: scoate `AddHttpClient(string.Empty, …)` + `AddTransient(... CreateClient())`; pune `AddBoardRentApiClient(o => { o.BaseAddress = <App.config ApiBaseUrl>; o.Timeout = TimeSpan.FromSeconds(10); })`.
- **Web** `Program.cs`: scoate `AddHttpClient(ApiClientNames…, …)` + `AddProxyServices()`; pune `AddBoardRentApiClient(o => o.BaseAddress = new Uri(config["ApiBaseUrl"]))`.

### D. Side-effects mutate în frontend

Clientul comun face doar HTTP. Se mută:

- **Sesiune** (`ISessionContext.Populate/Clear` în auth/account desktop) → în viewmodel/coordinator desktop, după apelul clientului. Web-ul are deja sesiune prin cookie în `AuthController`.
- **Auto-login după Register** (desktop) → în register viewmodel (Register, apoi Login). `RegisterAsync` comun doar înregistrează.
- **Gate autorizare admin** (`IDesktopAuthorizationService.IsAdministrator` în `AdminService`) → în admin viewmodel desktop. Web folosește `[Authorize]` cookie.
- **`ValidateGame`** (validare client-side) → helper în frontend desktop. Web nu validează client-side.
- **Erori tipate Request** (`Result<int,TError>` + `RequestServiceErrors`): clientul comun întoarce `ServiceResult` generic cu `ErrorCode`/`StatusCode`. Enum-urile + maparea (azi `ParseEnum` după nume, `MapApproveStatus`/`MapDenyStatus`/`MapCancelStatus`/`MapOfferStatus` după status HTTP) se mută într-un mapper desktop-only, ca viewmodel-urile request să se schimbe minim.
- **Rebase avatar URL**: se mută **în** clientul comun (are `BaseAddress`) → comportament uniform; web-ul scapă de rebasing-ul din `ProfileController`. Scade duplicarea, nu o mută.

### E. Notification (cel mai delicat)

`NotificationService` desktop e lipit de infra desktop-only: `IServerClient` (UDP), `IToastNotificationService`, `ICurrentUserContext`, `IObservable<NotificationDTO>` / `IObserver<IncomingNotification>`, `Dispose`, `SubscribeToServer`, `StartListening`/`StopListening`, `ToServerInt`.

Split:
- `INotificationService` în librărie = **doar** metodele HTTP: get by id, delete by id, update by id, get-for-user, delete-by-request, plus `PUT api/notifications/0` (persistarea folosită azi în `SendNotificationToUser`).
- Desktop: clasă nouă `DesktopNotificationService` (infra desktop-only) care **wrappează** clientul comun și adaugă UDP + observer/observable + toast + rutare după user curent. Deleagă partea HTTP la clientul comun. Înlocuiește `NotificationService`-ul vechi.
- Web: folosește direct `INotificationService` comun (prin adaptor, ca celelalte).

### F. Web: adaptoare throwing (controllere neatinse)

- Per serviciu, o clasă adaptor subțire în `GUI-BRAP/Infrastructure` care implementează interfața veche `I{Domain}ProxyService`, cheamă clientul comun și aruncă `ProxyServiceException(message, StatusCode, ErrorCode)` dacă `!result.Success`; altfel întoarce `result.Data`.
- `ServiceCollectionExtensions` / `Program.cs` înregistrează adaptoarele în loc de proxy-urile vechi.
- Interfețele `I{Domain}ProxyService` rămân în web ca simple contracte de adaptor.
- Implementările vechi `GUI-BRAP/ProxyServices/*ProxyService.cs` se șterg.
- Controllerele rămân exact cum sunt (compilează și se comportă la fel).
- Unde adaptorul nu poate furniza ce furniza proxy-ul vechi (ex. `AdminProxyService` întorcea `AdminAccountViewModel`), adaptorul/controllerul mapează din DTO-ul comun la view model-ul web (vezi tabelul Admin).

### G. Desktop: viewmodels → async

- Comenzile devin `AsyncRelayCommand`; apelurile `await client.XAsync(...)`.
- Tratare prin `result.Success` / `result.Data` / `result.Error`.
- Tipuri retur: clientul comun întoarce `IReadOnlyList<T>`; unde viewmodel-urile cer `ImmutableList`, se adaugă `.ToImmutableList()`.
- Serviciile vechi din `BoardRentAndProperty/Services/` (cele 8 API) se șterg. Rămân local-only: `FilePickerService`, `ToastNotificationService`, `FileDismissedNotificationStore`, plus noul `DesktopNotificationService`.

## Reconciliere per domeniu

Verb + path + DTO sunt identice oriunde ambele părți implementează aceeași operație; divergența e doar în async, retur/erori și metode lipsă. Forma țintă în clientul comun:

### Auth — `IAuthService`
- `RegisterAsync(RegisterDataTransferObject)` → `Task<ServiceResult>`. **Fără** auto-login (desktop VM face Register apoi Login).
- `LoginAsync(LoginDataTransferObject)` → `Task<ServiceResult<AccountProfileDataTransferObject>>`. Rebase avatar în client. Populate sesiune → desktop VM.
- `LogoutAsync()` → `Task<ServiceResult>`. Verifică statusul uniform (azi desktop ignoră statusul). Clear sesiune → desktop VM.
- `ForgotPasswordAsync()` → `Task<ServiceResult<string>>`. **De verificat în plan**: corpul răspunsului e JSON string (desktop `ReadFromJsonAsync<string>`) sau text brut (web `ReadAsStringAsync`)? Se aliniază pe ce întoarce API-ul real.

### Account — `IAccountService`
- `GetProfileAsync(Guid)` → `Task<ServiceResult<AccountProfileDataTransferObject>>` (rebase avatar în client).
- `UpdateProfileAsync(Guid, AccountProfileDataTransferObject)` → `Task<ServiceResult>` (refresh sesiune → desktop VM).
- `ChangePasswordAsync(Guid, string current, string new)` → `Task<ServiceResult>` (build `ChangePasswordDataTransferObject` cu `ConfirmPassword = new`; clear sesiune → desktop VM).
- `UploadAvatarAsync(Guid, string sourceFilePath)` → `Task<ServiceResult<string>>` (URL absolut din `AvatarUploadResponseDataTransferObject` + `ToAbsoluteUrl`). Multipart: standardizăm pe `ByteArrayContent` + `Content-Type` explicit (varianta desktop, mai bogată — API-ul poate depinde de content-type). Web adaptorul poate ignora string-ul întors.
- `RemoveAvatarAsync(Guid)` → `Task<ServiceResult>`.

### Admin — `IAdminService`
- `GetAllAccountsAsync(int page, int pageSize)` → `Task<ServiceResult<IReadOnlyList<AccountProfileDataTransferObject>>>`. **Decizie**: DTO comun + paginare (`?page=&pageSize=`, varianta desktop). `AdminAccountViewModel` rămâne web-only; adaptorul/controllerul web mapează `AccountProfileDataTransferObject` → `AdminAccountViewModel`. **De verificat în plan (cel mai mare necunoscut)**: ce întoarce de fapt `GET api/admin/accounts` din `BoardRentAndProperty.Api` și dacă acceptă paginare — web-ul azi cheamă fără paginare și deserializează altă formă.
- `SuspendAccountAsync(Guid)` / `UnsuspendAccountAsync(Guid)` / `UnlockAccountAsync(Guid)` → `Task<ServiceResult>`. Id `Guid` (web folosea `string` — unificăm pe `Guid`). PUT cu corp gol: alegem o singură variantă (probabil `content: null`).
- `ResetPasswordAsync(Guid, string newPassword)` → `Task<ServiceResult>` (`ResetPasswordDataTransferObject { NewPassword }`).
- Gate admin client-side → desktop admin VM.

### User — `IUserService`
- `GetUsersExceptAsync(Guid excludeAccountId)` → `Task<ServiceResult<IReadOnlyList<UserDTO>>>`. Era sincron, blocant, `ImmutableList`, doar pe desktop. Devine async. `CreateRentalViewModel` (desktop) îl consumă → async + tratare `ServiceResult` (păstrăm UX: pe eșec, listă goală). Web nu-l folosește (nu adăugăm UI web — în afara scope-ului).

### Game — `IGameService`
- `CreateGameAsync(GameDTO)` → `Task<ServiceResult>` (`ValidateGame` → helper desktop frontend).
- `UpdateGameAsync(int, GameDTO)` → `Task<ServiceResult>`.
- `DeleteGameAsync(int)` → `Task<ServiceResult<GameDTO>>`. Păstrăm semantica desktop (întoarce GameDTO șters); 409 → `Fail(..., StatusCode=Conflict)` în loc de throw. Web adaptorul aruncă pe eșec ca înainte.
- `GetGameByIdAsync(int)` → `Task<ServiceResult<GameDTO>>`. 404 → `Fail(NotFound)`, `Data = null` (azi web întoarce null, desktop arunca / `new GameDTO()`). Apelanții se ajustează.
- `GetGamesByOwnerAsync(Guid)` / `GetAllGamesAsync()` / `GetAvailableGamesForRenterAsync(Guid)` / `GetActiveGamesForOwnerAsync(Guid)` → `Task<ServiceResult<IReadOnlyList<GameDTO>>>`. `GetActiveGamesForOwnerAsync` e superset (web n-avea).
- `ValidateGame` — nu în clientul comun; helper desktop frontend.

### Rental — `IRentalService`
- `GetRentalsForRenterAsync(Guid)` / `GetRentalsForOwnerAsync(Guid)` → `Task<ServiceResult<IReadOnlyList<RentalDTO>>>`. Azi desktop înghite eroarea → listă goală; acum `Fail` e expus, desktop VM decide (probabil listă goală + mesaj). **Schimbare de comportament** — viewmodel-urile care se bazau pe gol-la-eroare trebuie ajustate.
- `IsSlotAvailableAsync(int gameId, DateTime start, DateTime end)` → `Task<ServiceResult<bool>>` (superset; web n-avea).
- `CreateConfirmedRentalAsync(CreateRentalDataTransferObject)` → `Task<ServiceResult>` (superset; web n-avea; parametrii liberi din desktop se normalizează la DTO).

### Request — `IRequestService`
- `GetRequestsForRenterAsync(Guid)` / `GetRequestsForOwnerAsync(Guid)` / `GetOpenRequestsForOwnerAsync(Guid)` → `Task<ServiceResult<IReadOnlyList<RequestDTO>>>`. `GetRequestsForOwnerAsync` e superset (web n-avea).
- `CreateRequestAsync(CreateRequestDataTransferObject)` → `Task<ServiceResult<int>>` (id-ul nou din `IdEnvelope`; web azi îl aruncă — păstrăm comportamentul superset).
- `ApproveRequestAsync(int, Guid)` → `Task<ServiceResult<int>>` (rental id din `RentalIdEnvelope`; superset; web n-avea).
- `DenyRequestAsync(int, RequestActionDataTransferObject)` / `CancelRequestAsync(int, RequestActionDataTransferObject)` → `Task<ServiceResult<int>>`.
- `OfferGameAsync(int, RequestActionDataTransferObject)` → `Task<ServiceResult<int>>` (rental id).
- `CheckAvailabilityAsync(int, DateTime, DateTime)` → `Task<ServiceResult<bool>>` (superset).
- `GetBookedDatesAsync(int gameId, int month, int year)` → `Task<ServiceResult<IReadOnlyList<BookedDateRangeDataTransferObject>>>` (superset; desktop VM mapează la tupluri `(DateTime,DateTime)` în frontend).
- `OnGameDeactivated(int)` — no-op azi pe desktop, nu e apel API. Se scoate din clientul comun; dacă ceva îl referă, rămâne concern desktop (probabil ștergibil — de confirmat în plan).
- Erori tipate: `RequestServiceErrors` + maparea → mapper desktop-only. **De verificat în plan**: API-ul întoarce `{Code}` cu valori care se potrivesc cu numele enum-urilor (azi `CreateRequest` mapează `Error` *string* după nume).

## În afara scope-ului

- API-ul în sine (`BoardRentAndProperty.Api`) — neatins.
- DTO-urile (`BoardRentAndProperty.Contracts`) — deja partajate, neatinse.
- Golurile de feature din UI-ul web — nu adăugăm controllere/view-uri noi.
- MVC vs MVVM — exact asta rămâne singura diferență, intenționat.

## Riscuri și puncte care nu-s mecanice

1. **Volum**: ~15 fișiere servicii × 2 + toate viewmodel-urile desktop care le consumă + DI în ambele app-uri. Build-urile intermediare vor fi roșii până se termină.
2. **Notification split** (E) — extragerea HTTP din infra UDP/observable/toast.
3. **Erori tipate Request** (D) — mutarea mapării enum în desktop fără regresii în viewmodel-urile request.
4. **Forma reală `GET api/admin/accounts`** — singurul necunoscut major; se citește `BoardRentAndProperty.Api` AdminController în plan.
5. **`ForgotPassword`** și **multipart avatar** — de aliniat pe ce așteaptă API-ul real.
6. Schimbare de comportament la liste (rental/request/notification azi înghit erorile → listă goală).

## Ipoteze de verificat în faza de plan

- `Result<TSuccess,TError>` (`Services/Result.cs`) e folosit de `RequestService`; `ServiceResult<T>` (`Utilities/ServiceResult.cs`) de auth/account/admin. Consolidăm pe un singur `ServiceResult<T>` în librărie; confirmăm că nu mai există alți consumatori de `Result<,>`.
- Forma răspunsului `GET api/admin/accounts` + suport paginare (AdminController din API).
- Forma corpului `forgot-password` (AuthController din API).
- Ce așteaptă endpoint-ul de upload avatar (content-type / nume parte).
- Inventarul viewmodel-urilor desktop care consumă fiecare serviciu (pentru migrarea async).
- Dependențele exacte ale controllerelor web pe fiecare `I{Domain}ProxyService` și pe `ProxyServiceException` (dimensionare adaptoare; niciun controller să nu citească date pe care adaptorul nu le poate furniza).

## Ordinea de migrare

Deși mergem „tot deodată", secvența care ține haosul în frâu:

1. Creează `BoardRentAndProperty.ApiClient`, adaugă în `.sln`, referințe. Adaugă `ServiceResult<T>`, `ApiClientNames`, parser envelope, `ApiUrlHelper`, `ApiClientOptions`, `AddBoardRentApiClient`.
2. Implementează cele 8 servicii unificate (superset, async, `ServiceResult`). Build librărie standalone.
3. Web: referință + adaptoare throwing pe `I{Domain}ProxyService` + swap DI; șterge implementările vechi din `ProxyServices/` (păstrează interfețele ca contracte de adaptor). Build + smoke web.
4. Desktop: referință + swap DI; viewmodels → async + `ServiceResult`; mută side-effects (sesiune/register/gate admin/validate/mapper enum request) în frontend; construiește `DesktopNotificationService`; șterge serviciile API vechi. Build + smoke desktop.
5. Curăță codul mort (proxy-uri web, servicii API desktop, `Result<,>` dacă rămâne neutilizat). Build complet verde. Ambele app-uri rulează pe API-ul deployed.

## Criterii de succes

- Soluția compilează; ambele app-uri rulează și parcurg fluxurile principale pe API.
- Zero logică de apel API duplicată: o singură implementare per domeniu, în librărie.
- Controllerele web neschimbate (compilează + comportament identic).
- Singura diferență web vs desktop: frontend-ul (controllere/view-uri vs viewmodels/view-uri) + învelișul realtime notificări desktop-only + serviciile local-only desktop.
