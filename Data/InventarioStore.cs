using InventarioActivos.Models;

namespace InventarioActivos.Data;


public static class InventarioStore
{
    public static List<Producto> Productos { get; } = new();

    public static List<Entrega> Entregas { get; } = new();

    public static List<Usuario> Usuarios { get; } = new();

    public static List<Proveedor> Proveedores { get; } = new();

    private static int _nextProductoId = 1;
    private static int _nextEntregaId = 1;
    private static int _nextProveedorId = 1;

    public static int NextProductoId() => _nextProductoId++;
    public static int NextEntregaId() => _nextEntregaId++;
    public static int NextProveedorId() => _nextProveedorId++;
}
