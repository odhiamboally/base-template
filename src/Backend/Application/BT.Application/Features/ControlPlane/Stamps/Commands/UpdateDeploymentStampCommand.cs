using System;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.ControlPlane.Stamps.Dtos;
using MediatR;

namespace BT.Application.Features.ControlPlane.Stamps.Commands;

public record UpdateDeploymentStampCommand(Guid Id, UpdateDeploymentStampRequest Request) : IRequest<AppResponse<DeploymentStampResponse>>;
