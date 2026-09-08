using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKhaltiPaymentTrackingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InitiatedAt",
                table: "PaymentRequestLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "PaymentRequestLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentProvider",
                table: "PaymentRequestLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "PaymentRequestLogs",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderReferenceId",
                table: "PaymentRequestLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderTransactionId",
                table: "PaymentRequestLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "PaymentRequestLogs",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitiatedAt",
                table: "PaymentRequestLogs");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "PaymentRequestLogs");

            migrationBuilder.DropColumn(
                name: "PaymentProvider",
                table: "PaymentRequestLogs");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "PaymentRequestLogs");

            migrationBuilder.DropColumn(
                name: "ProviderReferenceId",
                table: "PaymentRequestLogs");

            migrationBuilder.DropColumn(
                name: "ProviderTransactionId",
                table: "PaymentRequestLogs");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "PaymentRequestLogs");
        }
    }
}
