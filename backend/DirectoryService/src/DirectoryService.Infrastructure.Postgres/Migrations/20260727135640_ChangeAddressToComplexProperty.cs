using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAddressToComplexProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "address",
                table: "locations",
                newName: "street");

            migrationBuilder.AddColumn<string>(
                name: "apartment",
                table: "locations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "locations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "house",
                table: "locations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "apartment",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "city",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "house",
                table: "locations");

            migrationBuilder.RenameColumn(
                name: "street",
                table: "locations",
                newName: "address");
        }
    }
}
