using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioActivos.Pages;

public class IndexModel : PageModel
{
    public List<Producto> Productos { get; set; } = new();

    private readonly InventarioContext _context;

    public IndexModel(InventarioContext context)
    {
        _context = context;
    }

    public async Task OnGet()
    {
        Productos = await _context.Productos.ToListAsync();
    }
}