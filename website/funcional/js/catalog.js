import { PublicCatalogClient, resolveApiBase } from './api.js';
import {
  formatCurrency,
  formatLocation,
  formatPropertyFacts,
  formatPublishedAt,
  safeMediaUrl,
} from './format.js';

const filterNames = [
  'operation', 'propertyType', 'municipality', 'neighborhood',
  'minPrice', 'maxPrice', 'minArea', 'maxArea',
  'bedrooms', 'bathrooms', 'parkingSpaces',
];

const search = new URLSearchParams(window.location.search);
const client = new PublicCatalogClient(resolveApiBase(window.location.search));
const quickSearchForm = document.querySelector('[data-search-form]');
const filterForm = document.querySelector('[data-filter-form]');
const sortSelect = document.querySelector('[data-sort]');
const resultsRegion = document.querySelector('[data-results-region]');
const results = document.querySelector('[data-results]');
const loading = document.querySelector('[data-loading]');
const errorPanel = document.querySelector('[data-error]');
const emptyPanel = document.querySelector('[data-empty]');
const grid = document.querySelector('[data-property-grid]');
const template = document.querySelector('#property-card-template');
let activeRequest;
let currentPage = positiveInteger(search.get('page'), 1);
let totalPages = 1;

function positiveInteger(value, fallback) {
  const parsed = Number.parseInt(value, 10);
  return Number.isInteger(parsed) && parsed > 0 ? parsed : fallback;
}

function readFilters() {
  const filters = {
    page: currentPage,
    pageSize: 12,
    sort: search.get('sort') || 'publishedAtDesc',
  };
  for (const name of filterNames) {
    const value = search.get(name);
    if (value) filters[name] = value;
  }
  return filters;
}

function fillForms() {
  for (const form of [quickSearchForm, filterForm]) {
    for (const element of form.elements) {
      if (element.name && search.has(element.name)) element.value = search.get(element.name);
    }
  }
  sortSelect.value = search.get('sort') || 'publishedAtDesc';
}

function navigateWithForm(form) {
  const next = new URLSearchParams(search);
  next.set('page', '1');
  for (const name of filterNames) {
    const control = form.elements.namedItem(name);
    if (!control) continue;
    const value = control.value.trim();
    if (value) next.set(name, value);
    else next.delete(name);
  }
  window.location.search = next.toString();
}

function clearFilters() {
  const next = new URLSearchParams();
  if (search.has('api')) next.set('api', search.get('api'));
  window.location.search = next.toString();
}

function propertyHref(slug) {
  const params = new URLSearchParams({ slug });
  if (search.has('api')) params.set('api', search.get('api'));
  return `inmueble.html?${params}`;
}

function renderProperty(property) {
  const card = template.content.firstElementChild.cloneNode(true);
  const href = propertyHref(property.slug);
  for (const link of card.querySelectorAll('[data-property-link]')) link.href = href;

  card.querySelector('[data-operation]').textContent = property.operation;
  card.querySelector('[data-location]').textContent = formatLocation(property.location);
  card.querySelector('[data-title]').textContent = property.publicTitle;
  card.querySelector('[data-price]').textContent = formatCurrency(property.price);
  card.querySelector('[data-facts]').textContent = formatPropertyFacts(property);
  card.querySelector('[data-published]').textContent = `Publicado ${formatPublishedAt(property.publishedAt)}`;

  const image = card.querySelector('[data-property-image]');
  const fallback = card.querySelector('[data-image-fallback]');
  const imageUrl = safeMediaUrl(property.coverPhoto?.url);
  if (imageUrl) {
    image.src = imageUrl;
    image.alt = property.coverPhoto.altText || property.publicTitle;
    fallback.hidden = true;
    image.addEventListener('error', () => {
      image.hidden = true;
      fallback.hidden = false;
    }, { once: true });
  } else {
    image.hidden = true;
  }

  return card;
}

function showState(state) {
  loading.hidden = state !== 'loading';
  errorPanel.hidden = state !== 'error';
  emptyPanel.hidden = state !== 'empty';
  results.hidden = state !== 'results';
  resultsRegion.ariaBusy = String(state === 'loading');
}

async function loadCatalog() {
  activeRequest?.abort();
  activeRequest = new AbortController();
  showState('loading');

  try {
    const page = await client.getProperties(readFilters(), activeRequest.signal);
    if (!page.items.length) {
      showState('empty');
      return;
    }

    grid.replaceChildren(...page.items.map(renderProperty));
    totalPages = Math.max(1, Math.ceil(page.total / page.pageSize));
    document.querySelector('[data-total]').textContent = new Intl.NumberFormat('es-CO').format(page.total);
    document.querySelector('[data-page-label]').textContent = `Página ${page.page} de ${totalPages}`;
    document.querySelector('[data-pagination-label]').textContent = `${page.page} / ${totalPages}`;
    document.querySelector('[data-previous]').disabled = page.page <= 1;
    document.querySelector('[data-next]').disabled = page.page >= totalPages;
    showState('results');
  } catch (error) {
    if (error.name === 'AbortError') return;
    document.querySelector('[data-error-message]').textContent = error.message || 'Comprueba que Enlyce.Api esté corriendo en el puerto 5019.';
    showState('error');
  }
}

function changePage(offset) {
  const nextPage = Math.min(totalPages, Math.max(1, currentPage + offset));
  if (nextPage === currentPage) return;
  search.set('page', String(nextPage));
  window.location.search = search.toString();
}

quickSearchForm.addEventListener('submit', (event) => {
  event.preventDefault();
  navigateWithForm(quickSearchForm);
});

filterForm.addEventListener('submit', (event) => {
  event.preventDefault();
  navigateWithForm(filterForm);
});

sortSelect.addEventListener('change', () => {
  search.set('sort', sortSelect.value);
  search.set('page', '1');
  window.location.search = search.toString();
});

document.querySelector('[data-previous]').addEventListener('click', () => changePage(-1));
document.querySelector('[data-next]').addEventListener('click', () => changePage(1));
document.querySelector('[data-retry]').addEventListener('click', loadCatalog);
document.querySelector('[data-clear-filters]').addEventListener('click', clearFilters);
document.querySelector('[data-empty-clear]').addEventListener('click', clearFilters);
document.querySelector('[data-filter-toggle]').addEventListener('click', (event) => {
  const expanded = event.currentTarget.getAttribute('aria-expanded') === 'true';
  event.currentTarget.setAttribute('aria-expanded', String(!expanded));
  document.querySelector('[data-filters]').classList.toggle('is-open', !expanded);
});

fillForms();
loadCatalog();
