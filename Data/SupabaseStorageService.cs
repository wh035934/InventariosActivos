using System.Net.Http.Headers;
using System.Text.Json;

namespace InventarioActivos.Data;

public sealed class SupabaseStorageService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public SupabaseStorageService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task UploadAsync(string objectPath, Stream content, string contentType)
    {
        var settings = GetSettings();
        using var request = CreateRequest(
            HttpMethod.Post,
            $"{settings.BaseUrl}/storage/v1/object/{Escape(settings.Bucket)}/{EscapePath(objectPath)}",
            settings.ApiKey);
        request.Headers.TryAddWithoutValidation("x-upsert", "true");
        request.Content = new StreamContent(content);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<byte[]> DownloadAsync(string objectPath)
    {
        var signedUrl = await CreateSignedUrlAsync(objectPath);
        return await _httpClient.GetByteArrayAsync(signedUrl);
    }

    public async Task<string> CreateSignedUrlAsync(string objectPath)
    {
        var settings = GetSettings();
        using var request = CreateRequest(
            HttpMethod.Post,
            $"{settings.BaseUrl}/storage/v1/object/sign/{Escape(settings.Bucket)}/{EscapePath(objectPath)}",
            settings.ApiKey);
        request.Content = JsonContent.Create(new { expiresIn = 3600 });

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(responseStream);
        var signedUrl = json.RootElement.GetProperty("signedURL").GetString()
            ?? throw new InvalidOperationException("Supabase no devolvió una URL firmada para el archivo.");

        var fullUrl = signedUrl.StartsWith("/storage/v1/", StringComparison.Ordinal)
            ? $"{settings.BaseUrl}{signedUrl}"
            : $"{settings.BaseUrl}/storage/v1/{signedUrl.TrimStart('/')}";

        return fullUrl;
    }

    private StorageSettings GetSettings()
    {
        var url = _configuration["SupabaseStorage:Url"]?.TrimEnd('/');
        var apiKey = _configuration["SupabaseStorage:ServiceRoleKey"];
        var bucket = _configuration["SupabaseStorage:Bucket"];

        if (string.IsNullOrWhiteSpace(url) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(bucket))
        {
            throw new InvalidOperationException(
                "Configura SupabaseStorage:Url, SupabaseStorage:ServiceRoleKey y SupabaseStorage:Bucket.");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var storageUri) ||
            (storageUri.Scheme != Uri.UriSchemeHttps && !storageUri.IsLoopback))
        {
            throw new InvalidOperationException("SupabaseStorage:Url debe ser una URL HTTPS válida.");
        }

        return new StorageSettings(url, apiKey, bucket);
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, string apiKey)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.TryAddWithoutValidation("apikey", apiKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return request;
    }

    private static string Escape(string value) => Uri.EscapeDataString(value);

    private static string EscapePath(string path)
    {
        var segments = path.Split('/');
        if (segments.Length == 0 || segments.Any(segment =>
                string.IsNullOrWhiteSpace(segment) || segment is "." or ".."))
        {
            throw new ArgumentException("La ruta del archivo no es válida.", nameof(path));
        }

        return string.Join('/', segments.Select(Escape));
    }

    private sealed record StorageSettings(string BaseUrl, string ApiKey, string Bucket);
}
