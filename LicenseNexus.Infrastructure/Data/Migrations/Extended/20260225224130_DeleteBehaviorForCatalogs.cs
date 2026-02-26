using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicenseNexus.Infrastructure.Data.Migrations.Extended
{
    /// <inheritdoc />
    public partial class DeleteBehaviorForCatalogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Product_group_product_group_id",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Product_type_product_type_id",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Unit_measure_unit_measure_id",
                table: "Product");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Product_group_product_group_id",
                table: "Product",
                column: "product_group_id",
                principalTable: "Product_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Product_type_product_type_id",
                table: "Product",
                column: "product_type_id",
                principalTable: "Product_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Unit_measure_unit_measure_id",
                table: "Product",
                column: "unit_measure_id",
                principalTable: "Unit_measure",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Product_group_product_group_id",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Product_type_product_type_id",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Unit_measure_unit_measure_id",
                table: "Product");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Product_group_product_group_id",
                table: "Product",
                column: "product_group_id",
                principalTable: "Product_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Product_type_product_type_id",
                table: "Product",
                column: "product_type_id",
                principalTable: "Product_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Unit_measure_unit_measure_id",
                table: "Product",
                column: "unit_measure_id",
                principalTable: "Unit_measure",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
