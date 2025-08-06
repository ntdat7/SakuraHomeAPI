using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SakuraHomeAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingSystemTablesFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ReviewImages");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ReviewImages");

            migrationBuilder.RenameColumn(
                name: "DisplayOrder",
                table: "ReviewImages",
                newName: "SortOrder");

            migrationBuilder.AlterColumn<int>(
                name: "Width",
                table: "ReviewImages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Height",
                table: "ReviewImages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "FileSize",
                table: "ReviewImages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Caption",
                table: "ReviewImages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "AltText",
                table: "ReviewImages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileExtension",
                table: "ReviewImages",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Coupons",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Coupons",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Coupons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Coupons",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Coupons",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShippingOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    TrackingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShippingFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CODFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PickedUpAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SenderPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SenderAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReceiverName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReceiverPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReceiverAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    Dimensions = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsCOD = table.Column<bool>(type: "bit", nullable: false),
                    CODAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingOrders", x => x.Id);
                    table.CheckConstraint("CK_ShippingOrder_CODAmount", "CODAmount >= 0");
                    table.CheckConstraint("CK_ShippingOrder_Fees", "ShippingFee >= 0 AND CODFee >= 0 AND TotalFee >= 0");
                    table.CheckConstraint("CK_ShippingOrder_Weight", "Weight >= 0");
                    table.ForeignKey(
                        name: "FK_ShippingOrders_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShippingTrackings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShippingOrderId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingTrackings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShippingTrackings_ShippingOrders_ShippingOrderId",
                        column: x => x.ShippingOrderId,
                        principalTable: "ShippingOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShippingOrders_CreatedAt",
                table: "ShippingOrders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingOrders_OrderId",
                table: "ShippingOrders",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingOrders_Status",
                table: "ShippingOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingOrders_TrackingNumber",
                table: "ShippingOrders",
                column: "TrackingNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShippingTrackings_ShippingOrderId",
                table: "ShippingTrackings",
                column: "ShippingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingTrackings_Status",
                table: "ShippingTrackings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingTrackings_UpdatedAt",
                table: "ShippingTrackings",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShippingTrackings");

            migrationBuilder.DropTable(
                name: "ShippingOrders");

            migrationBuilder.DropColumn(
                name: "AltText",
                table: "ReviewImages");

            migrationBuilder.DropColumn(
                name: "FileExtension",
                table: "ReviewImages");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Coupons");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                table: "ReviewImages",
                newName: "DisplayOrder");

            migrationBuilder.AlterColumn<int>(
                name: "Width",
                table: "ReviewImages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Height",
                table: "ReviewImages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "FileSize",
                table: "ReviewImages",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Caption",
                table: "ReviewImages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "ReviewImages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "ReviewImages",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Coupons",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
