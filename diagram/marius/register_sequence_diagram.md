sequenceDiagram
    actor User
    participant AuthCtrl as AuthController
    participant AuthSvc as AuthService
    participant PwHash as PasswordHasher
    participant AcctRepo as AccountRepository
    participant DbCtx as AppDbContext
    participant DB as Database

    User->>+AuthCtrl: POST /api/auth/register<br/>(RegisterDataTransferObject)

    AuthCtrl->>+AuthSvc: RegisterAsync(registrationRequest)

    Note over AuthSvc: Check username uniqueness
    AuthSvc->>+AcctRepo: GetByUsernameAsync(username)
    AcctRepo->>+DbCtx: Accounts.FirstOrDefaultAsync(<br/>account => account.Username == username)
    DbCtx->>+DB: SELECT FROM Account<br/>WHERE Username = @username
    DB-->>-DbCtx: result (null or Account)
    DbCtx-->>-AcctRepo: Account? existingByUsername
    AcctRepo-->>-AuthSvc: Account? existingByUsername

    alt Username already taken
        AuthSvc-->>AuthCtrl: ServiceResult.Fail(<br/>"Username is already taken.")
        AuthCtrl->>AuthCtrl: FromServiceError(result.Error)
        Note over AuthCtrl: ApiErrorResults maps "already taken"<br/>to 409 Conflict (resource_conflict)
        AuthCtrl-->>User: 409 Conflict<br/>{ error: Username is already taken." }
    else Username is available
        Note over AuthSvc: Create new Account entity
        AuthSvc->>AuthSvc: Guid.NewGuid() → newAccount.Id
        AuthSvc->>+PwHash: HashPassword(registrationRequest.Password)
        PwHash-->>-AuthSvc: hashed password string

        Note over AuthSvc: Populate Account fields:<br/>DisplayName, Username, Email,<br/>PasswordHash, PhoneNumber,<br/>Country, City, StreetName,<br/>StreetNumber, CreatedAt, UpdatedAt

        Note over AuthSvc: Persist the new account
        AuthSvc->>+AcctRepo: AddAsync(newAccount)
        AcctRepo->>+DbCtx: Accounts.Add(account)
        DbCtx->>+DB: INSERT INTO Account (...)
        DB-->>-DbCtx: OK
        DbCtx->>+DB: SaveChangesAsync()
        DB-->>-DbCtx: OK
        DbCtx-->>-AcctRepo: done
        AcctRepo-->>-AuthSvc: done

        Note over AuthSvc: Assign "Standard User" role
        AuthSvc->>+AcctRepo: AddRoleAsync(newAccount.Id,<br/>"Standard User")
        AcctRepo->>+DbCtx: Roles.FirstOrDefaultAsync(<br/>role => role.Name == "Standard User")
        DbCtx->>+DB: SELECT FROM Role<br/>WHERE Name = 'Standard User'
        DB-->>-DbCtx: Role entity
        DbCtx-->>-AcctRepo: Role role

        AcctRepo->>+DbCtx: AccountRoles.AnyAsync(<br/>accountId, roleId)
        DbCtx->>+DB: SELECT EXISTS FROM AccountRoles<br/>WHERE AccountId AND RoleId
        DB-->>-DbCtx: false (not assigned yet)
        DbCtx-->>-AcctRepo: alreadyHasRole = false

        AcctRepo->>+DbCtx: AccountRoles.Add(<br/>new AccountRole { AccountId, RoleId })
        DbCtx->>+DB: INSERT INTO AccountRoles (...)
        DB-->>-DbCtx: OK
        DbCtx->>+DB: SaveChangesAsync()
        DB-->>-DbCtx: OK
        DbCtx-->>-AcctRepo: done
        AcctRepo-->>-AuthSvc: done

        AuthSvc-->>-AuthCtrl: ServiceResult.Ok(true)
        AuthCtrl-->>-User: 200 OK<br/>{ data: true }
    end