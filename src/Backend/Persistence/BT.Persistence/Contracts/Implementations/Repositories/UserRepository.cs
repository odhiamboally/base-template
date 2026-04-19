using BT.Domain.Contracts.Interfaces.Repositories;
using BT.Domain.Entities;
using BT.Persistence.DataContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.Contracts.Implementations.Repositories;

internal sealed class UserRepository(DBContext context) : Repository<AppUser>(context), IUserRepository
{

}


