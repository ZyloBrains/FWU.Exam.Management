using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fwu_examination_management_system.Migrations
{
    /// <inheritdoc />
    public partial class RedesignAddressTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colleges_Areas_AreaId",
                table: "Colleges");

            migrationBuilder.DropForeignKey(
                name: "FK_Colleges_Districts_DistrictId",
                table: "Colleges");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_Districts_DistrictId",
                table: "StudentRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_LocalLevels_LocalLevelId",
                table: "StudentRegistrations");

            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropTable(
                name: "Municipalities");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropIndex(
                name: "IX_Colleges_AreaId",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "MunicipalityVdc",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "WardNumber",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "Remark",
                table: "LocalLevels");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "HouseNumber",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "MunicipalityVdc",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "WardNumber",
                table: "Colleges");

            migrationBuilder.AlterColumn<int>(
                name: "DistrictId",
                table: "StudentRegistrations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "PermanentAddressId",
                table: "StudentRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TemporaryAddressId",
                table: "StudentRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvinceCode",
                table: "Provinces",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "LocalLevels",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocalLevelType",
                table: "LocalLevels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "DistrictId",
                table: "Colleges",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "Colleges",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocalLevelId = table.Column<int>(type: "int", nullable: false),
                    WardNumber = table.Column<int>(type: "int", nullable: true),
                    HouseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ToleStreet = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FullAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddressType = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_LocalLevels_LocalLevelId",
                        column: x => x.LocalLevelId,
                        principalTable: "LocalLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_PermanentAddressId",
                table: "StudentRegistrations",
                column: "PermanentAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_TemporaryAddressId",
                table: "StudentRegistrations",
                column: "TemporaryAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_AddressId",
                table: "Colleges",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_LocalLevelId",
                table: "Addresses",
                column: "LocalLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Colleges_Addresses_AddressId",
                table: "Colleges",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Colleges_Districts_DistrictId",
                table: "Colleges",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_Addresses_PermanentAddressId",
                table: "StudentRegistrations",
                column: "PermanentAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_Addresses_TemporaryAddressId",
                table: "StudentRegistrations",
                column: "TemporaryAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_Districts_DistrictId",
                table: "StudentRegistrations",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_LocalLevels_LocalLevelId",
                table: "StudentRegistrations",
                column: "LocalLevelId",
                principalTable: "LocalLevels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colleges_Addresses_AddressId",
                table: "Colleges");

            migrationBuilder.DropForeignKey(
                name: "FK_Colleges_Districts_DistrictId",
                table: "Colleges");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_Addresses_PermanentAddressId",
                table: "StudentRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_Addresses_TemporaryAddressId",
                table: "StudentRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_Districts_DistrictId",
                table: "StudentRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_LocalLevels_LocalLevelId",
                table: "StudentRegistrations");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_StudentRegistrations_PermanentAddressId",
                table: "StudentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_StudentRegistrations_TemporaryAddressId",
                table: "StudentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_Colleges_AddressId",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "PermanentAddressId",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "TemporaryAddressId",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "ProvinceCode",
                table: "Provinces");

            migrationBuilder.DropColumn(
                name: "LocalLevelType",
                table: "LocalLevels");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Colleges");

            migrationBuilder.AlterColumn<int>(
                name: "DistrictId",
                table: "StudentRegistrations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MunicipalityVdc",
                table: "StudentRegistrations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WardNumber",
                table: "StudentRegistrations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "LocalLevels",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<string>(
                name: "Remark",
                table: "LocalLevels",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DistrictId",
                table: "Colleges",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "Colleges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HouseNumber",
                table: "Colleges",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MunicipalityVdc",
                table: "Colleges",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WardNumber",
                table: "Colleges",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AreaName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Municipalities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MunicipalityName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MunicipalityType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipalities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Municipalities_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RegionCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    RegionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(55)", maxLength: 55, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_AreaId",
                table: "Colleges",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_DistrictId",
                table: "Municipalities",
                column: "DistrictId");

            migrationBuilder.AddForeignKey(
                name: "FK_Colleges_Areas_AreaId",
                table: "Colleges",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Colleges_Districts_DistrictId",
                table: "Colleges",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_Districts_DistrictId",
                table: "StudentRegistrations",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_LocalLevels_LocalLevelId",
                table: "StudentRegistrations",
                column: "LocalLevelId",
                principalTable: "LocalLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
