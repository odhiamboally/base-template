using BT.Domain.Contracts.Interfaces.Repositories;
using BT.Domain.Banking.Entities;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.Shared.Entities;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.IAM.DataContext;

namespace BT.Persistence.IAM.Repositories;

internal sealed class IamUserRepository(IamDbContext context) : Repository<AppUser>(context), IUserRepository { }
