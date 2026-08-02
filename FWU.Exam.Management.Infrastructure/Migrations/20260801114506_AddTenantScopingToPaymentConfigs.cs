using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantScopingToPaymentConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentType_PaymentTypeName",
                table: "PaymentType");

            migrationBuilder.DropIndex(
                name: "IX_Banks_BankCode",
                table: "Banks");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PaymentType",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "KhaltiConfigurations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ESewaConfiguration",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ConnectIpsPaymentConfiguration",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Banks",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [PaymentType] SET [TenantId] = (SELECT TOP 1 [Id] FROM [Tenants] ORDER BY [Id]) WHERE [TenantId] IS NULL;
                UPDATE [KhaltiConfigurations] SET [TenantId] = (SELECT TOP 1 [Id] FROM [Tenants] ORDER BY [Id]) WHERE [TenantId] IS NULL;
                UPDATE [ESewaConfiguration] SET [TenantId] = (SELECT TOP 1 [Id] FROM [Tenants] ORDER BY [Id]) WHERE [TenantId] IS NULL;
                UPDATE [ConnectIpsPaymentConfiguration] SET [TenantId] = (SELECT TOP 1 [Id] FROM [Tenants] ORDER BY [Id]) WHERE [TenantId] IS NULL;
                UPDATE [Banks] SET [TenantId] = (SELECT TOP 1 [Id] FROM [Tenants] ORDER BY [Id]) WHERE [TenantId] IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentType_TenantId_PaymentTypeName",
                table: "PaymentType",
                columns: new[] { "TenantId", "PaymentTypeName" },
                unique: true,
                filter: "[PaymentTypeName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_KhaltiConfigurations_TenantId",
                table: "KhaltiConfigurations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ESewaConfiguration_TenantId",
                table: "ESewaConfiguration",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectIpsPaymentConfiguration_TenantId",
                table: "ConnectIpsPaymentConfiguration",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Banks_TenantId_BankCode",
                table: "Banks",
                columns: new[] { "TenantId", "BankCode" },
                unique: true,
                filter: "[BankCode] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Banks_Tenants_TenantId",
                table: "Banks",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConnectIpsPaymentConfiguration_Tenants_TenantId",
                table: "ConnectIpsPaymentConfiguration",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ESewaConfiguration_Tenants_TenantId",
                table: "ESewaConfiguration",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KhaltiConfigurations_Tenants_TenantId",
                table: "KhaltiConfigurations",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentType_Tenants_TenantId",
                table: "PaymentType",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Banks_Tenants_TenantId",
                table: "Banks");

            migrationBuilder.DropForeignKey(
                name: "FK_ConnectIpsPaymentConfiguration_Tenants_TenantId",
                table: "ConnectIpsPaymentConfiguration");

            migrationBuilder.DropForeignKey(
                name: "FK_ESewaConfiguration_Tenants_TenantId",
                table: "ESewaConfiguration");

            migrationBuilder.DropForeignKey(
                name: "FK_KhaltiConfigurations_Tenants_TenantId",
                table: "KhaltiConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentType_Tenants_TenantId",
                table: "PaymentType");

            migrationBuilder.DropIndex(
                name: "IX_PaymentType_TenantId_PaymentTypeName",
                table: "PaymentType");

            migrationBuilder.DropIndex(
                name: "IX_KhaltiConfigurations_TenantId",
                table: "KhaltiConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_ESewaConfiguration_TenantId",
                table: "ESewaConfiguration");

            migrationBuilder.DropIndex(
                name: "IX_ConnectIpsPaymentConfiguration_TenantId",
                table: "ConnectIpsPaymentConfiguration");

            migrationBuilder.DropIndex(
                name: "IX_Banks_TenantId_BankCode",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PaymentType");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "KhaltiConfigurations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ESewaConfiguration");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ConnectIpsPaymentConfiguration");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Banks");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentType_PaymentTypeName",
                table: "PaymentType",
                column: "PaymentTypeName",
                unique: true,
                filter: "[PaymentTypeName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Banks_BankCode",
                table: "Banks",
                column: "BankCode",
                unique: true,
                filter: "[BankCode] IS NOT NULL");
        }
    }
}
