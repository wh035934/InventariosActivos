namespace InventarioActivos.Models;

public class Proveedor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Ubicacion { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Correo { get; set; } = "";
}
