using System;
using System.Collections.Generic;
using System.Text;

namespace BT.UI.Rcl.Features.IAM.Users.Contracts.Interfaces;

public interface ITokenStorage
{
    Task<bool> ClearAsync();
    Task<(string?, string?)> GetAsync();
    Task<bool> SaveAsync(string? accessToken, string? refreshToken);
}
