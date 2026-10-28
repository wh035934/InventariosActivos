using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarioActivos.Migrations
{
    /// <inheritdoc />
    public partial class Fase2_SembrarYMudarDatos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
        table: "Sucursales",
        columns: new[] { "Nombre", "Direccion", "Telefono", "Activa" },
        values: new object[] { "Sucursal Central", "", "", true });

    // 2) Copiar cada CATEGORÍA DISTINTA de los productos viejos
    migrationBuilder.Sql("""
        INSERT INTO "Categorias" ("Nombre")
        SELECT DISTINCT "Categoria" FROM "Productos"
        WHERE "Categoria" <> ''
          AND "Categoria" NOT IN (SELECT "Nombre" FROM "Categorias");
        """);

    // 3) Apuntar cada producto a su categoría nueva
    migrationBuilder.Sql("""
        UPDATE "Productos" p
        SET "CategoriaId" = c."Id"
        FROM "Categorias" c
        WHERE c."Nombre" = p."Categoria";
        """);

    // 4) MUDAR las cantidades a la bodega de Sucursal Central
    migrationBuilder.Sql("""
        INSERT INTO "Stock" ("ProductoId", "SucursalId", "Cantidad", "Estado", "FechaRegistro")
        SELECT p."Id",
               (SELECT s."Id" FROM "Sucursales" s WHERE s."Nombre" = 'Sucursal Central'),
               p."Cantidad",
               p."Estado",
               CASE WHEN p."FechaRegistro" ~ '^\d{4}-\d{2}-\d{2}'
                    THEN p."FechaRegistro"::timestamp
                    ELSE now() END
        FROM "Productos" p;
        """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"Stock\";");
            migrationBuilder.Sql("UPDATE \"Productos\" SET \"CategoriaId\" = NULL;");
            migrationBuilder.Sql("DELETE FROM \"Categorias\";");
            migrationBuilder.Sql("DELETE FROM \"Sucursales\" WHERE \"Nombre\" = 'Sucursal Central';");
        }
    }
}
