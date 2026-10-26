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
                    Modelo = found.Modelo,
                    Categoria = found.Categoria,
                    Ubicacion = found.Ubicacion,
                    Cantidad = found.Cantidad,
                    Estado = found.Estado,
                    FechaRegistro = found.FechaRegistro,
                    Precio = found.Precio,
                    Descripcion = found.Descripcion
                };
            }
        }
    }

    public async Task<IActionResult> OnPostGuardar()
    {
        if (Input.Id == 0)
        {
            _context.Productos.Add(Input);         
            await _context.SaveChangesAsync();  
            Bitacora("creado", "Producto", Input.Id, $"Producto '{Input.Nombre}' registrado");  
        }
        else
        {
            var found = await _context.Productos.FindAsync(Input.Id);   
            if (found is not null)
            {
                found.Id = Input.Id;
                found.Nombre = Input.Nombre;
                found.Modelo = Input.Modelo;
                found.Categoria = Input.Categoria;
                found.Ubicacion = Input.Ubicacion;
                found.Cantidad = Input.Cantidad;
                found.Estado = Input.Estado;
                found.FechaRegistro = Input.FechaRegistro;             
                found.Precio = Input.Precio;
                found.Descripcion = Input.Descripcion;
            }
            Bitacora("actualizado", "Producto", Input.Id, $"Producto '{Input.Nombre}' actualizado");
        }
        await _context.SaveChangesAsync();                 
        return RedirectToPage();
        
    }

    public async Task<IActionResult> OnPostEliminar(int id)
    {
        var found = await _context.Productos.FindAsync(id);   
        if (found is not null)
        {
            Bitacora("eliminado", "Producto", found.Id, $"Producto '{found.Nombre}' eliminado");
            _context.Productos.Remove(found);                  
        }
        await _context.SaveChangesAsync();                
        return RedirectToPage();
    }
    private void Bitacora(string accion, string entidad, int id, string detalle)
    {
    _context.Movimientos.Add(new Movimiento
    {
        Accion = accion,
        Entidad = entidad,
        EntidadId = id,
        Detalle = detalle
    });
    }
}