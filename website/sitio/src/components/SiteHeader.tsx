"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, useState } from "react";

const navigation = [
  { href: "/inmuebles", label: "Inmuebles" },
  { href: "/zonas", label: "Zonas" },
  { href: "/blog", label: "Blog" },
  { href: "/propietarios", label: "Propietarios" },
  { href: "/favoritos", label: "Favoritos" },
];

const aboutNavigation = [
  { href: "/nosotros", label: "Quiénes somos", description: "Historia, propósito y forma de trabajar" },
  { href: "/nosotros#servicios", label: "Servicios", description: "Acompañamiento inmobiliario integral" },
  { href: "/zonas", label: "Zonas", description: "Conocimiento local del Valle de Aburrá" },
  { href: "/brochure", label: "Brochure", description: "Presentación institucional interactiva y PDF" },
];

export function SiteHeader() {
  const pathname = usePathname();
  // El menu queda atado a la ruta en que se abrio: al navegar se cierra solo.
  const [openOn, setOpenOn] = useState<string | null>(null);
  const open = openOn === pathname;
  const setOpen = (value: boolean) => setOpenOn(value ? pathname : null);

  // Bloquear el scroll del fondo mientras el menu esta abierto.
  useEffect(() => {
    document.body.classList.toggle("menu-open", open);
    if (!open) return;
    const onKey = (event: KeyboardEvent) => event.key === "Escape" && setOpenOn(null);
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [open]);

  const isActive = (href: string) => pathname === href || pathname.startsWith(`${href}/`);

  return (
    <header className="site-header" data-menu-open={open}>
      <div className="site-header-bar">
        <Link href="/" className="brand-lockup" aria-label="L&C Propiedad Raíz, inicio">
          <span>
            <strong>L&amp;C</strong>
            <small>Propiedad Raíz S.A.S.</small>
          </span>
        </Link>
        <button
          type="button"
          className="site-menu-toggle"
          aria-expanded={open}
          aria-controls="site-navigation"
          onClick={() => setOpen(!open)}
        >
          <span aria-hidden="true" />
          <span className="sr-only">{open ? "Cerrar menú" : "Abrir menú"}</span>
        </button>
        <nav id="site-navigation" aria-label="Principal" className="site-navigation">
          {navigation.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className="site-navigation-link"
              aria-current={isActive(item.href) ? "page" : undefined}
            >
              {item.label}
            </Link>
          ))}
          <details className="site-navigation-menu">
            <summary>Nosotros</summary>
            <div className="site-navigation-dropdown">
              {aboutNavigation.map((item) => (
                <Link key={item.href} href={item.href}>
                  <strong>{item.label}</strong>
                  <span>{item.description}</span>
                </Link>
              ))}
            </div>
          </details>
          <Link href="/contacto" className="site-navigation-contact">
            Contacto
          </Link>
        </nav>
      </div>
    </header>
  );
}
