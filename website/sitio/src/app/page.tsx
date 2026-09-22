import Link from "next/link";
import { PropertyCard } from "@/components/PropertyCard";
import { getProperties } from "@/lib/api/catalog";
import { NEIGHBORHOODS } from "@/lib/zones";

export default async function HomePage() {
  const page = await getProperties({ pageSize: 6 });

  return (
    <div className="space-y-16 pb-16">
      <section className="border-b border-line bg-surface">
        <div className="mx-auto grid max-w-6xl gap-6 px-4 py-16 lg:grid-cols-[3fr_2fr] lg:items-end">
          <div className="space-y-4">
            <p className="text-sm uppercase tracking-wide text-accent">Medellín y Valle de Aburrá</p>
            <h1 className="text-balance text-5xl leading-[1.05]">
              Encuentra la casa que sí se parece a tu vida.
            </h1>
            <p className="max-w-prose text-lg text-muted">
              Inventario propio, acompañamiento de asesores reales y una lectura honesta de cada
              barrio. Sin listados fantasma.
            </p>
          </div>
          <div className="flex flex-wrap gap-3">
            <Link
              href="/inmuebles?operation=Venta"
              className="rounded bg-accent px-5 py-3 text-surface hover:bg-accent-strong"
            >
              Quiero comprar
            </Link>
            <Link
              href="/inmuebles?operation=Arriendo"
              className="rounded border border-line px-5 py-3 hover:text-accent"
            >
              Quiero arrendar
            </Link>
            <Link
              href="/propietarios"
              className="rounded border border-line px-5 py-3 hover:text-accent"
            >
              Tengo un inmueble
            </Link>
          </div>
        </div>
      </section>

      <section className="mx-auto max-w-6xl space-y-6 px-4">
        <div className="flex flex-wrap items-baseline justify-between gap-2">
          <h2 className="text-3xl">Publicaciones recientes</h2>
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
        <h2 className="text-3xl">Zonas que conocemos</h2>
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
