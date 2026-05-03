using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Features.Shared.Dashboard.Dtos;

public record DashboardSummaryResponse(
    int TotalClients,
    int ActiveClients,
    int PendingApprovalClients,
    int DraftClients,
    IReadOnlyList<BreakdownGroup> BySegment,
    IReadOnlyList<BreakdownGroup> ByClientType,
    AgingData Aging,
    IReadOnlyList<RmWorkloadRow> RmWorkload);
