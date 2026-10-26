namespace InventarioActivos.Models;

public class Entrega
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Puesto { get; set; } = "";
    public string Area { get; set; } = "";

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? FotoRuta { get; set; }
    public string? FirmaRuta { get; set; }
    public DateTime? FechaCierre { get; set; }

    public bool Cerrada => FirmaRuta != null;
    public List<EntregaDetalle> Detalles { get; set; } = new();
}

