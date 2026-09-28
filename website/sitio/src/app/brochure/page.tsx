import type { Metadata } from "next";
import { BrochureBook } from "@/components/BrochureBook";

export const metadata: Metadata = {
  title: "Brochure institucional",
  description: "Recorre el brochure interactivo de L&C Propiedad Raíz.",
};

export default function BrochurePage() {
  return (
    <div className="brochure-page">
      <header className="brochure-heading">
        <p className="section-kicker">Brochure interactivo · 2026</p>
        <h1>Conoce L&amp;C, hoja por hoja.</h1>
        <p>Arrastra una esquina, usa las flechas o entra en pantalla completa.</p>
      </header>
      <BrochureBook />
    </div>
  );
}
