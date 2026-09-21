using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexOnExamRegistrationSymbolNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_SymbolNumber",
                table: "ExamRegistrations",
                column: "SymbolNumber",
                unique: true,
                filter: "[SymbolNumber] IS NOT NULL AND [SymbolNumber] <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExamRegistrations_SymbolNumber",
                table: "ExamRegistrations");
        }
    }
}
