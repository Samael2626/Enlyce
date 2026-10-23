import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { test } from "node:test";
import { runInNewContext } from "node:vm";

const root = new URL("../", import.meta.url);
const source = readFileSync(new URL("public/sw.js", root), "utf8");

function worker(fetchImpl = async () => ({ status: 200 })) {
  const listeners = {};
  const added = [];
  const cached = { marker: "offline" };
  const cache = {
    add: async (path) => added.push(path),
    match: async (path) => path === "/offline.html" ? cached : undefined,
  };
  const storage = {
    open: async () => cache,
    keys: async () => [],
    delete: async () => true,
  };
  const scope = {
    location: { origin: "https://crm.example" },
    clients: { claim: async () => {} },
    skipWaiting: async () => {},
    addEventListener: (name, handler) => { listeners[name] = handler; },
  };
  runInNewContext(source, { self: scope, caches: storage, fetch: fetchImpl, URL, Response });
  return { listeners, added, cached };
}

test("precachea solo la pagina publica sin conexion", async () => {
  const { listeners, added } = worker();
  let install;
  listeners.install({ waitUntil: (promise) => { install = promise; } });
  await install;
  assert.deepEqual(added, ["/offline.html"]);
});

test("navegacion del CRM usa la red y muestra fallback si falla", async () => {
  const live = { marker: "network" };
  const online = worker(async () => live);
  let response;
  online.listeners.fetch({
    request: { method: "GET", mode: "navigate", url: "https://crm.example/contactos" },
    respondWith: (promise) => { response = promise; },
  });
  assert.equal(await response, live);

  const offline = worker(async () => { throw new Error("network unavailable"); });
  offline.listeners.fetch({
    request: { method: "GET", mode: "navigate", url: "https://crm.example/contactos" },
    respondWith: (promise) => { response = promise; },
  });
  assert.equal(await response, offline.cached);
  assert.deepEqual(offline.added, []);
});

test("API, media, otras solicitudes y otros origenes no son interceptados", () => {
  const { listeners } = worker();
  for (const [method, mode, url] of [
    ["GET", "navigate", "https://crm.example/api/contactos"],
    ["GET", "navigate", "https://crm.example/media/private.jpg"],
    ["GET", "cors", "https://crm.example/api/contactos"],
    ["POST", "navigate", "https://crm.example/login"],
    ["GET", "navigate", "https://other.example/dashboard"],
  ]) {
    listeners.fetch({
      request: { method, mode, url },
      respondWith: () => assert.fail(`Intercepted ${url}`),
    });
  }
});

test("manifiesto enlazado declara iconos reales y pantalla inicial del CRM", () => {
  const manifest = JSON.parse(readFileSync(new URL("public/manifest.json", root), "utf8"));
  const html = readFileSync(new URL("index.html", root), "utf8");
  assert.match(html, /rel="manifest" href="\/manifest.json"/);
  assert.equal(manifest.start_url, "/dashboard");
  assert.equal(manifest.display, "standalone");
  for (const size of [192, 512]) {
    const icon = manifest.icons.find((item) => item.sizes === `${size}x${size}`);
    assert.ok(icon);
    const png = readFileSync(new URL(`public${icon.src}`, root));
    assert.equal(png.toString("hex", 0, 8), "89504e470d0a1a0a");
    assert.equal(png.readUInt32BE(16), size);
    assert.equal(png.readUInt32BE(20), size);
  }
});
