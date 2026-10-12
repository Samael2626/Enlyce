import AxeBuilder from "@axe-core/playwright"
import { expect, test } from "@playwright/test"

test("comparador: elige favoritos y resalta el mejor valor", async ({ page }) => {
  await page.goto("/")
  await page.evaluate(() => localStorage.setItem("lyc:favoritos", JSON.stringify([
    { slug: "apartamento-laureles", title: "Apartamento amplio en Laureles" },
    { slug: "apartamento-estadio", title: "Apartamento cerca al Estadio" },
  ])))
  await page.goto("/favoritos")

  await page.getByRole("checkbox", { name: "Comparar Apartamento amplio en Laureles" }).check()
  await page.getByRole("checkbox", { name: "Comparar Apartamento cerca al Estadio" }).check()
  await page.getByRole("link", { name: "Comparar (2)" }).click()

  await expect(page.getByRole("heading", { name: "Comparar inmuebles" })).toBeVisible()
  const priceRow = page.getByRole("row", { name: /^Precio \$/ })
  await expect(priceRow.locator('td[data-best="true"]')).toContainText("380.000.000")
})

// Paginas publicas clave sin violaciones WCAG A/AA detectables automaticamente.
for (const path of ["/", "/inmuebles", "/inmuebles/apartamento-laureles", "/favoritos", "/contacto", "/propietarios", "/zonas"]) {
  test(`accesibilidad WCAG AA en ${path}`, async ({ page }) => {
    await page.goto(path)
    await page.waitForLoadState("networkidle")
    const results = await new AxeBuilder({ page })
      .withTags(["wcag2a", "wcag2aa", "wcag21a", "wcag21aa"])
      .exclude(".leaflet-container")
      .analyze()
    const summary = results.violations.map((violation) =>
      `${violation.id} (${violation.impact}): ${violation.nodes.slice(0, 3).map((node) => node.target.join(" ")).join(" | ")}`)
    expect(summary).toEqual([])
  })
}
