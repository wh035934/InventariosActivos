using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioActivos.Pages;

public class MovimientoModel : PageModel
{
    public List<Movimiento> Movimientos { get; set; } = new();    

    private readonly InventarioContext _context;             
    public MovimientoModel(InventarioContext context)         
    {
        _context = context;
    }
    public async Task OnGet()
    {
        Movimientos = await _context.Movimientos
            .OrderByDescending(m => m.Fecha)
            .Take(200)
            .ToListAsync();


    }
}
