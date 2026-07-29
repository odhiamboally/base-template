using System;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.ControlPlane.Stamps.Dtos;
using MediatR;

namespace BT.Application.Features.ControlPlane.Stamps.Queries;

public record GetDeploymentStampByIdQuery(Guid Id) : IRequest<AppResponse<DeploymentStampResponse>>;
