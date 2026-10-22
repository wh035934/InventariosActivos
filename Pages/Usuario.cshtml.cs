using System.Security.Claims;
using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioActivos.Pages;

public class UsuarioModel : PageModel
{
    public List<Usuario> Usuarios { get; set; } = new();
    public Usuario? Actual { get; set; }
    public Usuario? Seleccionado { get; set; }

    [BindProperty]
    public Usuario Input { get; set; } = new();

    [BindProperty]

    public Usuario PerfilInput { get; set; } = new();
    private readonly InventarioContext _context;    
    public UsuarioModel(InventarioContext context)
    {
        _context = context;
    }

    public void OnGet(string? ver)
    {
        Usuarios = InventarioStore.Usuarios;
        Actual = ObtenerActual();
        if (Actual is not null)
        {
            PerfilInput = new Usuario
            {
                Id = Actual.Id,
                Nombre = Actual.Nombre,
                Correo = Actual.Correo,
                Area = Actual.Area,
                Rol = Actual.Rol
            };
        }
        if (!string.IsNullOrEmpty(ver))
        {
            Seleccionado = InventarioStore.Usuarios.FirstOrDefault(u => u.Id == ver);
        }
    }

    private Usuario? ObtenerActual()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return null;
        }
        var id = User.FindFirst("UsuarioId")?.Value;
        return string.IsNullOrEmpty(id)
            ? null
            : InventarioStore.Usuarios.FirstOrDefault(u => u.Id == id);
    }

    public IActionResult OnPostCrear()
    {
        Input.Id = Guid.NewGuid().ToString();
        Input.Fecha = DateTime.Now.ToString("dd/MM/yyyy");
        Input.ContrasenaHash = PasswordHasher.Hash(Input.Contrasena);
        Input.Contrasena = "";
        InventarioStore.Usuarios.Add(Input);
        return RedirectToPage();
    }

    public IActionResult OnPostEliminar(string id)
    {
        var found = InventarioStore.Usuarios.FirstOrDefault(u => u.Id == id);
        if (found is not null)
        {
            InventarioStore.Usuarios.Remove(found);
        }
        return RedirectToPage();
    }

    public IActionResult OnPostEditarPerfil()
    {
        var actual = ObtenerActual();
        if (actual is not null)
        {
            actual.Nombre = PerfilInput.Nombre;
            actual.Correo = PerfilInput.Correo;
            actual.Area = PerfilInput.Area;
            actual.Rol = PerfilInput.Rol;
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSalir()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage();
    }
}
