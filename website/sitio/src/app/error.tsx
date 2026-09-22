"use client";

import { useEffect } from "react";

export default function Error({ error, reset }: { error: Error; reset: () => void }) {
  useEffect(() => {
    // Fallar ruidoso: el error llega a la consola aunque la vista se recupere.
    console.error(error);
  }, [error]);

  return (
    <div className="mx-auto max-w-2xl space-y-4 px-4 py-16 text-center">
      <h1 className="text-3xl">No pudimos cargar el inventario</h1>
      <p className="text-muted">
        La conexión con el catálogo falló. Vuelve a intentarlo en un momento.
      </p>
      <button
        type="button"
        onClick={reset}
        className="rounded bg-accent px-5 py-2 text-surface hover:bg-accent-strong"
      >
        Reintentar
      </button>
    </div>
  );
}
