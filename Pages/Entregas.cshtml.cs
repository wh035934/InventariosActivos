using InventarioActivos.Data;
using InventarioActivos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace InventarioActivos.Pages;

public class EntregasModel : PageModel
{
    public List<Entrega> Entregas { get; set; } = new();

    public Dictionary<int, string?> FotosUrl { get; set; } = new();
    public Dictionary<int, string?> FirmasUrl { get; set; } = new();
    public string? FotoEdicionUrl { get; set; }
    public string? FirmaEdicionUrl { get; set; }

    public List<Producto> Productos { get; set; } = new();

    [BindProperty]
    public Entrega Input { get; set; } = new();

    [BindProperty]
    public IFormFile? Foto { get; set; }

    [BindProperty]
    public IFormFile? Firma { get; set; }

    
    [BindProperty]
    public List<EntregaDetalle> Detalles { get; set; } = new();

    private const int FilasFormulario = 1; 

    private readonly InventarioContext _context;
    private readonly SupabaseStorageService _storage;
    private readonly EntregaPdfGenerator _pdfGenerator;
    private readonly IWebHostEnvironment _environment;

    public EntregasModel(
        InventarioContext context,
        SupabaseStorageService storage,
        EntregaPdfGenerator pdfGenerator,
        IWebHostEnvironment environment)
    {
        _context = context;
        _storage = storage;
        _pdfGenerator = pdfGenerator;
        _environment = environment;
    }

    private static readonly string[] Extensiones = { ".jpg", ".jpeg", ".png", ".webp" };

    private static string? ValidarArchivo(IFormFile? archivo)
    {
        if (archivo is null || archivo.Length == 0) return null;

        if (archivo.Length > 20 * 1024 * 1024)
            return "La imagen supera el máximo de 20 MB.";

        var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!Extensiones.Contains(ext))
            return $"Formato no permitido: {ext}. Usa jpg, jpeg, png o webp.";

        using var stream = archivo.OpenReadStream();
        using var bitmap = SKBitmap.Decode(stream);
        if (bitmap is null)
            return "El archivo seleccionado no es una imagen válida.";
        if ((long)bitmap.Width * bitmap.Height > 25_000_000)
            return "La imagen tiene demasiada resolución para procesarse.";

        return null;
    }

    private async Task<string?> GuardarArchivoAsync(IFormFile? archivo, string tipo)
    {
        if (archivo is null || archivo.Length == 0)
            return null;

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var ruta = $"evidencias/{Guid.NewGuid():N}/{tipo}{extension}";
        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => throw new InvalidOperationException("El formato de imagen no está permitido.")
        };

        await using var stream = archivo.OpenReadStream();
        await _storage.UploadAsync(ruta, stream, contentType);
        return ruta;
    }

    private async Task<string?> ObtenerUrlAsync(string? ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
            return null;

        if (ruta.StartsWith("/uploads/", StringComparison.Ordinal))
            return ruta;

        return await _storage.CreateSignedUrlAsync(ruta);
    }

    private async Task<byte[]?> LeerArchivoAsync(string? ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
            return null;

        if (ruta.StartsWith("/uploads/", StringComparison.Ordinal))
        {
            var nombre = Path.GetFileName(ruta);
            var archivoLocal = Path.Combine(_environment.WebRootPath, "uploads", nombre);
            if (!System.IO.File.Exists(archivoLocal))
                throw new FileNotFoundException("No se encontró una imagen de evidencia existente.");

            return await System.IO.File.ReadAllBytesAsync(archivoLocal);
        }

        return await _storage.DownloadAsync(ruta);
    }

    private static byte[]? ConvertirImagenParaPdf(byte[]? bytes)
    {
        if (bytes is null)
            return null;

        using var image = SKBitmap.Decode(bytes)
            ?? throw new InvalidOperationException("No se pudo leer una imagen de evidencia.");
        using var skImage = SKImage.FromBitmap(image);
        using var jpeg = skImage.Encode(SKEncodedImageFormat.Jpeg, 90)
            ?? throw new InvalidOperationException("No se pudo preparar una imagen para el PDF.");
        return jpeg.ToArray();
    }

    public async Task OnGet(int? editId)
    {
        Entregas = await _context.Entregas.ToListAsync();
        Productos = await _context.Productos.OrderBy(p => p.Nombre).ToListAsync();
        await CargarUrlsListaAsync();

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
                await CargarUrlsEdicionAsync();
            }
        }
    }

    private async Task CargarUrlsListaAsync()
    {
        FotosUrl = new Dictionary<int, string?>();
        FirmasUrl = new Dictionary<int, string?>();

        foreach (var entrega in Entregas)
        {
            FotosUrl[entrega.Id] = await ObtenerUrlAsync(entrega.FotoRuta);
            FirmasUrl[entrega.Id] = await ObtenerUrlAsync(entrega.FirmaRuta);
        }
    }

    private async Task CargarUrlsEdicionAsync()
    {
        FotoEdicionUrl = await ObtenerUrlAsync(Input.FotoRuta);
        FirmaEdicionUrl = await ObtenerUrlAsync(Input.FirmaRuta);
    }

    private async Task<IActionResult> ErrorPost(string mensaje)
    {
        TempData["Error"] = mensaje;
        Entregas = await _context.Entregas.ToListAsync();
        Productos = await _context.Productos.OrderBy(p => p.Nombre).ToListAsync();
        await CargarUrlsListaAsync();

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

            await CargarUrlsEdicionAsync();
        }
        return Page();
    }

    public async Task<IActionResult> OnPostGuardar()
    {
        var errorArchivo = ValidarArchivo(Foto) ?? ValidarArchivo(Firma);
        if (errorArchivo is not null)
            return await ErrorPost(errorArchivo);

        string? rutaFoto;
        string? rutaFirma;
        try
        {
            rutaFoto = await GuardarArchivoAsync(Foto, "foto");
            rutaFirma = await GuardarArchivoAsync(Firma, "firma");
        }
        catch (HttpRequestException ex)
        {
            return await ErrorPost($"No fue posible guardar la evidencia en Supabase Storage: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return await ErrorPost(ex.Message);
        }

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

    public async Task<IActionResult> OnGetPdf(int id)
    {
        var entrega = await _context.Entregas
            .Include(e => e.Detalles)
            .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entrega is null)
            return NotFound();

        try
        {
            var foto = ConvertirImagenParaPdf(await LeerArchivoAsync(entrega.FotoRuta));
            var firma = ConvertirImagenParaPdf(await LeerArchivoAsync(entrega.FirmaRuta));
            var pdf = _pdfGenerator.Generate(entrega, foto, firma);

            await using var pdfStream = new MemoryStream(pdf);
            await _storage.UploadAsync(
                $"entregas/{entrega.Id}/formato-entrega.pdf",
                pdfStream,
                "application/pdf");

            return File(pdf, "application/pdf", $"Entrega-{entrega.Id}.pdf");
        }
        catch (HttpRequestException ex)
        {
            TempData["Error"] = $"No fue posible acceder a Supabase Storage: {ex.Message}";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (FileNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
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