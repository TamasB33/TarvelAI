using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace TarvelAI.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            app.MapPost("/account/login", async (
                [FromForm] string email,
                [FromForm] string password,
                [FromForm] bool rememberMe,
                SignInManager<IdentityUser> signInManager) =>
            {
                var result = await signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                    return Results.Redirect("/");

                if (result.IsLockedOut)
                    return Results.Redirect("/login?error=locked");

                return Results.Redirect("/login?error=invalid");
            }).DisableAntiforgery();

            app.MapPost("/account/logout", async (SignInManager<IdentityUser> signInManager) =>
            {
                await signInManager.SignOutAsync();
                return Results.Redirect("/");
            }).DisableAntiforgery();

            app.MapPost("/api/auth/register", async (
                RegisterRequest req,
                UserManager<IdentityUser> userManager) =>
            {
              

                var user = new IdentityUser { UserName = req.Email, Email = req.Email };
                var userCreation = await userManager.CreateAsync(user, req.Password);

                if (!userCreation.Succeeded)
                {
                    return Results.BadRequest(userCreation.Errors.Select(e => e.Description));
                }

                var roleResult = await userManager.AddToRoleAsync(user, "User");
                
                if (!roleResult.Succeeded)
                {
                    return Results.BadRequest(roleResult.Errors.Select(e => e.Description));
                }

                return Results.Ok(new { message = "Registration successful." });
            });

            app.MapPost("/api/auth/login", async (
                LoginRequest req,
                SignInManager<IdentityUser> signInManager) =>
            {
                var result = await signInManager.PasswordSignInAsync(
                    req.Email, req.Password, req.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                    return Results.Ok(new { message = "Login successful." });

                if (result.IsLockedOut)
                    return Results.Problem("Account is locked out. Try again later.", statusCode: 423);

                return Results.Unauthorized();
            });

            app.MapPost("/api/auth/logout", async (SignInManager<IdentityUser> signInManager) =>
            {
                await signInManager.SignOutAsync();
                return Results.Ok(new { message = "Logged out." });
            }).RequireAuthorization();
        }

        public record LoginRequest(string Email, string Password, bool RememberMe = false);
        public record RegisterRequest(string Email, string Password);
    }
}