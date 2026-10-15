namespace InventarioActivos.Models;

public class Entrega
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Area { get; set; } = "";
    public string Estado { get; set; } = "activa";
}
