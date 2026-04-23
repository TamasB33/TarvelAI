using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace TarvelAI.Client.Auth;

/// <summary>
/// Client-side (WASM) provider - reads auth state from PersistentComponentState
/// that was serialized into the page during SSR. No HTTP call needed.
/// </summary>
public class PersistentAuthStateProvider : AuthenticationStateProvider
{
    private static readonly Task<AuthenticationState> _anonymous =
        Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

    private readonly Task<AuthenticationState> _authStateTask;

    public PersistentAuthStateProvider(PersistentComponentState state)
    {
        if (!state.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) || userInfo is null)
        {
            _authStateTask = _anonymous;
            return;
        }

        if (userInfo.IsAuthenticated && userInfo.Email is not null)
        {
            var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, userInfo.UserName ?? userInfo.Email),
                new Claim(ClaimTypes.Email, userInfo.Email),
            ], authenticationType: "Cookie");

            _authStateTask = Task.FromResult(
                new AuthenticationState(new ClaimsPrincipal(identity)));
        }
        else
        {
            _authStateTask = _anonymous;
        }
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _authStateTask;
}

public record UserInfo(bool IsAuthenticated, string? Email, string? UserName);
