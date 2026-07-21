using BT.Application.Contracts.Interfaces.Common;
using BT.Domain.Features.Shared.Contracts;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using System.Net;

namespace BT.Application.Features.Shared.TenantSettings.CommandHandlers;

internal sealed class DeleteTenantSettingHandler(ISharedUnitOfWork unitOfWork)
    : IRequestHandler<DeleteTenantSettingCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(DeleteTenantSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await unitOfWork.TenantSettingRepository.FindByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        
        if (setting is null)
            return AppResponses.Failure<bool>("Tenant setting not found.");

        await unitOfWork.TenantSettingRepository.DeleteAsync(setting.Id, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        return AppResponses.Success("Tenant setting deleted successfully.", true);
    }
}
