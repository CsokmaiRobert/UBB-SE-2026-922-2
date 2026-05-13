using System.IO;
using BoardRentAndProperty.Api.Data;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Repositories;
using BoardRentAndProperty.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("BoardRentAndProperty")
    ?? throw new InvalidOperationException("Connection string 'BoardRentAndProperty' was not found.");

builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddSingleton<UserMapper>();
builder.Services.AddSingleton<GameMapper>();
builder.Services.AddSingleton<RentalMapper>();
builder.Services.AddSingleton<RequestMapper>();
builder.Services.AddSingleton<NotificationMapper>();
builder.Services.AddSingleton<AccountProfileMapper>();

builder.Services.AddSingleton<IAccountRepository, AccountRepository>();
builder.Services.AddSingleton<IFailedLoginRepository, FailedLoginRepository>();
builder.Services.AddSingleton<IGameRepository, GameRepository>();
builder.Services.AddSingleton<IRequestRepository, RequestRepository>();
builder.Services.AddSingleton<IRentalRepository, RentalRepository>();
builder.Services.AddSingleton<INotificationRepository, NotificationRepository>();

builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IAccountService, AccountService>();
builder.Services.AddSingleton<IAdminService, AdminService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddSingleton<IRequestService, RequestService>();
builder.Services.AddSingleton<IRentalService, RentalService>();
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IAvatarStorageService, AvatarStorageService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var dbContext = contextFactory.CreateDbContext();
    dbContext.Database.Migrate();

    if (!dbContext.Games.Any())
    {
        var adminId = new Guid("00000000-0000-0000-0000-000000000010");
        var dariusId = new Guid("00000000-0000-0000-0000-000000000011");

        var admin = dbContext.Accounts.Find(adminId);
        var darius = dbContext.Accounts.Find(dariusId);

        if (admin != null && darius != null)
        {
            var testGame = new Game
            {
                Name = "Catan",
                Price = 15.0m,
                Description = "A classic strategy game.",
                Owner = admin,
                IsActive = true,
                MinimumPlayerNumber = 3,
                MaximumPlayerNumber = 4
            };
            dbContext.Games.Add(testGame);
            dbContext.SaveChanges();

            var testRequest = new Request
            {
                Game = testGame,
                Renter = darius,
                Owner = admin,
                StartDate = DateTime.UtcNow.AddDays(7),
                EndDate = DateTime.UtcNow.AddDays(10),
                Status = BoardRentAndProperty.Contracts.Models.RequestStatus.Open
            };
            dbContext.Requests.Add(testRequest);
            dbContext.SaveChanges();
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var avatarFolderRelative = app.Configuration["Storage:AvatarFolder"] ?? "Uploads/Avatars";
var avatarUrlPrefix = app.Configuration["Storage:AvatarUrlPrefix"] ?? "/avatars";
var avatarFolderAbsolute = Path.Combine(app.Environment.ContentRootPath, avatarFolderRelative);
Directory.CreateDirectory(avatarFolderAbsolute);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(avatarFolderAbsolute),
    RequestPath = avatarUrlPrefix,
});

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
