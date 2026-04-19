using BT.SharedKernel.Dtos.Auth;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.Auth.Commands;

public sealed record VerifyPasswordCommand(VerifyPasswordRequest Request) : IRequest<AppResponse<bool>>;

