using System;
using System.Threading;
using System.Threading.Tasks;
using BT.Application.Features.ControlPlane.Stamps.Commands;
using BT.SharedKernel.Dtos.Common;
using BT.Domain.Features.ControlPlane.Contracts;
using BT.SharedKernel.Features.ControlPlane.Stamps.Dtos;
using BT.SharedKernel.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Infrastructure.Features.ControlPlane.Stamps.CommandHandlers;

public class UpdateDeploymentStampCommandHandler : IRequestHandler<UpdateDeploymentStampCommand, AppResponse<DeploymentStampResponse>>
{
    private readonly IControlPlaneUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateDeploymentStampCommandHandler> _logger;

    public UpdateDeploymentStampCommandHandler(
        IControlPlaneUnitOfWork unitOfWork,
        ILogger<UpdateDeploymentStampCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AppResponse<DeploymentStampResponse>> Handle(UpdateDeploymentStampCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        var stamp = await _unitOfWork.DeploymentStamps.FindByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (stamp == null)
        {
            return AppResponses.Failure<DeploymentStampResponse>("Deployment stamp not found.");
        }

        var duplicateName = await _unitOfWork.DeploymentStamps.AnyAsync(s => s.Name == request.Request.Name && s.Id != request.Id, cancellationToken).ConfigureAwait(false);

        if (duplicateName)
        {
            return AppResponses.Failure<DeploymentStampResponse>("Another deployment stamp with this name already exists.");
        }

        BT.Domain.Features.ControlPlane.Tenants.Enums.IsolationTier parsedTier;
        try
        {
            parsedTier = request.Request.IsolationTier.ToEnum<BT.Domain.Features.ControlPlane.Tenants.Enums.IsolationTier>();
        }
        catch (ArgumentException)
        {
            return AppResponses.Failure<DeploymentStampResponse>("Invalid Isolation Tier.");
        }

        stamp.Name = request.Request.Name;
        stamp.TargetResourceGroup = request.Request.TargetResourceGroup;
        stamp.IsolationTier = parsedTier;
        stamp.KeyVaultUri = request.Request.KeyVaultUri;
        stamp.UpdatedAt = DateTimeOffset.UtcNow;
        stamp.UpdatedBy = "System";

        await _unitOfWork.DeploymentStamps.UpdateAsync(stamp, cancellationToken).ConfigureAwait(false)  ;
        await _unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Updated deployment stamp {StampId} ({Name})", stamp.Id, stamp.Name);

        var dto = new DeploymentStampResponse
        {
            Id = stamp.Id,
            Name = stamp.Name,
            TargetResourceGroup = stamp.TargetResourceGroup,
            IsolationTier = stamp.IsolationTier.ToDisplayString(),
            KeyVaultUri = stamp.KeyVaultUri,
            CreatedAt = stamp.CreatedAt,
            UpdatedAt = stamp.UpdatedAt
        };
        return AppResponses.Success(dto);
    }
}
