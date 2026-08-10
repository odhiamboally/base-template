using BT.Domain.Features.ControlPlane.Auditing.Entities;
using BT.Domain.Features.ControlPlane.Auditing.Enums;
using BT.Domain.Features.ControlPlane.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.ControlPlane.Auditing.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BT.Application.Features.ControlPlane.Auditing.Commands;

public record StartImpersonationCommand(Guid TargetTenantId, string Reason, int DurationHours = 1) : IRequest<AppResponse<ImpersonationRecordResponse>>;

public class StartImpersonationCommandValidator : AbstractValidator<StartImpersonationCommand>
{
    public StartImpersonationCommandValidator()
    {
        RuleFor(x => x.TargetTenantId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1024);
        RuleFor(x => x.DurationHours).GreaterThan(0).LessThanOrEqualTo(24);
    }
}

internal sealed class StartImpersonationCommandHandler(
    IControlPlaneUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider,
    ILogger<StartImpersonationCommandHandler> logger)
    : IRequestHandler<StartImpersonationCommand, AppResponse<ImpersonationRecordResponse>>
{
    public async Task<AppResponse<ImpersonationRecordResponse>> Handle(StartImpersonationCommand request, CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Tenants.FindByIdAsync(request.TargetTenantId, cancellationToken).ConfigureAwait(false);
        if (tenant == null)
        {
            return AppResponses.Failure<ImpersonationRecordResponse>("Tenant not found");
        }

        var expiryTime = DateTimeOffset.UtcNow.AddHours(request.DurationHours);
        var actorName = actorProvider.ActorId;

        var record = new ImpersonationRecord
        {
            ActorId = actorProvider.ActorId,
            ActorName = actorName,
            TargetTenantId = tenant.Id,
            TargetTenantName = tenant.DisplayName,
            Reason = request.Reason,
            ExpiryTime = expiryTime,
            Status = ImpersonationRecordStatus.Active,
            CreatedBy = actorProvider.ActorId
        };

        await unitOfWork.ImpersonationRecords.CreateAsync(record, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("User {ActorId} started impersonating Tenant {TenantId} until {ExpiryTime}. Reason: {Reason}",
            actorProvider.ActorId, tenant.Id, expiryTime, request.Reason);

        return AppResponses.Success(new ImpersonationRecordResponse(
            record.Id,
            record.ActorId,
            record.ActorName,
            record.TargetTenantId,
            record.TargetTenantName,
            record.Reason,
            record.CreatedAt,
            record.ExpiryTime,
            record.Status.ToString()
        ));
    }
}
