using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Features.Shared.Dashboard.Dtos;

/// <summary>
/// A breakdown group — e.g. "Corporate" with its own per-status counts.
/// </summary>
public record BreakdownGroup(
    string Label,
    int Total,
    IReadOnlyList<StatusCount> ByStatus);