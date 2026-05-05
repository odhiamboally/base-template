using BT.SharedKernel.Features.IAM.Users.Dtos;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.IAM.Users.Commands;

public sealed record LoginCommand(LoginRequest LoginRequest) : IRequest<AppResponse<LoginResponse>>;
