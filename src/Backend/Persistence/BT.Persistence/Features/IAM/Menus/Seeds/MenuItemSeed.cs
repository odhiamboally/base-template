using BT.Domain.Features.IAM.Menus.Entities;

namespace BT.Persistence.Features.IAM.Menus.Seeds;

internal static class MenuItemSeed
{
    private static readonly DateTimeOffset SeedCreatedAt = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly Guid SeedTenantId = new("0194f700-0000-7000-8000-000000000001");

    private static readonly Guid DashboardId = Guid.Parse("018fd81d-2c94-7ad0-a4a3-f1edb9c10101");
    private static readonly Guid AdminId = Guid.Parse("018fd81d-2c94-7ad0-a4a3-f1edb9c10201");
    private static readonly Guid OverviewId = Guid.Parse("018fd81d-2c94-7ad0-a4a3-f1edb9c10301");

    internal static IReadOnlyList<MenuItem> Items =>
    [
        Create(DashboardId, null, null, "dashboard", "Dashboard", "Operations dashboard.", "/dashboard", "Dashboard", "Sidebar", null),
        Create(AdminId, null, null, "admin-center", "Admin Center", "Administrative workspace.", "/admin", "AdminPanelSettings", "Sidebar", null),
        Create(OverviewId, null, null, "solution-overview", "Solution Overview", "Architecture and solution overview.", "/overview", "AutoStories", "Sidebar", null),

        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c20101", AdminId, null, "admin-customers", "Customers", "Customer records and onboarding.", "/admin/customers", "Business", "AdminCenter", null),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c20102", AdminId, null, "admin-departments", "Departments", "Department catalog and staff grouping.", "/admin/departments", "AccountTree", "AdminCenter", null),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c20103", AdminId, null, "admin-employees", "Employees", "Staff records and system access.", "/admin/employees", "Badge", "AdminCenter", null),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c20104", AdminId, null, "admin-menus", "Menus", "Navigation catalog and menu visibility.", "/admin/menus", "MenuOpen", "AdminCenter", null),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c20105", AdminId, null, "admin-permissions", "Permissions", "Permission catalog and access keys.", "/admin/permissions", "LockPerson", "AdminCenter", null),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c20106", AdminId, null, "admin-roles", "Roles", "Role catalog and assignments.", "/admin/roles", "AdminPanelSettings", "AdminCenter", null),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c20107", AdminId, null, "admin-settings", "Settings", "Platform configuration surface.", "/admin/settings", "Settings", "AdminCenter", null),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c20108", AdminId, null, "admin-user-devices", "User Devices", "Trusted device review and revocation.", "/admin/user-devices", "Devices", "AdminCenter", null),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c20109", AdminId, null, "admin-users", "Users", "Create accounts and manage lifecycle.", "/admin/users", "Group", "AdminCenter", null),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c20110", AdminId, null, "admin-access-catalog", "Access Catalog", "Source-of-truth permission and menu reference data.", "/admin/access-catalog", "LockPerson", "AdminCenter", null)
    ];

    private static MenuItem Create(string id, Guid? parentId, Guid? departmentId, string key, string title, string description, string url, string icon, string placement, string? requiredPermissionKey)
        => Create(Guid.Parse(id), parentId, departmentId, key, title, description, url, icon, placement, requiredPermissionKey);

    private static MenuItem Create(Guid id, Guid? parentId, Guid? departmentId, string key, string title, string description, string url, string icon, string placement, string? requiredPermissionKey)
    {
        var menu = MenuItem.Create(parentId, departmentId, key, title, description, url, icon, placement, requiredPermissionKey, "System");
        menu.Id = id;
        menu.TenantId = SeedTenantId;
        menu.CreatedAt = SeedCreatedAt;
        return menu;
    }
}
