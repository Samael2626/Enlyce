using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Imports;
using Enlyce.Domain.Media;
using Enlyce.Domain.Ports;
using Microsoft.Extensions.Options;

namespace Enlyce.Infrastructure.Imports;

// Adaptador de la API de Wasi (api.wasi.co/guide/es). Falla ruidoso: credenciales
// malas, plan sin API o respuestas inesperadas abortan la importacion completa.
public sealed partial class WasiListingSource : IExternalListingSource
{
    private const int PageSize = 100; // maximo que admite Wasi en take

    // Ids reales de /v1/property-type/all (verificado 2026-10-12).
    private static readonly IReadOnlyDictionary<int, TipoInmueble> TypeMap = new Dictionary<int, TipoInmueble>
    {
        [1] = TipoInmueble.Casa,
        [2] = TipoInmueble.Apartamento,
        [3] = TipoInmueble.Local,
        [4] = TipoInmueble.Oficina,
        [5] = TipoInmueble.Lote,
        [6] = TipoInmueble.Lote,
        [7] = TipoInmueble.Finca,
        [8] = TipoInmueble.Bodega,
        [10] = TipoInmueble.Casa,
        [11] = TipoInmueble.Casa,
        [12] = TipoInmueble.Local,
        [13] = TipoInmueble.Finca,
        [14] = TipoInmueble.Apartamento,
        [15] = TipoInmueble.Oficina,
        [16] = TipoInmueble.Local,
        [17] = TipoInmueble.Lote,
        [18] = TipoInmueble.Local,
        [19] = TipoInmueble.Apartamento,
        [20] = TipoInmueble.Casa,
        [21] = TipoInmueble.Apartamento,
        [22] = TipoInmueble.Casa,
        [23] = TipoInmueble.Bodega,
        [24] = TipoInmueble.Casa,
        [25] = TipoInmueble.Apartamento,
        [26] = TipoInmueble.Bodega,
        [27] = TipoInmueble.Finca,
        [28] = TipoInmueble.Casa,
        [29] = TipoInmueble.Lote,
        [30] = TipoInmueble.Bodega,
        [31] = TipoInmueble.Finca,
        [32] = TipoInmueble.Lote,
        [33] = TipoInmueble.Apartamento
    };

    private readonly HttpClient _http;
    private readonly WasiOptions _options;

    public WasiListingSource(HttpClient http, IOptions<WasiOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public string Source => "wasi";

    public async Task<IReadOnlyList<ExternalListing>> GetListingsAsync(CancellationToken ct = default)
    {
        if (!_options.IsConfigured)
            throw new InvalidOperationException(
                "Wasi SIN CONFIGURAR: defina Wasi:CompanyId y Wasi:Token.");

        var listings = new List<ExternalListing>();
        int total;
        var skip = 0;

        do
        {
            using var page = await PostAsync("property/search", new Dictionary<string, string>
            {
                ["scope"] = "1", // solo inmuebles propios, no los de aliados
                ["take"] = PageSize.ToString(CultureInfo.InvariantCulture),
                ["skip"] = skip.ToString(CultureInfo.InvariantCulture),
                ["order"] = "asc"
            }, ct);

            var root = page.RootElement;
            total = ReadInt(root, "total");
            var pageItems = NumberedItems(root).ToList();
            listings.AddRange(pageItems.Select(Map));

            if (pageItems.Count == 0)
                break;
            skip += pageItems.Count;
        }
        while (skip < total);

        if (listings.Count != total)
            throw new ExternalSourceException(
                $"Wasi reporto {total} inmuebles pero entrego {listings.Count}.");

        return listings;
    }

    private async Task<JsonDocument> PostAsync(
        string path, Dictionary<string, string> parameters, CancellationToken ct)
    {
        // Credenciales por POST, como recomienda Wasi, para que no queden en logs de URL.
        parameters["id_company"] = _options.CompanyId!;
        parameters["wasi_token"] = _options.Token!;

        using var content = new FormUrlEncodedContent(parameters);
        string body;
        try
        {
            using var response = await _http.PostAsync(new Uri(new Uri(_options.BaseUrl), path), content, ct);
            body = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
                throw new ExternalSourceException(
                    $"Wasi respondio HTTP {(int)response.StatusCode} en {path}.");
        }
        catch (Exception error) when (error is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            throw new ExternalSourceException($"No se pudo contactar a Wasi en {path}: {error.Message}", error);
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(body);
        }
        catch (JsonException error)
        {
            throw new ExternalSourceException($"Wasi devolvio una respuesta no JSON en {path}.", error);
        }

        var status = ReadString(document.RootElement, "status");
        if (status != "success")
        {
            var code = ReadString(document.RootElement, "code");
            var message = ReadString(document.RootElement, "message");
            document.Dispose();
            throw new ExternalSourceException($"Wasi rechazo {path}: codigo {code}, {message}");
        }

        return document;
    }

    public async Task<ExternalPhotoContent> DownloadPhotoAsync(ExternalPhoto photo, CancellationToken ct = default)
    {
        // Solo se descarga del CDN de Wasi: evita que un dato manipulado convierta
        // el importador en un proxy hacia la red interna (SSRF).
        if (!Uri.TryCreate(photo.Url, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            !(uri.Host.Equals("wasi.co", StringComparison.OrdinalIgnoreCase) ||
              uri.Host.EndsWith(".wasi.co", StringComparison.OrdinalIgnoreCase)))
            throw new ExternalSourceException($"URL de foto fuera de Wasi: {photo.Url}");

        try
        {
            using var response = await _http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
                throw new ExternalSourceException($"Wasi respondio HTTP {(int)response.StatusCode} al bajar la foto.");

            var length = response.Content.Headers.ContentLength;
            if (length > MediaConstraints.MaxSizeBytes)
                throw new ExternalSourceException("La foto supera el tamano maximo permitido.");

            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            return new ExternalPhotoContent(
                $"wasi-{photo.Position}.{ExtensionFor(contentType)}",
                contentType,
                bytes.LongLength,
                new MemoryStream(bytes, writable: false));
        }
        catch (Exception error) when (error is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            throw new ExternalSourceException($"No se pudo bajar la foto {photo.Position}: {error.Message}", error);
        }
    }

    private static string ExtensionFor(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/png" => "png",
        "image/webp" => "webp",
        _ => "jpg"
    };

    internal static IReadOnlyList<ExternalPhoto> ReadPhotos(JsonElement item)
    {
        if (!item.TryGetProperty("galleries", out var galleries) || galleries.ValueKind != JsonValueKind.Array)
            return [];

        return galleries.EnumerateArray()
            .Where(gallery => gallery.ValueKind == JsonValueKind.Object)
            .SelectMany(NumberedItems)
            .Select(image => new ExternalPhoto(
                FirstNonEmpty(ReadString(image, "url_original"), ReadString(image, "url_big"), ReadString(image, "url")),
                ReadInt(image, "position"),
                ReadString(image, "description")))
            .Where(photo => photo.Url.Length > 0)
            .GroupBy(photo => photo.Url)
            .Select(group => group.First())
            .ToList();
    }

    private static string FirstNonEmpty(params string[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    private static decimal? ReadCoordinate(JsonElement element, string name) =>
        decimal.TryParse(ReadString(element, name), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

    internal static ExternalListing Map(JsonElement item)
    {
        var typeId = ReadInt(item, "id_property_type");
        var builtArea = ReadDecimal(item, "built_area");

        return new ExternalListing(
            Source: "wasi",
            SourceId: ReadString(item, "id_property"),
            Title: ReadString(item, "title"),
            Description: HtmlToText(ReadString(item, "observations")),
            Tipo: TypeMap.TryGetValue(typeId, out var tipo) ? tipo : null,
            SourceTypeLabel: typeId.ToString(CultureInfo.InvariantCulture),
            SalePrice: ReadBool(item, "for_sale") ? ReadDecimal(item, "sale_price") : null,
            RentPrice: ReadBool(item, "for_rent") ? ReadDecimal(item, "rent_price") : null,
            Currency: ReadString(item, "iso_currency") is { Length: 3 } currency ? currency : "COP",
            Street: ReadString(item, "address"),
            City: ReadString(item, "city_label"),
            Neighborhood: NullIfEmpty(ReadString(item, "zone_label")),
            Latitude: ReadCoordinate(item, "latitude"),
            Longitude: ReadCoordinate(item, "longitude"),
            Area: builtArea > 0 ? builtArea : ReadDecimal(item, "area"),
            Bedrooms: (int)ReadDecimal(item, "bedrooms"),
            Bathrooms: (int)ReadDecimal(item, "bathrooms"),
            Garages: (int)ReadDecimal(item, "garages"),
            Stratum: ReadDecimal(item, "stratum") is > 0 and var stratum ? (int)stratum : null,
            Availability: ReadString(item, "availability_label").ToLowerInvariant() switch
            {
                "available" or "disponible" => ExternalAvailability.Disponible,
                "sold" or "vendido" or "vendida" => ExternalAvailability.Vendido,
                "rented" or "arrendado" or "arrendada" => ExternalAvailability.Arrendado,
                _ => ExternalAvailability.Desconocido
            },
            Photos: ReadPhotos(item));
    }

    // Wasi entrega colecciones como objeto con claves "0", "1", ... junto a total/status.
    internal static IEnumerable<JsonElement> NumberedItems(JsonElement root) =>
        root.EnumerateObject()
            .Where(property => property.Name.All(char.IsAsciiDigit) && property.Value.ValueKind == JsonValueKind.Object)
            .OrderBy(property => int.Parse(property.Name, CultureInfo.InvariantCulture))
            .Select(property => property.Value);

    internal static string HtmlToText(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var withBreaks = BlockBreakRegex().Replace(html, "\n");
        var text = WebUtility.HtmlDecode(TagRegex().Replace(withBreaks, string.Empty));
        var lines = text.Split('\n').Select(line => SpacesRegex().Replace(line, " ").Trim());
        return ExtraBlankLinesRegex().Replace(string.Join('\n', lines), "\n\n").Trim();
    }

    private static string ReadString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value)
            ? value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? string.Empty,
                JsonValueKind.Number => value.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => string.Empty
            }
            : string.Empty;

    private static decimal ReadDecimal(JsonElement element, string name) =>
        decimal.TryParse(ReadString(element, name), NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0m;

    private static int ReadInt(JsonElement element, string name) => (int)ReadDecimal(element, name);

    private static bool ReadBool(JsonElement element, string name) =>
        ReadString(element, name).Equals("true", StringComparison.OrdinalIgnoreCase);

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [GeneratedRegex(@"<\s*(br|/p|/div|/li)\s*/?\s*>", RegexOptions.IgnoreCase)]
    private static partial Regex BlockBreakRegex();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"[ \t ]+")]
    private static partial Regex SpacesRegex();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex ExtraBlankLinesRegex();
}
