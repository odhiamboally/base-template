using BT.Domain.Features.ControlPlane.Tenants.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using BT.Application.Features.ControlPlane.Tenants.Commands;
using BT.SharedKernel.Dtos.Common;
using BT.Domain.Features.ControlPlane.Contracts;
using BT.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using BT.SharedKernel.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Infrastructure.Features.ControlPlane.Tenants.CommandHandlers;

public class SuspendTenantCommandHandler : IRequestHandler<SuspendTenantCommand, AppResponse<TenantResponse>>
{
    private readonly IControlPlaneUnitOfWork _unitOfWork;
    private readonly ILogger<SuspendTenantCommandHandler> _logger;

    public SuspendTenantCommandHandler(
        IControlPlaneUnitOfWork unitOfWork,
        ILogger<SuspendTenantCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AppResponse<TenantResponse>> Handle(SuspendTenantCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        var tenant = await _unitOfWork.Tenants.FindByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (tenant == null)
        {
            return AppResponses.Failure<TenantResponse>("Tenant not found.");
        }

        tenant.Status = TenantStatus.Suspended;
        tenant.UpdatedAt = DateTimeOffset.UtcNow;
        tenant.UpdatedBy = "System";

        await _unitOfWork.Tenants.UpdateAsync(tenant, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Suspended tenant {TenantId} ({Identifier})", tenant.Id, tenant.Identifier);

        var dto = new TenantResponse
        {
            Id = tenant.Id,
            Identifier = tenant.Identifier,
            DisplayName = tenant.DisplayName,
            HostName = tenant.HostName,
            ContactEmail = tenant.ContactEmail,
            MaxUsers = tenant.MaxUsers,
            SubscriptionTier = tenant.SubscriptionTier.ToDisplayString(),
            Status = tenant.Status.ToDisplayString(),
            DeploymentStampId = tenant.DeploymentStampId,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt
        };
        return AppResponses.Success(dto);
    }
}


