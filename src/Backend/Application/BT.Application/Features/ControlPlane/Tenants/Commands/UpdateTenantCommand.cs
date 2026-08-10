using BT.Application.Contracts.Interfaces.Common;
using System.Collections.Generic;
using System; using BT.SharedKernel.Dtos.Common; using BT.SharedKernel.Features.ControlPlane.Tenants.Dtos; using MediatR;  using BT.Application.Contracts.Interfaces.Common; using System.Collections.Generic;  namespace BT.Application.Features.ControlPlane.Tenants.Commands;  public record UpdateTenantCommand(Guid Id, UpdateTenantRequest Request) : IRequest<AppResponse<TenantResponse>>, ICacheInvalidatorRequest {     public IEnumerable<string> CacheGroups => ["tenants"];     public string? CacheUserId => null; }
