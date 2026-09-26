import Image from "next/image";
import Link from "next/link";
import { PropertyCard } from "@/components/PropertyCard";
import { PropertySearchPanel } from "@/components/PropertySearchPanel";
import { getProperties } from "@/lib/api/catalog";

export const dynamic = "force-dynamic";

export default async function HomePage() {
  const page = await getProperties({ pageSize: 6 });

  return (
    <div className="space-y-20 pb-20">
      <div>
        <section className="hero-shell">
          <div className="hero-visual" aria-hidden="true" />
          <div className="hero-shade" aria-hidden="true" />
          <div className="relative mx-auto grid min-h-[660px] max-w-6xl items-end gap-10 px-4 pb-28 pt-14 lg:grid-cols-[1fr_21rem] lg:pb-32 lg:pt-20">
          <Image
            src="/lyc-logo-transparent.png"
            alt="L&C Propiedad Raíz S.A.S."
            width={1280}
            height={1280}
            priority
            className="hero-brand-mark"
          />
          <div className="max-w-3xl space-y-6">
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
        <div className="relative z-10 mx-auto -mt-16 max-w-[82rem] px-4">
          <PropertySearchPanel />
        </div>
      </div>

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

      <section className="home-opportunity" aria-labelledby="home-opportunity-title">
        <div className="home-opportunity-intro">
          <p className="section-kicker">El siguiente movimiento</p>
          <h2 id="home-opportunity-title">Tu propiedad ya tiene valor. Hagamos que el mercado lo vea.</h2>
          <p>
            Habla con un asesor o inicia la publicación de tu inmueble. Cada solicitud entra
            directamente a ENLYCE como una oportunidad comercial con seguimiento.
          </p>
        </div>

        <div className="home-opportunity-actions">
          <Link href="/contacto" className="home-opportunity-card">
            <span>01</span>
            <div>
              <strong>Contactar a un asesor</strong>
              <p>Comprar, arrendar o resolver una duda con acompañamiento humano.</p>
            </div>
          </Link>
          <Link href="/propietarios" className="home-opportunity-card home-opportunity-card-primary">
            <span>02</span>
            <div>
              <strong>Publicar una propiedad</strong>
              <p>Cuéntanos qué tienes y si quieres vender, arrendar, administrar o avaluar.</p>
            </div>
          </Link>
        </div>
      </section>
    </div>
  );
}
