using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventarioActivos.Pages;

public class UsuarioModel : PageModel
{
    public List<Usuario> Usuarios { get; set; } = new();
    public Usuario? Actual { get; set; }
    public Usuario? Seleccionado { get; set; }

    [BindProperty]
    public Usuario Input { get; set; } = new();

    public void OnGet(string? ver)
    {
        Usuarios = InventarioStore.Usuarios;
        Actual = InventarioStore.Usuarios.FirstOrDefault();
        if (!string.IsNullOrEmpty(ver))
        {
            Seleccionado = InventarioStore.Usuarios.FirstOrDefault(u => u.Key == ver);
        }
    }

    public IActionResult OnPostCrear()
    {
        Input.Key = Guid.NewGuid().ToString();
        Input.Fecha = DateTime.Now.ToString("dd/MM/yyyy");
        InventarioStore.Usuarios.Add(Input);
        return RedirectToPage();
    }

    public IActionResult OnPostEliminar(string key)
    {
        var found = InventarioStore.Usuarios.FirstOrDefault(u => u.Key == key);
        if (found is not null)
        {
            InventarioStore.Usuarios.Remove(found);
        }
        return RedirectToPage();
    }
}
