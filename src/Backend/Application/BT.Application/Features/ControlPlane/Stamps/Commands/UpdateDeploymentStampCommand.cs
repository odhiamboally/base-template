using System;
using System.Collections.Generic;
using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.ControlPlane.Stamps.Dtos;
using MediatR;

namespace BT.Application.Features.ControlPlane.Stamps.Commands;

public record UpdateDeploymentStampCommand(Guid Id, UpdateDeploymentStampRequest Request) : IRequest<AppResponse<DeploymentStampResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("stamps")];
}
