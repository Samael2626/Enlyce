namespace Enlyce.Infrastructure.Media;

public sealed class MediaStorageOptions
{
    public const string SectionName = "MediaStorage";

    // Backend de almacenamiento: "local" (disco + static files) o "s3"
    // (object storage S3-compatible, p. ej. Cloudflare R2).
    public string Provider { get; set; } = "local";

    // Carpeta fisica donde se escriben las variantes (solo Provider=local). Relativa al content root.
    public string RootPath { get; set; } = "wwwroot/media";

    // Prefijo publico servido por static files (solo Provider=local).
    public string RequestPath { get; set; } = "/media";

    // Origen absoluto con el que se construyen las URL publicas (Provider=local).
    public string PublicBaseUrl { get; set; } = "http://localhost:5019";

    // Anchos generados, de mayor a menor. El mayor es la URL canonica.
    public int[] VariantWidths { get; set; } = [1600, 800, 400];

    public int Quality { get; set; } = 82;

    // Configuracion del backend S3-compatible (solo Provider=s3).
    public S3StorageOptions S3 { get; set; } = new();
}

public sealed class S3StorageOptions
{
    // Endpoint del servicio S3-compatible. En Cloudflare R2:
    // https://<account-id>.r2.cloudflarestorage.com
    public string ServiceUrl { get; set; } = string.Empty;

    // Region. R2 ignora la region real pero el SDK exige una; "auto" es valida.
    public string Region { get; set; } = "auto";

    public string Bucket { get; set; } = string.Empty;

    // Credenciales. Nunca en appsettings versionado: via secretos/entorno.
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    // Base publica para construir las URL (bucket publico de R2 o dominio propio),
    // sin barra final. Ej: https://media.enlyce.com
    public string PublicBaseUrl { get; set; } = string.Empty;

    // R2 y la mayoria de compatibles requieren path-style.
    public bool ForcePathStyle { get; set; } = true;

    // Prefijo de clave base dentro del bucket (sin barras en los extremos).
    public string KeyPrefix { get; set; } = string.Empty;
}
