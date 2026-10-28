namespace InventarioActivos.Models;

public class Producto
{
    
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Modelo { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string Ubicacion { get; set; } = "";
    public int Cantidad { get; set; }
    public string Estado { get; set; } = "disponible";
    public string FechaRegistro { get; set; } = "";
    public decimal Precio { get; set; }
    public string Descripcion { get; set; } = "";

    
    public int? CategoriaId { get; set; }
    public Categoria? CategoriaAsignada { get; set; }

    public int? ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }

    public List<EntregaDetalle> Detalles { get; set; } = new();
    public List<Stock> Stocks { get; set; } = new();
}
