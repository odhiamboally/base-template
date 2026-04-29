using System;
using System.Collections.Generic;
using System.Text;

namespace BT.UI.Rcl.Contracts.Interfaces.Auth;

public interface ITokenStorage
{
    Task<bool> ClearAsync();
    Task<(string?, string?)> GetAsync();
    Task<bool> SaveAsync(string? accessToken, string? refreshToken);
}
