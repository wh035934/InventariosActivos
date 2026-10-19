using System.ComponentModel.DataAnnotations;
namespace InventarioActivos.Models;

public class Usuario
{
    
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Area { get; set; } = "";
    public string Rol { get; set; } = "Empleado";
    public string Fecha { get; set; } = "";
}
