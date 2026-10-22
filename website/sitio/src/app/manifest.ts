import type { MetadataRoute } from "next";

export default function manifest(): MetadataRoute.Manifest {
  return {
    name: "L&C Propiedad Raíz",
    short_name: "L&C",
    description: "Inmuebles en Medellín y el Valle de Aburrá.",
    start_url: "/",
    scope: "/",
    display: "standalone",
    background_color: "#0F1A3D",
    theme_color: "#0F1A3D",
    lang: "es-CO",
    icons: [
      { src: "/lyc-logo.png", sizes: "400x400", type: "image/png", purpose: "any" },
      { src: "/lyc-logo-transparent.png", sizes: "1254x1254", type: "image/png", purpose: "any" },
    ],
  };
}
