using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BT.Application.Features.ControlPlane.Tenants.Queries;
using BT.SharedKernel.Dtos.Common;
using BT.Domain.Features.ControlPlane.Contracts;
using BT.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using BT.SharedKernel.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BT.Infrastructure.Features.ControlPlane.Tenants.QueryHandlers;

public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, AppResponse<List<TenantResponse>>>
{
    private readonly IControlPlaneUnitOfWork _unitOfWork;

    public GetAllTenantsQueryHandler(IControlPlaneUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AppResponse<List<TenantResponse>>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        var rawTenants = await _unitOfWork.Tenants.FindAll()
            .AsNoTracking()
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var tenants = rawTenants
            .Select(t => new TenantResponse
            {
                Id = t.Id,
                Identifier = t.Identifier,
                DisplayName = t.DisplayName,
                HostName = t.HostName,
                ContactEmail = t.ContactEmail,
                MaxUsers = t.MaxUsers,
                SubscriptionTier = t.SubscriptionTier.ToDisplayString(),
                Status = t.Status.ToDisplayString(),
                DeploymentStampId = t.DeploymentStampId,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToList();

        return AppResponses.Success(tenants);
    }
}
