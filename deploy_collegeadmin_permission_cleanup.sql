-- Removes permissions removed from CollegeAdmin in Permissions.cs:
--   colleges.view, students.view, students.create, students.edit, permissions.manage
-- Safe to run on production. Idempotent (no-op if rows already absent).
BEGIN TRANSACTION;

DELETE rp
FROM [RolePermissions] rp
INNER JOIN [AspNetRoles] r ON r.[Id] = rp.[RoleId]
INNER JOIN [Permissions] p ON p.[Id] = rp.[PermissionId]
WHERE r.[Name] = N'CollegeAdmin'
  AND p.[Name] IN (N'colleges.view', N'students.view', N'students.create', N'students.edit', N'permissions.manage');

COMMIT;
GO
