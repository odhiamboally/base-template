using BT.Application.Contracts.Interfaces.Common;
using System.Collections.Generic;
using BT.SharedKernel.Dtos.Common; using BT.SharedKernel.Features.ControlPlane.Tenants.Dtos; using MediatR;  using BT.Application.Contracts.Interfaces.Common; using System.Collections.Generic;  namespace BT.Application.Features.ControlPlane.Tenants.Commands;  public record CreateTenantCommand(CreateTenantRequest Request) : IRequest<AppResponse<TenantResponse>>, ICacheInvalidatorRequest {     public IEnumerable<string> CacheGroups => ["tenants"];     public string? CacheUserId => null; }
