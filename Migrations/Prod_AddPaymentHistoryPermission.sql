-- Adds the Payment History permission (paymenthistory.view) and grants it
-- to SuperAdmin and FacultyAdmin only. CollegeAdmin is intentionally excluded.
-- Idempotent: safe to re-run.
SET NOCOUNT ON;

DECLARE @PermissionId int;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [Name] = N'paymenthistory.view')
BEGIN
    INSERT INTO [dbo].[Permissions] ([Name], [DisplayName], [Description], [Group], [IsActive])
    VALUES (N'paymenthistory.view', N'View Payment History', N'Search all payment records regardless of status', N'paymenthistory', 1);
    SET @PermissionId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @PermissionId = [Id] FROM [dbo].[Permissions] WHERE [Name] = N'paymenthistory.view';
END

INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
SELECT r.[Id], @PermissionId
FROM [dbo].[Roles] r
WHERE r.[Name] IN (N'SuperAdmin', N'FacultyAdmin')
  AND NOT EXISTS (SELECT 1 FROM [dbo].[RolePermissions] rp
                  WHERE rp.[RoleId] = r.[Id] AND rp.[PermissionId] = @PermissionId);

SELECT @PermissionId AS PermissionId;