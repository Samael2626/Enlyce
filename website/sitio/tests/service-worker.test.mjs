import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { test } from "node:test";
import vm from "node:vm";
import { fileURLToPath } from "node:url";

const script = readFileSync(join(fileURLToPath(new URL(".", import.meta.url)), "../public/sw.js"), "utf8");

function createWorker(network) {
  const listeners = {};
  const cached = { url: "/offline.html" };
  const cache = {
    addAll: async (urls) => assert.deepEqual(Array.from(urls), ["/offline.html", "/lyc-logo.png"]),
    match: async (url) => url === "/offline.html" ? cached : undefined,
  };
  const deleted = [];
  const context = {
    self: {
      location: { origin: "https://example.com" },
      clients: { claim: async () => undefined },
      skipWaiting: async () => undefined,
      addEventListener: (type, listener) => { listeners[type] = listener; },
    },
    caches: {
      open: async () => cache,
      keys: async () => ["lyc-public-offline-old", "other-app-cache"],
      delete: async (name) => { deleted.push(name); return true; },
    },
    fetch: network,
    URL,
    Promise,
    Response,
  };
  vm.runInNewContext(script, context);
  return { listeners, deleted, cached };
}

test("instala fallback y limpia solo caches anteriores propios", async () => {
  const { listeners, deleted } = createWorker(async () => ({}));
  let installation;
  listeners.install({ waitUntil: (promise) => { installation = promise; } });
  await installation;
  let activation;
  listeners.activate({ waitUntil: (promise) => { activation = promise; } });
  await activation;
  assert.deepEqual(deleted, ["lyc-public-offline-old"]);
});

test("navegacion usa red y muestra fallback sin conexion", async () => {
  const online = { url: "/catalogo" };
  const worker = createWorker(async () => online);
  const request = { method: "GET", mode: "navigate", url: "https://example.com/catalogo" };
  let response;
  worker.listeners.fetch({ request, respondWith: (promise) => { response = promise; } });
  assert.equal(await response, online);

  const offlineWorker = createWorker(async () => { throw new Error("offline"); });
  offlineWorker.listeners.fetch({ request, respondWith: (promise) => { response = promise; } });
  assert.equal(await response, offlineWorker.cached);
});

test("no intercepta API, otros origenes ni peticiones que no son navegacion", () => {
  const { listeners } = createWorker(async () => ({}));
  const ignored = [
    { method: "POST", mode: "navigate", url: "https://example.com/contacto" },
    { method: "GET", mode: "cors", url: "https://example.com/api/propiedades" },
    { method: "GET", mode: "navigate", url: "https://api.example.com/contacto" },
  ];
  for (const request of ignored) {
    listeners.fetch({ request, respondWith: () => assert.fail("intercepted") });
  }
});
