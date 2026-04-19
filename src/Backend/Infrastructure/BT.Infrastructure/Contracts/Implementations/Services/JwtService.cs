using BT.Application.Contracts.Interfaces.Services;
using BT.Application.Extensions;
using BT.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BT.Infrastructure.Contracts.Implementations.Services;

internal sealed class JwtService(
    IOptions<JwtSettings> jwtSettings,
    ILogger<JwtService> logger) : IJwtService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly ILogger<JwtService> _logger = logger;

    public async Task<string> CreateTokenAsync(List<Claim> userClaims)
    {
        var now = DateTime.UtcNow;
        var authSigningKey = _jwtSettings.GetSymmetricSecurityKey();
        var credentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: userClaims,
            notBefore: now,
            expires: now.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> CreateTempTokenAsync(List<Claim> userClaims, TimeSpan expiry)
    {
        var now = DateTime.UtcNow;
        var authSigningKey = _jwtSettings.GetSymmetricSecurityKey();
        var credentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: userClaims,
            notBefore: now,
            expires: now.Add(expiry),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateRefreshToken()
    {
        try
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating refresh token");
            throw;
        }
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string token, bool validateLifetime)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Token is null or empty");
                return new();
            }

            if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
            {
                _logger.LogWarning("JWT Security Key is not configured");
                return new();

            }

            var tokenHandler = new JwtSecurityTokenHandler();

            // Create token validation parameters with NO expiry validation
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = validateLifetime,
                ClockSkew = TimeSpan.FromMinutes(Convert.ToDouble(_jwtSettings.ClockSkew))
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Invalid JWT algorithm or token type");
                return new();
            }

            return principal;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Security token exception while parsing expired token");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing expired token");
            throw;
        }
    }

    public DateTimeOffset GetTokenExpiry(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            return DateTimeOffset.FromUnixTimeSeconds(jsonToken.Payload.Expiration ?? 0);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token expiry");
            throw;
        }
    }

    public bool IsTokenValid(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token is null");

            AuthExtensions.SecurityKey(out _);
            var tokenHandler = new JwtSecurityTokenHandler();
            var clockSkew = Convert.ToDouble(_jwtSettings.ClockSkew);

            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _jwtSettings.GetSymmetricSecurityKey(),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(clockSkew)

            };

            tokenHandler.ValidateToken(token, tokenValidationParameters, out _);

            return true;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning(ex, "Token has expired");
            throw;
        }
        catch (SecurityTokenValidationException ex)
        {
            _logger.LogWarning(ex, "Invalid token");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation");
            throw;
        }
    }


}

