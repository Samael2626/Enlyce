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
import { buildWhatsAppUrl, resolveWhatsAppNumber } from './funcional/js/contact.js';

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
