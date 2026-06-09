using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.Permissions.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.IAM.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Permissions.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.IAM.Permissions.Queries;

public sealed record GetPermissionByIdQuery(Guid Id, string UserId) : IRequest<AppResponse<PermissionResponse>>, ICachableRequest
{
    public string CacheGroup => "permissions";

    public string Discriminator => CacheKeys.Entity("permissions", Id.ToString());

    public string? CacheUserId => null;

    public bool IsVersioned => true;
}

internal sealed class GetPermissionByIdQueryHandler(IIamUnitOfWork unitOfWork, ILogger<GetPermissionByIdQueryHandler> logger)
    : IRequestHandler<GetPermissionByIdQuery, AppResponse<PermissionResponse>>
{
    public async Task<AppResponse<PermissionResponse>> Handle(GetPermissionByIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var permission = await unitOfWork.PermissionRepository.FindByIdAsync(query.Id, cancellationToken).ConfigureAwait(false);
            return permission is null
                ? AppResponse.Failure<PermissionResponse>($"Permission {query.Id} not found.")
                : AppResponse.Success(permission.ToPermissionResponse());
        }
        catch (Exception ex)
        {
            LogDefinitions.LogPipelineException(logger, nameof(GetPermissionByIdQueryHandler), ex);
            throw;
        }
    }
}
