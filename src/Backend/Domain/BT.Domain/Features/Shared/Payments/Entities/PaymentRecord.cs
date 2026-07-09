using BT.Domain.Features.Shared.Payments.Enums;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Shared.Entities;
using BT.Domain.Shared.ValueObjects;

namespace BT.Domain.Features.Shared.Payments.Entities;

public class PaymentRecord : BaseEntity, IAuditable, ISoftDeletable
{
    public Money Amount { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string CustomerReference { get; private set; } = null!;
    public string Provider { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public string? ProviderReference { get; private set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public void MarkAsDeleted(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }

    // Required by EF Core
    protected PaymentRecord() { }

    public PaymentRecord(
        Guid id,
        Money amount,
        string description,
        string customerReference,
        string provider,
        PaymentStatus status = PaymentStatus.Initiated)
    {
        Id = id;
        Amount = amount;
        Description = description;
        CustomerReference = customerReference;
        Provider = provider;
        Status = status;
    }

    public void UpdateStatus(PaymentStatus status, string? providerReference = null)
    {
        Status = status;
        if (providerReference != null)
        {
            ProviderReference = providerReference;
        }
    }
}
