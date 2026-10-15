using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventarioActivos.Pages;

public class GestionModel : PageModel
{
    public List<Producto> Productos { get; set; } = new();

    [BindProperty]
    public Producto Input { get; set; } = new();

    public void OnGet(int? editId)
    {
        Productos = InventarioStore.Productos;
        if (editId.HasValue)
        {
            var found = InventarioStore.Productos.FirstOrDefault(p => p.Id == editId.Value);
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

    public IActionResult OnPostGuardar()
    {
        if (Input.Id == 0)
        {
            Input.Id = InventarioStore.NextProductoId();
            InventarioStore.Productos.Add(Input);
        }
        else
        {
            var found = InventarioStore.Productos.FirstOrDefault(p => p.Id == Input.Id);
            if (found is not null)
            {
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
        }
        return RedirectToPage();
    }

    public IActionResult OnPostEliminar(int id)
    {
        var found = InventarioStore.Productos.FirstOrDefault(p => p.Id == id);
        if (found is not null)
        {
            InventarioStore.Productos.Remove(found);
        }
        return RedirectToPage();
    }
}
