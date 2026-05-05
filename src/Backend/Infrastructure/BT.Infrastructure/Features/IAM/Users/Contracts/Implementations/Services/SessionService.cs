using BT.Application.Features.IAM.Users.Contracts.Interfaces;
using BT.Application.Features.Shared.Notifications.Contracts.Interfaces;
using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.IAM.Users.Entities;
using BT.Infrastructure.Logging;
using BT.Infrastructure.Configuration;
using BT.SharedKernel.Dtos.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BT.Infrastructure.Features.IAM.Users.Contracts.Implementations.Services;

internal sealed class SessionService(
    IIamUnitOfWork iamUnitOfWork,
    ILogger<SessionService> logger,
    IOptions<SessionSettings> sessionSettings) : ISessionService
{
    private readonly IIamUnitOfWork _unitOfWork = iamUnitOfWork;
    private readonly ILogger<SessionService> _logger = logger;
    private readonly SessionSettings _sessionSettings = sessionSettings.Value;

    public async Task<AppResponse<bool>> CheckConcurrentSessionsAsync(string userId)
    {
        try
        {
            var activeSessions = await _unitOfWork.SessionRepository
                .GetActiveSessionsByUserIdAsync(userId)
                .ConfigureAwait(false);

            return activeSessions.Count >= _sessionSettings.MaxConcurrentSessions
                ? AppResponse.Failure<bool>($"Maximum {_sessionSettings.MaxConcurrentSessions} concurrent sessions allowed")
                : AppResponse.Success("Concurrent session check passed", true);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogSessionConcurrentCheckError(_logger, userId, ex);
            throw;
        }
    }

    public async Task<AppResponse<bool>> CreateSessionAsync(string userId, Guid sessionId, string ipAddress, string userAgent)
    {
        try
        {
            // Check concurrent session limit
            var concurrentCheck = await CheckConcurrentSessionsAsync(userId).ConfigureAwait(false);
            if (!concurrentCheck.Successful)
            {
                // If at limit, end the oldest sessions
                var endSessionsResponse = await RevokeAllUserSessionsAsync(userId).ConfigureAwait(false);
                if (!endSessionsResponse.Successful)
                {
                    ServiceLogDefinitions.LogFailedToEndOldSessions(_logger, userId, endSessionsResponse.Message ?? string.Empty);

                    return AppResponse.Failure<bool>("Failed to end old sessions");
                }

            }

            var now = DateTimeOffset.UtcNow;
            var session = AppUserSession.Create(
                sessionId,
                userId,
                ipAddress,
                userAgent,
                now.AddMinutes(_sessionSettings.SessionTimeoutMinutes),
                userId);

            await _unitOfWork.SessionRepository.CreateAsync(session).ConfigureAwait(false);
            await _unitOfWork.CompleteAsync().ConfigureAwait(false);

            return AppResponse.Success("Session created successfully", true);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogSessionCreateError(_logger, userId, ex);
            throw;
        }
    }

    public async Task<AppResponse<bool>> CreateSessionAsync(string userId, Guid sessionId, string ipAddress, string userAgent, string deviceFingerprint)
    {
        const int maxRetries = 3;
        try
        {
            return await _unitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                // Query INSIDE transaction for consistency
                var activeSessions = await _unitOfWork.SessionRepository.GetActiveSessionsByUserIdAsync(userId).ConfigureAwait(false);

                // Check if session for THIS device exists
                var existingSessionForDevice = activeSessions.FirstOrDefault(s => s.DeviceFingerprint == deviceFingerprint);

                // Get sessions to end (exclude current device session)
                var sessionsToEnd = activeSessions.Where(s => s.DeviceFingerprint != deviceFingerprint).ToList();

                // End other device sessions
                if (sessionsToEnd.Any())
                {
                    foreach (var session in sessionsToEnd)
                    {
                        session.Revoke("New login from another device.");
                    }

                    await _unitOfWork.SessionRepository.UpdateRangeAsync(new Collection<AppUserSession>(sessionsToEnd)).ConfigureAwait(false);
                }

                var now = DateTimeOffset.UtcNow;

                // Update existing session OR create new one (not both)
                if (existingSessionForDevice is not null)
                {
                    existingSessionForDevice.RefreshAccess(
                        now.AddMinutes(_sessionSettings.SessionTimeoutMinutes),
                        ipAddress,
                        userAgent);

                    await _unitOfWork.SessionRepository.UpdateAsync(existingSessionForDevice).ConfigureAwait(false);
                }
                else
                {
                    var newSession = AppUserSession.Create(
                        sessionId,
                        userId,
                        ipAddress,
                        userAgent,
                        now.AddMinutes(_sessionSettings.SessionTimeoutMinutes),
                        userId,
                        deviceFingerprint);

                    await _unitOfWork.SessionRepository.CreateAsync(newSession).ConfigureAwait(false);
                }

                await _unitOfWork.CompleteAsync().ConfigureAwait(false);
                return AppResponse.Success("Session created successfully", true);
            }, maxRetries, 50).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogSessionCreateError(_logger, userId, ex);
            throw;
        }
    }

    public async Task<AppResponse<bool>> RevokeSessionAsync(string sessionId)
    {
        try
        {
            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                return AppResponse.Failure<bool>("Invalid session id");
            }

            var session = await _unitOfWork.SessionRepository
                .FindByCondition(x => x.Id == sessionGuid)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (session == null)
            {
                return AppResponse.Success("Session not found", true);
            }

            session.Revoke("Session revoked by request");

            await _unitOfWork.SessionRepository.UpdateAsync(session).ConfigureAwait(false);
            await _unitOfWork.CompleteAsync().ConfigureAwait(false);
            return AppResponse.Success("Session ended successfully", true);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogSessionEndError(_logger, sessionId, ex);
            throw;
        }
    }

    public async Task<AppResponse<bool>> RevokeAllUserSessionsAsync(string userId, string? excludeSessionId = null)
    {
        try
        {
            var activeSessions = await _unitOfWork.SessionRepository
                .GetActiveSessionsByUserIdAsync(userId)
                .ConfigureAwait(false);

            Guid? excludeSessionGuid = null;
            if (!string.IsNullOrWhiteSpace(excludeSessionId) && Guid.TryParse(excludeSessionId, out var parsedExcludeSessionGuid))
            {
                excludeSessionGuid = parsedExcludeSessionGuid;
            }

            var sessionsToEnd = excludeSessionGuid.HasValue
                ? activeSessions.Where(s => s.Id != excludeSessionGuid.Value).ToList()
                : activeSessions;

            if (sessionsToEnd.Any())
            {
                foreach (var session in sessionsToEnd)
                {
                    session.Revoke("Concurrent session limit exceeded");
                }

                await _unitOfWork.SessionRepository.UpdateRangeAsync(new Collection<AppUserSession>(sessionsToEnd)).ConfigureAwait(false);

                await _unitOfWork.CompleteAsync().ConfigureAwait(false);
            }

            return AppResponse.Success($"Ended {sessionsToEnd.Count} sessions", true);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogEndAllSessionsError(_logger, userId, ex);
            throw;
        }
    }

    public async Task<AppResponse<Collection<AppUserSession>>> GetActiveSessionsAsync(string userId)
    {
        try
        {
            var sessions = await _unitOfWork.SessionRepository
                .GetActiveSessionsByUserIdAsync(userId)
                .ConfigureAwait(false);

            var sessionList = new Collection<AppUserSession>(sessions);

            return AppResponse.Success("Active sessions retrieved", sessionList);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogGetActiveSessionsError(_logger, userId, ex);
            throw;
        }
    }

    public async Task<AppResponse<bool>> CleanupExpiredSessionsAsync()
    {
        try
        {
            var now = DateTimeOffset.UtcNow;
            var retentionLimit = now.AddDays(-30);
            var expiredSessions = await _unitOfWork.SessionRepository
                .GetExpiredSessionsAsync()
                .ConfigureAwait(false);

            if (expiredSessions.Any())
            {
                foreach (var session in expiredSessions)
                {
                    session.Revoke("Session expired");
                }

                await _unitOfWork.SessionRepository.UpdateRangeAsync(new Collection<AppUserSession>(expiredSessions)).ConfigureAwait(false);

                var purgeResult = await _unitOfWork.SessionRepository
                    .PurgeOldSessionsAsync(retentionLimit)
                    .ConfigureAwait(false);

                await _unitOfWork.CompleteAsync().ConfigureAwait(false);
            }

            return AppResponse.Success($"Cleaned up {expiredSessions.Count} expired sessions", true);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogSessionCleanupError(_logger, ex);
            throw;
        }
    }

    public async Task<AppResponse<bool>> IsSessionValidAsync(string sessionId, string userId)
    {
        try
        {
            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                return AppResponse.Failure<bool>("Invalid session id");
            }

            var session = await _unitOfWork.SessionRepository
                .FindByCondition(s => s.Id == sessionGuid)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (session == null || session.AppUserId != userId)
            {
                return AppResponse.Failure<bool>("Session not found");
            }

            if (!session.IsActive)
            {
                return AppResponse.Failure<bool>("Session is not active");
            }

            if (session.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                // Mark as expired
                session.Expire();
                await _unitOfWork.SessionRepository.UpdateAsync(session).ConfigureAwait(false);
                await _unitOfWork.CompleteAsync().ConfigureAwait(false);

                return AppResponse.Failure<bool>("Session has expired");
            }

            session.Touch(_sessionSettings.SlidingExpiration
                ? DateTimeOffset.UtcNow.AddMinutes(_sessionSettings.SessionTimeoutMinutes)
                : null);

            await _unitOfWork.SessionRepository.UpdateAsync(session).ConfigureAwait(false);
            await _unitOfWork.CompleteAsync().ConfigureAwait(false);

            return AppResponse.Success("Session is valid", true);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogSessionValidationError(_logger, sessionId, ex);
            throw;
        }
    }

    public async Task<AppResponse<bool>> ValidateSessionAsync(string sessionId)
    {
        try
        {
            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                return AppResponse.Failure<bool>("Invalid session id");
            }

            var session = await _unitOfWork.SessionRepository
                .FindByCondition(s => s.Id == sessionGuid)
                .FirstOrDefaultAsync().ConfigureAwait(false);

            if (session == null)
            {
                return AppResponse.Failure<bool>("Session not found");
            }

            if (!session.IsActive)
            {
                return AppResponse.Failure<bool>("Session is not active");
            }

            if (session.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                // Mark as expired
                session.Expire();
                await _unitOfWork.SessionRepository.UpdateAsync(session).ConfigureAwait(false);
                await _unitOfWork.CompleteAsync().ConfigureAwait(false);

                return AppResponse.Failure<bool>("Session has expired");
            }

            session.Touch(_sessionSettings.SlidingExpiration
                ? DateTimeOffset.UtcNow.AddMinutes(_sessionSettings.SessionTimeoutMinutes)
                : null);

            await _unitOfWork.SessionRepository.UpdateAsync(session).ConfigureAwait(false);
            await _unitOfWork.CompleteAsync().ConfigureAwait(false);

            return AppResponse.Success("Session is valid", true);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogSessionValidationError(_logger, sessionId, ex);
            throw;
        }
    }


}

