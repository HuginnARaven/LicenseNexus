using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicenseNexus.Infrastructure.Data.Migrations.Base
{
    /// <inheritdoc />
    public partial class InitialRefactorBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "country_id",
                table: "Partner");

            migrationBuilder.DropColumn(
                name: "country_name",
                table: "Customer");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "country_code",
                table: "Partner");

            migrationBuilder.DropColumn(
                name: "country_code",
                table: "Customer");

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
        }
    }
}
