using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SakuraHomeAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddVietnamAddressLookupTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VietnamProvinces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VietnamProvinces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VietnamWards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProvinceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VietnamWards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VietnamWards_VietnamProvinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "VietnamProvinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VietnamProvinces_DisplayOrder",
                table: "VietnamProvinces",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_VietnamProvinces_Name",
                table: "VietnamProvinces",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_VietnamWards_Name",
                table: "VietnamWards",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_VietnamWards_ProvinceId",
                table: "VietnamWards",
                column: "ProvinceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VietnamWards");

            migrationBuilder.DropTable(
                name: "VietnamProvinces");
        }
    }
}
