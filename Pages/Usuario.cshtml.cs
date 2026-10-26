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


    public async Task OnGet(string? ver)
    {

        Usuarios = await _context.Usuarios.ToListAsync();

        Actual = await ObtenerActualAsync();

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
            Seleccionado = await _context.Usuarios.FindAsync(ver);
        }
    }

    private async Task<Usuario?> ObtenerActualAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return null;
        }
        var id = User.FindFirst("UsuarioId")?.Value;
        return string.IsNullOrEmpty(id)
            ? null
            : await _context.Usuarios.FindAsync(id);
    }

    public async Task<IActionResult> OnPostCrear()
    {
        
        var correoExiste = await _context.Usuarios
            .AnyAsync(u => u.Correo == Input.Correo);
        if (correoExiste)
        {

            ModelState.AddModelError("Input.Correo", "Ese correo ya está registrado");
            await OnGet(null);          
            return Page();              
        }


        if (string.IsNullOrWhiteSpace(Input.Contrasena))
        {
            ModelState.AddModelError("Input.Contrasena", "La contraseña es obligatoria");
            await OnGet(null);
            return Page();
        }

        Input.Id = Guid.NewGuid().ToString();
        Input.Fecha = DateTime.Now.ToString("dd/MM/yyyy");
        Input.ContrasenaHash = PasswordHasher.Hash(Input.Contrasena);
        Input.Contrasena = "";                      

        await _context.Usuarios.AddAsync(Input);      
        await _context.SaveChangesAsync();            
        return RedirectToPage();                     
    }


    public async Task<IActionResult> OnPostEliminar(string id)
    {

        var found = await _context.Usuarios.FindAsync(id);
        if (found is not null)
        {

            if (found.Id == User.FindFirst("UsuarioId")?.Value)
            {
                TempData["Error"] = "No puedes eliminar tu propia cuenta";
                return RedirectToPage();
            }
            _context.Usuarios.Remove(found);           
            await _context.SaveChangesAsync();         
        }
        return RedirectToPage();
    }


    public async Task<IActionResult> OnPostEditarPerfil()
    {
        var actual = await ObtenerActualAsync();      
        if (actual is null)
        {
            return RedirectToPage();
        }

        actual.Nombre = PerfilInput.Nombre;            
        actual.Correo = PerfilInput.Correo;
        actual.Area = PerfilInput.Area;


        await _context.SaveChangesAsync();             
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSalir()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage();
    }
}