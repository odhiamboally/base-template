using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Mappings;
using BT.Application.Utilities;
using BT.Domain.Contracts.Interfaces.Common;
using BT.SharedKernel.Dtos.Client;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.HR.Employees.QueryHandlers;

// ── Get Active Staff Members (for RM dropdown) ────────────────────────────────

public record GetEmployeesQuery(string UserId) : IRequest<AppResponse<List<EmployeeResponse>>>, ICachableRequest
{
    public string CacheGroup => "staff-members";
    public string Discriminator => "all";           // no filter — one entry per user
    public string? CacheUserId => UserId;
    public bool IsVersioned => false;
}


internal sealed class GetEmployeesQueryHandler(IHrUnitOfWork _hrUnitOfWork, ILogger<GetEmployeesQueryHandler> _logger)
    : IRequestHandler<GetEmployeesQuery, AppResponse<List<EmployeeResponse>>>
{
    public async Task<AppResponse<List<EmployeeResponse>>> Handle(GetEmployeesQuery query, CancellationToken ct)
    {
        try
        {
            var staff = await _hrUnitOfWork.EmployeeRepository.FindAll().ToListAsync(cancellationToken: ct).ConfigureAwait(false);
            var mapped = staff.Select(s => s.ToEmployeeResponse()).ToList();
            return AppResponse.Success($"Success", mapped);
        }
        catch (Exception ex)
        {
            LogDefinitions.LogStaffMembersFetchFailed(_logger, ex);
            throw;
        }
    }
}

