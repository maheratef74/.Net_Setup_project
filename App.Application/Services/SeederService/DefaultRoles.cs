using Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace App.Application.Services.SeederService;

public static class DefaultRoles
{
    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
    {
        if (!roleManager.Roles.Any())
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
        }
    }
}