using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AudiSoft.School.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedByColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add DeletedBy column to Estudiantes table
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Estudiantes",
                type: "nvarchar(max)",
                nullable: true);

            // Add DeletedBy column to Profesores table
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Profesores",
                type: "nvarchar(max)",
                nullable: true);

            // Add DeletedBy column to Notas table
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Notas",
                type: "nvarchar(max)",
                nullable: true);

            // Add DeletedBy column to Roles table
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: true);

            // Add DeletedBy column to Usuarios table
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);

            // Add DeletedBy column to UsuarioRoles table
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "UsuarioRoles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove DeletedBy column from Estudiantes table
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Estudiantes");

            // Remove DeletedBy column from Profesores table
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Profesores");

            // Remove DeletedBy column from Notas table
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Notas");

            // Remove DeletedBy column from Roles table
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Roles");

            // Remove DeletedBy column from Usuarios table
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Usuarios");

            // Remove DeletedBy column from UsuarioRoles table
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "UsuarioRoles");
        }
    }
}
