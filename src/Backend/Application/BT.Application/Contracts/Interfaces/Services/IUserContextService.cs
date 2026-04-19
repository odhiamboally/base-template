using BT.Application.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Contracts.Interfaces.Services;

public interface IUserContextService
{
    /// <summary>
    /// Resolves the full identity context for the currently authenticated user.
    /// Reads from JWT claims — no DB call.
    /// </summary>
    UserIdentityContext GetCurrentContext();

    /// <summary>
    /// Switches the active context for dual-role users.
    /// Triggers claim refresh (re-issue token or update session cookie).
    /// </summary>
    Task SwitchContextAsync(string context, CancellationToken ct = default);
}
