using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_departments_path",
                table: "departments");

            migrationBuilder.DropIndex(
                name: "IX_departments_slug",
                table: "departments");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "positions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "positions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "locations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "locations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "departments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "departments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_positions_deleted_at",
                table: "positions",
                column: "deleted_at",
                filter: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_positions_name",
                table: "positions",
                column: "name",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "IX_locations_deleted_at",
                table: "locations",
                column: "deleted_at",
                filter: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_departments_deleted_at",
                table: "departments",
                column: "deleted_at",
                filter: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_departments_path",
                table: "departments",
                column: "path",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "IX_departments_slug",
                table: "departments",
                column: "slug",
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_positions_deleted_at",
                table: "positions");

            migrationBuilder.DropIndex(
                name: "IX_positions_name",
                table: "positions");

            migrationBuilder.DropIndex(
                name: "IX_locations_deleted_at",
                table: "locations");

            migrationBuilder.DropIndex(
                name: "IX_departments_deleted_at",
                table: "departments");

            migrationBuilder.DropIndex(
                name: "IX_departments_path",
                table: "departments");

            migrationBuilder.DropIndex(
                name: "IX_departments_slug",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "positions");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "positions");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "departments");

            migrationBuilder.CreateIndex(
                name: "IX_departments_path",
                table: "departments",
                column: "path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_departments_slug",
                table: "departments",
                column: "slug",
                unique: true);
        }
    }
}
