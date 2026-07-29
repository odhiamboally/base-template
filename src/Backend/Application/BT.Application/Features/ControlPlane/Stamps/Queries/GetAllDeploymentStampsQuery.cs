using System.Collections.Generic;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.ControlPlane.Stamps.Dtos;
using MediatR;

namespace BT.Application.Features.ControlPlane.Stamps.Queries;

public record GetAllDeploymentStampsQuery : IRequest<AppResponse<List<DeploymentStampResponse>>>;
