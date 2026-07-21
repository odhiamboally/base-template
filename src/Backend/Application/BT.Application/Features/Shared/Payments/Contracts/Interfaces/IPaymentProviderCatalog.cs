using BT.SharedKernel.Features.Shared.Payments.Dtos;

namespace BT.Application.Features.Shared.Payments.Contracts.Interfaces;

public interface IPaymentProviderCatalog
{
    IReadOnlyCollection<PaymentProviderCapabilityResponse> GetCapabilities();
}
