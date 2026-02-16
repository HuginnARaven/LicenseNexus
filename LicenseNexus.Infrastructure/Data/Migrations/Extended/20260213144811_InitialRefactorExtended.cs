using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicenseNexus.Infrastructure.Data.Migrations.Extended
{
    /// <inheritdoc />
    public partial class InitialRefactorExtended : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "country_id",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "country_id",
                table: "Product_price");

            migrationBuilder.DropColumn(
                name: "country_id",
                table: "Partner");

            migrationBuilder.DropColumn(
                name: "country_name",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "name_country",
                table: "Currency");

            migrationBuilder.AddColumn<string>(
                name: "country_code",
                table: "Vendor",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "country_code",
                table: "Product_price",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "country_code",
                table: "Partner",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "country_code",
                table: "Customer",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "country_code",
                table: "Currency",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "country_code",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "country_code",
                table: "Product_price");

            migrationBuilder.DropColumn(
                name: "country_code",
                table: "Partner");

            migrationBuilder.DropColumn(
                name: "country_code",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "country_code",
                table: "Currency");

            migrationBuilder.AddColumn<int>(
                name: "country_id",
                table: "Vendor",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "country_id",
                table: "Product_price",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "country_id",
                table: "Partner",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "country_name",
                table: "Customer",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name_country",
                table: "Currency",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
