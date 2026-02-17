using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicenseNexus.Infrastructure.Data.Migrations.Extended
{
    /// <inheritdoc />
    public partial class RemovedVendorFKfromCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Category_Vendor_vendor_id",
                table: "Category");

            migrationBuilder.DropIndex(
                name: "IX_Category_vendor_id",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "vendor_id",
                table: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "vendor_id",
                table: "Category",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Category_vendor_id",
                table: "Category",
                column: "vendor_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Category_Vendor_vendor_id",
                table: "Category",
                column: "vendor_id",
                principalTable: "Vendor",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
