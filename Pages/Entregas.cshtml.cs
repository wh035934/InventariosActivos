using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventarioActivos.Pages;

public class EntregasModel : PageModel
{
    public List<Entrega> Entregas { get; set; } = new();

    [BindProperty]
    public Entrega Input { get; set; } = new();

    public void OnGet(int? editId)
    {
        Entregas = InventarioStore.Entregas;
        if (editId.HasValue)
        {
            var found = InventarioStore.Entregas.FirstOrDefault(e => e.Id == editId.Value);
            if (found is not null)
            {
                Input = new Entrega { Id = found.Id, Nombre = found.Nombre, Area = found.Area, Estado = found.Estado };
            }
        }
    }

    public IActionResult OnPostGuardar()
    {
        if (Input.Id == 0)
        {
            Input.Id = InventarioStore.NextEntregaId();
            InventarioStore.Entregas.Add(Input);
        }
        else
        {
            var found = InventarioStore.Entregas.FirstOrDefault(e => e.Id == Input.Id);
            if (found is not null)
            {
                found.Nombre = Input.Nombre;
                found.Area = Input.Area;
                found.Estado = Input.Estado;
            }
        }
        return RedirectToPage();
    }

    public IActionResult OnPostEliminar(int id)
    {
        var found = InventarioStore.Entregas.FirstOrDefault(e => e.Id == id);
        if (found is not null)
        {
            InventarioStore.Entregas.Remove(found);
        }
        return RedirectToPage();
    }
}
