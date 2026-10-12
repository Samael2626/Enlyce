"use client";

import { useState } from "react";

// Menu nativo de compartir en movil; en escritorio copia el enlace.
export function ShareButton({ title, className }: { title: string; className?: string }) {
  const [copied, setCopied] = useState(false);

  const share = async () => {
    const url = window.location.href;
    if (typeof navigator.share === "function") {
      try {
        await navigator.share({ title, text: `Mira este inmueble de L&C: ${title}`, url });
      } catch {
        // Cancelar el menu nativo no es un error.
      }
      return;
    }
    try {
      await navigator.clipboard.writeText(url);
      setCopied(true);
      window.setTimeout(() => setCopied(false), 2500);
    } catch {
      window.prompt("Copia el enlace", url);
    }
  };

  return (
    <button type="button" onClick={share} className={className}>
      {copied ? "Enlace copiado" : "Compartir"}
    </button>
  );
}
