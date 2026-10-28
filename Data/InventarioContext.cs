
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
    public DbSet<EntregaDetalle> EntregasDetalles { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<Movimiento> Movimientos { get; set; }
    public DbSet<Sucursal> Sucursales { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Stock> Stock { get; set; }

protected override void OnModelCreating(ModelBuilder mb)
{

    mb.Entity<Producto>(e =>
    {
        e.HasOne(p => p.CategoriaAsignada)
         .WithMany(c => c.Productos)
         .HasForeignKey(p => p.CategoriaId);

        e.HasOne(p => p.Proveedor)
         .WithMany()
         .HasForeignKey(p => p.ProveedorId);
    });

    mb.Entity<Stock>(e =>
    {
    e.HasIndex(s => new { s.ProductoId, s.SucursalId }).IsUnique();

    e.HasOne(s => s.Producto)
     .WithMany(p => p.Stocks)
     .HasForeignKey(s => s.ProductoId)
     .OnDelete(DeleteBehavior.Restrict);

    e.HasOne(s => s.Sucursal)
     .WithMany(su => su.Stocks)
     .HasForeignKey(s => s.SucursalId)
     .OnDelete(DeleteBehavior.Restrict);
    });

    mb.Entity<EntregaDetalle>(e =>
    {
        e.HasIndex(d => new { d.EntregaId, d.ProductoId }).IsUnique();
        e.HasOne(d => d.Producto)
         .WithMany(p => p.Detalles)
         .HasForeignKey(d => d.ProductoId)
         .OnDelete(DeleteBehavior.Restrict);
    });
}
}