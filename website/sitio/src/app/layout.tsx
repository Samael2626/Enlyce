import type { Metadata, Viewport } from "next";
import { DM_Sans, Fraunces } from "next/font/google";
import Link from "next/link";
import { Suspense } from "react";
import { AnalyticsPageView } from "@/components/AnalyticsPageView";
import { SiteHeader } from "@/components/SiteHeader";
import { SocialContactDock } from "@/components/SocialContactDock";
import { INSTAGRAM_URL, whatsappLink } from "@/lib/contact";
import "./globals.css";
import "leaflet/dist/leaflet.css";

const fraunces = Fraunces({ subsets: ["latin"], variable: "--font-fraunces", display: "swap" });
const dmSans = DM_Sans({ subsets: ["latin"], variable: "--font-dm-sans", display: "swap" });

const siteUrl = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  manifest: "/manifest.webmanifest",
  icons: { apple: "/lyc-logo.png" },
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

export const viewport: Viewport = {
  themeColor: "#0F1A3D",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="es-CO" className={`${fraunces.variable} ${dmSans.variable}`}>
      <body className="min-h-dvh flex flex-col">
        <Suspense fallback={null}><AnalyticsPageView /></Suspense>
        <a
          href="#contenido"
          className="sr-only focus:not-sr-only focus:absolute focus:z-50 focus:m-4 focus:rounded focus:bg-accent focus:px-4 focus:py-2 focus:text-surface"
        >
          Saltar al contenido
        </a>

        <SiteHeader />

        <main id="contenido" className="flex-1">
          {children}
        </main>

        <SocialContactDock />

        <footer className="site-footer">
          <div className="site-footer-grid">
            <div>
              <strong className="site-footer-brand">L&amp;C</strong>
              <p>Propiedad Raíz S.A.S. Asesoría inmobiliaria en Medellín y el Valle de Aburrá.</p>
            </div>
            <nav aria-label="Explorar">
              <p>Explorar</p>
              <Link href="/inmuebles?operation=Venta">Comprar</Link>
              <Link href="/inmuebles?operation=Arriendo">Arrendar</Link>
              <Link href="/zonas">Zonas</Link>
              <Link href="/favoritos">Mis favoritos</Link>
            </nav>
            <nav aria-label="Empresa">
              <p>L&amp;C</p>
              <Link href="/propietarios">Publicar mi inmueble</Link>
              <Link href="/nosotros">Quiénes somos</Link>
              <Link href="/brochure">Brochure</Link>
              <Link href="/contacto">Contacto</Link>
            </nav>
            <div>
              <p>Hablemos</p>
              <a href={whatsappLink()} target="_blank" rel="noreferrer">WhatsApp +57 324 626 6362</a>
              <a href={INSTAGRAM_URL} target="_blank" rel="noreferrer">Instagram</a>
            </div>
          </div>
          <div className="site-footer-legal">
            <p>© {new Date().getFullYear()} L&amp;C Propiedad Raíz S.A.S. — Medellín, Colombia.</p>
            <Link href="/privacidad">Política de tratamiento de datos personales</Link>
          </div>
        </footer>
      </body>
    </html>
  );
}
