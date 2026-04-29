using BT.Domain.Banking.Contracts.Repositories;
using BT.Domain.HR.Contracts.Repositories;
using BT.Domain.IAM.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.IAM.Entities;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.IAM.DataContext;

namespace BT.Persistence.IAM.Repositories;

internal sealed class IamUserRepository(IamDbContext context) : Repository<AppUser>(context), IUserRepository { }
