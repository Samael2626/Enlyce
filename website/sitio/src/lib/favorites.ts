"use client";

import { useCallback, useSyncExternalStore } from "react";

const STORAGE_KEY = "lyc:favoritos";
const CHANGE_EVENT = "lyc:favoritos:cambio";

export type Favorite = { slug: string; title: string };

const EMPTY: Favorite[] = [];

// Cache del snapshot: useSyncExternalStore compara por identidad, asi que
// devolver un array nuevo en cada lectura provocaria renders infinitos.
let cachedRaw: string | null = null;
let cachedValue: Favorite[] = EMPTY;

// MVP sin cuentas: los favoritos viven en el navegador. Si el almacenamiento
// esta bloqueado la pagina sigue funcionando, solo no recuerda.
function parse(raw: string | null): Favorite[] {
  if (!raw) return EMPTY;
  try {
    const parsed: unknown = JSON.parse(raw);
    if (!Array.isArray(parsed)) return EMPTY;
    return parsed.filter(
      (item): item is Favorite =>
        typeof item === "object" && item !== null &&
        typeof (item as Favorite).slug === "string" &&
        typeof (item as Favorite).title === "string",
    );
  } catch {
    return EMPTY;
  }
}

function read(): Favorite[] {
  try {
    return parse(window.localStorage.getItem(STORAGE_KEY));
  } catch {
    return EMPTY;
  }
}

function getSnapshot(): Favorite[] {
  let raw: string | null = null;
  try {
    raw = window.localStorage.getItem(STORAGE_KEY);
  } catch {
    return EMPTY;
  }

  if (raw !== cachedRaw) {
    cachedRaw = raw;
    cachedValue = parse(raw);
  }
  return cachedValue;
}

// En el servidor no hay favoritos; el cliente los hidrata tras montar.
function getServerSnapshot(): Favorite[] {
  return EMPTY;
}

function subscribe(onChange: () => void): () => void {
  window.addEventListener(CHANGE_EVENT, onChange);
  window.addEventListener("storage", onChange);
  return () => {
    window.removeEventListener(CHANGE_EVENT, onChange);
    window.removeEventListener("storage", onChange);
  };
}

function write(favorites: Favorite[]): void {
  try {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(favorites));
  } catch {
    // Modo privado o almacenamiento lleno: se pierde la persistencia, no la vista.
  }
  window.dispatchEvent(new Event(CHANGE_EVENT));
}

export function useFavorites() {
  const favorites = useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot);

  const toggle = useCallback((favorite: Favorite) => {
    const current = read();
    write(
      current.some((item) => item.slug === favorite.slug)
        ? current.filter((item) => item.slug !== favorite.slug)
        : [...current, favorite],
    );
  }, []);

  const remove = useCallback((slug: string) => {
    write(read().filter((item) => item.slug !== slug));
  }, []);

  return { favorites, toggle, remove };
}
