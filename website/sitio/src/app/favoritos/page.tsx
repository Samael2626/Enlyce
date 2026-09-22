"use client";

import Link from "next/link";
import { useFavorites } from "@/lib/favorites";
import { propertyPath } from "@/lib/format";

export default function FavoritosPage() {
  const { favorites, remove } = useFavorites();

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
          <Link href="/inmuebles" className="text-accent underline">Explorar el inventario</Link>
        </div>
      ) : (
        <ul className="divide-y divide-line rounded-sheet bg-surface shadow-card">
          {favorites.map((favorite) => (
            <li key={favorite.slug} className="flex items-center justify-between gap-4 p-4">
              <Link href={propertyPath(favorite.slug)} className="hover:text-accent">
                {favorite.title}
              </Link>
              <button
                type="button"
                onClick={() => remove(favorite.slug)}
                className="text-sm text-muted underline hover:text-accent"
              >
                Quitar
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
