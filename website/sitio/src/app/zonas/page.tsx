import type { Metadata } from "next";
import Link from "next/link";
import { NEIGHBORHOODS, ZONE_GROUPS } from "@/lib/zones";

export const metadata: Metadata = {
  title: "Barrios y zonas del Valle de Aburrá",
  description:
    "Explora inmuebles por municipio y barrio en Medellín, Envigado, Sabaneta e Itagüí.",
};

export default function ZonasPage() {
  return (
    <main className="zones-page">
      <header className="zones-hero">
        <div>
          <p className="section-kicker">Conocimiento local</p>
          <h1>Elige primero el lugar. Después, el inmueble.</h1>
        </div>
        <div className="zones-hero-aside">
          <p>
            Explora el inventario por municipio y barrio. Cada página conserva el filtro para
            mostrar únicamente las propiedades publicadas en esa ubicación.
          </p>
          <dl>
            <div>
              <dt>{NEIGHBORHOODS.length}</dt>
              <dd>zonas navegables</dd>
            </div>
            <div>
              <dt>{ZONE_GROUPS.length}</dt>
              <dd>municipios</dd>
            </div>
          </dl>
        </div>
      </header>

      <nav className="zones-directory" aria-label="Municipios disponibles">
        {ZONE_GROUPS.map((group, groupIndex) => (
          <a key={group.municipality} href={`#municipio-${groupIndex + 1}`}>
            {group.municipality}
            <span>{group.zones.length.toString().padStart(2, "0")}</span>
          </a>
        ))}
      </nav>

      <div className="zones-groups">
        {ZONE_GROUPS.map((group, groupIndex) => (
          <section
            key={group.municipality}
            id={`municipio-${groupIndex + 1}`}
            className="zones-group"
            aria-labelledby={`zone-group-${groupIndex}`}
          >
            <header>
              <span aria-hidden="true">{(groupIndex + 1).toString().padStart(2, "0")}</span>
              <div>
                <p>Valle de Aburrá</p>
                <h2 id={`zone-group-${groupIndex}`}>{group.municipality}</h2>
              </div>
            </header>

            <ol className="zones-list">
              {group.zones.map((zone, zoneIndex) => (
                <li key={zone.slug}>
                  <Link href={`/zonas/${zone.slug}`}>
                    <span>{(zoneIndex + 1).toString().padStart(2, "0")}</span>
                    <div>
                      <small>{zone.kind}</small>
                      <h3>{zone.name}</h3>
                      <p>{zone.summary}</p>
                    </div>
                    <strong aria-hidden="true">Ver zona</strong>
                  </Link>
                </li>
              ))}
            </ol>
          </section>
        ))}
      </div>

      <section className="zones-cta" aria-labelledby="zones-cta-title">
        <div>
          <p className="section-kicker">¿No encuentras tu sector?</p>
          <h2 id="zones-cta-title">Cuéntanos dónde quieres comprar, arrendar o publicar.</h2>
        </div>
        <div>
          <Link href="/contacto">Hablar con un asesor</Link>
          <Link href="/propietarios">Publicar mi propiedad</Link>
        </div>
      </section>
    </main>
  );
}
