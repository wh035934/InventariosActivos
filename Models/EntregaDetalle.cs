namespace InventarioActivos.Models;

public class EntregaDetalle
{
    public int Id { get; set; }

    public int EntregaId { get; set; }
    public Entrega? Entrega { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public int Cantidad { get; set; }
}
