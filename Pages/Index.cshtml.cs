using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioActivos.Pages;

public class IndexModel : PageModel
{
    public List<Producto> Productos { get; set; } = new();
    public List<string> Ubicaciones { get; set; } = new();
    public List<string> Categorias { get; set; } = new();
    public List<Producto> ProductosReporte { get; set; } = new();
    public List<ResumenUbicacion> ResumenPorUbicacion { get; set; } = new();
    public string? UbicacionSeleccionada { get; private set; }
    public string? CategoriaSeleccionada { get; private set; }
    public int TotalProductos => ProductosReporte.Count;
    public int TotalUnidades => ProductosReporte.Sum(p => p.Cantidad);
    public decimal ValorTotal => ProductosReporte.Sum(p => p.Cantidad * p.Precio);

    private readonly InventarioContext _context;
    private readonly EntregaPdfGenerator _pdfGenerator;

    public IndexModel(InventarioContext context, EntregaPdfGenerator pdfGenerator)
    {
        _context = context;
        _pdfGenerator = pdfGenerator;
    }

    public async Task OnGet(string? ubicacion, string? categoria)
    {
        await CargarDatosAsync(ubicacion, categoria);
    }

    public async Task<IActionResult> OnGetPdf(string? ubicacion, string? categoria)
    {
        await CargarDatosAsync(ubicacion, categoria);
        var pdf = _pdfGenerator.GenerateInventario(
            ProductosReporte,
            UbicacionSeleccionada,
            CategoriaSeleccionada);

        return File(pdf, "application/pdf", $"Reporte-Inventario-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    private async Task CargarDatosAsync(string? ubicacion, string? categoria)
    {
        Productos = await _context.Productos
            .AsNoTracking()
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        Ubicaciones = Productos
            .Select(p => p.Ubicacion)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct()
            .OrderBy(value => value)
            .ToList();
        Categorias = Productos
            .Select(p => p.Categoria)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct()
            .OrderBy(value => value)
            .ToList();

        UbicacionSeleccionada = Ubicaciones.Contains(ubicacion ?? "", StringComparer.Ordinal)
            ? ubicacion
            : null;
        CategoriaSeleccionada = Categorias.Contains(categoria ?? "", StringComparer.Ordinal)
            ? categoria
            : null;

        ProductosReporte = Productos
            .Where(p => UbicacionSeleccionada is null ||
                        p.Ubicacion == UbicacionSeleccionada)
            .Where(p => CategoriaSeleccionada is null ||
                        p.Categoria == CategoriaSeleccionada)
            .ToList();

        ResumenPorUbicacion = ProductosReporte
            .GroupBy(p => p.Ubicacion)
            .Select(grupo => new ResumenUbicacion(
                string.IsNullOrWhiteSpace(grupo.Key) ? "Sin ubicación" : grupo.Key,
                grupo.Count(),
                grupo.Sum(p => p.Cantidad),
                grupo.Sum(p => p.Cantidad * p.Precio)))
            .OrderBy(resumen => resumen.Ubicacion)
            .ToList();
    }

    public sealed record ResumenUbicacion(
        string Ubicacion,
        int Productos,
        int Unidades,
        decimal Valor);
}