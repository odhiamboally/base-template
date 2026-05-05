using BT.Domain.Features.Shared.FailedMessages.Enums;
using BT.Domain.Features.Shared.Lookups.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BT.Persistence.Features.Shared.Lookups.EntityConfigurations;

internal sealed class FailedMessageStatusLookupConfiguration : BaseLookupConfiguration<FailedMessageStatusLookup>
{
    public override void Configure(EntityTypeBuilder<FailedMessageStatusLookup> builder)
    {
        base.Configure(builder);
        builder.ToTable("Lkp_FailedMessageStatuses");

        builder.HasData(
            Row((int)FailedMessageStatus.Transient, "Transient", "Transient", 1),
            Row((int)FailedMessageStatus.Permanent, "Permanent", "Permanent", 2),
            Row((int)FailedMessageStatus.ManualRetry, "ManualRetry", "Manual Retry", 3)
        );
    }
}
