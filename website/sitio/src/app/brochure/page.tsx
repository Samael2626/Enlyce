import type { Metadata } from "next";
import { BrochureBook } from "@/components/BrochureBook";

export const metadata: Metadata = {
  title: "Brochure institucional",
  description: "Conoce los servicios, el enfoque y las zonas de trabajo de L&C Propiedad Raíz.",
};

export default function BrochurePage() {
  return (
    <div className="brochure-page">
      <header className="brochure-heading">
        <p className="section-kicker">Edición institucional · 2026</p>
        <h1>Una presentación para leer, recorrer y guardar.</h1>
        <p>Usa las flechas del teclado o los controles. Descarga la misma edición en PDF cuando quieras compartirla.</p>
      </header>
      <BrochureBook />
    </div>
  );
}
