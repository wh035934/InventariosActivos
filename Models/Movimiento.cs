namespace InventarioActivos.Models;

public class Movimiento
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Accion { get; set; } = "";    
    public string Entidad { get; set; } = "";       
    public int EntidadId { get; set; }          
    public string Detalle { get; set; } = "";
}