-- Adds practical marks entry permissions for FacultyAdmin / Central (SuperAdmin).
-- Safe to run on production. Idempotent (no-op if rows already present).
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Name] = N'practicalmarks.view')
BEGIN
    INSERT INTO [Permissions] ([Name], [DisplayName], [Description], [Group], [IsActive])
    VALUES (N'practicalmarks.view', N'View Practical Marks Entry', N'View practical marks entry for exam schedule students', N'practicalmarks', 1);
END;

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Name] = N'practicalmarks.submit')
BEGIN
    INSERT INTO [Permissions] ([Name], [DisplayName], [Description], [Group], [IsActive])
    VALUES (N'practicalmarks.submit', N'Submit Practical Marks', N'Submit practical marks for exam schedule students', N'practicalmarks', 1);
END;

INSERT INTO [RolePermissions] ([RoleId], [PermissionId])
SELECT r.[Id], p.[Id]
FROM [AspNetRoles] r
CROSS JOIN [Permissions] p
WHERE r.[Name] IN (N'SuperAdmin', N'FacultyAdmin')
  AND p.[Name] IN (N'practicalmarks.view', N'practicalmarks.submit')
  AND NOT EXISTS (
      SELECT 1 FROM [RolePermissions] rp
      WHERE rp.[RoleId] = r.[Id] AND rp.[PermissionId] = p.[Id]
  );

COMMIT;
GO
