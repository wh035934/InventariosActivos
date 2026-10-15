using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventarioActivos.Pages;

public class IndexModel : PageModel
{
    public List<Producto> Productos { get; set; } = new();

    public void OnGet()
    {
        Productos = InventarioStore.Productos;
    }
}
