using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConnectIpsPaymentConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConnectIpsPaymentConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GatewayUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    MerchantId = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    AppName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ValidationApiUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    UsernameForValidationApi = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    PasswordForValidationApi = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    PasswordForCreditorPfx = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    TransactionCurrency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectIpsPaymentConfiguration", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConnectIpsPaymentConfiguration");
        }
    }
}
