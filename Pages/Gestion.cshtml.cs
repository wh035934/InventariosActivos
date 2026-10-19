using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioActivos.Pages;

public class GestionModel : PageModel
{
    public List<Producto> Productos { get; set; } = new();

    [BindProperty]
    public Producto Input { get; set; } = new();

    private readonly InventarioContext _context;

    public GestionModel(InventarioContext context)
    {
        _context = context;
    }

    public async Task OnGet(int? editId)
    {
        Productos = await _context.Productos.ToListAsync();

        if (editId.HasValue)
        {
            var found = await _context.Productos.FindAsync(editId.Value);  
            if (found is not null)
            {
                Input = new Producto
                {
                    Id = found.Id,            
                    Nombre = found.Nombre,
                    Precio = found.Precio
                };
            }
        }
    }

    public async Task<IActionResult> OnPostGuardar()
    {
        if (Input.Id == 0)
        {
            _context.Productos.Add(Input);                 
        }
        else
        {
            var found = await _context.Productos.FindAsync(Input.Id);   
            if (found is not null)
            {
                found.Nombre = Input.Nombre;              
                found.Precio = Input.Precio;
            }
        }
        await _context.SaveChangesAsync();                 
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminar(int id)
    {
        var found = await _context.Productos.FindAsync(id);   
        if (found is not null)
        {
            _context.Productos.Remove(found);                 
        }
        await _context.SaveChangesAsync();                 
        return RedirectToPage();
    }
}