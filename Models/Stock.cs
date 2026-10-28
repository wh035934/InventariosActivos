namespace InventarioActivos.Models;

public class Stock
{
    public int Id { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }

    public int Cantidad { get; set; }
    public string Estado { get; set; } = "disponible";
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}