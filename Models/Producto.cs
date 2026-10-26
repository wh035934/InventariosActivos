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

    public List<EntregaDetalle> Detalles { get; set; } = new();
}
