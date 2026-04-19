using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Dashboard;

public record DashboardSummaryRequest(string UserId, string? RoleScope = null);
