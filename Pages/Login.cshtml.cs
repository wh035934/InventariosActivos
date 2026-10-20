using System.Security.Claims;
using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventarioActivos.Pages;

public class LoginModel : PageModel
{
    [BindProperty]
    public Credenciales Input { get; set; } = new();

    public string? Error { get; set; }

    // Credenciales ESTÁTICas (temporal) - mismas del proyecto React.
    // Pendiente: validar contra la tabla Usuarios en Supabase.
    private const string CorreoEstatico = "wh035934@gmail.com";
    private const string ContrasenaEstatica = "321contra";

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.Correo == CorreoEstatico && Input.Contrasena == ContrasenaEstatica)
        {
            var usuario = InventarioStore.Usuarios.FirstOrDefault(u => u.Correo == CorreoEstatico);
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
                InventarioStore.Usuarios.Add(usuario);
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

            // Si venía de querer entrar a una página, regresa ahí; si no, a Inicio (como el React)
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
