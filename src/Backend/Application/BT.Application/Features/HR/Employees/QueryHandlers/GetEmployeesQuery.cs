using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.Banking.Customers.Mappings;
using BT.Application.Features.HR.Employees.Mappings;
using BT.Application.Features.IAM.Users.Mappings;
using BT.Application.Features.Shared.EmailTemplates.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.SharedKernel.Features.HR.Employees.Dtos;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.HR.Employees.QueryHandlers;

// ── Get Active Employees (for RM dropdown) ────────────────────────────────


public record GetEmployeesQuery(string UserId) : IRequest<AppResponse<List<EmployeeResponse>>>, ICachableRequest
{
    public string CacheGroup => "employees";
    public string Discriminator => "all";
    public string? CacheUserId => null;
    public bool IsVersioned => true;
}

