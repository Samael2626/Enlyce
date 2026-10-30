// Medicion de laboratorio de Core Web Vitals (LCP, CLS, INP) sobre una build
// de produccion. Uso: node tests/perf/web-vitals.mjs [baseUrl]
// Requiere `npm run build && npm start` (o equivalente) ya levantado.
import { chromium } from "playwright"

const baseUrl = process.argv[2] ?? "http://127.0.0.1:3000"
const executablePath = process.env.CHROMIUM_PATH || undefined

const routes = [
  "/",
  "/inmuebles",
  "/inmuebles/apartamento-laureles",
  "/zonas",
  "/zonas/medellin",
  "/contacto",
  "/propietarios",
  "/nosotros",
  "/brochure",
]

// Umbrales "bueno" de web.dev.
const limits = { lcp: 2500, cls: 0.1, inp: 200 }

const profiles = {
  movil: {
    viewport: { width: 390, height: 844 },
    isMobile: true,
    hasTouch: true,
    cpu: 4,
    // Red 4G lenta, igual que el perfil movil de Lighthouse.
    network: { latency: 150, downloadThroughput: (1.6 * 1024 * 1024) / 8, uploadThroughput: (750 * 1024) / 8 },
  },
  escritorio: {
    viewport: { width: 1350, height: 940 },
    isMobile: false,
    hasTouch: false,
    cpu: 1,
    network: { latency: 40, downloadThroughput: (10 * 1024 * 1024) / 8, uploadThroughput: (10 * 1024 * 1024) / 8 },
  },
}

// Observadores inyectados antes de cargar cualquier script de la pagina.
const observerScript = () => {
  window.__vitals = { lcp: 0, lcpElement: "", cls: 0, inp: 0 }
  new PerformanceObserver((list) => {
    for (const entry of list.getEntries()) {
      window.__vitals.lcp = entry.startTime
      const el = entry.element
      window.__vitals.lcpElement = el ? `${el.tagName.toLowerCase()}${el.className ? "." + String(el.className).split(" ")[0] : ""}` : ""
    }
  }).observe({ type: "largest-contentful-paint", buffered: true })
  new PerformanceObserver((list) => {
    for (const entry of list.getEntries()) {
      if (!entry.hadRecentInput) window.__vitals.cls += entry.value
    }
  }).observe({ type: "layout-shift", buffered: true })
  new PerformanceObserver((list) => {
    for (const entry of list.getEntries()) {
      if (entry.interactionId) window.__vitals.inp = Math.max(window.__vitals.inp, entry.duration)
    }
  }).observe({ type: "event", buffered: true, durationThreshold: 16 })
}

async function measure(browser, profileName, route) {
  const profile = profiles[profileName]
  const context = await browser.newContext({ viewport: profile.viewport, isMobile: profile.isMobile, hasTouch: profile.hasTouch })
  const page = await context.newPage()
  const cdp = await context.newCDPSession(page)
  await cdp.send("Network.enable")
  await cdp.send("Network.emulateNetworkConditions", { offline: false, ...profile.network })
  await cdp.send("Emulation.setCPUThrottlingRate", { rate: profile.cpu })
  await page.addInitScript(observerScript)

  await page.goto(baseUrl + route, { waitUntil: "load", timeout: 90_000 })
  await page.waitForTimeout(1500)
  // Desplazamiento para capturar cambios de diseno tardios (imagenes, fuentes).
  await page.mouse.wheel(0, 1200)
  await page.waitForTimeout(800)

  // Interacciones representativas para INP: abrir menu o pulsar el primer boton visible.
  const target = page.locator("main button:visible, header button:visible").first()
  if (await target.count()) {
    await target.click({ timeout: 5000 }).catch(() => {})
    await page.waitForTimeout(500)
  }
  await page.keyboard.press("Tab")
  await page.waitForTimeout(500)

  const vitals = await page.evaluate(() => window.__vitals)
  await context.close()
  return vitals
}

const browser = await chromium.launch({ executablePath })
const rows = []
let failures = 0
for (const profileName of Object.keys(profiles)) {
  for (const route of routes) {
    const v = await measure(browser, profileName, route)
    const ok = v.lcp <= limits.lcp && v.cls <= limits.cls && v.inp <= limits.inp
    if (!ok) failures++
    rows.push({
      perfil: profileName,
      ruta: route,
      "LCP ms": Math.round(v.lcp),
      "elemento LCP": v.lcpElement,
      CLS: Number(v.cls.toFixed(3)),
      "INP ms": Math.round(v.inp),
      estado: ok ? "bueno" : "revisar",
    })
  }
}
await browser.close()
console.table(rows)
console.log(`Umbrales: LCP <= ${limits.lcp} ms, CLS <= ${limits.cls}, INP <= ${limits.inp} ms. Rutas a revisar: ${failures}`)
process.exitCode = failures ? 1 : 0
