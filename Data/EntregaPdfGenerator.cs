using System.Globalization;
using System.Text;
using InventarioActivos.Models;
using SkiaSharp;

namespace InventarioActivos.Data;

public sealed class EntregaPdfGenerator
{
    private const double AnchoPagina = 595.28;
    private const double AltoPagina = 841.89;
    private const double Margen = 42;

    public byte[] Generate(Entrega entrega, byte[]? foto, byte[]? firma)
    {
        var paginas = new List<ContenidoPagina>();
        var imagenes = new List<ImagenPdf>();
        var pagina = CrearContenidoPagina();
        var y = Margen;

        void Texto(string texto, double x, double top, double tamano, bool negrita = false)
        {
            var fuente = negrita ? "F2" : "F1";
            var baseline = AltoPagina - top - tamano;
            pagina.Comandos.Append("BT /").Append(fuente).Append(' ')
                .Append(Formato(tamano)).Append(" Tf 1 0 0 1 ")
                .Append(Formato(x)).Append(' ').Append(Formato(baseline))
                .Append(" Tm (").Append(EscaparTexto(texto)).Append(") Tj ET\n");
        }

        double TextoEnLineas(string texto, double x, double top, double ancho, double tamano)
        {
            var lineas = AjustarLineas(texto, ancho, tamano);
            for (var i = 0; i < lineas.Count; i++)
                Texto(lineas[i], x, top + i * tamano * 1.4, tamano);

            return lineas.Count * tamano * 1.4;
        }

        void Linea(double x1, double top1, double x2, double top2)
        {
            pagina.Comandos.Append(Formato(x1)).Append(' ')
                .Append(Formato(AltoPagina - top1)).Append(" m ")
                .Append(Formato(x2)).Append(' ')
                .Append(Formato(AltoPagina - top2)).Append(" l S\n");
        }

        void Rectangulo(double x, double top, double ancho, double alto)
        {
            pagina.Comandos.Append(Formato(x)).Append(' ')
                .Append(Formato(AltoPagina - top - alto)).Append(' ')
                .Append(Formato(ancho)).Append(' ').Append(Formato(alto))
                .Append(" re S\n");
        }

        void NuevaPagina(bool continuacion)
        {
            pagina = CrearContenidoPagina();
            y = Margen;
            if (continuacion)
            {
                Texto($"FORMATO DE ENTREGA - Folio {entrega.Id} (continuación)",
                    Margen, y, 13, true);
                y += 30;
            }
        }

        void AsegurarEspacio(double altoNecesario)
        {
            if (y + altoNecesario > AltoPagina - Margen)
                NuevaPagina(continuacion: true);
        }

        void EncabezadoTabla()
        {
            const double altoFila = 24;
            Rectangulo(Margen, y, AnchoPagina - Margen * 2, altoFila);
            Texto("Producto", Margen + 6, y + 6, 10, true);
            Texto("Cantidad", AnchoPagina - Margen - 80, y + 6, 10, true);
            y += altoFila;
        }

        void Imagen(byte[] bytes, string titulo, double x, double anchoMaximo, double altoMaximo)
        {
            using var bitmap = SKBitmap.Decode(bytes)
                ?? throw new InvalidOperationException("No se pudo leer una imagen de evidencia.");

            var nombre = $"Im{imagenes.Count + 1}";
            imagenes.Add(new ImagenPdf(nombre, bitmap.Width, bitmap.Height, bytes));
            Texto(titulo, x, y, 8);

            var escala = Math.Min(anchoMaximo / bitmap.Width, altoMaximo / bitmap.Height);
            var ancho = bitmap.Width * escala;
            var alto = bitmap.Height * escala;
            var bottom = AltoPagina - y - 12 - alto;
            pagina.Comandos.Append("q ").Append(Formato(ancho)).Append(" 0 0 ")
                .Append(Formato(alto)).Append(' ').Append(Formato(x)).Append(' ')
                .Append(Formato(bottom)).Append(" cm /").Append(nombre).Append(" Do Q\n");
        }

        Texto("FORMATO DE ENTREGA DE ACTIVOS", Margen, y, 17, true);
        y += 34;
        Linea(Margen, y, AnchoPagina - Margen, y);
        y += 18;

        Texto($"Folio: {entrega.Id}", Margen, y, 12, true);
        Texto($"Fecha: {entrega.Fecha.ToLocalTime():dd/MM/yyyy HH:mm}",
            Margen + 255, y + 2, 10);
        y += 25;

        foreach (var campo in new[]
        {
            $"Nombre: {entrega.Nombre}",
            $"Puesto: {entrega.Puesto}",
            $"Área: {entrega.Area}"
        })
        {
            y += TextoEnLineas(campo, Margen, y, AnchoPagina - Margen * 2, 10) + 5;
        }

        y += 12;
        Texto("Bienes entregados", Margen, y, 12, true);
        y += 20;
        EncabezadoTabla();

        foreach (var detalle in entrega.Detalles)
        {
            var nombreProducto = detalle.Producto?.Nombre ?? $"Producto #{detalle.ProductoId}";
            var lineas = AjustarLineas(nombreProducto, AnchoPagina - Margen * 2 - 100, 10);
            var altoFila = Math.Max(24, lineas.Count * 14);
            AsegurarEspacio(altoFila);

            if (y == Margen + 30)
                EncabezadoTabla();

            Rectangulo(Margen, y, AnchoPagina - Margen * 2, altoFila);
            for (var i = 0; i < lineas.Count; i++)
                Texto(lineas[i], Margen + 6, y + 6 + i * 14, 10);
            Texto(detalle.Cantidad.ToString(CultureInfo.InvariantCulture),
                AnchoPagina - Margen - 80, y + 6, 10);
            y += altoFila;
        }

        var hayImagen = foto is not null || firma is not null;
        AsegurarEspacio(hayImagen ? 190 : 100);
        y += 18;
        Texto("Evidencia", Margen, y, 12, true);
        y += 22;

        if (foto is not null)
            Imagen(foto, "Foto de artículos", Margen, 220, 118);
        if (firma is not null)
            Imagen(firma, "Firma / acuse", Margen + 240, 220, 118);

        if (hayImagen)
            y += 145;
        else
        {
            Texto("No se adjuntó evidencia fotográfica.", Margen, y, 8);
            y += 18;
        }

        AsegurarEspacio(100);
        y += 12;
        Linea(Margen + 10, y + 34, Margen + 190, y + 34);
        Linea(AnchoPagina - Margen - 190, y + 34,
            AnchoPagina - Margen - 10, y + 34);
        Texto("Firma de quien recibe", Margen + 10, y + 40, 8);
        Texto("Firma de quien entrega", AnchoPagina - Margen - 190, y + 40, 8);
        y += 70;
        Texto("Documento generado para impresión y firma.", Margen, y, 8);

        return CrearPdf(paginas, imagenes);

        ContenidoPagina CrearContenidoPagina()
        {
            var nueva = new ContenidoPagina();
            paginas.Add(nueva);
            return nueva;
        }
    }

    public byte[] GenerateInventario(
        IReadOnlyList<Producto> productos,
        string? ubicacion,
        string? categoria)
    {
        var paginas = new List<ContenidoPagina>();
        var pagina = CrearContenidoPagina();
        var y = Margen;
        var anchoContenido = AnchoPagina - Margen * 2;
        var cultura = CultureInfo.GetCultureInfo("es-MX");

        void Texto(string texto, double x, double top, double tamano, bool negrita = false)
        {
            var fuente = negrita ? "F2" : "F1";
            var baseline = AltoPagina - top - tamano;
            pagina.Comandos.Append("BT /").Append(fuente).Append(' ')
                .Append(Formato(tamano)).Append(" Tf 1 0 0 1 ")
                .Append(Formato(x)).Append(' ').Append(Formato(baseline))
                .Append(" Tm (").Append(EscaparTexto(texto)).Append(") Tj ET\n");
        }

        double TextoEnLineas(string texto, double x, double top, double ancho, double tamano)
        {
            var lineas = AjustarLineas(texto, ancho, tamano);
            for (var i = 0; i < lineas.Count; i++)
                Texto(lineas[i], x, top + i * tamano * 1.4, tamano);

            return lineas.Count * tamano * 1.4;
        }

        void Linea(double x1, double top1, double x2, double top2)
        {
            pagina.Comandos.Append(Formato(x1)).Append(' ')
                .Append(Formato(AltoPagina - top1)).Append(" m ")
                .Append(Formato(x2)).Append(' ')
                .Append(Formato(AltoPagina - top2)).Append(" l S\n");
        }

        void Rectangulo(double top, double alto)
        {
            pagina.Comandos.Append(Formato(Margen)).Append(' ')
                .Append(Formato(AltoPagina - top - alto)).Append(' ')
                .Append(Formato(anchoContenido)).Append(' ')
                .Append(Formato(alto)).Append(" re S\n");
        }

        string FormatearDinero(decimal valor) => "$" + valor.ToString("N2", cultura);

        void EncabezadoTabla()
        {
            const double altoFila = 28;
            Rectangulo(y, altoFila);
            Texto("Producto", Margen + 6, y + 8, 8, true);
            Texto("Categoría", Margen + 163, y + 8, 8, true);
            Texto("Unidades", Margen + 267, y + 8, 8, true);
            Texto("Precio unitario", Margen + 314, y + 8, 8, true);
            Texto("Valor", Margen + 421, y + 8, 8, true);
            y += altoFila;
        }

        void EncabezadoContinuacion(string nombreUbicacion)
        {
            pagina = CrearContenidoPagina();
            y = Margen;
            Texto("REPORTE DE INVENTARIO (continuación)", Margen, y, 13, true);
            y += 28;
            TextoEnLineas($"Sección o bodega: {nombreUbicacion}",
                Margen, y, anchoContenido, 10);
            y += 20;
            EncabezadoTabla();
        }

        Texto("REPORTE DE INVENTARIO", Margen, y, 17, true);
        y += 28;
        Texto($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}", Margen, y, 9);
        y += 17;
        TextoEnLineas($"Sección o bodega: {ubicacion ?? "Todas"}",
            Margen, y, anchoContenido, 9);
        y += 14;
        TextoEnLineas($"Categoría: {categoria ?? "Todas"}",
            Margen, y, anchoContenido, 9);
        y += 20;

        Linea(Margen, y, AnchoPagina - Margen, y);
        y += 15;
        Texto($"Productos distintos: {productos.Count}", Margen, y, 10, true);
        y += 16;
        Texto($"Unidades en inventario: {productos.Sum(p => p.Cantidad)}", Margen, y, 10, true);
        y += 16;
        Texto($"Valor total: {FormatearDinero(productos.Sum(p => p.Cantidad * p.Precio))}",
            Margen, y, 10, true);
        y += 24;

        if (productos.Count == 0)
        {
            Texto("No hay productos para los filtros seleccionados.", Margen, y, 10);
        }

        foreach (var grupo in productos
                     .GroupBy(p => string.IsNullOrWhiteSpace(p.Ubicacion)
                         ? "Sin ubicación"
                         : p.Ubicacion)
                     .OrderBy(g => g.Key))
        {
            if (y + 75 > AltoPagina - Margen)
            {
                pagina = CrearContenidoPagina();
                y = Margen;
            }

            TextoEnLineas($"Sección o bodega: {grupo.Key}",
                Margen, y, anchoContenido, 11);
            y += 19;
            Texto(
                $"Subtotal: {grupo.Count()} productos | {grupo.Sum(p => p.Cantidad)} unidades | {FormatearDinero(grupo.Sum(p => p.Cantidad * p.Precio))}",
                Margen, y, 9, true);
            y += 18;
            EncabezadoTabla();

            foreach (var producto in grupo.OrderBy(p => p.Nombre))
            {
                var lineasNombre = AjustarLineas(producto.Nombre, 150, 8);
                var lineasCategoria = AjustarLineas(producto.Categoria, 96, 8);
                var altoFila = Math.Max(28, Math.Max(lineasNombre.Count, lineasCategoria.Count) * 12 + 10);

                if (y + altoFila > AltoPagina - Margen)
                {
                    EncabezadoContinuacion(grupo.Key);
                }

                Rectangulo(y, altoFila);
                for (var i = 0; i < lineasNombre.Count; i++)
                    Texto(lineasNombre[i], Margen + 6, y + 6 + i * 12, 8);
                for (var i = 0; i < lineasCategoria.Count; i++)
                    Texto(lineasCategoria[i], Margen + 163, y + 6 + i * 12, 8);

                Texto(producto.Cantidad.ToString(CultureInfo.InvariantCulture),
                    Margen + 267, y + 6, 8);
                Texto(FormatearDinero(producto.Precio), Margen + 314, y + 6, 8);
                Texto(FormatearDinero(producto.Cantidad * producto.Precio),
                    Margen + 421, y + 6, 8);
                y += altoFila;
            }

            y += 16;
        }

        return CrearPdf(paginas, []);

        ContenidoPagina CrearContenidoPagina()
        {
            var nueva = new ContenidoPagina();
            paginas.Add(nueva);
            return nueva;
        }
    }

    private static byte[] CrearPdf(IReadOnlyList<ContenidoPagina> paginas, IReadOnlyList<ImagenPdf> imagenes)
    {
        var objetos = new List<byte[]?> { null };
        objetos.Add([]);
        objetos.Add([]);
        objetos.Add(Ascii("3 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>\nendobj\n"));
        objetos.Add(Ascii("4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>\nendobj\n"));

        var idsImagenes = new Dictionary<string, int>();
        foreach (var imagen in imagenes)
        {
            var id = objetos.Count;
            idsImagenes[imagen.Nombre] = id;
            objetos.Add(CrearObjetoStream(
                id,
                $"/Type /XObject /Subtype /Image /Width {imagen.Ancho} /Height {imagen.Alto} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode",
                imagen.Bytes));
        }

        var idsPaginas = new List<int>();
        foreach (var contenido in paginas)
        {
            var idPagina = objetos.Count;
            var idContenido = idPagina + 1;
            idsPaginas.Add(idPagina);

            var recursosImagenes = string.Join(' ', idsImagenes.Select(item =>
                $"/{item.Key} {item.Value} 0 R"));
            var objetosPagina = string.IsNullOrEmpty(recursosImagenes)
                ? string.Empty
                : $"/XObject << {recursosImagenes} >>";
            var recursos = $"/Font << /F1 3 0 R /F2 4 0 R >> {objetosPagina}";

            objetos.Add(Ascii(
                $"{idPagina} 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {Formato(AnchoPagina)} {Formato(AltoPagina)}] /Resources << {recursos} >> /Contents {idContenido} 0 R >>\nendobj\n"));
            objetos.Add(CrearObjetoStream(
                idContenido,
                string.Empty,
                Encoding.ASCII.GetBytes(contenido.Comandos.ToString())));
        }

        objetos[1] = Ascii("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
        objetos[2] = Ascii($"2 0 obj\n<< /Type /Pages /Kids [{string.Join(' ', idsPaginas.Select(id => $"{id} 0 R"))}] /Count {idsPaginas.Count} >>\nendobj\n");

        using var output = new MemoryStream();
        output.Write(Ascii("%PDF-1.4\n"));
        var offsets = new long[objetos.Count];
        for (var id = 1; id < objetos.Count; id++)
        {
            offsets[id] = output.Position;
            output.Write(objetos[id]!);
        }

        var inicioXref = output.Position;
        output.Write(Ascii($"xref\n0 {objetos.Count}\n0000000000 65535 f \n"));
        for (var id = 1; id < objetos.Count; id++)
            output.Write(Ascii($"{offsets[id]:D10} 00000 n \n"));

        output.Write(Ascii(
            $"trailer\n<< /Size {objetos.Count} /Root 1 0 R >>\nstartxref\n{inicioXref}\n%%EOF\n"));
        return output.ToArray();
    }

    private static byte[] CrearObjetoStream(int id, string diccionario, byte[] contenido)
    {
        using var output = new MemoryStream();
        output.Write(Ascii($"{id} 0 obj\n<< {diccionario} /Length {contenido.Length} >>\nstream\n"));
        output.Write(contenido);
        output.Write(Ascii("\nendstream\nendobj\n"));
        return output.ToArray();
    }

    private static List<string> AjustarLineas(string texto, double ancho, double tamano)
    {
        var maxCaracteres = Math.Max(1, (int)(ancho / (tamano * 0.52)));
        var lineas = new List<string>();
        var actual = new StringBuilder();

        foreach (var palabra in texto.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (actual.Length > 0 && actual.Length + palabra.Length + 1 > maxCaracteres)
            {
                lineas.Add(actual.ToString());
                actual.Clear();
            }

            if (actual.Length > 0)
                actual.Append(' ');
            actual.Append(palabra);
        }

        if (actual.Length > 0)
            lineas.Add(actual.ToString());

        return lineas.Count > 0 ? lineas : [string.Empty];
    }

    private static string EscaparTexto(string texto)
    {
        var bytes = Encoding.Latin1.GetBytes(texto);
        var escaped = new StringBuilder(bytes.Length);
        foreach (var value in bytes)
        {
            if (value is (byte)'(' or (byte)')' or (byte)'\\')
                escaped.Append('\\').Append((char)value);
            else if (value < 32 || value > 126)
                escaped.Append('\\').Append(Convert.ToString(value, 8)!.PadLeft(3, '0'));
            else
                escaped.Append((char)value);
        }

        return escaped.ToString();
    }

    private static string Formato(double valor) => valor.ToString("0.##", CultureInfo.InvariantCulture);

    private static byte[] Ascii(string texto) => Encoding.ASCII.GetBytes(texto);

    private sealed class ContenidoPagina
    {
        public StringBuilder Comandos { get; } = new();
    }

    private sealed record ImagenPdf(string Nombre, int Ancho, int Alto, byte[] Bytes);
}
