export const FAVORITES_STORAGE_KEY = 'lyc.favorite-property-slugs.v1';
export const MAX_FAVORITES = 24;

function normalizeSlug(value) {
  return typeof value === 'string' ? value.trim().toLowerCase() : '';
}

export function normalizeFavoriteSlugs(value) {
  if (!Array.isArray(value)) return [];

  const unique = new Set();
  for (const valueSlug of value) {
    const slug = normalizeSlug(valueSlug);
    if (!slug || unique.has(slug)) continue;
    unique.add(slug);
    if (unique.size === MAX_FAVORITES) break;
  }
  return [...unique];
}

export function resolveBrowserStorage() {
  try {
    const storage = globalThis.localStorage;
    const probe = `${FAVORITES_STORAGE_KEY}.probe`;
    storage.setItem(probe, probe);
    storage.removeItem(probe);
    return storage;
  } catch {
    return null;
  }
}

export class FavoritesStore {
  constructor(storage = resolveBrowserStorage()) {
    this.storage = storage;
    this.slugs = new Set(this.#read());
  }

  #read() {
    if (!this.storage) return [];
    try {
      return normalizeFavoriteSlugs(JSON.parse(this.storage.getItem(FAVORITES_STORAGE_KEY) || '[]'));
    } catch {
      return [];
    }
  }

  #persist() {
    if (!this.storage) return false;
    try {
      this.storage.setItem(FAVORITES_STORAGE_KEY, JSON.stringify(this.values()));
      return true;
    } catch {
      return false;
    }
  }

  has(slug) {
    return this.slugs.has(normalizeSlug(slug));
  }

  toggle(slug) {
    const normalized = normalizeSlug(slug);
    if (!normalized) return { isFavorite: false, persisted: false };

    const wasFavorite = this.slugs.has(normalized);
    if (wasFavorite) this.slugs.delete(normalized);
    else if (this.slugs.size < MAX_FAVORITES) this.slugs.add(normalized);

    return {
      isFavorite: this.slugs.has(normalized),
      persisted: this.#persist(),
    };
  }

  remove(slug) {
    const removed = this.slugs.delete(normalizeSlug(slug));
    if (removed) this.#persist();
    return removed;
  }

  reload() {
    this.slugs = new Set(this.#read());
    return this.values();
  }

  values() {
    return [...this.slugs];
  }

  get size() {
    return this.slugs.size;
  }
}

export function updateFavoriteButton(button, isFavorite) {
  button.setAttribute('aria-pressed', String(isFavorite));
  button.setAttribute('aria-label', isFavorite ? 'Quitar de favoritos' : 'Guardar en favoritos');
  button.title = isFavorite ? 'Quitar de favoritos' : 'Guardar en favoritos';
  button.textContent = isFavorite ? '♥' : '♡';
}

export function updateFavoriteCounts(store, root = document) {
  for (const counter of root.querySelectorAll('[data-favorites-count]')) {
    counter.textContent = String(store.size);
    counter.hidden = store.size === 0;
  }
}
