BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory_IAM] ([MigrationId], [ProductVersion])
VALUES (N'20260507052059_Align_IAM_QueryFilters', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [RefreshTokens] ADD [DeletedAt] datetimeoffset NULL;

ALTER TABLE [RefreshTokens] ADD [DeletedBy] nvarchar(max) NULL;

ALTER TABLE [RefreshTokens] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [AppUserTotpSecrets] ADD [DeletedAt] datetimeoffset NULL;

ALTER TABLE [AppUserTotpSecrets] ADD [DeletedBy] nvarchar(max) NULL;

ALTER TABLE [AppUserTotpSecrets] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [AppUserSessions] ADD [DeletedAt] datetimeoffset NULL;

ALTER TABLE [AppUserSessions] ADD [DeletedBy] nvarchar(max) NULL;

ALTER TABLE [AppUserSessions] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [AppUserDevices] ADD [DeletedAt] datetimeoffset NULL;

ALTER TABLE [AppUserDevices] ADD [DeletedBy] nvarchar(max) NULL;

ALTER TABLE [AppUserDevices] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory_IAM] ([MigrationId], [ProductVersion])
VALUES (N'20260601080011_AddIamSoftDelete', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [AspNetRoles] ADD [DeletedAt] datetimeoffset NULL;

ALTER TABLE [AspNetRoles] ADD [DeletedBy] nvarchar(100) NULL;

ALTER TABLE [AspNetRoles] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);

CREATE INDEX [IX_AspNetRoles_IsDeleted] ON [AspNetRoles] ([IsDeleted]);

INSERT INTO [__EFMigrationsHistory_IAM] ([MigrationId], [ProductVersion])
VALUES (N'20260601132909_AddSoftDeleteToRolesAndIamFilterAlignment', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [Permissions] (
    [Id] uniqueidentifier NOT NULL,
    [Key] nvarchar(160) NOT NULL,
    [Context] nvarchar(80) NOT NULL,
    [Resource] nvarchar(80) NOT NULL,
    [Action] nvarchar(80) NOT NULL,
    [Description] nvarchar(300) NOT NULL,
    [IsActive] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [DeletedBy] nvarchar(120) NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(120) NOT NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedBy] nvarchar(120) NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Action', N'Context', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'IsActive', N'IsDeleted', N'Key', N'Resource', N'TenantId', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Permissions]'))
    SET IDENTITY_INSERT [Permissions] ON;
INSERT INTO [Permissions] ([Id], [Action], [Context], [CreatedAt], [CreatedBy], [DeletedAt], [DeletedBy], [Description], [IsActive], [IsDeleted], [Key], [Resource], [TenantId], [UpdatedAt], [UpdatedBy])
VALUES ('018fd81d-2c94-7ad0-a4a3-f1edb9b10101', N'view', N'IAM', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'View application users.', CAST(1 AS bit), CAST(0 AS bit), N'users.view', N'users', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10102', N'create', N'IAM', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Create application users.', CAST(1 AS bit), CAST(0 AS bit), N'users.create', N'users', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10103', N'edit', N'IAM', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Update application users.', CAST(1 AS bit), CAST(0 AS bit), N'users.edit', N'users', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10104', N'deactivate', N'IAM', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Deactivate application users.', CAST(1 AS bit), CAST(0 AS bit), N'users.deactivate', N'users', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10105', N'manage_roles', N'IAM', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Manage user role assignments.', CAST(1 AS bit), CAST(0 AS bit), N'users.manage_roles', N'users', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10106', N'manage_permissions', N'IAM', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Manage direct user permissions.', CAST(1 AS bit), CAST(0 AS bit), N'users.manage_permissions', N'users', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10201', N'view', N'IAM', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'View platform roles.', CAST(1 AS bit), CAST(0 AS bit), N'roles.view', N'roles', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10202', N'create', N'IAM', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Create platform roles.', CAST(1 AS bit), CAST(0 AS bit), N'roles.create', N'roles', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10203', N'edit', N'IAM', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Update platform roles.', CAST(1 AS bit), CAST(0 AS bit), N'roles.edit', N'roles', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10204', N'delete', N'IAM', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Delete platform roles.', CAST(1 AS bit), CAST(0 AS bit), N'roles.delete', N'roles', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10205', N'manage_permissions', N'IAM', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Manage role permission assignments.', CAST(1 AS bit), CAST(0 AS bit), N'roles.manage_permissions', N'roles', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10301', N'view', N'HR', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'View departments.', CAST(1 AS bit), CAST(0 AS bit), N'departments.view', N'departments', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10302', N'create', N'HR', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Create departments.', CAST(1 AS bit), CAST(0 AS bit), N'departments.create', N'departments', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10303', N'edit', N'HR', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Update departments.', CAST(1 AS bit), CAST(0 AS bit), N'departments.edit', N'departments', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10304', N'delete', N'HR', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Delete departments.', CAST(1 AS bit), CAST(0 AS bit), N'departments.delete', N'departments', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10401', N'view', N'HR', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'View employees.', CAST(1 AS bit), CAST(0 AS bit), N'employees.view', N'employees', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10402', N'create', N'HR', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Create employees.', CAST(1 AS bit), CAST(0 AS bit), N'employees.create', N'employees', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10403', N'edit', N'HR', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Update employees.', CAST(1 AS bit), CAST(0 AS bit), N'employees.edit', N'employees', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10404', N'delete', N'HR', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Delete employees.', CAST(1 AS bit), CAST(0 AS bit), N'employees.delete', N'employees', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10501', N'view', N'Banking', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'View customers.', CAST(1 AS bit), CAST(0 AS bit), N'customers.view', N'customers', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10502', N'create', N'Banking', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Create customers.', CAST(1 AS bit), CAST(0 AS bit), N'customers.create', N'customers', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10503', N'edit', N'Banking', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Update customers.', CAST(1 AS bit), CAST(0 AS bit), N'customers.edit', N'customers', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10504', N'delete', N'Banking', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Delete customers.', CAST(1 AS bit), CAST(0 AS bit), N'customers.delete', N'customers', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10601', N'view', N'Platform', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'View menu catalog.', CAST(1 AS bit), CAST(0 AS bit), N'menus.view', N'menus', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10602', N'create', N'Platform', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Create menu items.', CAST(1 AS bit), CAST(0 AS bit), N'menus.create', N'menus', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10603', N'edit', N'Platform', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Update menu items.', CAST(1 AS bit), CAST(0 AS bit), N'menus.edit', N'menus', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9b10604', N'delete', N'Platform', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Delete menu items.', CAST(1 AS bit), CAST(0 AS bit), N'menus.delete', N'menus', '00000000-0000-0000-0000-000000000000', NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Action', N'Context', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'IsActive', N'IsDeleted', N'Key', N'Resource', N'TenantId', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[Permissions]'))
    SET IDENTITY_INSERT [Permissions] OFF;

CREATE UNIQUE INDEX [UX_Permissions_Key] ON [Permissions] ([Key]);

INSERT INTO [__EFMigrationsHistory_IAM] ([MigrationId], [ProductVersion])
VALUES (N'20260601171352_AddPermissionCatalog', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [MenuItems] (
    [Id] uniqueidentifier NOT NULL,
    [ParentId] uniqueidentifier NULL,
    [Key] nvarchar(120) NOT NULL,
    [Title] nvarchar(120) NOT NULL,
    [Description] nvarchar(300) NOT NULL,
    [Url] nvarchar(240) NOT NULL,
    [Icon] nvarchar(80) NOT NULL,
    [Placement] nvarchar(40) NOT NULL,
    [DisplayOrder] int NOT NULL,
    [RequiredPermissionKey] nvarchar(160) NULL,
    [IsActive] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [DeletedBy] nvarchar(120) NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(120) NOT NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedBy] nvarchar(120) NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_MenuItems] PRIMARY KEY ([Id])
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'DisplayOrder', N'Icon', N'IsActive', N'IsDeleted', N'Key', N'ParentId', N'Placement', N'RequiredPermissionKey', N'TenantId', N'Title', N'UpdatedAt', N'UpdatedBy', N'Url') AND [object_id] = OBJECT_ID(N'[MenuItems]'))
    SET IDENTITY_INSERT [MenuItems] ON;
INSERT INTO [MenuItems] ([Id], [CreatedAt], [CreatedBy], [DeletedAt], [DeletedBy], [Description], [DisplayOrder], [Icon], [IsActive], [IsDeleted], [Key], [ParentId], [Placement], [RequiredPermissionKey], [TenantId], [Title], [UpdatedAt], [UpdatedBy], [Url])
VALUES ('018fd81d-2c94-7ad0-a4a3-f1edb9c10101', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Operations dashboard.', 10, N'Dashboard', CAST(1 AS bit), CAST(0 AS bit), N'dashboard', NULL, N'Sidebar', NULL, '00000000-0000-0000-0000-000000000000', N'Dashboard', NULL, NULL, N'/dashboard'),
('018fd81d-2c94-7ad0-a4a3-f1edb9c10201', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Administrative workspace.', 20, N'AdminPanelSettings', CAST(1 AS bit), CAST(0 AS bit), N'admin-center', NULL, N'Sidebar', NULL, '00000000-0000-0000-0000-000000000000', N'Admin Center', NULL, NULL, N'/admin'),
('018fd81d-2c94-7ad0-a4a3-f1edb9c10301', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Architecture and solution overview.', 90, N'AutoStories', CAST(1 AS bit), CAST(0 AS bit), N'solution-overview', NULL, N'Sidebar', NULL, '00000000-0000-0000-0000-000000000000', N'Solution Overview', NULL, NULL, N'/overview'),
('018fd81d-2c94-7ad0-a4a3-f1edb9c20101', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Customer records and onboarding.', 10, N'Business', CAST(1 AS bit), CAST(0 AS bit), N'admin-customers', '018fd81d-2c94-7ad0-a4a3-f1edb9c10201', N'AdminCenter', NULL, '00000000-0000-0000-0000-000000000000', N'Customers', NULL, NULL, N'/admin/customers'),
('018fd81d-2c94-7ad0-a4a3-f1edb9c20102', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Department catalog and staff grouping.', 20, N'AccountTree', CAST(1 AS bit), CAST(0 AS bit), N'admin-departments', '018fd81d-2c94-7ad0-a4a3-f1edb9c10201', N'AdminCenter', NULL, '00000000-0000-0000-0000-000000000000', N'Departments', NULL, NULL, N'/admin/departments'),
('018fd81d-2c94-7ad0-a4a3-f1edb9c20103', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Staff records and system access.', 30, N'Badge', CAST(1 AS bit), CAST(0 AS bit), N'admin-employees', '018fd81d-2c94-7ad0-a4a3-f1edb9c10201', N'AdminCenter', NULL, '00000000-0000-0000-0000-000000000000', N'Employees', NULL, NULL, N'/admin/employees'),
('018fd81d-2c94-7ad0-a4a3-f1edb9c20104', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Navigation catalog and menu visibility.', 40, N'MenuOpen', CAST(1 AS bit), CAST(0 AS bit), N'admin-menus', '018fd81d-2c94-7ad0-a4a3-f1edb9c10201', N'AdminCenter', NULL, '00000000-0000-0000-0000-000000000000', N'Menus', NULL, NULL, N'/admin/menus'),
('018fd81d-2c94-7ad0-a4a3-f1edb9c20105', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Permission catalog and access keys.', 50, N'LockPerson', CAST(1 AS bit), CAST(0 AS bit), N'admin-permissions', '018fd81d-2c94-7ad0-a4a3-f1edb9c10201', N'AdminCenter', NULL, '00000000-0000-0000-0000-000000000000', N'Permissions', NULL, NULL, N'/admin/permissions'),
('018fd81d-2c94-7ad0-a4a3-f1edb9c20106', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Role catalog and assignments.', 60, N'AdminPanelSettings', CAST(1 AS bit), CAST(0 AS bit), N'admin-roles', '018fd81d-2c94-7ad0-a4a3-f1edb9c10201', N'AdminCenter', NULL, '00000000-0000-0000-0000-000000000000', N'Roles', NULL, NULL, N'/admin/roles'),
('018fd81d-2c94-7ad0-a4a3-f1edb9c20107', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Platform configuration surface.', 70, N'Settings', CAST(1 AS bit), CAST(0 AS bit), N'admin-settings', '018fd81d-2c94-7ad0-a4a3-f1edb9c10201', N'AdminCenter', NULL, '00000000-0000-0000-0000-000000000000', N'Settings', NULL, NULL, N'/admin/settings'),
('018fd81d-2c94-7ad0-a4a3-f1edb9c20108', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Trusted device review and revocation.', 80, N'Devices', CAST(1 AS bit), CAST(0 AS bit), N'admin-user-devices', '018fd81d-2c94-7ad0-a4a3-f1edb9c10201', N'AdminCenter', NULL, '00000000-0000-0000-0000-000000000000', N'User Devices', NULL, NULL, N'/admin/user-devices'),
('018fd81d-2c94-7ad0-a4a3-f1edb9c20109', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, N'Create accounts and manage lifecycle.', 90, N'Group', CAST(1 AS bit), CAST(0 AS bit), N'admin-users', '018fd81d-2c94-7ad0-a4a3-f1edb9c10201', N'AdminCenter', NULL, '00000000-0000-0000-0000-000000000000', N'Users', NULL, NULL, N'/admin/users');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'DisplayOrder', N'Icon', N'IsActive', N'IsDeleted', N'Key', N'ParentId', N'Placement', N'RequiredPermissionKey', N'TenantId', N'Title', N'UpdatedAt', N'UpdatedBy', N'Url') AND [object_id] = OBJECT_ID(N'[MenuItems]'))
    SET IDENTITY_INSERT [MenuItems] OFF;

CREATE INDEX [IX_MenuItems_Placement_DisplayOrder] ON [MenuItems] ([Placement], [DisplayOrder]);

CREATE UNIQUE INDEX [UX_MenuItems_Key] ON [MenuItems] ([Key]);

INSERT INTO [__EFMigrationsHistory_IAM] ([MigrationId], [ProductVersion])
VALUES (N'20260602015650_AddMenuCatalog', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
DROP INDEX [IX_MenuItems_Placement_DisplayOrder] ON [MenuItems];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItems]') AND [c].[name] = N'DisplayOrder');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [MenuItems] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [MenuItems] DROP COLUMN [DisplayOrder];

ALTER TABLE [Permissions] ADD [DepartmentId] uniqueidentifier NULL;

ALTER TABLE [MenuItems] ADD [DepartmentId] uniqueidentifier NULL;

ALTER TABLE [AspNetRoles] ADD [DepartmentId] uniqueidentifier NULL;

UPDATE [MenuItems] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c10101';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c10201';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c10301';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20101';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20102';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20103';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20104';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20105';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20106';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20107';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20108';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20109';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10101';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10102';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10103';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10104';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10105';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10106';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10201';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10202';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10203';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10204';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10205';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10301';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10302';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10303';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10304';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10401';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10402';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10403';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10404';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10501';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10502';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10503';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10504';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10601';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10602';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10603';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [DepartmentId] = NULL
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10604';
SELECT @@ROWCOUNT;


CREATE INDEX [IX_Permissions_DepartmentId] ON [Permissions] ([DepartmentId]);

CREATE INDEX [IX_MenuItems_Placement_Parent_Department_Title] ON [MenuItems] ([Placement], [ParentId], [DepartmentId], [Title]);

CREATE INDEX [IX_AspNetRoles_DepartmentId] ON [AspNetRoles] ([DepartmentId]);

INSERT INTO [__EFMigrationsHistory_IAM] ([MigrationId], [ProductVersion])
VALUES (N'20260602104152_HardenIamScopeAndMenus', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [MenuIcons] (
    [Id] uniqueidentifier NOT NULL,
    [Key] nvarchar(80) NOT NULL,
    [Label] nvarchar(120) NOT NULL,
    [Description] nvarchar(300) NOT NULL,
    [IsActive] bit NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(120) NOT NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedBy] nvarchar(120) NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_MenuIcons] PRIMARY KEY ([Id])
);

CREATE TABLE [MenuPlacements] (
    [Id] uniqueidentifier NOT NULL,
    [Key] nvarchar(80) NOT NULL,
    [Label] nvarchar(120) NOT NULL,
    [Description] nvarchar(300) NOT NULL,
    [IsActive] bit NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(120) NOT NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedBy] nvarchar(120) NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_MenuPlacements] PRIMARY KEY ([Id])
);

CREATE TABLE [MenuRoutes] (
    [Id] uniqueidentifier NOT NULL,
    [Key] nvarchar(120) NOT NULL,
    [Label] nvarchar(120) NOT NULL,
    [Url] nvarchar(240) NOT NULL,
    [PlacementKey] nvarchar(80) NOT NULL,
    [Description] nvarchar(300) NOT NULL,
    [IsActive] bit NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(120) NOT NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedBy] nvarchar(120) NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_MenuRoutes] PRIMARY KEY ([Id])
);

CREATE TABLE [PermissionActions] (
    [Id] uniqueidentifier NOT NULL,
    [Key] nvarchar(80) NOT NULL,
    [Label] nvarchar(120) NOT NULL,
    [Description] nvarchar(300) NOT NULL,
    [IsActive] bit NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(120) NOT NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedBy] nvarchar(120) NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_PermissionActions] PRIMARY KEY ([Id])
);

CREATE TABLE [PermissionContexts] (
    [Id] uniqueidentifier NOT NULL,
    [Key] nvarchar(80) NOT NULL,
    [Label] nvarchar(120) NOT NULL,
    [Description] nvarchar(300) NOT NULL,
    [IsActive] bit NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(120) NOT NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedBy] nvarchar(120) NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_PermissionContexts] PRIMARY KEY ([Id])
);

CREATE TABLE [PermissionResources] (
    [Id] uniqueidentifier NOT NULL,
    [Key] nvarchar(80) NOT NULL,
    [Label] nvarchar(120) NOT NULL,
    [ContextKey] nvarchar(80) NOT NULL,
    [Description] nvarchar(300) NOT NULL,
    [IsActive] bit NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(120) NOT NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedBy] nvarchar(120) NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_PermissionResources] PRIMARY KEY ([Id])
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'Key', N'Label', N'TenantId', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[MenuIcons]'))
    SET IDENTITY_INSERT [MenuIcons] ON;
INSERT INTO [MenuIcons] ([Id], [CreatedAt], [CreatedBy], [Description], [IsActive], [Key], [Label], [TenantId], [UpdatedAt], [UpdatedBy])
VALUES ('018fd81d-2c94-7ad0-a4a3-f1edb9d10501', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved MudBlazor icon key.', CAST(1 AS bit), N'AccountTree', N'Account tree', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10502', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved MudBlazor icon key.', CAST(1 AS bit), N'AdminPanelSettings', N'Admin panel', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10503', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved MudBlazor icon key.', CAST(1 AS bit), N'AutoStories', N'Story/book', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10504', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved MudBlazor icon key.', CAST(1 AS bit), N'Badge', N'Badge', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10505', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved MudBlazor icon key.', CAST(1 AS bit), N'Business', N'Business', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10506', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved MudBlazor icon key.', CAST(1 AS bit), N'Dashboard', N'Dashboard', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10507', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved MudBlazor icon key.', CAST(1 AS bit), N'Devices', N'Devices', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10508', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved MudBlazor icon key.', CAST(1 AS bit), N'Group', N'Group', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10509', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved MudBlazor icon key.', CAST(1 AS bit), N'LockPerson', N'Security lock', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10510', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved MudBlazor icon key.', CAST(1 AS bit), N'Menu', N'Generic menu', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10511', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved MudBlazor icon key.', CAST(1 AS bit), N'MenuOpen', N'Menu', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10512', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved MudBlazor icon key.', CAST(1 AS bit), N'Settings', N'Settings', '00000000-0000-0000-0000-000000000000', NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'Key', N'Label', N'TenantId', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[MenuIcons]'))
    SET IDENTITY_INSERT [MenuIcons] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'Key', N'Label', N'TenantId', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[MenuPlacements]'))
    SET IDENTITY_INSERT [MenuPlacements] ON;
INSERT INTO [MenuPlacements] ([Id], [CreatedAt], [CreatedBy], [Description], [IsActive], [Key], [Label], [TenantId], [UpdatedAt], [UpdatedBy])
VALUES ('018fd81d-2c94-7ad0-a4a3-f1edb9d10401', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Main application navigation.', CAST(1 AS bit), N'Sidebar', N'Sidebar', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10402', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Administration landing tiles.', CAST(1 AS bit), N'AdminCenter', N'Admin Center', '00000000-0000-0000-0000-000000000000', NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'Key', N'Label', N'TenantId', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[MenuPlacements]'))
    SET IDENTITY_INSERT [MenuPlacements] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'Key', N'Label', N'PlacementKey', N'TenantId', N'UpdatedAt', N'UpdatedBy', N'Url') AND [object_id] = OBJECT_ID(N'[MenuRoutes]'))
    SET IDENTITY_INSERT [MenuRoutes] ON;
INSERT INTO [MenuRoutes] ([Id], [CreatedAt], [CreatedBy], [Description], [IsActive], [Key], [Label], [PlacementKey], [TenantId], [UpdatedAt], [UpdatedBy], [Url])
VALUES ('018fd81d-2c94-7ad0-a4a3-f1edb9d10601', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved application route.', CAST(1 AS bit), N'dashboard', N'Dashboard', N'Sidebar', '00000000-0000-0000-0000-000000000000', NULL, NULL, N'/dashboard'),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10602', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved application route.', CAST(1 AS bit), N'admin-center', N'Admin Center', N'Sidebar', '00000000-0000-0000-0000-000000000000', NULL, NULL, N'/admin'),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10603', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved application route.', CAST(1 AS bit), N'solution-overview', N'Solution Overview', N'Sidebar', '00000000-0000-0000-0000-000000000000', NULL, NULL, N'/overview'),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10604', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved application route.', CAST(1 AS bit), N'admin-customers', N'Customers', N'AdminCenter', '00000000-0000-0000-0000-000000000000', NULL, NULL, N'/admin/customers'),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10605', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved application route.', CAST(1 AS bit), N'admin-departments', N'Departments', N'AdminCenter', '00000000-0000-0000-0000-000000000000', NULL, NULL, N'/admin/departments'),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10606', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved application route.', CAST(1 AS bit), N'admin-employees', N'Employees', N'AdminCenter', '00000000-0000-0000-0000-000000000000', NULL, NULL, N'/admin/employees'),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10607', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved application route.', CAST(1 AS bit), N'admin-menus', N'Menus', N'AdminCenter', '00000000-0000-0000-0000-000000000000', NULL, NULL, N'/admin/menus'),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10608', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved application route.', CAST(1 AS bit), N'admin-permissions', N'Permissions', N'AdminCenter', '00000000-0000-0000-0000-000000000000', NULL, NULL, N'/admin/permissions'),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10609', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved application route.', CAST(1 AS bit), N'admin-roles', N'Roles', N'AdminCenter', '00000000-0000-0000-0000-000000000000', NULL, NULL, N'/admin/roles'),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10610', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved application route.', CAST(1 AS bit), N'admin-settings', N'Settings', N'AdminCenter', '00000000-0000-0000-0000-000000000000', NULL, NULL, N'/admin/settings'),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10611', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved application route.', CAST(1 AS bit), N'admin-user-devices', N'User Devices', N'AdminCenter', '00000000-0000-0000-0000-000000000000', NULL, NULL, N'/admin/user-devices'),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10612', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved application route.', CAST(1 AS bit), N'admin-users', N'Users', N'AdminCenter', '00000000-0000-0000-0000-000000000000', NULL, NULL, N'/admin/users');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'Key', N'Label', N'PlacementKey', N'TenantId', N'UpdatedAt', N'UpdatedBy', N'Url') AND [object_id] = OBJECT_ID(N'[MenuRoutes]'))
    SET IDENTITY_INSERT [MenuRoutes] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'Key', N'Label', N'TenantId', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[PermissionActions]'))
    SET IDENTITY_INSERT [PermissionActions] ON;
INSERT INTO [PermissionActions] ([Id], [CreatedAt], [CreatedBy], [Description], [IsActive], [Key], [Label], [TenantId], [UpdatedAt], [UpdatedBy])
VALUES ('018fd81d-2c94-7ad0-a4a3-f1edb9d10301', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Read and list records.', CAST(1 AS bit), N'view', N'View', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10302', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Create new records.', CAST(1 AS bit), N'create', N'Create', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10303', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Update existing records.', CAST(1 AS bit), N'edit', N'Edit', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10304', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Soft-delete or remove records.', CAST(1 AS bit), N'delete', N'Delete', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10305', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Disable active records or accounts.', CAST(1 AS bit), N'deactivate', N'Deactivate', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10306', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Assign or revoke permissions.', CAST(1 AS bit), N'manage_permissions', N'Manage permissions', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10307', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Assign or revoke roles.', CAST(1 AS bit), N'manage_roles', N'Manage roles', '00000000-0000-0000-0000-000000000000', NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'Key', N'Label', N'TenantId', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[PermissionActions]'))
    SET IDENTITY_INSERT [PermissionActions] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'Key', N'Label', N'TenantId', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[PermissionContexts]'))
    SET IDENTITY_INSERT [PermissionContexts] ON;
INSERT INTO [PermissionContexts] ([Id], [CreatedAt], [CreatedBy], [Description], [IsActive], [Key], [Label], [TenantId], [UpdatedAt], [UpdatedBy])
VALUES ('018fd81d-2c94-7ad0-a4a3-f1edb9d10101', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Customer, accounts, loans, and financial operations.', CAST(1 AS bit), N'Banking', N'Banking', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10102', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Departments, employees, and staff operations.', CAST(1 AS bit), N'HR', N'Human Resources', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10103', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Users, roles, permissions, sessions, and devices.', CAST(1 AS bit), N'IAM', N'Identity and Access', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10104', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Cross-cutting platform configuration and navigation.', CAST(1 AS bit), N'Platform', N'Platform', '00000000-0000-0000-0000-000000000000', NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'Key', N'Label', N'TenantId', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[PermissionContexts]'))
    SET IDENTITY_INSERT [PermissionContexts] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ContextKey', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'Key', N'Label', N'TenantId', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[PermissionResources]'))
    SET IDENTITY_INSERT [PermissionResources] ON;
INSERT INTO [PermissionResources] ([Id], [ContextKey], [CreatedAt], [CreatedBy], [Description], [IsActive], [Key], [Label], [TenantId], [UpdatedAt], [UpdatedBy])
VALUES ('018fd81d-2c94-7ad0-a4a3-f1edb9d10201', N'Banking', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Customer records and onboarding.', CAST(1 AS bit), N'customers', N'Customers', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10202', N'HR', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Department catalog and staff grouping.', CAST(1 AS bit), N'departments', N'Departments', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10203', N'HR', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Employee records and IAM linkage.', CAST(1 AS bit), N'employees', N'Employees', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10204', N'Platform', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Navigation registry and menu visibility.', CAST(1 AS bit), N'menus', N'Menus', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10205', N'IAM', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Permission catalog and assignment surface.', CAST(1 AS bit), N'permissions', N'Permissions', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10206', N'IAM', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Role catalog and permission bundles.', CAST(1 AS bit), N'roles', N'Roles', '00000000-0000-0000-0000-000000000000', NULL, NULL),
('018fd81d-2c94-7ad0-a4a3-f1edb9d10207', N'IAM', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Application user accounts.', CAST(1 AS bit), N'users', N'Users', '00000000-0000-0000-0000-000000000000', NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ContextKey', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'Key', N'Label', N'TenantId', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[PermissionResources]'))
    SET IDENTITY_INSERT [PermissionResources] OFF;

CREATE UNIQUE INDEX [UX_MenuIcons_Key] ON [MenuIcons] ([Key]);

CREATE UNIQUE INDEX [UX_MenuPlacements_Key] ON [MenuPlacements] ([Key]);

CREATE UNIQUE INDEX [UX_MenuRoutes_Key] ON [MenuRoutes] ([Key]);

CREATE UNIQUE INDEX [UX_MenuRoutes_Url] ON [MenuRoutes] ([Url]);

CREATE UNIQUE INDEX [UX_PermissionActions_Key] ON [PermissionActions] ([Key]);

CREATE UNIQUE INDEX [UX_PermissionContexts_Key] ON [PermissionContexts] ([Key]);

CREATE INDEX [IX_PermissionResources_Key] ON [PermissionResources] ([Key]);

CREATE UNIQUE INDEX [UX_PermissionResources_Context_Key] ON [PermissionResources] ([ContextKey], [Key]);

INSERT INTO [__EFMigrationsHistory_IAM] ([MigrationId], [ProductVersion])
VALUES (N'20260602231445_AddIamReferenceCatalogs', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'DepartmentId', N'Description', N'Icon', N'IsActive', N'IsDeleted', N'Key', N'ParentId', N'Placement', N'RequiredPermissionKey', N'TenantId', N'Title', N'UpdatedAt', N'UpdatedBy', N'Url') AND [object_id] = OBJECT_ID(N'[MenuItems]'))
    SET IDENTITY_INSERT [MenuItems] ON;
INSERT INTO [MenuItems] ([Id], [CreatedAt], [CreatedBy], [DeletedAt], [DeletedBy], [DepartmentId], [Description], [Icon], [IsActive], [IsDeleted], [Key], [ParentId], [Placement], [RequiredPermissionKey], [TenantId], [Title], [UpdatedAt], [UpdatedBy], [Url])
VALUES ('018fd81d-2c94-7ad0-a4a3-f1edb9c20110', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, NULL, N'Source-of-truth permission and menu reference data.', N'LockPerson', CAST(1 AS bit), CAST(0 AS bit), N'admin-access-catalog', '018fd81d-2c94-7ad0-a4a3-f1edb9c10201', N'AdminCenter', NULL, '00000000-0000-0000-0000-000000000000', N'Access Catalog', NULL, NULL, N'/admin/access-catalog');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'DepartmentId', N'Description', N'Icon', N'IsActive', N'IsDeleted', N'Key', N'ParentId', N'Placement', N'RequiredPermissionKey', N'TenantId', N'Title', N'UpdatedAt', N'UpdatedBy', N'Url') AND [object_id] = OBJECT_ID(N'[MenuItems]'))
    SET IDENTITY_INSERT [MenuItems] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'Key', N'Label', N'PlacementKey', N'TenantId', N'UpdatedAt', N'UpdatedBy', N'Url') AND [object_id] = OBJECT_ID(N'[MenuRoutes]'))
    SET IDENTITY_INSERT [MenuRoutes] ON;
INSERT INTO [MenuRoutes] ([Id], [CreatedAt], [CreatedBy], [Description], [IsActive], [Key], [Label], [PlacementKey], [TenantId], [UpdatedAt], [UpdatedBy], [Url])
VALUES ('018fd81d-2c94-7ad0-a4a3-f1edb9d10613', '2026-01-01T00:00:00.0000000+00:00', N'System', N'Approved application route.', CAST(1 AS bit), N'admin-access-catalog', N'Access Catalog', N'AdminCenter', '00000000-0000-0000-0000-000000000000', NULL, NULL, N'/admin/access-catalog');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'Key', N'Label', N'PlacementKey', N'TenantId', N'UpdatedAt', N'UpdatedBy', N'Url') AND [object_id] = OBJECT_ID(N'[MenuRoutes]'))
    SET IDENTITY_INSERT [MenuRoutes] OFF;

INSERT INTO [__EFMigrationsHistory_IAM] ([MigrationId], [ProductVersion])
VALUES (N'20260602234334_AddAccessCatalogManagement', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [PermissionResources] ADD [DeletedAt] datetimeoffset NULL;

ALTER TABLE [PermissionResources] ADD [DeletedBy] nvarchar(max) NULL;

ALTER TABLE [PermissionResources] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [PermissionContexts] ADD [DeletedAt] datetimeoffset NULL;

ALTER TABLE [PermissionContexts] ADD [DeletedBy] nvarchar(max) NULL;

ALTER TABLE [PermissionContexts] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [PermissionActions] ADD [DeletedAt] datetimeoffset NULL;

ALTER TABLE [PermissionActions] ADD [DeletedBy] nvarchar(max) NULL;

ALTER TABLE [PermissionActions] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [MenuRoutes] ADD [DeletedAt] datetimeoffset NULL;

ALTER TABLE [MenuRoutes] ADD [DeletedBy] nvarchar(max) NULL;

ALTER TABLE [MenuRoutes] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [MenuPlacements] ADD [DeletedAt] datetimeoffset NULL;

ALTER TABLE [MenuPlacements] ADD [DeletedBy] nvarchar(max) NULL;

ALTER TABLE [MenuPlacements] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [MenuIcons] ADD [DeletedAt] datetimeoffset NULL;

ALTER TABLE [MenuIcons] ADD [DeletedBy] nvarchar(max) NULL;

ALTER TABLE [MenuIcons] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);

UPDATE [MenuIcons] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10501';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10502';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10503';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10504';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10505';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10506';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10507';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10508';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10509';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10510';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10511';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10512';
SELECT @@ROWCOUNT;


UPDATE [MenuPlacements] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10401';
SELECT @@ROWCOUNT;


UPDATE [MenuPlacements] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10402';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10601';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10602';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10603';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10604';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10605';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10606';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10607';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10608';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10609';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10610';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10611';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10612';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10613';
SELECT @@ROWCOUNT;


UPDATE [PermissionActions] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10301';
SELECT @@ROWCOUNT;


UPDATE [PermissionActions] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10302';
SELECT @@ROWCOUNT;


UPDATE [PermissionActions] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10303';
SELECT @@ROWCOUNT;


UPDATE [PermissionActions] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10304';
SELECT @@ROWCOUNT;


UPDATE [PermissionActions] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10305';
SELECT @@ROWCOUNT;


UPDATE [PermissionActions] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10306';
SELECT @@ROWCOUNT;


UPDATE [PermissionActions] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10307';
SELECT @@ROWCOUNT;


UPDATE [PermissionContexts] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10101';
SELECT @@ROWCOUNT;


UPDATE [PermissionContexts] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10102';
SELECT @@ROWCOUNT;


UPDATE [PermissionContexts] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10103';
SELECT @@ROWCOUNT;


UPDATE [PermissionContexts] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10104';
SELECT @@ROWCOUNT;


UPDATE [PermissionResources] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10201';
SELECT @@ROWCOUNT;


UPDATE [PermissionResources] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10202';
SELECT @@ROWCOUNT;


UPDATE [PermissionResources] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10203';
SELECT @@ROWCOUNT;


UPDATE [PermissionResources] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10204';
SELECT @@ROWCOUNT;


UPDATE [PermissionResources] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10205';
SELECT @@ROWCOUNT;


UPDATE [PermissionResources] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10206';
SELECT @@ROWCOUNT;


UPDATE [PermissionResources] SET [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit)
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10207';
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory_IAM] ([MigrationId], [ProductVersion])
VALUES (N'20260606114952_AddIamReferenceDataSoftDelete', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
UPDATE MenuIcons SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
UPDATE MenuItems SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
UPDATE MenuPlacements SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
UPDATE MenuRoutes SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
UPDATE PermissionActions SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
UPDATE PermissionContexts SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
UPDATE PermissionResources SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
UPDATE Permissions SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';

UPDATE [MenuIcons] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10501';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10502';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10503';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10504';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10505';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10506';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10507';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10508';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10509';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10510';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10511';
SELECT @@ROWCOUNT;


UPDATE [MenuIcons] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10512';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c10101';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c10201';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c10301';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20101';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20102';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20103';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20104';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20105';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20106';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20107';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20108';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20109';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20110';
SELECT @@ROWCOUNT;


UPDATE [MenuPlacements] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10401';
SELECT @@ROWCOUNT;


UPDATE [MenuPlacements] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10402';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10601';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10602';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10603';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10604';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10605';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10606';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10607';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10608';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10609';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10610';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10611';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10612';
SELECT @@ROWCOUNT;


UPDATE [MenuRoutes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10613';
SELECT @@ROWCOUNT;


UPDATE [PermissionActions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10301';
SELECT @@ROWCOUNT;


UPDATE [PermissionActions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10302';
SELECT @@ROWCOUNT;


UPDATE [PermissionActions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10303';
SELECT @@ROWCOUNT;


UPDATE [PermissionActions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10304';
SELECT @@ROWCOUNT;


UPDATE [PermissionActions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10305';
SELECT @@ROWCOUNT;


UPDATE [PermissionActions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10306';
SELECT @@ROWCOUNT;


UPDATE [PermissionActions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10307';
SELECT @@ROWCOUNT;


UPDATE [PermissionContexts] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10101';
SELECT @@ROWCOUNT;


UPDATE [PermissionContexts] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10102';
SELECT @@ROWCOUNT;


UPDATE [PermissionContexts] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10103';
SELECT @@ROWCOUNT;


UPDATE [PermissionContexts] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10104';
SELECT @@ROWCOUNT;


UPDATE [PermissionResources] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10201';
SELECT @@ROWCOUNT;


UPDATE [PermissionResources] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10202';
SELECT @@ROWCOUNT;


UPDATE [PermissionResources] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10203';
SELECT @@ROWCOUNT;


UPDATE [PermissionResources] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10204';
SELECT @@ROWCOUNT;


UPDATE [PermissionResources] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10205';
SELECT @@ROWCOUNT;


UPDATE [PermissionResources] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10206';
SELECT @@ROWCOUNT;


UPDATE [PermissionResources] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9d10207';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10101';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10102';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10103';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10104';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10105';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10106';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10201';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10202';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10203';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10204';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10205';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10301';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10302';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10303';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10304';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10401';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10402';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10403';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10404';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10501';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10502';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10503';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10504';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10601';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10602';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10603';
SELECT @@ROWCOUNT;


UPDATE [Permissions] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9b10604';
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory_IAM] ([MigrationId], [ProductVersion])
VALUES (N'20260609013225_AlignIamSeedsWithTenantFiltering', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
DELETE FROM [MenuItems]
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20105';
SELECT @@ROWCOUNT;


DELETE FROM [MenuItems]
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20106';
SELECT @@ROWCOUNT;


DELETE FROM [MenuItems]
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20108';
SELECT @@ROWCOUNT;


DELETE FROM [MenuItems]
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20109';
SELECT @@ROWCOUNT;


ALTER TABLE [MenuItems] ADD [DisplayOrder] int NOT NULL DEFAULT 0;

UPDATE [MenuItems] SET [DisplayOrder] = 1
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c10101';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DisplayOrder] = 2
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c10201';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DisplayOrder] = 3
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c10301';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DisplayOrder] = 10
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20101';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DisplayOrder] = 20
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20102';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DisplayOrder] = 30
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20103';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DisplayOrder] = 40
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20104';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DisplayOrder] = 50
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20107';
SELECT @@ROWCOUNT;


UPDATE [MenuItems] SET [DisplayOrder] = 70
WHERE [Id] = '018fd81d-2c94-7ad0-a4a3-f1edb9c20110';
SELECT @@ROWCOUNT;


IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'DepartmentId', N'Description', N'DisplayOrder', N'Icon', N'IsActive', N'IsDeleted', N'Key', N'ParentId', N'Placement', N'RequiredPermissionKey', N'TenantId', N'Title', N'UpdatedAt', N'UpdatedBy', N'Url') AND [object_id] = OBJECT_ID(N'[MenuItems]'))
    SET IDENTITY_INSERT [MenuItems] ON;
INSERT INTO [MenuItems] ([Id], [CreatedAt], [CreatedBy], [DeletedAt], [DeletedBy], [DepartmentId], [Description], [DisplayOrder], [Icon], [IsActive], [IsDeleted], [Key], [ParentId], [Placement], [RequiredPermissionKey], [TenantId], [Title], [UpdatedAt], [UpdatedBy], [Url])
VALUES ('018fd81d-2c94-7ad0-a4a3-f1edb9c10401', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, NULL, N'Manage subscription and payments.', 4, N'CreditCard', CAST(1 AS bit), CAST(0 AS bit), N'billing', NULL, N'Sidebar', NULL, '0194f700-0000-7000-8000-000000000001', N'Billing', NULL, NULL, N'/billing'),
('018fd81d-2c94-7ad0-a4a3-f1edb9c20111', '2026-01-01T00:00:00.0000000+00:00', N'System', NULL, NULL, NULL, N'Manage users, roles, permissions, and trusted devices.', 60, N'Group', CAST(1 AS bit), CAST(0 AS bit), N'admin-iam', '018fd81d-2c94-7ad0-a4a3-f1edb9c10201', N'AdminCenter', NULL, '0194f700-0000-7000-8000-000000000001', N'Identity & Access', NULL, NULL, N'/admin/iam');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'DepartmentId', N'Description', N'DisplayOrder', N'Icon', N'IsActive', N'IsDeleted', N'Key', N'ParentId', N'Placement', N'RequiredPermissionKey', N'TenantId', N'Title', N'UpdatedAt', N'UpdatedBy', N'Url') AND [object_id] = OBJECT_ID(N'[MenuItems]'))
    SET IDENTITY_INSERT [MenuItems] OFF;

INSERT INTO [__EFMigrationsHistory_IAM] ([MigrationId], [ProductVersion])
VALUES (N'20260708094838_AddMenuDisplayOrder', N'10.0.8');

COMMIT;
GO

