using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGumpNowEmailConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GumpNowEmailConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    FromAddr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OverrideUnsubscription = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GumpNowEmailConfigurations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GumpNowEmailConfigurations");
        }
    }
}
