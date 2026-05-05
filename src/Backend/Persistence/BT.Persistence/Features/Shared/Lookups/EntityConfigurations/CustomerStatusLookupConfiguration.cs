using BT.Domain.Features.Banking.Customers.Enums;
using BT.Domain.Features.Banking.Customers.Lookups;
using BT.Domain.Features.Shared.Lookups.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.Features.Shared.Lookups.EntityConfigurations;

// =============================================================================
// Each configuration:
//   1. Inherits BaseLookupConfiguration<T> — gets all common column config free.
//   2. Declares its table name.
//   3. Seeds rows whose Id = (int)EnumValue, Code = nameof(EnumValue),
//      Label = [Description] attribute value, DisplayOrder = natural reading order.
//
// ADDING A NEW ENUM VALUE IN FUTURE:
//   1. Add the C# enum member.
//   2. Add a matching HasData row here with the next DisplayOrder.
//   3. dotnet ef migrations add Add<ValueName>To<LookupName>
//   4. dotnet ef database update
//   No other code changes required — EF's string conversion picks it up automatically.
// =============================================================================

internal sealed class CustomerStatusLookupConfiguration : BaseLookupConfiguration<CustomerStatusLookup>
{
    public override void Configure(EntityTypeBuilder<CustomerStatusLookup> builder)
    {
        base.Configure(builder);
        builder.ToTable("Lkp_CustomerStatuses");

        builder.HasData(
            Row((int)CustomerStatus.Draft, "Draft", "Draft", 1),
            Row((int)CustomerStatus.PendingApproval, "PendingApproval", "Pending Approval", 2),
            Row((int)CustomerStatus.Active, "Active", "Active", 3),
            Row((int)CustomerStatus.Suspended, "Suspended", "Suspended", 4),
            Row((int)CustomerStatus.Closed, "Closed", "Closed", 5)
        );
    }
}
