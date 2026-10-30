import AxeBuilder from "@axe-core/playwright"
import { expect, test, type Page } from "@playwright/test"

// Auditoria WCAG 2.1 AA: todas las rutas publicas, escritorio y movil, mas
// comprobaciones de teclado que axe no puede detectar por si solo.
const routes = [
  "/",
  "/inmuebles",
  "/inmuebles/apartamento-laureles",
  "/zonas",
  "/zonas/medellin",
  "/favoritos",
  "/comparar?slugs=apartamento-laureles,apartamento-estadio",
  "/contacto",
  "/propietarios",
  "/propietario",
  "/nosotros",
  "/privacidad",
  "/brochure",
  "/alertas/baja?id=x&token=y",
]

const viewports = [
  { name: "escritorio", width: 1280, height: 800 },
  { name: "movil", width: 390, height: 844 },
]

async function axeViolations(page: Page) {
  const results = await new AxeBuilder({ page })
    .withTags(["wcag2a", "wcag2aa", "wcag21a", "wcag21aa"])
    .exclude(".leaflet-container")
    .analyze()
  return results.violations.map((violation) =>
    `${violation.id} (${violation.impact}): ${violation.nodes.slice(0, 3).map((node) => node.target.join(" ")).join(" | ")}`)
}

for (const viewport of viewports) {
  test.describe(`axe ${viewport.name}`, () => {
    test.use({ viewport: { width: viewport.width, height: viewport.height } })
    for (const route of routes) {
      test(`sin violaciones en ${route}`, async ({ page }) => {
        await page.goto(route)
        await page.waitForLoadState("networkidle")
        expect(await axeViolations(page)).toEqual([])
      })
    }
  })
}

test("el enlace de salto es el primer foco y lleva al contenido", async ({ page }) => {
  await page.goto("/")
  await page.keyboard.press("Tab")
  const skip = page.getByRole("link", { name: "Saltar al contenido" })
  await expect(skip).toBeFocused()
  await expect(skip).toBeInViewport()
  await skip.press("Enter")
  await expect(page).toHaveURL(/#contenido$/)
})

// Cada elemento enfocable con Tab debe mostrar un indicador de foco visible (WCAG 2.4.7).
for (const route of ["/", "/inmuebles", "/inmuebles/apartamento-laureles", "/contacto", "/propietarios"]) {
  test(`foco visible al tabular en ${route}`, async ({ page }) => {
    await page.goto(route)
    await page.waitForLoadState("networkidle")
    const withoutFocus: string[] = []
    for (let step = 0; step < 40; step++) {
      await page.keyboard.press("Tab")
      const info = await page.evaluate(() => {
        const el = document.activeElement as HTMLElement | null
        if (!el || el === document.body) return null
        // El overlay de desarrollo de Next no existe en produccion.
        if (el.tagName === "NEXTJS-PORTAL") return { label: "nextjs-portal", visible: true }
        if (el.closest(".leaflet-container")) return { label: "leaflet", visible: true }
        const hasIndicator = (node: Element) => {
          const style = getComputedStyle(node)
          const outline = style.outlineStyle !== "none" && parseFloat(style.outlineWidth) > 0
          return outline || style.boxShadow !== "none"
        }
        // Se acepta el anillo en el contenedor inmediato (patron :focus-within).
        const visible = hasIndicator(el) || (el.parentElement !== null && el.parentElement.matches(":focus-within") && hasIndicator(el.parentElement))
        const label = `${el.tagName.toLowerCase()} "${(el.getAttribute("aria-label") ?? el.textContent ?? "").trim().slice(0, 40)}"`
        return { label, visible }
      })
      if (!info) break
      if (!info.visible) withoutFocus.push(info.label)
    }
    expect(withoutFocus).toEqual([])
  })
}
