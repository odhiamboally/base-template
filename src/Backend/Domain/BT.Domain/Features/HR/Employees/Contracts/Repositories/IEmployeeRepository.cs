using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Features.HR.Employees.Contracts.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
}

