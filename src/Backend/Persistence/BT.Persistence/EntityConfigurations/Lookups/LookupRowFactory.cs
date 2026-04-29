using BT.Domain.Shared.Lookups;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BT.Persistence.EntityConfigurations.Lookups;

file static class LookupRowFactory
{
    public static TLookup Row<TLookup>(
        Enum enumValue,
        string code,
        string label,
        int displayOrder,
        bool isActive = true)
        where TLookup : BaseLookup, new()
        => new()
        {
            Id = Convert.ToInt32(enumValue, CultureInfo.InvariantCulture),
            Code = code,
            Description = label,
            DisplayOrder = displayOrder
        };
}
