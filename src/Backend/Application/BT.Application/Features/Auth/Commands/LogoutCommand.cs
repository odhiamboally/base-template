using BT.SharedKernel.Dtos.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.Auth.Commands;

public sealed record LogoutCommand : IRequest<AppResponse<bool>>;

