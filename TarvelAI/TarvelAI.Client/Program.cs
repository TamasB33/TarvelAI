using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using TarvelAI.Client.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddMudServices();

// Auth state for WASM
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, PersistentAuthStateProvider>();
builder.Services.AddCascadingAuthenticationState();

// HttpClient for API calls (including /api/auth/user)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
