using BT.SharedKernel.Features.IAM.Users.Dtos;
using BT.SharedKernel.Dtos.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.UI.Rcl.Contracts.Interfaces.Auth;

public interface IAuthService
{
    Task<AppResponse<LoginResponse>> LoginAsync(LoginRequest loginRequest);
    Task<AppResponse<CurrentUserResponse>> GetCurrentUserAsync();
    Task<AppResponse<bool>> LogoutAsync();
}
