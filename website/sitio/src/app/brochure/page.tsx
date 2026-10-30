import type { Metadata } from "next";
import { preload } from "react-dom";
import { BrochureBook } from "@/components/BrochureBook";

export const metadata: Metadata = {
  title: "Brochure institucional",
  description: "Recorre el brochure interactivo de L&C Propiedad Raíz.",
};

export default function BrochurePage() {
  // La portada es fondo CSS: se precarga para que no espere al CSS (LCP).
  preload("/brochure/casa-contemporanea.webp", { as: "image", fetchPriority: "high" });

  return (
    <div className="brochure-page">
      <h1 className="sr-only">Brochure institucional de L&amp;C Propiedad Raíz</h1>
      <BrochureBook />
    </div>
  );
}
