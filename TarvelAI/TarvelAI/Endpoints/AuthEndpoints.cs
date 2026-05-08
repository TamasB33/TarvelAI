using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
            });

            app.MapPost("/account/register", async (
                [FromForm] string email,
                [FromForm] string password,
                [FromForm] string confirmPassword,
                UserManager<IdentityUser> userManager) =>
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return Results.Redirect("/register?error=missing");
                }

                if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
                {
                    return Results.Redirect("/register?error=password_mismatch");
                }

                var user = new IdentityUser { UserName = email, Email = email };
                var userCreation = await userManager.CreateAsync(user, password);

                if (!userCreation.Succeeded)
                {
                    return Results.Redirect("/register?error=invalid");
                }

                var roleResult = await userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                {
                    await userManager.DeleteAsync(user);
                    return Results.Redirect("/register?error=invalid");
                }

                return Results.Redirect("/login?registered=1");
            });

            app.MapPost("/account/logout", async (SignInManager<IdentityUser> signInManager) =>
            {
                await signInManager.SignOutAsync();
                return Results.Redirect("/");
            }).RequireAuthorization();
        }
    }
}