using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Banking.Customers;

/// <summary>
/// Search and filter parameters for the client listing.
/// All filter fields are optional — the spec only applies a filter when the field has a value.
/// Uses cursor-based pagination — no page number, just the last seen ID.
/// </summary>
public record CustomerSearchRequest(

    // ── Free-text global search ─────────────────────────────────────────────
    /// <summary>
    /// Searches across ClientNumber, CompanyName, RegistrationNumber,
    /// TINNumber, Mobile, and EmailId simultaneously.
    /// </summary>
    string? GlobalSearch = null,

    // ── Discrete field filters — all optional ───────────────────────────────
    string? ClientType = null,
    string? SegmentType = null,
    string? SubSegmentType = null,
    string? IdentificationType = null,
    string? LineOfBusiness = null,
    string? Status = null,
    Guid? RelationshipManagerId = null,
    Guid? Cursor = null,
    int PageSize = 50
);
