"use client";

import Link from "next/link";
import { useEffect, useState } from "react";

type BrochurePage = {
  readonly number: string;
  readonly eyebrow: string;
  readonly title: string;
  readonly copy: string;
  readonly details: readonly string[];
  readonly variant: "cover" | "light" | "image" | "dark";
};

const pages: readonly BrochurePage[] = [
  {
    number: "01",
    eyebrow: "L&C Propiedad Raíz · Medellín",
    title: "Patrimonio bien acompañado.",
    copy: "Una mirada clara para comprar, vender, arrendar y administrar inmuebles en Medellín y el Valle de Aburrá.",
    details: ["Selección inmobiliaria", "Criterio comercial", "Acompañamiento humano"],
    variant: "cover",
  },
  {
    number: "02",
    eyebrow: "Nuestra forma de trabajar",
    title: "Menos ruido. Mejores decisiones.",
    copy: "Cada inmueble recibe lectura de mercado, presentación cuidada y seguimiento comercial. Cada cliente sabe qué ocurre y qué sigue.",
    details: ["Diagnóstico inicial", "Estrategia a la medida", "Seguimiento en ENLYCE"],
    variant: "light",
  },
  {
    number: "03",
    eyebrow: "Para propietarios",
    title: "Tu inmueble entra al mercado con dirección.",
    copy: "Construimos una ruta comercial desde la valoración y la documentación hasta la publicación, las visitas y la negociación.",
    details: ["Venta y arriendo", "Administración", "Valoración comercial"],
    variant: "image",
  },
  {
    number: "04",
    eyebrow: "Para compradores y arrendatarios",
    title: "Encontrar también es descartar bien.",
    copy: "Filtramos opciones, explicamos el contexto y acompañamos la visita para que compares con información útil, no con promesas.",
    details: ["Búsqueda enfocada", "Visitas acompañadas", "Soporte documental"],
    variant: "light",
  },
  {
    number: "05",
    eyebrow: "Conocimiento local",
    title: "Medellín cambia de calle en calle.",
    copy: "Trabajamos con lectura de barrio, movilidad, servicios y dinámica comercial en las zonas donde realmente operamos.",
    details: ["Medellín", "Envigado", "Sabaneta · Itagüí · Bello"],
    variant: "image",
  },
  {
    number: "06",
    eyebrow: "El siguiente movimiento",
    title: "Hablemos de tu propiedad.",
    copy: "Cuéntanos qué necesitas. Un asesor de L&C organizará la conversación y ENLYCE mantendrá el seguimiento.",
    details: ["lycpropiedadraiz.com", "Medellín, Colombia", "Atención personalizada"],
    variant: "dark",
  },
];

export function BrochureBook() {
  const [currentPage, setCurrentPage] = useState(0);

  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "ArrowRight") setCurrentPage((page) => Math.min(page + 1, pages.length - 1));
      if (event.key === "ArrowLeft") setCurrentPage((page) => Math.max(page - 1, 0));
    }

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, []);

  return (
    <section className="brochure-reader" aria-label="Brochure institucional L&C">
      <div className="brochure-reader-bar">
        <div>
          <span>Brochure institucional</span>
          <strong>{String(currentPage + 1).padStart(2, "0")} / {String(pages.length).padStart(2, "0")}</strong>
        </div>
        <div className="brochure-reader-actions">
          <a href="/brochure-lyc.pdf" download className="brochure-download">Descargar PDF</a>
          <Link href="/contacto" className="brochure-contact">Conversar con un asesor</Link>
        </div>
      </div>

      <div className="brochure-book" aria-live="polite">
        {pages.map((page, index) => (
          <article
            key={page.number}
            className={`brochure-sheet brochure-sheet-${page.variant}`}
            data-active={index === currentPage ? "true" : "false"}
            aria-hidden={index !== currentPage}
          >
            <div className="brochure-sheet-index">{page.number}</div>
            <div className="brochure-sheet-copy">
              <p>{page.eyebrow}</p>
              <h1>{page.title}</h1>
              <div className="brochure-rule" aria-hidden="true" />
              <p>{page.copy}</p>
            </div>
            <ul>
              {page.details.map((detail) => <li key={detail}>{detail}</li>)}
            </ul>
            <span className="brochure-monogram" aria-hidden="true">L&amp;C</span>
          </article>
        ))}
      </div>

      <div className="brochure-controls">
        <button type="button" onClick={() => setCurrentPage((page) => Math.max(page - 1, 0))} disabled={currentPage === 0}>
          ← Anterior
        </button>
        <div className="brochure-progress" aria-hidden="true">
          {pages.map((page, index) => <span key={page.number} data-active={index === currentPage ? "true" : "false"} />)}
        </div>
        <button type="button" onClick={() => setCurrentPage((page) => Math.min(page + 1, pages.length - 1))} disabled={currentPage === pages.length - 1}>
          Siguiente →
        </button>
      </div>
    </section>
  );
}
