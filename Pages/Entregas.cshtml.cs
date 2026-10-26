using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventarioActivos.Pages;

public class EntregasModel : PageModel
{
    public List<Entrega> Entregas { get; set; } = new();


    public List<Producto> Productos { get; set; } = new();

    [BindProperty]
    public Entrega Input { get; set; } = new();

    [BindProperty]
    public IFormFile? Foto { get; set; }

    [BindProperty]
    public IFormFile? Firma { get; set; }

    
    [BindProperty]
    public List<EntregaDetalle> Detalles { get; set; } = new();

    private const int FilasFormulario = 8; 

    private readonly InventarioContext _context;
    public EntregasModel(InventarioContext context)
    {
        _context = context;
    }

    private static readonly string[] Extensiones = { ".jpg", ".jpeg", ".png", ".webp" };

    private static (string? ruta, string? error) GuardarArchivo(IFormFile? archivo)
    {
        if (archivo is null || archivo.Length == 0) return (null, null);

        if (archivo.Length > 20 * 1024 * 1024)
            return (null, "La imagen supera el máximo de 20 MB.");

        var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!Extensiones.Contains(ext))
            return (null, $"Formato no permitido: {ext}. Usa jpg, jpeg, png o webp.");

        var nombre = $"{Guid.NewGuid()}{ext}";
        var destino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", nombre);

        using var stream = new FileStream(destino, FileMode.Create);
        archivo.CopyTo(stream);

        return ($"/uploads/{nombre}", null);
    }

    public async Task OnGet(int? editId)
    {
        Entregas = await _context.Entregas.ToListAsync();
        Productos = await _context.Productos.OrderBy(p => p.Nombre).ToListAsync();

        Detalles = Enumerable.Range(0, FilasFormulario)
                             .Select(_ => new EntregaDetalle()).ToList();

        if (editId.HasValue)
        {
            
            var found = await _context.Entregas
                .Include(e => e.Detalles).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(e => e.Id == editId.Value);

            if (found is not null)
            {
                Input = new Entrega
                {
                    Id = found.Id,
                    Nombre = found.Nombre,
                    Puesto = found.Puesto,
                    Area = found.Area,
                    Fecha = found.Fecha,
                    FotoRuta = found.FotoRuta,
                    FirmaRuta = found.FirmaRuta,
                    FechaCierre = found.FechaCierre,
                    Detalles = found.Detalles   
                };
            }
        }
    }

    
    private async Task<IActionResult> ErrorPost(string mensaje)
    {
        TempData["Error"] = mensaje;
        Entregas = await _context.Entregas.ToListAsync();
        Productos = await _context.Productos.OrderBy(p => p.Nombre).ToListAsync();

        if (Input.Id != 0)
        {
            var ant = await _context.Entregas.AsNoTracking()
                      .FirstOrDefaultAsync(e => e.Id == Input.Id);
            if (ant is not null)
            {
                Input.Fecha = ant.Fecha;
                Input.FotoRuta = ant.FotoRuta;
                Input.FirmaRuta = ant.FirmaRuta;
                Input.FechaCierre = ant.FechaCierre;
            }

            Input.Detalles = await _context.EntregasDetalles
                .Include(d => d.Producto)
                .Where(d => d.EntregaId == Input.Id)
                .ToListAsync();
        }
        return Page();
    }

    public async Task<IActionResult> OnPostGuardar()
    {
        var (rutaFoto, errFoto) = GuardarArchivo(Foto);
        var (rutaFirma, errFirma) = GuardarArchivo(Firma);
        var errorArchivo = errFoto ?? errFirma;
        if (errorArchivo is not null)
            return await ErrorPost(errorArchivo); 

        if (Input.Id == 0)
        {

            var renglones = Detalles
                .Where(d => d.ProductoId > 0 && d.Cantidad > 0)
                .ToList();

            if (renglones.Count == 0)
                return await ErrorPost("Agrega al menos un producto con cantidad mayor a cero.");


            var ids = renglones.Select(d => d.ProductoId).Distinct().ToList();
            var productos = await _context.Productos
                .Where(p => ids.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            if (productos.Count != ids.Count)
                return await ErrorPost("Uno de los productos ya no existe. Recarga la página.");

            
            var requerido = renglones
                .GroupBy(d => d.ProductoId)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Cantidad));


            foreach (var (id, cant) in requerido)
            {
                var p = productos[id];
                if (p.Cantidad < cant)
                    return await ErrorPost(
                        $"Stock insuficiente para '{p.Nombre}': pides {cant}, hay {p.Cantidad} disponibles.");
            }


            Input.Fecha = DateTime.UtcNow;
            Input.FotoRuta = rutaFoto;
            Input.FirmaRuta = rutaFirma;
            if (rutaFirma != null) Input.FechaCierre = DateTime.UtcNow;

            foreach (var (id, cant) in requerido)
                productos[id].Cantidad -= cant;          

            foreach (var d in renglones)
                Input.Detalles.Add(new EntregaDetalle    
                {
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad
                });

            _context.Entregas.Add(Input);


            await _context.SaveChangesAsync();

            Bitacora("creado", "Entrega", Input.Id,
                     $"Entrega a '{Input.Nombre}' con {renglones.Count} renglón(es)");
            await _context.SaveChangesAsync();
        }
        else
        {
            
            var found = await _context.Entregas.FindAsync(Input.Id);
            if (found is not null)
            {
                var resumen = $"'{found.Nombre}' → '{Input.Nombre}'"
                            + (rutaFoto != null ? " + foto" : "")
                            + (rutaFirma != null ? " + firma" : "");

                found.Nombre = Input.Nombre;
                found.Puesto = Input.Puesto;
                found.Area = Input.Area;

                if (rutaFoto != null) found.FotoRuta = rutaFoto;
                if (rutaFirma != null)
                {
                    found.FirmaRuta = rutaFirma;
                    found.FechaCierre = DateTime.UtcNow;
                }

                Bitacora("actualizado", "Entrega", Input.Id, resumen);
            }
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminar(int id)
    {
        var found = await _context.Entregas
            .Include(e => e.Detalles)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (found is not null)
        {

            if (found.Detalles.Count > 0)
            {
                var ids = found.Detalles.Select(d => d.ProductoId).Distinct().ToList();
                var productos = await _context.Productos
                    .Where(p => ids.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                foreach (var grupo in found.Detalles.GroupBy(d => d.ProductoId))
                {
                    if (productos.TryGetValue(grupo.Key, out var p))
                        p.Cantidad += grupo.Sum(d => d.Cantidad);
                }
            }

            Bitacora("eliminado", "Entrega", found.Id,
                     $"Entrega a '{found.Nombre}' ({found.Puesto}, {found.Area}) eliminada");

            _context.Entregas.Remove(found); 
        }

        await _context.SaveChangesAsync(); 
        return RedirectToPage();
    }

    private void Bitacora(string accion, string entidad, int id, string detalle)
    {
        _context.Movimientos.Add(new Movimiento
        {
            Accion = accion,
            Entidad = entidad,
            EntidadId = id,
            Detalle = detalle
        });
    }
}