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

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, AppResponse<TenantResponse>>
{
    private readonly IControlPlaneUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateTenantCommandHandler> _logger;

    public UpdateTenantCommandHandler(
        IControlPlaneUnitOfWork unitOfWork,
        ILogger<UpdateTenantCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AppResponse<TenantResponse>> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _unitOfWork.Tenants.FindByIdAsync(request.Id, cancellationToken);

        if (tenant == null)
        {
            return AppResponses.Failure<TenantResponse>("Tenant not found.");
        }

        var duplicateHostName = await _unitOfWork.Tenants.AnyAsync(t => t.HostName == request.Request.HostName && t.Id != request.Id, cancellationToken);

        if (duplicateHostName)
        {
            return AppResponses.Failure<TenantResponse>("Another tenant with this hostname already exists.");
        }

        BT.Domain.Features.ControlPlane.Tenants.Enums.SubscriptionTier parsedTier;
        try
        {
            parsedTier = request.Request.SubscriptionTier.ToEnum<BT.Domain.Features.ControlPlane.Tenants.Enums.SubscriptionTier>();
        }
        catch (ArgumentException)
        {
            return AppResponses.Failure<TenantResponse>("Invalid Subscription Tier.");
        }

        tenant.DisplayName = request.Request.DisplayName;
        tenant.HostName = request.Request.HostName;
        tenant.ContactEmail = request.Request.ContactEmail;
        tenant.MaxUsers = request.Request.MaxUsers;
        tenant.SubscriptionTier = parsedTier;
        tenant.UpdatedAt = DateTimeOffset.UtcNow;
        tenant.UpdatedBy = "System";

        await _unitOfWork.Tenants.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.LogInformation("Updated tenant {TenantId} ({Identifier})", tenant.Id, tenant.Identifier);

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
