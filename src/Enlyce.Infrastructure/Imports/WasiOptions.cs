namespace Enlyce.Infrastructure.Imports;

public sealed class WasiOptions
{
    public const string SectionName = "Wasi";

    public string BaseUrl { get; set; } = "https://api.wasi.co/v1/";
    public string? CompanyId { get; set; }
    public string? Token { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(CompanyId) && !string.IsNullOrWhiteSpace(Token);
}
