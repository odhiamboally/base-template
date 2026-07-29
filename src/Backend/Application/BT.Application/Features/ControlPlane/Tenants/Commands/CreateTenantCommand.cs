using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using MediatR;

namespace BT.Application.Features.ControlPlane.Tenants.Commands;

public record CreateTenantCommand(CreateTenantRequest Request) : IRequest<AppResponse<TenantResponse>>;
