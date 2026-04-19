using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace BT.Application.Contracts.Interfaces.Services;

public interface IJwtService
{
    Task<string> CreateTokenAsync(List<Claim> userClaims);
    Task<string> CreateTempTokenAsync(List<Claim> userClaims, TimeSpan expiry);
    string CreateRefreshToken();
    bool IsTokenValid(string token);
    ClaimsPrincipal? GetPrincipalFromToken(string token, bool validateLifetime);
    DateTimeOffset GetTokenExpiry(string token);

}
