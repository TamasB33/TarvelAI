using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using TarvelAI.Auth;
using TarvelAI.Components;
using TarvelAI.Data;
using TarvelAI.Endpoints;
using TarvelAI.Hubs;
using TarvelAI.Repositories;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);

var builder = WebApplication.CreateBuilder(args);

// ── Blazor Server (no WASM) ───────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddValidation();

builder.Services.AddRazorPages();
builder.Services.AddMudServices();
builder.Services.AddSignalR();

// ── Auth ──────────────────────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(o =>
{
    o.Password.RequireDigit = true;
    o.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath  = "/login";
    o.LogoutPath = "/logout";
});

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IFlightRepository, FlightRepository>();

// ── Authorization policies ────────────────────────────────────────────────────
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy.RequireRole("Admin"));

var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var roleManager    = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager    = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var db             = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await RoleSeeder.SeedAsync(roleManager);
    await TripSeeder.SeedAsync(db, userManager);
}

// ── Pipeline ──────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapAuthEndpoints();
app.MapHotelEndpoints();
app.MapFlightEndpoints();
app.MapRazorPages();

// ── SignalR hub ───────────────────────────────────────────────────────────────
app.MapHub<TravelHub>("/hubs/travel");

// ── Blazor ────────────────────────────────────────────────────────────────────
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
