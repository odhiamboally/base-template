using BT.SharedKernel.Dtos.Auth;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.IAM.Commands;

public sealed record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest<AppResponse<bool>>;

