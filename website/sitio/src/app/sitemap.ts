import type { MetadataRoute } from "next";
import { MAX_PAGE_SIZE, getProperties } from "@/lib/api/catalog";
import { NEIGHBORHOODS } from "@/lib/zones";
import type { PropertyListItem } from "@/lib/api/client";

const siteUrl = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";

// El catalogo pagina a 100 como maximo; el sitemap recorre todas las paginas.
async function collectProperties(): Promise<PropertyListItem[]> {
  const items: PropertyListItem[] = [];
  let page = 1;

  for (;;) {
    const result = await getProperties({ page, pageSize: MAX_PAGE_SIZE });
    items.push(...result.items);

    const lastPage = Math.ceil(result.total / Math.max(1, result.pageSize));
    if (page >= lastPage || result.items.length === 0) break;
    page += 1;
  }

  return items;
}

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const statics: MetadataRoute.Sitemap = [
    "", "/inmuebles", "/zonas", "/propietarios", "/contacto", "/privacidad",
  ].map((path) => ({ url: `${siteUrl}${path}`, changeFrequency: "weekly", priority: 0.7 }));

  const zones: MetadataRoute.Sitemap = NEIGHBORHOODS.map((zone) => ({
    url: `${siteUrl}/zonas/${zone.slug}`,
    changeFrequency: "weekly",
    priority: 0.6,
  }));

  try {
    const properties = await collectProperties();
    return [
      ...statics,
      ...zones,
      ...properties.map((property) => ({
        url: `${siteUrl}/inmuebles/${property.slug}`,
        lastModified: new Date(property.publishedAt),
        changeFrequency: "daily" as const,
        priority: 0.9,
      })),
    ];
  } catch (error) {
    // Ruidoso en logs: un sitemap sin fichas es un fallo de SEO, no un detalle.
    console.error("Sitemap sin inmuebles: el catalogo no respondio.", error);
    return [...statics, ...zones];
  }
}
