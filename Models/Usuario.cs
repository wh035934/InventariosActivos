using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace InventarioActivos.Models;

public class Usuario
{
    
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Area { get; set; } = "";
    public string Rol { get; set; } = "Empleado";
    public string Fecha { get; set; } = "";

    // Lo que la BD guarda: NUNCA la contraseña pura, sino su "impresión digital" (hash)
    public string ContrasenaHash { get; set; } = "";

    // Solo para el formulario: el usuario teclea esto, EF lo ignora (no es columna)
    [NotMapped]
    public string Contrasena { get; set; } = "";
}
