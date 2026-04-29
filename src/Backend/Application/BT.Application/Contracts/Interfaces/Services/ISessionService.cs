using BT.Domain.IAM.Entities;
using BT.SharedKernel.Dtos.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BT.Application.Contracts.Interfaces.Services;

public interface ISessionService
{
    Task<AppResponse<bool>> CheckConcurrentSessionsAsync(string userId);
    Task<AppResponse<bool>> CleanupExpiredSessionsAsync();
    Task<AppResponse<bool>> CreateSessionAsync(string userId, Guid sessionId, string ipAddress, string userAgent);
    Task<AppResponse<bool>> CreateSessionAsync(string userId, Guid sessionId, string ipAddress, string userAgent, string deviceFingerprint);
    Task<AppResponse<Collection<AppUserSession>>> GetActiveSessionsAsync(string userId);
    Task<AppResponse<bool>> IsSessionValidAsync(string sessionId, string userId);
    Task<AppResponse<bool>> RevokeSessionAsync(string sessionId);
    Task<AppResponse<bool>> RevokeAllUserSessionsAsync(string userId, string? excludeSessionId = null);
    Task<AppResponse<bool>> ValidateSessionAsync(string sessionId);
}

