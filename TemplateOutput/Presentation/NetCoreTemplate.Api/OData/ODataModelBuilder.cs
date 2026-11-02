using Microsoft.OData.ModelBuilder;
using NetCoreTemplate.Domain.Entities;

namespace NetCoreTemplate.Api.OData;

public static class ODataModelBuilder
{
    public static Microsoft.OData.Edm.IEdmModel GetEdmModel() => GetEdmModel(1.0);

    public static Microsoft.OData.Edm.IEdmModel GetEdmModel(double apiVersion)
    {
        var builder = new ODataConventionModelBuilder();

        var users = builder.EntitySet<AppUser>("Users");
        users.EntityType.HasKey(u => u.Id);

        var userProfiles = builder.EntitySet<AppUserProfile>("UserProfiles");
        userProfiles.EntityType.HasKey(p => p.Id);

        var roles = builder.EntitySet<AppRole>("Roles");
        roles.EntityType.HasKey(r => r.Id);

        var permissions = builder.EntitySet<AppPermission>("Permissions");
        permissions.EntityType.HasKey(p => p.Id);

        var systemLogs = builder.EntitySet<SystemLog>("SystemLogs");
        systemLogs.EntityType.HasKey(s => s.Id);

        var userActivities = builder.EntitySet<AppUserActivited>("UserActivities");
        userActivities.EntityType.HasKey(a => a.Id);

        var auditEntries = builder.EntitySet<AuditEntry>("AuditEntries");
        auditEntries.EntityType.HasKey(a => a.Id);

        var notifications = builder.EntitySet<AppNotification>("Notifications");
        notifications.EntityType.HasKey(n => n.Id);

        return builder.GetEdmModel();
    }
}
