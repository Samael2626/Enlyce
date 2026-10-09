import { createServer } from "node:http";

const port = Number(process.env.E2E_API_PORT ?? 5198);
const requests = [];

const property = {
  id: "2b76d76d-8ca4-4b34-8ce4-0f6040dc514f",
  slug: "apartamento-laureles",
  publicTitle: "Apartamento amplio en Laureles",
  propertyType: "Apartment",
  operation: "Sale",
  price: { amount: 420000000, currency: "COP" },
  location: {
    municipality: "Medellín",
    neighborhood: "Laureles",
    approximateLatitude: 6.2442,
    approximateLongitude: -75.5937,
  },
  areaSquareMeters: 92,
  bedrooms: 3,
  bathrooms: 2,
  parkingSpaces: 1,
  coverPhoto: { url: "/brochure/interior-lujo.jpg", altText: "Sala del apartamento" },
  publishedAt: "2026-01-15T12:00:00Z",
};

const detail = {
  id: property.id,
  slug: property.slug,
  publicTitle: property.publicTitle,
  publicDescription: "Apartamento iluminado en el sector de Laureles.",
  propertyType: property.propertyType,
  operation: property.operation,
  price: property.price,
  administrationFee: { amount: 350000, currency: "COP" },
  location: property.location,
  features: {
    areaSquareMeters: property.areaSquareMeters,
    bedrooms: property.bedrooms,
    bathrooms: property.bathrooms,
    parkingSpaces: property.parkingSpaces,
    stratum: 5,
    amenities: ["Balcón", "Ascensor"],
  },
  photos: [{ url: "/brochure/interior-lujo.jpg", altText: "Sala del apartamento", order: 0, isCover: true }],
  advisor: {
    id: "6e4f8517-8e81-467a-b487-1acfe7af7d96",
    displayName: "Asesor de prueba",
    publicPhone: null,
  },
  publishedAt: property.publishedAt,
};

function sendJson(response, status, body) {
  response.writeHead(status, {
    "Content-Type": "application/json; charset=utf-8",
    "Access-Control-Allow-Origin": "http://127.0.0.1:3301",
    "Access-Control-Allow-Methods": "GET, POST, PUT, DELETE, OPTIONS",
    "Access-Control-Allow-Headers": "Content-Type, Accept",
  });
  response.end(JSON.stringify(body));
}

async function readJson(request) {
  let raw = "";
  for await (const chunk of request) raw += chunk;
  if (!raw) return null;
  try {
    return JSON.parse(raw);
  } catch {
    return { _invalidJson: raw };
  }
}

const server = createServer(async (request, response) => {
  const url = new URL(request.url ?? "/", `http://${request.headers.host ?? "localhost"}`);
  const { method } = request;

  if (method === "OPTIONS") {
    response.writeHead(204, {
      "Access-Control-Allow-Origin": "http://127.0.0.1:3301",
      "Access-Control-Allow-Methods": "GET, POST, PUT, DELETE, OPTIONS",
      "Access-Control-Allow-Headers": "Content-Type, Accept",
    });
    return response.end();
  }

  if (method === "GET" && url.pathname === "/health") {
    return sendJson(response, 200, { status: "ok" });
  }

  if (method === "GET" && url.pathname === "/api/public/inmuebles") {
    const page = Math.max(1, Number.parseInt(url.searchParams.get("page") ?? "1", 10) || 1);
    const pageSize = Math.max(1, Number.parseInt(url.searchParams.get("pageSize") ?? "12", 10) || 12);
    if (url.searchParams.get("neighborhood") === "MapaTest") {
      const fixtures = [
        property,
        {
          ...property,
          id: "8a6e7d15-65b4-45eb-8d25-c0fba0912fc2",
          slug: "casa-belén",
          publicTitle: "Casa de prueba en Belén",
          location: { ...property.location, neighborhood: "Belén", approximateLatitude: 6.231, approximateLongitude: -75.601 },
        },
        {
          ...property,
          id: "6ea787e7-0265-432b-a6a9-c7f90e94b48e",
          slug: "sin-coordenadas",
          publicTitle: "Publicación sin coordenadas",
          location: { ...property.location, neighborhood: "Centro", approximateLatitude: 0, approximateLongitude: 0 },
        },
      ];
      const items = page === 1 ? fixtures.slice(0, pageSize) : [];
      return sendJson(response, 200, { page, pageSize, total: fixtures.length, items });
    }
    const items = page === 1 ? [property].slice(0, pageSize) : [];
    return sendJson(response, 200, { page, pageSize, total: 1, items });
  }

  if (method === "GET" && url.pathname === "/api/public/inmuebles/apartamento-laureles") {
    return sendJson(response, 200, detail);
  }

  if (method === "GET" && url.pathname === "/api/politica/activa") {
    return sendJson(response, 200, {
      id: "7ec2b6db-51bc-40c6-84bd-4b718b4756b0",
      version: "fixture-1",
      textoCompleto: "Politica de prueba para ejecucion E2E.",
      fechaVigencia: "2026-01-01T00:00:00Z",
    });
  }

  if ((method === "POST" && url.pathname === "/api/leads") ||
      (method === "PUT" && url.pathname === "/api/leads/owner-details")) {
    const body = await readJson(request);
    requests.push({ method, path: url.pathname, body });
    if (method === "POST") {
      return sendJson(response, 202, {
        continuationToken: "e2e-fixture-continuation-token",
        message: "Solicitud recibida.",
      });
    }
    return sendJson(response, 202, {});
  }

  if (method === "GET" && url.pathname === "/__test/requests") {
    return sendJson(response, 200, requests);
  }

  if (method === "DELETE" && url.pathname === "/__test/requests") {
    requests.length = 0;
    return sendJson(response, 200, { cleared: true });
  }

  return sendJson(response, 404, { title: "Not Found", status: 404 });
});

server.listen(port, "127.0.0.1", () => {
  console.log(`E2E mock API listening on http://127.0.0.1:${port}`);
});
