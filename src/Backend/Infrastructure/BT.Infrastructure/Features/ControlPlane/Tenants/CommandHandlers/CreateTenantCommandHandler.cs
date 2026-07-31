using BT.Application.Contracts.Interfaces.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using BT.Application.Features.ControlPlane.Tenants.Commands;
using BT.Domain.Features.ControlPlane.Tenants.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.Domain.Features.ControlPlane.Contracts;
using BT.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using BT.SharedKernel.Extensions;
using BT.Infrastructure.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BT.Domain.Features.ControlPlane.Tenants.Enums;

namespace BT.Infrastructure.Features.ControlPlane.Tenants.CommandHandlers;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, AppResponse<TenantResponse>>
{
    private readonly IControlPlaneUnitOfWork _unitOfWork;
    private readonly ILogger<CreateTenantCommandHandler> _logger;
    private readonly IEncryptionService _encryptionService;

    public CreateTenantCommandHandler(
        IControlPlaneUnitOfWork unitOfWork,
        ILogger<CreateTenantCommandHandler> logger,
        IEncryptionService encryptionService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _encryptionService = encryptionService;
    }

    public async Task<AppResponse<TenantResponse>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        ArgumentNullException.ThrowIfNull(request.Request, nameof(request.Request));

        var req = request.Request;
        var existingTenant = await _unitOfWork.Tenants.AnyAsync(t => t.Identifier == req.Identifier || t.HostName == req.HostName, cancellationToken).ConfigureAwait(false);

        if (existingTenant)
        {
            return AppResponses.Failure<TenantResponse>("A tenant with this identifier or hostname already exists.");
        }

        var stampExists = await _unitOfWork.DeploymentStamps.AnyAsync(s => s.Id == req.DeploymentStampId, cancellationToken).ConfigureAwait(false);

        if (!stampExists)
        {
            return AppResponses.Failure<TenantResponse>("The specified Deployment Stamp does not exist.");
        }

        SubscriptionTier parsedTier;
        try
        {
            parsedTier = req.SubscriptionTier.ToEnum<SubscriptionTier>();
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
            Status = TenantStatus.Active,
            DeploymentStampId = req.DeploymentStampId,
            DatabaseProvider = req.DatabaseProvider,
            DatabaseConnectionString = !string.IsNullOrWhiteSpace(req.DatabaseConnectionString) 
                ? _encryptionService.Encrypt(req.DatabaseConnectionString) 
                : null,
            CreatedBy = "System"
        };
        
        await _unitOfWork.Tenants.CreateAsync(tenant, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        ControlPlaneLogDefinitions.LogTenantCreated(_logger, tenant.Id, tenant.Identifier);

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
            DatabaseProvider = tenant.DatabaseProvider,
            DatabaseConnectionString = tenant.DatabaseConnectionString != null ? "********" : null,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt
        };
        return AppResponses.Success(dto);
    }
}


