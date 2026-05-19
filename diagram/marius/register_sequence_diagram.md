sequenceDiagram
    actor User
    participant View as Register.cshtml<br/>(GUI-BRAP)
    participant GUIAuth as AuthController<br/>(GUI-BRAP)
    participant Adapter as AuthProxyServiceAdapter<br/>(GUI-BRAP)
    participant Client as AuthService<br/>(BoardRentAndProperty.ApiClient)
    participant ApiAuth as AuthController<br/>(BoardRentAndProperty.Api)
    participant Service as AuthService<br/>(BoardRentAndProperty.Api)
    participant Repo as AccountRepository<br/>(BoardRentAndProperty.Api)
    participant Ctx as AppDbContext<br/>(BoardRentAndProperty.Api)
    participant DB as SQL Server

    User->>View: fills form and submits
    View->>GUIAuth: POST Register(RegisterViewModel)
    GUIAuth->>Adapter: RegisterAsync(RegisterDataTransferObject)
    Adapter->>Client: RegisterAsync(RegisterDataTransferObject)
    Client->>ApiAuth: POST api/auth/register
    ApiAuth->>Service: RegisterAsync(RegisterDataTransferObject)
    Service->>Repo: GetByUsernameAsync(username)
    Repo->>Ctx: Accounts.FirstOrDefaultAsync()
    Ctx->>DB: SELECT FROM Account
    DB-->>Ctx: result
    Ctx-->>Repo: Account?
    Repo-->>Service: Account?

    alt username already exists
        Service-->>ApiAuth: ServiceResult<bool>.Fail("Username|Username is already taken.")
        ApiAuth-->>Client: 409 Conflict
        Client-->>Adapter: ServiceResult.Fail(...)
        Adapter-->>GUIAuth: throw ProxyServiceException
        GUIAuth-->>View: View(model) with ModelState errors
        View-->>User: re-renders form with validation error
    else username is available
        Service->>Service: PasswordHasher.HashPassword(password)
        Service->>Repo: AddAsync(Account)
        Repo->>Ctx: Accounts.Add(account)
        Ctx->>DB: INSERT INTO Account
        DB-->>Ctx: OK
        Ctx-->>Repo: SaveChangesAsync()
        Repo-->>Service: done
        Service->>Repo: AddRoleAsync(accountId, "Standard User")
        Repo->>Ctx: Roles.FirstOrDefaultAsync()
        Ctx->>DB: SELECT FROM Role
        DB-->>Ctx: Role
        Repo->>Ctx: Set<AccountRole>().Add(AccountRole)
        Ctx->>DB: INSERT INTO AccountRoles
        DB-->>Ctx: OK
        Ctx-->>Repo: SaveChangesAsync()
        Repo-->>Service: done
        Service-->>ApiAuth: ServiceResult<bool>.Ok(true)
        ApiAuth-->>Client: 200 OK
        Client-->>Adapter: ServiceResult.Ok()
        Adapter-->>GUIAuth: done
        GUIAuth-->>View: RedirectToAction("Index", "Games")
        View-->>User: redirected to Games page
    end