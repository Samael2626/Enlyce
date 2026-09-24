namespace Enlyce.Infrastructure.Media;

public sealed class MediaStorageOptions
{
    public const string SectionName = "MediaStorage";

    // Carpeta fisica donde se escriben las variantes. Relativa al content root.
    public string RootPath { get; set; } = "wwwroot/media";

    // Prefijo publico servido por static files.
    public string RequestPath { get; set; } = "/media";

    // Origen absoluto con el que se construyen las URL publicas.
    public string PublicBaseUrl { get; set; } = "http://localhost:5019";

    // Anchos generados, de mayor a menor. El mayor es la URL canonica.
    public int[] VariantWidths { get; set; } = [1600, 800, 400];

    public int Quality { get; set; } = 82;
}
