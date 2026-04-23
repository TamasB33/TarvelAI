using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace TarvelAI.Auth;

/// <summary>
/// Server-side Blazor auth provider — reads the authenticated user
/// directly from HttpContext (populated by ASP.NET Core Identity cookies).
/// No WASM persistence needed in server-only mode.
/// </summary>
public class CookieAuthStateProvider(IHttpContextAccessor httpContextAccessor)
    : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = httpContextAccessor.HttpContext?.User
                   ?? new ClaimsPrincipal(new ClaimsIdentity());

        return Task.FromResult(new AuthenticationState(user));
    }
}
