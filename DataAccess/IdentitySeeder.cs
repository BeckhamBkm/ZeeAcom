using Microsoft.AspNetCore.Identity;
using ZeeAcom.Common;
using ZeeAcom.Common.Entities;

namespace ZeeAcom.DataAccess;

/// <summary>
/// Creates the roles and the admin account on startup.
/// The admin login comes from configuration ("AdminUser:Email" / "AdminUser:Password"),
/// which should be kept in user-secrets locally and in environment variables on the server, never in appsettings.json.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(IdentitySeeder));

        try
        {
            await SeedAsync(scope.ServiceProvider, configuration, logger);
        }
        catch (Exception ex) when (ex is Microsoft.Data.SqlClient.SqlException or InvalidOperationException)
        {
            // Don't stop the whole site from starting because the database is unavailable;
            // pages that need the database will show the error instead. Seeding runs again on the next start.
            logger.LogError(ex,
                "Could not seed roles/admin account because the database is unavailable. " +
                "For LocalDB, run `sqllocaldb start MSSQLLocalDB` and restart the app.");
        }
    }

    private static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var email = configuration["AdminUser:Email"];
        var password = configuration["AdminUser:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("AdminUser:Email / AdminUser:Password are not configured, so no admin account was created.");
            return;
        }

        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var created = await userManager.CreateAsync(admin, password);
            if (!created.Succeeded)
            {
                logger.LogError("Could not create the admin account: {Errors}",
                    string.Join("; ", created.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Created admin account {Email}", email);
        }

        if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
        {
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}
