using System;
using BT.Application.Contracts.Interfaces.Common;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using MediatR;

namespace BT.Application.Features.ControlPlane.Tenants.Commands;

public record AddTenantModuleCommand(Guid TenantId, AddTenantModuleRequest Request) : IRequest<AppResponse<TenantResponse>>;
