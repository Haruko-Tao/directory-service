using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class LtreePathAndDepth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:ltree", ",,");

            migrationBuilder.Sql("UPDATE departments SET path = replace(path, '/', '.');");

            migrationBuilder.Sql("ALTER TABLE departments ALTER COLUMN path TYPE ltree USING path::ltree;");

            migrationBuilder.AddColumn<int>(
                name: "depth",
                table: "departments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.Sql("UPDATE departments SET depth = nlevel(path) - 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "depth",
                table: "departments");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:ltree", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "path",
                table: "departments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "ltree");
        }
    }
}
