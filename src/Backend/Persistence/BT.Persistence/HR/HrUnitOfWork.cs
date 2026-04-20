using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Contracts.Interfaces.Repositories;
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
