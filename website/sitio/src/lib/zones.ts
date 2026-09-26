export type ZoneKind = "Municipio" | "Barrio";

export type Zone = {
  slug: string;
  name: string;
  municipality: string;
  neighborhood: string;
  kind: ZoneKind;
  summary: string;
};

export type ZoneGroup = {
  municipality: string;
  zones: readonly Zone[];
};

// Cobertura comercial navegable. Los nombres siguen la division territorial
// oficial; el inventario siempre se consulta en la API y no se duplica aqui.
export const NEIGHBORHOODS: readonly Zone[] = [
  { slug: "medellin", name: "Medellín", municipality: "Medellín", neighborhood: "", kind: "Municipio", summary: "Consulta en una sola vista la oferta publicada en Medellín antes de elegir un barrio específico." },
  { slug: "laureles", name: "Laureles", municipality: "Medellín", neighborhood: "Laureles", kind: "Barrio", summary: "Vida a pie, calles arboladas y una oferta residencial consolidada en el centro-occidente." },
  { slug: "estadio", name: "Estadio", municipality: "Medellín", neighborhood: "Estadio", kind: "Barrio", summary: "Entorno residencial conectado con escenarios deportivos, comercio y transporte público." },
  { slug: "conquistadores", name: "Conquistadores", municipality: "Medellín", neighborhood: "Los Conquistadores", kind: "Barrio", summary: "Sector central y residencial junto al corredor del río y los equipamientos de la ciudad." },
  { slug: "el-poblado", name: "El Poblado", municipality: "Medellín", neighborhood: "El Poblado", kind: "Barrio", summary: "Oferta residencial y empresarial diversa, con servicios y conexiones hacia el sur del valle." },
  { slug: "castropol", name: "Castropol", municipality: "Medellín", neighborhood: "Castropol", kind: "Barrio", summary: "Vivienda urbana en una ubicación estratégica entre El Poblado y los principales corredores viales." },
  { slug: "el-tesoro", name: "El Tesoro", municipality: "Medellín", neighborhood: "El Tesoro", kind: "Barrio", summary: "Laderas residenciales, servicios cercanos y una oferta marcada por apartamentos y unidades cerradas." },
  { slug: "los-balsos", name: "Los Balsos", municipality: "Medellín", neighborhood: "Los Balsos Nº 1", kind: "Barrio", summary: "Sector residencial de ladera con conexiones hacia Las Palmas, la avenida El Poblado y el sur." },
  { slug: "san-lucas", name: "San Lucas", municipality: "Medellín", neighborhood: "San Lucas", kind: "Barrio", summary: "Entorno residencial próximo a Envigado, con vivienda de baja y media densidad." },
  { slug: "manila", name: "Manila", municipality: "Medellín", neighborhood: "Manila", kind: "Barrio", summary: "Barrio caminable con mezcla residencial, gastronómica y de servicios cerca del metro." },
  { slug: "belen", name: "Belén", municipality: "Medellín", neighborhood: "Belén", kind: "Barrio", summary: "Zona tradicional y familiar con amplia variedad de vivienda y conexiones hacia el occidente." },
  { slug: "loma-de-los-bernal", name: "Loma de Los Bernal", municipality: "Medellín", neighborhood: "La Loma de Los Bernal", kind: "Barrio", summary: "Corredor residencial de Belén con unidades cerradas y acceso al suroccidente de Medellín." },
  { slug: "la-mota", name: "La Mota", municipality: "Medellín", neighborhood: "La Mota", kind: "Barrio", summary: "Sector residencial cercano a equipamientos, comercio y conexiones con Guayabal y Belén." },
  { slug: "la-america", name: "La América", municipality: "Medellín", neighborhood: "La América", kind: "Barrio", summary: "Barrio tradicional del occidente con comercio de proximidad y buena conexión urbana." },
  { slug: "robledo", name: "Robledo", municipality: "Medellín", neighborhood: "Robledo", kind: "Barrio", summary: "Amplia zona del noroccidente con oferta residencial diversa y presencia universitaria." },
  { slug: "buenos-aires", name: "Buenos Aires", municipality: "Medellín", neighborhood: "Buenos Aires", kind: "Barrio", summary: "Sector tradicional del oriente conectado con el centro y el corredor del tranvía." },
  { slug: "guayabal", name: "Guayabal", municipality: "Medellín", neighborhood: "Guayabal", kind: "Barrio", summary: "Ubicación estratégica entre el centro y el sur, con mezcla residencial, comercial e industrial." },
  { slug: "envigado", name: "Envigado", municipality: "Envigado", neighborhood: "", kind: "Municipio", summary: "Municipio del sur con centralidades propias y una oferta residencial que va del casco urbano a la ladera." },
  { slug: "zuniga", name: "Zúñiga", municipality: "Envigado", neighborhood: "Zúñiga", kind: "Barrio", summary: "Barrio residencial del norte de Envigado, próximo a Medellín y a corredores comerciales del sur." },
  { slug: "el-esmeraldal", name: "El Esmeraldal", municipality: "Envigado", neighborhood: "El Esmeraldal", kind: "Barrio", summary: "Sector de ladera con predominio residencial y conexiones hacia El Poblado y el centro de Envigado." },
  { slug: "bosques-de-zuniga", name: "Bosques de Zúñiga", municipality: "Envigado", neighborhood: "Bosques de Zúñiga", kind: "Barrio", summary: "Entorno residencial próximo a la frontera con Medellín y a los servicios del corredor de Las Vegas." },
  { slug: "sabaneta", name: "Sabaneta", municipality: "Sabaneta", neighborhood: "", kind: "Municipio", summary: "Municipio compacto del sur con centro tradicional, proyectos residenciales y conexión metropolitana." },
  { slug: "prados-de-sabaneta", name: "Prados de Sabaneta", municipality: "Sabaneta", neighborhood: "Prados de Sabaneta", kind: "Barrio", summary: "Barrio urbano con acceso a servicios cotidianos y a las centralidades de Sabaneta." },
  { slug: "vegas-de-la-doctora", name: "Vegas de La Doctora", municipality: "Sabaneta", neighborhood: "Vegas de La Doctora", kind: "Barrio", summary: "Sector residencial de Sabaneta alrededor del corredor de La Doctora y sus nuevas centralidades." },
  { slug: "itagui", name: "Itagüí", municipality: "Itagüí", neighborhood: "", kind: "Municipio", summary: "Municipio de vocación urbana, industrial y comercial con acceso directo al corredor metropolitano." },
];

function groupZones(zones: readonly Zone[]): readonly ZoneGroup[] {
  const groups = new Map<string, Zone[]>();

  for (const zone of zones) {
    const municipalityZones = groups.get(zone.municipality) ?? [];
    municipalityZones.push(zone);
    groups.set(zone.municipality, municipalityZones);
  }

  return Array.from(groups, ([municipality, municipalityZones]) => ({
    municipality,
    zones: municipalityZones,
  }));
}

export const ZONE_GROUPS = groupZones(NEIGHBORHOODS);

export function findZone(slug: string): Zone | undefined {
  return NEIGHBORHOODS.find((zone) => zone.slug === slug);
}

export function findNearbyZones(zone: Zone, limit = 3): readonly Zone[] {
  return NEIGHBORHOODS.filter(
    (candidate) => candidate.slug !== zone.slug && candidate.municipality === zone.municipality,
  ).slice(0, limit);
}
