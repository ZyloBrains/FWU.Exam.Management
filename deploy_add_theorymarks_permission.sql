-- Adds theory marks entry permissions for FacultyAdmin / Central (SuperAdmin).
-- Safe to run on production. Idempotent (no-op if rows already present).
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Name] = N'theorymarks.view')
BEGIN
    INSERT INTO [Permissions] ([Name], [DisplayName], [Description], [Group], [IsActive])
    VALUES (N'theorymarks.view', N'View Theory Marks Entry', N'View theory marks entry for exam schedule students', N'theorymarks', 1);
END;

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Name] = N'theorymarks.submit')
BEGIN
    INSERT INTO [Permissions] ([Name], [DisplayName], [Description], [Group], [IsActive])
    VALUES (N'theorymarks.submit', N'Submit Theory Marks', N'Submit theory marks for exam schedule students', N'theorymarks', 1);
END;

INSERT INTO [RolePermissions] ([RoleId], [PermissionId])
SELECT r.[Id], p.[Id]
FROM [AspNetRoles] r
CROSS JOIN [Permissions] p
WHERE r.[Name] IN (N'SuperAdmin', N'FacultyAdmin')
  AND p.[Name] IN (N'theorymarks.view', N'theorymarks.submit')
  AND NOT EXISTS (
      SELECT 1 FROM [RolePermissions] rp
      WHERE rp.[RoleId] = r.[Id] AND rp.[PermissionId] = p.[Id]
  );

COMMIT;
GO
