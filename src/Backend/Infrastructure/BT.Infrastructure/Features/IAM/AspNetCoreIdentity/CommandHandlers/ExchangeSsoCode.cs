using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.Users.Commands;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class ExchangeSsoCode(IServiceManager serviceManager) : IRequestHandler<ExchangeSsoCodeCommand, AppResponse<LoginResponse>>
{
    public async Task<AppResponse<LoginResponse>> Handle(ExchangeSsoCodeCommand command, CancellationToken cancellationToken)
    {
        var cacheKey = $"SSO_Exchange_{command.Code}";
        var loginResponse = await serviceManager.CacheService.GetAsync<LoginResponse>(cacheKey, cancellationToken).ConfigureAwait(false);

        if (loginResponse == null)
        {
            return AppResponses.Failure<LoginResponse>("Invalid or expired exchange code.");
        }

        await serviceManager.CacheService.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);

        return AppResponses.Success("Code exchanged successfully", loginResponse);
    }
}
