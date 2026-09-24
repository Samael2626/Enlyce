import type { Metadata } from "next";
import { DM_Sans, Fraunces } from "next/font/google";
import Link from "next/link";
import { WhatsAppButton } from "@/components/WhatsAppButton";
import "./globals.css";

const fraunces = Fraunces({ subsets: ["latin"], variable: "--font-fraunces", display: "swap" });
const dmSans = DM_Sans({ subsets: ["latin"], variable: "--font-dm-sans", display: "swap" });

const siteUrl = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  title: {
    default: "L&C Propiedad Raíz — Inmuebles en Medellín y el Valle de Aburrá",
    template: "%s | L&C Propiedad Raíz",
  },
  description:
    "Venta y arriendo de apartamentos, casas, oficinas y locales en Medellín, Envigado, Sabaneta e Itagüí, con acompañamiento de asesores de L&C.",
  openGraph: {
    type: "website",
    locale: "es_CO",
    siteName: "L&C Propiedad Raíz",
  },
};

const navigation = [
  { href: "/inmuebles", label: "Inmuebles" },
  { href: "/zonas", label: "Zonas" },
  { href: "/propietarios", label: "Propietarios" },
  { href: "/favoritos", label: "Favoritos" },
  { href: "/contacto", label: "Contacto" },
];

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="es-CO" className={`${fraunces.variable} ${dmSans.variable}`}>
      <body className="min-h-dvh flex flex-col">
        <a
          href="#contenido"
          className="sr-only focus:not-sr-only focus:absolute focus:z-50 focus:m-4 focus:rounded focus:bg-accent focus:px-4 focus:py-2 focus:text-surface"
        >
          Saltar al contenido
        </a>

        <header className="site-header">
          <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-x-8 gap-y-3 px-4 py-3">
            <Link href="/" className="brand-lockup" aria-label="L&C Propiedad Raíz, inicio">
              <span>
                <strong>L&amp;C</strong>
                <small>Propiedad Raíz S.A.S.</small>
              </span>
            </Link>
            <nav aria-label="Principal" className="flex flex-wrap justify-end gap-x-6 gap-y-2 text-sm font-semibold">
              {navigation.map((item) => (
                <Link key={item.href} href={item.href} className="hover:text-accent">
                  {item.label}
                </Link>
              ))}
            </nav>
          </div>
        </header>

        <main id="contenido" className="flex-1">
          {children}
        </main>

        <WhatsAppButton />

        <footer className="site-footer">
          <div className="mx-auto flex max-w-6xl flex-col gap-2 px-4 py-10 text-sm">
            <p>L&amp;C Propiedad Raíz S.A.S. — Medellín, Colombia.</p>
            <Link href="/privacidad" className="hover:text-accent w-fit">
              Política de tratamiento de datos personales
            </Link>
          </div>
        </footer>
      </body>
    </html>
  );
}
