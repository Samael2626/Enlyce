import Link from "next/link";
import { PropertyCard } from "@/components/PropertyCard";
import { getProperties } from "@/lib/api/catalog";
import { NEIGHBORHOODS } from "@/lib/zones";

export default async function HomePage() {
  const page = await getProperties({ pageSize: 6 });

  return (
    <div className="space-y-20 pb-20">
      <section className="hero-shell">
        <div className="hero-visual" aria-hidden="true" />
        <div className="hero-shade" aria-hidden="true" />
        <div className="relative mx-auto grid min-h-[660px] max-w-6xl items-end gap-10 px-4 py-14 lg:grid-cols-[1fr_21rem] lg:py-20">
          <div className="max-w-3xl space-y-6">
            <p className="hero-kicker">Medellín · Valle de Aburrá</p>
            <h1 className="text-balance text-5xl leading-[.98] text-white sm:text-6xl lg:text-7xl">
              Propiedades con altura. Decisiones con criterio.
            </h1>
            <p className="max-w-2xl text-lg leading-relaxed text-white/80">
              Inmuebles seleccionados y asesoría cercana para comprar, arrendar o vender con
              seguridad en Medellín y sus alrededores.
            </p>
          </div>
          <div className="hero-actions">
            <p className="text-xs font-bold uppercase tracking-[.2em] text-white/55">Comienza aquí</p>
            <Link
              href="/inmuebles?operation=Venta"
              className="hero-action hero-action-primary"
            >
              Explorar propiedades
            </Link>
            <Link
              href="/inmuebles?operation=Arriendo"
              className="hero-action"
            >
              Buscar arriendo
            </Link>
            <Link
              href="/propietarios"
              className="hero-action"
            >
              Publicar mi inmueble
            </Link>
          </div>
        </div>
      </section>

      <section className="mx-auto max-w-6xl space-y-6 px-4">
        <div className="flex flex-wrap items-baseline justify-between gap-2">
          <div>
            <p className="section-kicker">Selección L&amp;C</p>
            <h2 className="mt-2 text-4xl">Propiedades destacadas</h2>
          </div>
          <Link href="/inmuebles" className="text-accent underline">Ver todo el inventario</Link>
        </div>

        {page.items.length === 0 ? (
          <p className="rounded-sheet border border-line p-8 text-center text-muted">
            Todavía no hay inmuebles publicados.
          </p>
        ) : (
          <ul className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {page.items.map((property) => (
              <li key={property.id}>
                <PropertyCard property={property} />
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="mx-auto max-w-6xl space-y-6 px-4">
        <p className="section-kicker">Conocimiento local</p>
        <h2 className="text-4xl">Zonas que conocemos de verdad</h2>
        <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {NEIGHBORHOODS.map((zone) => (
            <li key={zone.slug}>
              <Link
                href={`/zonas/${zone.slug}`}
                className="block rounded-sheet bg-surface p-5 shadow-card hover:shadow-raised"
              >
                <h3 className="text-xl">{zone.name}</h3>
                <p className="mt-1 text-sm text-muted">{zone.summary}</p>
              </Link>
            </li>
          ))}
        </ul>
      </section>
    </div>
  );
}
