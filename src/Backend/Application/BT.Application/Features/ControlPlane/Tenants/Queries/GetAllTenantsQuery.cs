using System.Collections.Generic;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using MediatR;

namespace BT.Application.Features.ControlPlane.Tenants.Queries;

public record GetAllTenantsQuery : IRequest<AppResponse<List<TenantResponse>>>;
