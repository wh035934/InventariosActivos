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

    private const string CorreoEstatico = "wh035934@gmail.com";
    private const string ContrasenaEstatica = "321contra";


    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.Correo == CorreoEstatico && Input.Contrasena == ContrasenaEstatica)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == CorreoEstatico);
            if (usuario is null)
            {
                usuario = new Usuario
                {
                    Id = Guid.NewGuid().ToString(),
                    Nombre = "Juan Pérez",
                    Correo = CorreoEstatico,
                    Area = "Sistemas",
                    Rol = "Administrador",
                    Fecha = DateTime.Now.ToString("dd/MM/yyyy")
                };
                await _context.Usuarios.AddAsync(usuario);
                InventarioStore.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
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

        Error = "Usuario o contraseña incorrectos";
        return Page();
    }
}

public class Credenciales
{
    public string Correo { get; set; } = "";
    public string Contrasena { get; set; } = "";
}
