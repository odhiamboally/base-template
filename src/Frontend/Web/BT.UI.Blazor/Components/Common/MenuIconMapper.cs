using MudBlazor;

namespace BT.UI.Blazor.Components.Common;

internal static class MenuIconMapper
{
    public static IReadOnlyList<MenuIconOption> Options { get; } =
    [
        new("AccountTree", "Account tree", Icons.Material.Filled.AccountTree),
        new("AdminPanelSettings", "Admin panel", Icons.Material.Filled.AdminPanelSettings),
        new("AutoStories", "Story/book", Icons.Material.Filled.AutoStories),
        new("Badge", "Badge", Icons.Material.Filled.Badge),
        new("Business", "Business", Icons.Material.Filled.Business),
        new("Dashboard", "Dashboard", Icons.Material.Filled.Dashboard),
        new("Devices", "Devices", Icons.Material.Filled.Devices),
        new("Group", "Group", Icons.Material.Filled.Group),
        new("LockPerson", "Security lock", Icons.Material.Filled.LockPerson),
        new("Menu", "Generic menu", Icons.Material.Filled.Menu),
        new("MenuOpen", "Menu", Icons.Material.Filled.MenuOpen),
        new("Settings", "Settings", Icons.Material.Filled.Settings)
    ];

    public static string Resolve(string? icon)
        => Options.FirstOrDefault(option => option.Key.Equals(icon?.Trim(), StringComparison.OrdinalIgnoreCase))?.Icon
            ?? Icons.Material.Filled.Menu;
}

internal sealed record MenuIconOption(string Key, string Label, string Icon);
