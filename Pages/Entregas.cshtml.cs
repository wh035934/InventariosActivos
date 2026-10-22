using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioActivos.Pages;

public class EntregasModel : PageModel
{
    public List<Entrega> Entregas { get; set; } = new();

    [BindProperty]
    public Entrega Input { get; set; } = new();
    private readonly InventarioContext _context;
    public EntregasModel(InventarioContext context)
    {
        _context = context;
    }

    public async Task OnGet(int? editId)
    {
        Entregas = await _context.Entregas.ToListAsync();
        if (editId.HasValue)
        {
            var found = await _context.Entregas.FindAsync(editId.Value);
            if (found is not null)
            {
                Input = new Entrega 
                {
                    Id = found.Id, 
                    Nombre = found.Nombre, 
                    Puesto = found.Puesto, 
                    Area = found.Area, 
                    Estado = found.Estado
                    
                };
            }
        }
    }

    public async Task<IActionResult> OnPostGuardar()
    {
        if (Input.Id == 0)
        {
            _context.Entregas.Add(Input);
        }
        else
        {
            var found = await _context.Entregas.FindAsync(Input.Id);
            if (found is not null)
            {
                found.Nombre = Input.Nombre;
                found.Puesto = Input.Puesto;
                found.Area = Input.Area;
                found.Estado = Input.Estado;
            }
        }
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminar(int id)
    {
        if (!User.IsInRole("Administrador"))
    {
        return Forbid();
    }
        var found = await _context.Entregas.FindAsync(id);
        if (found is not null)
        {
            _context.Entregas.Remove(found);
        }
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }
}
