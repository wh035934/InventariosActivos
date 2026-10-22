using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarioActivos.Migrations
{
    /// <inheritdoc />
    public partial class AgregarContrasenaUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContrasenaHash",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContrasenaHash",
                table: "Usuarios");
        }
    }
}
