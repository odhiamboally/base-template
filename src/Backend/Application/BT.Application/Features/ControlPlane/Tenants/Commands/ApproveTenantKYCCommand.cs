using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using MediatR;
using System;

namespace BT.Application.Features.ControlPlane.Tenants.Commands;

public record ApproveTenantKYCCommand(Guid TenantId) : IRequest<AppResponse<TenantResponse>>;
