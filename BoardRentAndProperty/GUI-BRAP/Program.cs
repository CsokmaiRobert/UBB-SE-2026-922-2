using BoardRentAndProperty.Api.Data;
using GUI_BRAP.Infrastructure;
using GUI_BRAP.ProxyServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews()
    .ConfigureApplicationPartManager(manager =>
    {
        string apiAssemblyName = typeof(AppDbContext).Assembly.GetName().Name!;
        for (int index = manager.ApplicationParts.Count - 1; index >= 0; index--)
        {
            if (manager.ApplicationParts[index].Name == apiAssemblyName)
            {
                manager.ApplicationParts.RemoveAt(index);
            }
        }
    });

builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("BoardRentAndProperty")));

string apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("Configuration value 'ApiBaseUrl' is required.");

builder.Services.AddHttpClient(ApiClientNames.BoardRentApi, client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddProxyServices();
builder.Services.AddScoped<IAccountProxyService, AccountProxyService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run("http://localhost:5175");
