"use client";

import Link from "next/link";
import { useState } from "react";
import { useFavorites } from "@/lib/favorites";
import { propertyPath } from "@/lib/format";

const MAX_COMPARE = 3;

export default function FavoritosPage() {
  const { favorites, remove } = useFavorites();
  const [selected, setSelected] = useState<string[]>([]);
  // Si se quita un favorito, deja de contar para comparar.
  const chosen = selected.filter((slug) => favorites.some((favorite) => favorite.slug === slug));

  const toggle = (slug: string) =>
    setSelected((current) => current.includes(slug)
      ? current.filter((item) => item !== slug)
      : current.length >= MAX_COMPARE ? current : [...current, slug]);

  return (
    <div className="mx-auto max-w-4xl space-y-6 px-4 py-10">
      <header className="space-y-2">
        <h1 className="text-3xl">Tus favoritos</h1>
        <p className="text-muted">
          Se guardan solo en este navegador. No necesitas cuenta y no compartimos nada.
        </p>
      </header>

      {favorites.length === 0 ? (
        <div className="rounded-sheet border border-line p-8 text-center">
          <p className="mb-3 text-muted">Todavía no has guardado ningún inmueble.</p>
          <Link href="/inmuebles" className="text-accent-strong underline">Explorar el inventario</Link>
        </div>
      ) : (
        <>
          {favorites.length > 1 && (
            <div className="favorites-compare-bar" aria-live="polite">
              <p>Marca hasta {MAX_COMPARE} para compararlos lado a lado.</p>
              {chosen.length >= 2 ? (
                <Link href={`/comparar?slugs=${chosen.map(encodeURIComponent).join(",")}`}>
                  Comparar ({chosen.length})
                </Link>
              ) : (
                <span>{chosen.length === 1 ? "Elige uno más" : "Comparar"}</span>
              )}
            </div>
          )}
          <ul className="divide-y divide-line rounded-sheet bg-surface shadow-card">
            {favorites.map((favorite) => {
              const isChosen = chosen.includes(favorite.slug);
              return (
                <li key={favorite.slug} className="flex items-center justify-between gap-4 p-4">
                  <label className="flex min-w-0 items-center gap-3">
                    {favorites.length > 1 && (
                      <input
                        type="checkbox"
                        className="h-5 w-5 accent-[#b78a2d]"
                        checked={isChosen}
                        disabled={!isChosen && chosen.length >= MAX_COMPARE}
                        onChange={() => toggle(favorite.slug)}
                        aria-label={`Comparar ${favorite.title}`}
                      />
                    )}
                    <Link href={propertyPath(favorite.slug)} className="truncate hover:text-accent-strong">
                      {favorite.title}
                    </Link>
                  </label>
                  <button
                    type="button"
                    onClick={() => remove(favorite.slug)}
                    className="shrink-0 text-sm text-muted underline hover:text-accent-strong"
                  >
                    Quitar
                  </button>
                </li>
              );
            })}
          </ul>
        </>
      )}
    </div>
  );
}
