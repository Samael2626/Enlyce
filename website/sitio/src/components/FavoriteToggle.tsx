"use client";

import { useFavorites } from "@/lib/favorites";

type Props = { slug: string; title: string; className?: string };

const overlayClass =
  "absolute right-3 top-3 z-10 rounded-full bg-surface/90 px-3 py-1 text-xs shadow-card hover:text-accent";

export function FavoriteToggle({ slug, title, className = overlayClass }: Props) {
  const { favorites, toggle } = useFavorites();
  const saved = favorites.some((item) => item.slug === slug);

  return (
    <button
      type="button"
      className={className}
      aria-pressed={saved}
      onClick={() => toggle({ slug, title })}
    >
      {saved ? "Guardado" : "Guardar"}
    </button>
  );
}
