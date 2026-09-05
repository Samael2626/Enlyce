import { ApiError, PublicCatalogClient, resolveApiBase } from './api.js';
import { FavoritesStore, FAVORITES_STORAGE_KEY, updateFavoriteCounts } from './favorites.js';
import { createPropertyCard } from './property-card.js';

const params = new URLSearchParams(window.location.search);
const client = new PublicCatalogClient(resolveApiBase(window.location.search));
const favorites = new FavoritesStore();
const grid = document.querySelector('[data-property-grid]');
const template = document.querySelector('#property-card-template');
const loading = document.querySelector('[data-loading]');
const empty = document.querySelector('[data-empty]');
const errorPanel = document.querySelector('[data-error]');
const results = document.querySelector('[data-results]');
const unavailable = document.querySelector('[data-unavailable]');

function propertyHref(slug) {
  const next = new URLSearchParams({ slug });
  if (params.has('api')) next.set('api', params.get('api'));
  if (params.has('whatsapp')) next.set('whatsapp', params.get('whatsapp'));
  return `inmueble.html?${next}`;
}

function showState(state) {
  loading.hidden = state !== 'loading';
  empty.hidden = state !== 'empty';
  errorPanel.hidden = state !== 'error';
  results.hidden = state !== 'results';
}

function renderUnavailable(slugs) {
  unavailable.hidden = slugs.length === 0;
  unavailable.replaceChildren(...slugs.map((slug) => {
    const item = document.createElement('li');
    const label = document.createElement('span');
    const remove = document.createElement('button');
    label.textContent = `La referencia ${slug} ya no está disponible.`;
    remove.type = 'button';
    remove.textContent = 'Quitar';
    remove.addEventListener('click', () => {
      favorites.remove(slug);
      loadFavorites();
    });
    item.append(label, remove);
    return item;
  }));
}

async function loadFavorites() {
  favorites.reload();
  updateFavoriteCounts(favorites);
  const slugs = favorites.values();
  if (!slugs.length) {
    grid.replaceChildren();
    renderUnavailable([]);
    showState('empty');
    return;
  }

  showState('loading');
  const settled = await Promise.allSettled(slugs.map((slug) => client.getProperty(slug)));
  const properties = [];
  const unavailableSlugs = [];
  let connectionError;

  settled.forEach((result, index) => {
    if (result.status === 'fulfilled') properties.push(result.value);
    else if (result.reason instanceof ApiError && result.reason.status === 404) unavailableSlugs.push(slugs[index]);
    else connectionError ??= result.reason;
  });

  if (connectionError && !properties.length) {
    document.querySelector('[data-error-message]').textContent = connectionError.message || 'No pudimos cargar tus favoritos.';
    showState('error');
    return;
  }

  grid.replaceChildren(...properties.map((property) => createPropertyCard({
    property,
    template,
    href: propertyHref(property.slug),
    favorites,
    onFavoriteChange: () => loadFavorites(),
  })));
  document.querySelector('[data-total]').textContent = String(properties.length);
  renderUnavailable(unavailableSlugs);
  showState('results');
}

document.querySelector('[data-retry]').addEventListener('click', loadFavorites);
window.addEventListener('storage', (event) => {
  if (event.key === FAVORITES_STORAGE_KEY) loadFavorites();
});

loadFavorites();
