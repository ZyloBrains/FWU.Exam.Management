using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelAfterRecentChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[KhaltiConfiguration]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[KhaltiConfigurations]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[KhaltiConfiguration]', N'KhaltiConfigurations';
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[KhaltiConfigurations]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[KhaltiConfiguration]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[KhaltiConfigurations]', N'KhaltiConfiguration';
END
");
        }
    }
}
