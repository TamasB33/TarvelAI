using Microsoft.AspNetCore.Identity;

namespace TarvelAI.Data
{
    public static class RoleSeeder
    {
        public static readonly string[]Roles = ["Admin", "User"];
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager) 
        { 
          foreach(var role in Roles)
            {
                if(!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
