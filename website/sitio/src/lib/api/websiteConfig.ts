import { apiGetOrNull, resolveMediaUrl } from "./client";

export interface PublicWebsiteConfig {
  agencyName: string;
  slogan: string;
  heroTitle: string;
  heroSubtitle: string;
  heroBannerUrl?: string | null;
  aboutTitle: string;
  aboutText: string;
  contactPhone: string;
  contactWhatsapp: string;
  contactEmail: string;
  address: string;
  instagramUrl?: string | null;
  facebookUrl?: string | null;
  seoTitle: string;
  seoDescription: string;
  primaryColor?: string | null;
  accentColor?: string | null;
  updatedAtUtc: string;
}

export const DEFAULT_WEBSITE_CONFIG: PublicWebsiteConfig = {
  agencyName: "L&C Propiedad Raíz S.A.S.",
  slogan: "Propiedades con criterio y respaldo en Medellín y Valle de Aburrá.",
  heroTitle: "Propiedades con altura. Decisiones con criterio.",
  heroSubtitle:
    "Inmuebles seleccionados y asesoría cercana para comprar, arrendar o vender con seguridad en Medellín y sus alrededores.",
  heroBannerUrl: "/medellin-hero.png",
  aboutTitle: "Sobre L&C Propiedad Raíz",
  aboutText:
    "Somos una agencia inmobiliaria en Medellín comprometida con la transparencia, la rigurosidad jurídica y el servicio personalizado.",
  contactPhone: "+57 324 626 6362",
  contactWhatsapp: "+57 324 626 6362",
  contactEmail: "contacto@lycpropiedadraiz.com",
  address: "Medellín, Antioquia, Colombia",
  instagramUrl: "https://instagram.com/lyc_propiedad_raiz_sas",
  facebookUrl: "https://facebook.com",
  seoTitle: "L&C Propiedad Raíz — Inmuebles en Medellín y el Valle de Aburrá",
  seoDescription:
    "Venta y arriendo de apartamentos, casas, oficinas y locales en Medellín, Envigado, Sabaneta e Itagüí, con acompañamiento de asesores de L&C.",
  primaryColor: "#0f2b1d",
  accentColor: "#c59b27",
  updatedAtUtc: "2026-10-23T00:00:00Z",
};

export async function getPublicWebsiteConfig(): Promise<PublicWebsiteConfig> {
  const remote = await apiGetOrNull<PublicWebsiteConfig>("/api/public/sitio-web", {
    revalidate: 60,
  });

  if (!remote) {
    return DEFAULT_WEBSITE_CONFIG;
  }

  return {
    ...DEFAULT_WEBSITE_CONFIG,
    ...remote,
    heroBannerUrl: remote.heroBannerUrl
      ? resolveMediaUrl(remote.heroBannerUrl)
      : DEFAULT_WEBSITE_CONFIG.heroBannerUrl,
  };
}
