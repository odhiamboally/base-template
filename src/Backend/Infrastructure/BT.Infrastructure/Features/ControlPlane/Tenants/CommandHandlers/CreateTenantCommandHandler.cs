using System;
using System.Threading;
using System.Threading.Tasks;
using BT.Application.Features.ControlPlane.Tenants.Commands;
using BT.Domain.Features.ControlPlane.Tenants.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.Domain.Features.ControlPlane.Contracts;
using BT.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using BT.SharedKernel.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Infrastructure.Features.ControlPlane.Tenants.CommandHandlers;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, AppResponse<TenantResponse>>
{
    private readonly IControlPlaneUnitOfWork _unitOfWork;
    private readonly ILogger<CreateTenantCommandHandler> _logger;

    public CreateTenantCommandHandler(
        IControlPlaneUnitOfWork unitOfWork,
        ILogger<CreateTenantCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AppResponse<TenantResponse>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var existingTenant = await _unitOfWork.Tenants.AnyAsync(t => t.Identifier == req.Identifier || t.HostName == req.HostName, cancellationToken);

        if (existingTenant)
        {
            return AppResponses.Failure<TenantResponse>("A tenant with this identifier or hostname already exists.");
        }

        var stampExists = await _unitOfWork.DeploymentStamps.AnyAsync(s => s.Id == req.DeploymentStampId, cancellationToken);

        if (!stampExists)
        {
            return AppResponses.Failure<TenantResponse>("The specified Deployment Stamp does not exist.");
        }

        BT.Domain.Features.ControlPlane.Tenants.Enums.SubscriptionTier parsedTier;
        try
        {
            parsedTier = req.SubscriptionTier.ToEnum<BT.Domain.Features.ControlPlane.Tenants.Enums.SubscriptionTier>();
        }
        catch (ArgumentException)
        {
            return AppResponses.Failure<TenantResponse>("Invalid Subscription Tier.");
        }

        var tenant = new Tenant
        {
            Identifier = req.Identifier,
            DisplayName = req.DisplayName,
            HostName = req.HostName,
            ContactEmail = req.ContactEmail,
            MaxUsers = req.MaxUsers,
            SubscriptionTier = parsedTier,
            Status = BT.Domain.Features.ControlPlane.Tenants.Enums.TenantStatus.Active,
            DeploymentStampId = req.DeploymentStampId,
            CreatedBy = "System"
        };
        
        await _unitOfWork.Tenants.CreateAsync(tenant, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.LogInformation("Created new tenant {TenantId} ({Identifier})", tenant.Id, tenant.Identifier);

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
