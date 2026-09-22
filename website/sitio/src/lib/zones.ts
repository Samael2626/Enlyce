export type Zone = {
  slug: string;
  name: string;
  municipality: string;
  neighborhood: string;
  summary: string;
};

// Zonas indexables. El inventario se consulta por barrio contra la API, no se
// duplica contenido: cada pagina describe la zona y lista lo publicado ahi.
export const NEIGHBORHOODS: Zone[] = [
  {
    slug: "laureles",
    name: "Laureles",
    municipality: "Medellín",
    neighborhood: "Laureles",
    summary: "Calles arboladas, vida a pie y una de las mejores relaciones entre precio y ubicación.",
  },
  {
    slug: "el-poblado",
    name: "El Poblado",
    municipality: "Medellín",
    neighborhood: "El Poblado",
    summary: "Zona empresarial y residencial de mayor valorización, con oferta amplia de estratos altos.",
  },
  {
    slug: "belen",
    name: "Belén",
    municipality: "Medellín",
    neighborhood: "Belén",
    summary: "Barrio familiar, bien conectado y con precios más accesibles que el centro-sur.",
  },
  {
    slug: "envigado",
    name: "Envigado",
    municipality: "Envigado",
    neighborhood: "",
    summary: "Municipio con identidad propia, buen equipamiento y demanda sostenida de vivienda.",
  },
  {
    slug: "sabaneta",
    name: "Sabaneta",
    municipality: "Sabaneta",
    neighborhood: "",
    summary: "Crecimiento reciente, proyectos nuevos y cercanía al metro.",
  },
  {
    slug: "itagui",
    name: "Itagüí",
    municipality: "Itagüí",
    neighborhood: "",
    summary: "Fuerte componente industrial y comercial, con oferta de bodegas y locales.",
  },
];

export function findZone(slug: string): Zone | undefined {
  return NEIGHBORHOODS.find((zone) => zone.slug === slug);
}
