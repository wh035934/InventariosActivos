using System.Security.Claims;
using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioActivos.Pages;

public class LoginModel : PageModel
{
    [BindProperty]
    public Credenciales Input { get; set; } = new();

    public string? Error { get; set; }

    private readonly InventarioContext _context;

    public LoginModel(InventarioContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == Input.Correo);

        if (usuario is null && !await _context.Usuarios.AnyAsync())
        {
            usuario = new Usuario
            {
                Id = Guid.NewGuid().ToString(),
                Nombre = Input.Correo.Split('@')[0],
                Correo = Input.Correo,
                Area = "Sistemas",
                Rol = "Administrador",
                Fecha = DateTime.Now.ToString("dd/MM/yyyy"),
                ContrasenaHash = PasswordHasher.Hash(Input.Contrasena)
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
        }

        if (usuario is null || !PasswordHasher.Verificar(Input.Contrasena, usuario.ContrasenaHash))
        {
            Error = "Usuario o contraseña incorrectos";
            return Page();
        }

        var claims = new List<Claim>
        {
            new("UsuarioId", usuario.Id),
            new(ClaimTypes.Name, usuario.Nombre)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        var returnUrl = Request.Query["ReturnUrl"].FirstOrDefault();
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return Redirect("/");
    }
}

public class Credenciales
{
    public string Correo { get; set; } = "";
    public string Contrasena { get; set; } = "";
}
