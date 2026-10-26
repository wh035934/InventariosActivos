using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarioActivos.Migrations
{
    /// <inheritdoc />
    public partial class EntregaFotoFirma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Entregas");

            migrationBuilder.AddColumn<DateTime>(
                name: "Fecha",
                table: "Entregas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCierre",
                table: "Entregas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirmaRuta",
                table: "Entregas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoRuta",
                table: "Entregas",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fecha",
                table: "Entregas");

            migrationBuilder.DropColumn(
                name: "FechaCierre",
                table: "Entregas");

            migrationBuilder.DropColumn(
                name: "FirmaRuta",
                table: "Entregas");

            migrationBuilder.DropColumn(
                name: "FotoRuta",
                table: "Entregas");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Entregas",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
