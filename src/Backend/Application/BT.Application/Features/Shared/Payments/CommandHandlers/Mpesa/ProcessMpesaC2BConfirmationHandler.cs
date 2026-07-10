using BT.SharedKernel.Dtos.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BT.Application.Features.Shared.Payments.CommandHandlers.Mpesa;

internal sealed class ProcessMpesaC2BConfirmationHandler : IRequestHandler<ProcessMpesaC2BConfirmationCommand, AppResponse<string>>
{
    public Task<AppResponse<string>> Handle(ProcessMpesaC2BConfirmationCommand request, CancellationToken cancellationToken)
    {
        // Confirmation logic - update database
        return Task.FromResult(AppResponses.Success("Confirmation processed."));
    }
}
