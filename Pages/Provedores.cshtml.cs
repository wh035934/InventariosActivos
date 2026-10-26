using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioActivos.Pages;

public class ProvedoresModel : PageModel
{
    public List<Proveedor> Proveedores { get; set; } = new();

    [BindProperty]
    public Proveedor Input { get; set; } = new();
    private readonly InventarioContext _context;

    public ProvedoresModel(InventarioContext context)
    {
        _context = context;
    }

    public async Task OnGet(int? editId)
    {
        Proveedores = await _context.Proveedores.ToListAsync();
        if (editId.HasValue)
        {
            var found = await _context.Proveedores.FindAsync(editId.Value);
            if (found is not null)
            {
                Input = new Proveedor
                {
                    Id = found.Id,
                    Nombre = found.Nombre,
                    Ubicacion = found.Ubicacion,
                    Telefono = found.Telefono,
                    Correo = found.Correo,
                };
            }
        }
    }

    public async Task<IActionResult> OnPostGuardar()
    {
    if (Input.Id == 0)
    {
        _context.Proveedores.Add(Input);
        await _context.SaveChangesAsync();

        Bitacora("creado", "Proveedor", Input.Id, $"Proveedor '{Input.Nombre}' registrado");
    }
    else
    {
        var found = await _context.Proveedores.FindAsync(Input.Id);
        if (found is not null)
        {
            var resumen = $"'{found.Nombre}' → '{Input.Nombre}', teléfono {found.Telefono} → {Input.Telefono}";

            found.Nombre = Input.Nombre;
            found.Ubicacion = Input.Ubicacion;
            found.Telefono = Input.Telefono;
            found.Correo = Input.Correo;

            Bitacora("actualizado", "Proveedor", Input.Id, resumen);
        }
    }
    await _context.SaveChangesAsync();
    return RedirectToPage();
    }

   public async Task<IActionResult> OnPostEliminar(int id)
    {
    var found = await _context.Proveedores.FindAsync(id);
    if (found is not null)
    {
        Bitacora("eliminado", "Proveedor", found.Id,
                 $"Proveedor '{found.Nombre}' ({found.Telefono}) eliminado");

        _context.Proveedores.Remove(found);
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
