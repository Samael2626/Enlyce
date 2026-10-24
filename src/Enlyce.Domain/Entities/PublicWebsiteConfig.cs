namespace Enlyce.Domain.Entities;

public sealed class PublicWebsiteConfig
{
    public int Id { get; set; } = 1;
    public string AgencyName { get; set; } = "L&C Propiedad Raíz S.A.S.";
    public string Slogan { get; set; } = "Tu hogar, nuestra prioridad.";
    public string HeroTitle { get; set; } = "Propiedades con altura. Decisiones con criterio.";
    public string HeroSubtitle { get; set; } = "Inmuebles seleccionados y asesoría cercana para comprar, arrendar o vender con seguridad en Medellín y sus alrededores.";
    public string? HeroBannerUrl { get; set; } = "/medellin-hero.png";
    public string AboutTitle { get; set; } = "Quiénes somos";
    public string AboutText { get; set; } = "L&C Propiedad Raíz es una agencia inmobiliaria especializada en el Valle de Aburrá, comprometida con la transparencia, la rigurosidad jurídica y el acompañamiento personalizado para propietarios, compradores y arrendatarios.";
    public string ContactPhone { get; set; } = "+57 324 626 6362";
    public string ContactWhatsapp { get; set; } = "573246266362";
    public string ContactEmail { get; set; } = "contacto@lycpropiedadraiz.com";
    public string Address { get; set; } = "Medellín, Colombia";
    public string? InstagramUrl { get; set; } = "https://instagram.com/lyc_propiedad_raiz_sas";
    public string? FacebookUrl { get; set; }
    public string SeoTitle { get; set; } = "L&C Propiedad Raíz — Inmuebles en Medellín y el Valle de Aburrá";
    public string SeoDescription { get; set; } = "Venta y arriendo de apartamentos, casas, oficinas y locales en Medellín, Envigado, Sabaneta e Itagüí, con acompañamiento de asesores de L&C.";
    public string? PrimaryColor { get; set; } = "#0f1a3d";
    public string? AccentColor { get; set; } = "#b78a2d";
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedByUserId { get; set; }

    public static PublicWebsiteConfig CreateDefault() => new();
}
