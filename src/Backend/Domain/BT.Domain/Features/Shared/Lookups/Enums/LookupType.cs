using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Features.Shared.Lookups.Enums;

/// <summary>
/// Discriminator used by <c>GetLookupQuery</c> to identify which lookup table to query.
/// One value per lookup entity — keeps the single handler extensible without new classes.
/// </summary>
public enum LookupType
{
    CustomerStatus,
    CustomerType,
    SegmentType,
    SubSegmentType,
    LineOfBusiness,
    IdentificationType,
    DirectorRelationType,
    FailedMessageStatus
}
