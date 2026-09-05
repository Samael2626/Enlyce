import { PublicCatalogClient, resolveApiBase } from './api.js';
import { FavoritesStore, FAVORITES_STORAGE_KEY, updateFavoriteCounts } from './favorites.js';
import { buildNeighborhoodFilters } from './neighborhood-query.js';
import { createPropertyCard } from './property-card.js';

const params = new URLSearchParams(window.location.search);
const neighborhood = document.body.dataset.neighborhood;
const municipality = document.body.dataset.municipality;
const client = new PublicCatalogClient(resolveApiBase(window.location.search));
const favorites = new FavoritesStore();
const grid = document.querySelector('[data-property-grid]');
const template = document.querySelector('#property-card-template');
const loading = document.querySelector('[data-loading]');
const empty = document.querySelector('[data-empty]');
const errorPanel = document.querySelector('[data-error]');
const results = document.querySelector('[data-results]');

function propertyHref(slug) {
  const next = new URLSearchParams({ slug });
  if (params.has('api')) next.set('api', params.get('api'));
  if (params.has('whatsapp')) next.set('whatsapp', params.get('whatsapp'));
  return `../inmueble.html?${next}`;
}

function catalogHref() {
  const next = new URLSearchParams({ municipality, neighborhood });
  if (params.has('api')) next.set('api', params.get('api'));
  return `../index.html?${next}`;
}

function showState(state) {
  loading.hidden = state !== 'loading';
  empty.hidden = state !== 'empty';
  errorPanel.hidden = state !== 'error';
  results.hidden = state !== 'results';
}

async function loadProperties() {
  showState('loading');
  try {
    const page = await client.getProperties(buildNeighborhoodFilters(neighborhood, municipality));
    if (!page.items.length) {
      showState('empty');
      return;
    }

    grid.replaceChildren(...page.items.map((property) => createPropertyCard({
      property,
      template,
      href: propertyHref(property.slug),
      favorites,
      onFavoriteChange: () => updateFavoriteCounts(favorites),
    })));
    document.querySelector('[data-total]').textContent = new Intl.NumberFormat('es-CO').format(page.total);
    document.querySelector('[data-catalog-link]').href = catalogHref();
    showState('results');
  } catch (error) {
    document.querySelector('[data-error-message]').textContent = error.message || 'No pudimos consultar el inventario.';
    showState('error');
  }
}

document.querySelector('[data-retry]').addEventListener('click', loadProperties);
document.querySelector('[data-empty-catalog]').href = catalogHref();
updateFavoriteCounts(favorites);
window.addEventListener('storage', (event) => {
  if (event.key !== FAVORITES_STORAGE_KEY) return;
  favorites.reload();
  updateFavoriteCounts(favorites);
  loadProperties();
});
loadProperties();
