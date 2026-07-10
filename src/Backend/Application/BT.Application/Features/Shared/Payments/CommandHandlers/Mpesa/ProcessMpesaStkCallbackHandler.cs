using BT.SharedKernel.Dtos.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BT.Application.Features.Shared.Payments.CommandHandlers.Mpesa;

internal sealed class ProcessMpesaStkCallbackHandler : IRequestHandler<ProcessMpesaStkCallbackCommand, AppResponse<string>>
{
    public Task<AppResponse<string>> Handle(ProcessMpesaStkCallbackCommand request, CancellationToken cancellationToken)
    {
        // Implementation
        return Task.FromResult(AppResponses.Success("STK callback processed."));
    }
}
