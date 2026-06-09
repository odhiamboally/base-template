using BT.Domain.Shared.Contracts;
using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Menus.Contracts.Repositories;
using BT.Domain.Features.IAM.Permissions.Contracts.Repositories;
using BT.Domain.Features.IAM.ReferenceData.Entities;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;

namespace BT.Domain.Features.IAM.Contracts;

public interface IIamUnitOfWork : ITransactionalUnitOfWork
{
    IUserRepository UserRepository { get; }
    ISessionRepository SessionRepository { get; }
    ITokenRepository TokenRepository { get; }
    IAppUserProfileRepository AppUserProfileRepository { get; }
    IAppUserTotpSecretRepository AppUserTotpSecretRepository { get; }
    ITempTotpSecretRepository TempTotpSecretRepository { get; }
    IPermissionRepository PermissionRepository { get; }
    IMenuRepository MenuRepository { get; }
    IRepository<PermissionContext> PermissionContextRepository { get; }
    IRepository<PermissionResource> PermissionResourceRepository { get; }
    IRepository<PermissionAction> PermissionActionRepository { get; }
    IRepository<MenuPlacement> MenuPlacementRepository { get; }
    IRepository<MenuIcon> MenuIconRepository { get; }
    IRepository<MenuRoute> MenuRouteRepository { get; }
}
