using BT.Domain.Banking.Contracts.Repositories;
using BT.Domain.HR.Contracts.Repositories;
using BT.Domain.IAM.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.IAM.Entities;
using BT.Persistence.DataContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.Contracts.Implementations.Repositories;

internal sealed class UserRepository(DBContext context) : Repository<AppUser>(context), IUserRepository
{

}


