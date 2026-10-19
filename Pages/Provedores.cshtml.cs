using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventarioActivos.Pages;

public class ProvedoresModel : PageModel
{
    public List<Proveedor> Proveedores { get; set; } = new();

    [BindProperty]
    public Proveedor Input { get; set; } = new();

    public void OnGet(int? editId)
    {
        Proveedores = InventarioStore.Proveedores;
        if (editId.HasValue)
        {
            var found = InventarioStore.Proveedores.FirstOrDefault(p => p.Id == editId.Value);
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

    public IActionResult OnPostGuardar()
    {
        if (Input.Id == 0)
        {
            Input.Id = InventarioStore.NextProveedorId();
            InventarioStore.Proveedores.Add(Input);
        }
        else
        {
            var found = InventarioStore.Proveedores.FirstOrDefault(p => p.Id == Input.Id);
            if (found is not null)
            {
                found.Nombre = Input.Nombre;
                found.Ubicacion = Input.Ubicacion;
                found.Telefono = Input.Telefono;
                found.Correo = Input.Correo;
            }
        }
        return RedirectToPage();
    }

    public IActionResult OnPostEliminar(int id)
    {
        var found = InventarioStore.Proveedores.FirstOrDefault(p => p.Id == id);
        if (found is not null)
        {
            InventarioStore.Proveedores.Remove(found);
        }
        return RedirectToPage();
    }
}
