using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioActivos.Pages;

public class HistorialModel : PageModel
{
    public List<Producto> Productos { get; set; } = new();
    public List<Entrega> Entregas { get; set; } = new();

    private readonly InventarioContext _context;             
    public HistorialModel(InventarioContext context)         
    {
        _context = context;
    }
    public async Task OnGet()
    {
        Productos = await _context.Productos.ToListAsync();
        Entregas = await _context.Entregas.ToListAsync();
    }
}
