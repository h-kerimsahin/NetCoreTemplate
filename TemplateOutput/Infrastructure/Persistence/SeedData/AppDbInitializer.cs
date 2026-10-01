using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;
using Serilog;

namespace $safeprojectname$.Infrastructure.Persistence.SeedData;

public static class AppDbInitializer
{
    public static async Task InitializeDatabaseAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var hostEnvironment = scope.ServiceProvider.GetService<IHostEnvironment>();
        var isDevelopment = hostEnvironment?.IsDevelopment() ?? true;

        try
        {
            if ((await context.Database.GetPendingMigrationsAsync(ct)).Any())
                await context.Database.MigrateAsync(ct);

            await SeedRolesAsync(context, ct);
            await SeedPermissionsAsync(context, ct);
            await SeedRolePermissionsAsync(context, ct);
            if (isDevelopment)
            {
                await SeedSuperAdminUserAsync(context, passwordHasher, ct);
            }
            else
            {
                Log.Information("Production environment detected — skipping SuperAdmin seed user. Create the first SuperAdmin manually via CLI or DB script.");
            }
            context.ChangeTracker.Clear();
            Log.Information("Database Initialization: OK. Migrations applied, seed data ready.");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Database Initialization FAILED (DB connection or migration error). " +
                           "Application will start WITHOUT DB seed. Check ConnectionStrings in appsettings. " +
                           "If LocalDB missing, use SQL Server Express or set InMemory provider.");
        }
    }

    private static async Task SeedRolesAsync(AppDbContext context, CancellationToken ct)
    {
        if (await context.AppRoles.IgnoreQueryFilters().AnyAsync(ct)) return;
        var roles = new (string Name, RoleType Type)[]
        {
            ("SuperAdmin", RoleType.SuperAdmin),
            ("Admin", RoleType.Admin),
            ("Manager", RoleType.Manager),
            ("Support", RoleType.Support),
            ("User", RoleType.User),
            ("Customer", RoleType.Customer)
        };
        foreach (var (name, type) in roles)
        {
            context.Add(Domain.Entities.AppRole.Create(name, $"{type} rolü"));
        }
        await context.SaveChangesAsync(ct);
    }

    private static async Task SeedPermissionsAsync(AppDbContext context, CancellationToken ct)
    {
        if (await context.AppPermissions.IgnoreQueryFilters().AnyAsync(ct)) return;
        var groups = new (PermissionGroup Group, string[] Codes)[]
        {
            (PermissionGroup.Auth, new[] { "auth.login", "auth.register", "auth.logout", "auth.refresh", "auth.forgot-password", "auth.reset-password", "auth.two-factor", "auth.change-password", "auth.confirm-email" }),
            (PermissionGroup.UserManagement, new[] { "users.view", "users.create", "users.edit", "users.delete", "users.assign-role", "users.revoke-role", "users.unlock", "users.logout-all" }),
            (PermissionGroup.Roles, new[] { "roles.view", "roles.create", "roles.edit", "roles.delete", "roles.assign-permission" }),
            (PermissionGroup.SystemLogs, new[] { "logs.view", "logs.delete", "logs.export" }),
            (PermissionGroup.Audit, new[] { "audit.view", "audit.export" }),
            (PermissionGroup.Notifications, new[] { "notifications.send", "notifications.view", "notifications.mark-read" }),
            (PermissionGroup.Reports, new[] { "reports.view", "reports.create", "reports.export" }),
            (PermissionGroup.Restore, new[] { "restore.view", "restore.entity" }),
            (PermissionGroup.Storage, new[] { "storage.upload", "storage.delete", "storage.view" }),
            (PermissionGroup.Hangfire, new[] { "hangfire.dashboard", "hangfire.jobs.view", "hangfire.jobs.retry", "hangfire.jobs.delete" }),
            (PermissionGroup.File, new[] { "files.download", "files.archive" }),
            (PermissionGroup.Settings, new[] { "settings.view", "settings.edit", "settings.email", "settings.security" })
        };
        foreach (var (grp, codes) in groups)
        {
            foreach (var code in codes)
            {
                var parts = code.Split('.');
                var name = string.Join(' ', parts.Select(p => char.ToUpper(p[0]).ToString() + p[1..]));
                context.Add(Domain.Entities.AppPermission.Create(name, code, grp, $"{grp} - {code} izni"));
            }
        }
        await context.SaveChangesAsync(ct);
    }

    private static async Task SeedRolePermissionsAsync(AppDbContext context, CancellationToken ct)
    {
        var superAdmin = await context.AppRoles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Name == "SuperAdmin", ct);
        if (superAdmin == null) return;
        var allPermissions = await context.AppPermissions.IgnoreQueryFilters().Select(p => p.Id).ToListAsync(ct);
        var existing = await context.AppRolePermissions.IgnoreQueryFilters().Where(rp => rp.RoleId == superAdmin.Id).Select(rp => rp.PermissionId).ToListAsync(ct);
        var toAdd = allPermissions.Except(existing).ToList();
        foreach (var p in toAdd)
        {
            context.Add(new Domain.Entities.AppRolePermission(superAdmin.Id, p));
        }
        await context.SaveChangesAsync(ct);
    }

    private static async Task SeedSuperAdminUserAsync(AppDbContext context, IPasswordHasher passwordHasher, CancellationToken ct)
    {
        const string email = "superadmin@$safeprojectname$.com";
        if (await context.AppUsers.IgnoreQueryFilters().AnyAsync(u => u.Email == email, ct)) return;
        var hash = passwordHasher.HashPassword("Qwerty123!");
        var user = new Domain.Entities.AppUser("SuperAdmin", email, hash);
        user.ConfirmEmail();
        user.EnableTwoFactor(TwoFactorType.None);
        context.Add(user);
        await context.SaveChangesAsync(ct);
        var superAdminRole = await context.AppRoles.IgnoreQueryFilters().FirstAsync(r => r.Name == "SuperAdmin", ct);
        context.Add(new Domain.Entities.AppUserRole(user.Id, superAdminRole.Id));
        await context.SaveChangesAsync(ct);
    }

    private static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpper(input[0]) + input[1..];
    }
}
