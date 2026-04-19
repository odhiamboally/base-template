using MediatR;
using BT.SharedKernel.Dtos.Common;

namespace BT.SharedKernel.Dtos.Auth;
public abstract record ForgotPasswordRequest(string Email, string? LogoBase64) : AppRequest(), IRequest<AppResponse<bool>>;
