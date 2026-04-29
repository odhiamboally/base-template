using BT.Domain.Contracts.Interfaces.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BT.Domain.Shared.Lookups;

/// <summary>
/// Base class for all lookup / reference-data entities.
/// </summary>
/// <remarks>
/// <para>
/// Lookup tables serve three purposes:
/// 1. UI dropdowns — the Blazor layer queries these instead of reflecting over C# enums.
/// 2. Reporting/BI — joins are readable because the label is stored alongside the int key.
/// 3. Future extensibility — adding a new value is a migration + seed row, not a code deployment
///    that touches every layer.
/// </para>
/// <para>
/// <b>Id</b> maps to the corresponding C# enum's integer value so that the two stay in sync
/// without any extra mapping logic. Cast the enum to int to get the lookup row:
/// <code>var id = (int)ClientStatus.Active;</code>
/// </para>
/// <para>
/// <b>Code</b> holds the enum member name as a string (e.g. "PendingApproval"). This is what
/// EF Core stores in the parent table when you use <c>HasConversion&lt;string&gt;()</c>, so the
/// lookup table and the parent column always agree.
/// </para>
/// </remarks>
public abstract class BaseLookup : ISoftDeletable
{
    /// <summary>Integer value of the corresponding enum member.</summary>

    [Key]
    public int Id { get; set; }

    /// <summary>Enum member name — matches what EF stores via HasConversion&lt;string&gt;().</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Human-readable label shown in the UI.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Controls the order in which values appear in dropdowns.</summary>
    public int DisplayOrder { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public void MarkAsDeleted(string deletedBy)
    {
        ArgumentNullException.ThrowIfNull(deletedBy);
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;


    }
}
