using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Entities;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.Features.IAM.DataContext;

namespace BT.Persistence.Features.IAM.Users.Repositories;

internal sealed class IamUserRepository(IamDBContext context) : Repository<AppUser>(context), IUserRepository { }
