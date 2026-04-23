using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;

namespace TarvelAI.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            app.MapPost("/api/auth/register", async (
                RegisterRequest req,
                UserManager<IdentityUser> userManager) =>
            {
              

                var user = new IdentityUser { UserName = req.Email, Email = req.Email };
                var result = await userManager.CreateAsync(user, req.Password);

                return result.Succeeded
                    ? Results.Ok(new { message = "Registration successful." })
                    : Results.BadRequest(result.Errors.Select(e => e.Description));
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