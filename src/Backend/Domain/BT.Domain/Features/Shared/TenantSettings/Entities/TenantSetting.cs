using BT.Domain.Shared.Entities;

namespace BT.Domain.Features.Shared.TenantSettings.Entities;

public class TenantSetting : BaseEntity
{
    public required string Key { get; set; }
    public required string Value { get; set; }
    public string? Description { get; set; }
    
    // For EF Core
    protected TenantSetting() { }
    
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public TenantSetting(string key, string value, string createdBy, string? description = null)
    {
        Key = key;
        Value = value;
        Description = description;
        CreatedBy = createdBy;
    }
}
