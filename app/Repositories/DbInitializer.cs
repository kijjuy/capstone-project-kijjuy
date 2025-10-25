using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using app.Models;
using app.Repositories;

namespace app.Repositories;
public static class DbInitializer
{

    private static String adminPass;
    public static void SetAdminPass(string pass)
    {
	adminPass = pass;
    }

    public static async Task<int> SeedUsersAndRoles(IServiceProvider serviceProvider)
    {
        // create the database if it doesn't exist
        var context = serviceProvider.GetRequiredService<IdentityContext>();
        context.Database.Migrate();

        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
	var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
	var logger = loggerFactory.CreateLogger<Program>();

        // Check if roles already exist and exit if there are
        if (roleManager.Roles.Count() > 0) {
	    logger.LogWarning($"Roles count was greater than 0.");
            return 1;  // should log an error message here
	}

        // Seed roles
        int result = await SeedRoles(roleManager);
        if (result != 0) {
	    logger.LogWarning($"result of SeedRoles was not 0. result={result}");
            return 2;  // should log an error message here
	}
        // Check if users already exist and exit if there are
        if (userManager.Users.Count() > 0) {
	    logger.LogWarning($"Users count was greater than 0.");
            return 3;  // should log an error message here
	}
        // Seed users
        result = await SeedUsers(userManager);
        if (result != 0) {
	    logger.LogWarning($"result of SeedUsers was not 0. result={result}");
            return 4;  // should log an error message here
	}
        return 0;
    }

    private static async Task<int> SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        // Create Manager Role
        var result = await roleManager.CreateAsync(new IdentityRole("Admin"));
        if (!result.Succeeded)
            return 1;  // should log an error message here

        return 0;
    }

    private static async Task<int> SeedUsers(UserManager<ApplicationUser> userManager)
    {
        // Create Manager User
        var adminUser = new ApplicationUser
        {
            UserName = "admin@email.com",
            Email = "admin@email.com",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPass);

        // Assign user to Manager role
        result = await userManager.AddToRoleAsync(adminUser, "Admin");
        if (!result.Succeeded)
            return 2;  // should log an error message here

        return 0;
    }
}



