using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Shared.Enums;

/// <summary>
/// Discriminator used by <c>GetLookupQuery</c> to identify which lookup table to query.
/// One value per lookup entity — keeps the single handler extensible without new classes.
/// </summary>
public enum LookupType
{
    ClientStatus,
    ClientType,
    SegmentType,
    SubSegmentType,
    LineOfBusiness,
    IdentificationType,
    DirectorRelationType
}
