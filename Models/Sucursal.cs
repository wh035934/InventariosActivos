namespace InventarioActivos.Models;

public class Sucursal
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string Telefono { get; set; } = "";
    public bool Activa { get; set; } = true;

    public List<Stock> Stocks { get; set; } = new();
    public List<Entrega> Entregas { get; set; } = new();
}