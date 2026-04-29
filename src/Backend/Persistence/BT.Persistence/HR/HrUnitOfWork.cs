using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Banking.Contracts.Repositories;
using BT.Domain.HR.Contracts.Repositories;
using BT.Domain.IAM.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Persistence.Common;
using BT.Persistence.HR.DataContext;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Persistence.HR;

public sealed class HrUnitOfWork(
    HrDbContext context,
    IEmployeeRepository employeeRepository,
    IPublisher publisher,
    ILogger<HrUnitOfWork> logger
) : BaseUnitOfWork<HrDbContext>(context, publisher, logger), IHrUnitOfWork
{
    public IEmployeeRepository EmployeeRepository { get; } = employeeRepository;
}
