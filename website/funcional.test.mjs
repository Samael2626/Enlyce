import assert from 'node:assert/strict';
import test from 'node:test';

import {
  ApiError,
  PublicCatalogClient,
  buildCatalogUrl,
  buildDetailUrl,
  normalizeProblem,
} from './funcional/js/api.js';
import {
  formatCurrency,
  formatPropertyFacts,
  safeMediaUrl,
} from './funcional/js/format.js';
import { buildOwnerWhatsAppUrl, buildWhatsAppUrl, resolveWhatsAppNumber } from './funcional/js/contact.js';
import { buildOwnerLeadPayload } from './funcional/js/owner-inquiry.js';
import { FavoritesStore, MAX_FAVORITES, normalizeFavoriteSlugs } from './funcional/js/favorites.js';
import { toPropertyCardModel } from './funcional/js/property-card.js';
import { buildNeighborhoodFilters } from './funcional/js/neighborhood-query.js';

function createMemoryStorage(initial = {}) {
  const values = new Map(Object.entries(initial));
  return {
    getItem: (key) => values.get(key) ?? null,
    setItem: (key, value) => values.set(key, String(value)),
    removeItem: (key) => values.delete(key),
  };
}

test('buildCatalogUrl serializa solo filtros con valor', () => {
  const url = buildCatalogUrl('http://localhost:5019/', {
    page: 2,
    pageSize: 12,
    operation: 'Venta',
    propertyType: '',
    municipality: 'Medellín',
    minPrice: null,
  });

  assert.equal(
    url,
    'http://localhost:5019/api/public/inmuebles?page=2&pageSize=12&operation=Venta&municipality=Medell%C3%ADn',
  );
});

test('buildDetailUrl codifica el slug', () => {
  assert.equal(
    buildDetailUrl('http://localhost:5019', 'apartamento laureles'),
    'http://localhost:5019/api/public/inmuebles/apartamento%20laureles',
  );
});

test('normalizeProblem conserva errores de validacion', () => {
  assert.deepEqual(
    normalizeProblem({ title: 'Consulta inválida', errors: { page: ['Debe ser mayor que cero.'] } }),
    {
      title: 'Consulta inválida',
      messages: ['Debe ser mayor que cero.'],
    },
  );
});

test('formatCurrency usa pesos colombianos sin decimales', () => {
  assert.equal(formatCurrency({ amount: 620000000, currency: 'COP' }), '$ 620.000.000');
});

test('formatPropertyFacts crea resumen compacto', () => {
  assert.equal(
    formatPropertyFacts({ areaSquareMeters: 92, bedrooms: 3, bathrooms: 2, parkingSpaces: 1 }),
    '92 m² · 3 hab. · 2 baños · 1 parq.',
  );
});

test('safeMediaUrl bloquea protocolos inseguros', () => {
  assert.equal(safeMediaUrl('javascript:alert(1)'), '');
  assert.equal(safeMediaUrl('https://media.example.test/a.webp'), 'https://media.example.test/a.webp');
});

test('resolveWhatsAppNumber acepta configuracion o parametro temporal', () => {
  assert.equal(resolveWhatsAppNumber('?whatsapp=%2B57+300+123+4567', ''), '573001234567');
  assert.equal(resolveWhatsAppNumber('', '57 301 765 4321'), '573017654321');
});

test('buildWhatsAppUrl crea mensaje contextual del inmueble', () => {
  assert.equal(
    buildWhatsAppUrl('573001234567', { publicTitle: 'Apartamento Laureles', slug: 'apartamento-laureles' }),
    'https://wa.me/573001234567?text=Hola%2C%20quiero%20informaci%C3%B3n%20sobre%20Apartamento%20Laureles%20(Ref.%20apartamento-laureles).',
  );
  assert.equal(buildWhatsAppUrl('', { publicTitle: 'Casa', slug: 'casa' }), '');
});

test('buildOwnerLeadPayload envia administracion como dato estructurado', () => {
  assert.deepEqual(buildOwnerLeadPayload({
    name: 'Laura Gómez',
    email: 'laura@example.test',
    phone: '3001234567',
    ownerService: 'Administrar',
    propertyType: 'Apartamento',
    municipality: 'Medellín',
    neighborhood: 'Belén|Rosales',
    consent: true,
  }), {
    nombre: 'Laura Gómez',
    email: 'laura@example.test',
    telefono: '3001234567',
    fuente: 'WebsiteOwner:Apartamento:Medellín:Belén Rosales',
    autorizacionDatos: true,
    tipoOperacion: 'Arriendo',
    ownerService: 'Manage',
  });
});

test('buildOwnerLeadPayload distingue arrendar de administrar', () => {
  const baseInput = {
    name: 'Laura Gómez',
    email: 'laura@example.test',
    phone: '3001234567',
    propertyType: 'Apartamento',
    municipality: 'Medellín',
    neighborhood: 'Belén',
    consent: true,
  };

  assert.equal(buildOwnerLeadPayload({ ...baseInput, ownerService: 'Arrendar' }).ownerService, 'Rent');
  assert.equal(buildOwnerLeadPayload({ ...baseInput, ownerService: 'Administrar' }).ownerService, 'Manage');
});

test('buildOwnerLeadPayload rechaza un servicio desconocido', () => {
  assert.throws(
    () => buildOwnerLeadPayload({ ownerService: 'Hipoteca' }),
    /servicio no válido/i,
  );
});

test('buildOwnerWhatsAppUrl crea mensaje para administracion', () => {
  assert.equal(
    buildOwnerWhatsAppUrl('573001234567', 'Administrar'),
    'https://wa.me/573001234567?text=Hola%2C%20quiero%20informaci%C3%B3n%20para%20administrar%20mi%20inmueble%20con%20L%26C.',
  );
});

test('normalizeFavoriteSlugs limpia duplicados y limita almacenamiento', () => {
  const input = [' Casa-1 ', 'casa-1', ...Array.from({ length: 40 }, (_, index) => `prop-${index}`)];
  const result = normalizeFavoriteSlugs(input);
  assert.equal(result[0], 'casa-1');
  assert.equal(result.length, MAX_FAVORITES);
});

test('FavoritesStore persiste y alterna favoritos', () => {
  const storage = createMemoryStorage();
  const store = new FavoritesStore(storage);
  assert.deepEqual(store.toggle('Apartamento-Laureles'), { isFavorite: true, persisted: true });
  assert.equal(store.has('apartamento-laureles'), true);
  assert.deepEqual(new FavoritesStore(storage).values(), ['apartamento-laureles']);
  assert.deepEqual(store.toggle('apartamento-laureles'), { isFavorite: false, persisted: true });
});

test('FavoritesStore funciona en memoria cuando el almacenamiento no existe', () => {
  const store = new FavoritesStore(null);
  assert.deepEqual(store.toggle('casa-belen'), { isFavorite: true, persisted: false });
  assert.equal(store.has('casa-belen'), true);
});

test('toPropertyCardModel adapta la ficha detallada a tarjeta', () => {
  const result = toPropertyCardModel({
    slug: 'apartamento-laureles',
    features: { areaSquareMeters: 91, bedrooms: 3, bathrooms: 2, parkingSpaces: 1 },
    photos: [
      { url: 'https://example.test/secondary.webp', isCover: false },
      { url: 'https://example.test/cover.webp', isCover: true },
    ],
  });

  assert.equal(result.areaSquareMeters, 91);
  assert.equal(result.bedrooms, 3);
  assert.equal(result.coverPhoto.url, 'https://example.test/cover.webp');
});

test('buildNeighborhoodFilters fija una consulta estable por barrio', () => {
  assert.deepEqual(buildNeighborhoodFilters(' Laureles ', ' Medellín '), {
    page: 1,
    pageSize: 12,
    sort: 'publishedAtDesc',
    municipality: 'Medellín',
    neighborhood: 'Laureles',
  });
  assert.throws(() => buildNeighborhoodFilters('', 'Medellín'), /obligatorios/i);
});

test('PublicCatalogClient traduce respuesta Problem Details', async () => {
  const client = new PublicCatalogClient('http://localhost:5019', async () => new Response(
    JSON.stringify({ title: 'Inmueble no encontrado' }),
    { status: 404, headers: { 'content-type': 'application/problem+json' } },
  ));

  await assert.rejects(
    client.getProperty('retirado'),
    (error) => error instanceof ApiError
      && error.status === 404
      && error.problem.title === 'Inmueble no encontrado',
  );
});

test('PublicCatalogClient ejecuta fetch con el receptor global del navegador', async () => {
  const receiverAwareFetch = function () {
    assert.equal(this, globalThis);
    return Promise.resolve(new Response(JSON.stringify({ items: [], total: 0, page: 1, pageSize: 12 }), {
      status: 200,
      headers: { 'content-type': 'application/json' },
    }));
  };
  const client = new PublicCatalogClient('http://localhost:5019', receiverAwareFetch);

  const result = await client.getProperties({ page: 1, pageSize: 12 });

  assert.equal(result.total, 0);
});

test('PublicCatalogClient crea lead con JSON', async () => {
  let capturedRequest;
  const client = new PublicCatalogClient('http://localhost:5019', async (url, options) => {
    capturedRequest = { url, options };
    return new Response(JSON.stringify({ id: 'lead-1' }), {
      status: 201,
      headers: { 'content-type': 'application/json' },
    });
  });
  const payload = {
    nombre: 'Ana',
    email: 'ana@example.test',
    telefono: '3000000000',
    fuente: 'Website:apartamento-laureles',
    autorizacionDatos: true,
    tipoOperacion: 'Venta',
  };

  await client.createLead(payload);

  assert.equal(capturedRequest.url, 'http://localhost:5019/api/leads/');
  assert.equal(capturedRequest.options.method, 'POST');
  assert.deepEqual(JSON.parse(capturedRequest.options.body), payload);
});
