import type { Metadata } from "next";
import Link from "next/link";
import { NEIGHBORHOODS } from "@/lib/zones";

export const metadata: Metadata = {
  title: "Zonas de Medellín y el Valle de Aburrá",
  description:
    "Laureles, El Poblado, Belén, Envigado, Sabaneta e Itagüí: qué esperar de cada zona y qué tiene L&C publicado allí.",
};

export default function ZonasPage() {
  return (
    <div className="mx-auto max-w-6xl space-y-8 px-4 py-10">
      <header className="space-y-2">
        <h1 className="text-3xl">Zonas</h1>
        <p className="max-w-prose text-muted">
          Cada zona tiene su ritmo, sus precios y su tipo de comprador. Esto es lo que vemos
          desde adentro.
        </p>
      </header>
      <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {NEIGHBORHOODS.map((zone) => (
          <li key={zone.slug}>
            <Link
              href={`/zonas/${zone.slug}`}
              className="block h-full rounded-sheet bg-surface p-5 shadow-card hover:shadow-raised"
            >
              <h2 className="text-xl">{zone.name}</h2>
              <p className="mt-1 text-sm text-muted">{zone.summary}</p>
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}
