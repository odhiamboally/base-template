using System;
using System.Threading;
using System.Threading.Tasks;
using BT.Application.Features.ControlPlane.Stamps.Commands;
using BT.Domain.Features.ControlPlane.Tenants.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.Domain.Features.ControlPlane.Contracts;
using BT.SharedKernel.Features.ControlPlane.Stamps.Dtos;
using BT.SharedKernel.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Infrastructure.Features.ControlPlane.Stamps.CommandHandlers;

public class CreateDeploymentStampCommandHandler : IRequestHandler<CreateDeploymentStampCommand, AppResponse<DeploymentStampResponse>>
{
    private readonly IControlPlaneUnitOfWork _unitOfWork;
    private readonly ILogger<CreateDeploymentStampCommandHandler> _logger;
    private readonly BT.Application.Contracts.Interfaces.Common.IEncryptionService _encryptionService;

    public CreateDeploymentStampCommandHandler(
        IControlPlaneUnitOfWork unitOfWork,
        ILogger<CreateDeploymentStampCommandHandler> logger,
        BT.Application.Contracts.Interfaces.Common.IEncryptionService encryptionService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _encryptionService = encryptionService;
    }

    public async Task<AppResponse<DeploymentStampResponse>> Handle(CreateDeploymentStampCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        var req = request.Request;
        var existingStamp = await _unitOfWork.DeploymentStamps.AnyAsync(s => s.Name == req.Name, cancellationToken).ConfigureAwait(false);

        if (existingStamp)
        {
            return AppResponses.Failure<DeploymentStampResponse>("A deployment stamp with this name already exists.");
        }

        BT.Domain.Features.ControlPlane.Tenants.Enums.IsolationTier parsedTier;
        try
        {
            parsedTier = req.IsolationTier.ToEnum<BT.Domain.Features.ControlPlane.Tenants.Enums.IsolationTier>();
        }
        catch (ArgumentException)
        {
            return AppResponses.Failure<DeploymentStampResponse>("Invalid Isolation Tier.");
        }

        var stamp = new DeploymentStamp
        {
            Name = req.Name,
            TargetResourceGroup = req.TargetResourceGroup,
            IsolationTier = parsedTier,
            KeyVaultUri = req.KeyVaultUri,
            DatabaseProvider = req.DatabaseProvider,
            DatabaseConnectionString = !string.IsNullOrWhiteSpace(req.DatabaseConnectionString) 
                ? _encryptionService.Encrypt(req.DatabaseConnectionString) 
                : null,
            CreatedBy = "System"
        };
        
        await _unitOfWork.DeploymentStamps.CreateAsync(stamp, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Created new deployment stamp {StampId} ({Name})", stamp.Id, stamp.Name);

        var dto = new DeploymentStampResponse
        {
            Id = stamp.Id,
            Name = stamp.Name,
            TargetResourceGroup = stamp.TargetResourceGroup,
            IsolationTier = stamp.IsolationTier.ToDisplayString(),
            KeyVaultUri = stamp.KeyVaultUri,
            DatabaseProvider = stamp.DatabaseProvider,
            DatabaseConnectionString = stamp.DatabaseConnectionString != null ? "********" : null,
            CreatedAt = stamp.CreatedAt,
            UpdatedAt = stamp.UpdatedAt
        };
        return AppResponses.Success(dto);
    }
}
