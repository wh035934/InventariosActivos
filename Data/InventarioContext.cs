
using InventarioActivos.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioActivos.Data;

public class InventarioContext : DbContext
{
    public InventarioContext(DbContextOptions<InventarioContext> options)
        : base(options)
    {
    }

    public DbSet<Producto> Productos { get; set; }
    public DbSet<Entrega> Entregas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Proveedor> Proveedores { get; set; }
}
