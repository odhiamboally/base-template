using BT.Domain.Features.IAM.Permissions.Contracts.Repositories;
using BT.Domain.Features.IAM.Permissions.Entities;
using BT.Persistence.Common.Repositories;
using BT.Persistence.Features.IAM.DataContext;

namespace BT.Persistence.Features.IAM.Permissions.Repositories;

public sealed class IamPermissionRepository(IamDBContext context)
    : Repository<Permission>(context), IPermissionRepository;
