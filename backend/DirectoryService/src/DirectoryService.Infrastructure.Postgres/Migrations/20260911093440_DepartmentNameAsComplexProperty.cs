using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class DepartmentNameAsComplexProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Схема не меняется: Department.Name и через HasConversion, и через ComplexProperty
            //ложится в одну колонку "name". Миграция нужна для обновления снимка модели.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Схема не меняется: Department.Name и через HasConversion, и через ComplexProperty
            //ложится в одну колонку "name". Миграция нужна для обновления снимка модели.
        }
    }
}
