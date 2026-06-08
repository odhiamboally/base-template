using BT.Domain.Features.IAM.Menus.Contracts.Repositories;
using BT.Domain.Features.IAM.Menus.Entities;
using BT.Persistence.Common.Repositories;
using BT.Persistence.Features.IAM.DataContext;

namespace BT.Persistence.Features.IAM.Menus.Repositories;

public sealed class IamMenuRepository(IamDBContext context)
    : Repository<MenuItem>(context), IMenuRepository;
