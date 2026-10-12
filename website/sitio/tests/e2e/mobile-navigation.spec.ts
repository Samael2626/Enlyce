import { expect, test } from "@playwright/test"

test.use({ viewport: { width: 390, height: 844 } })

test("mobile menu opens, navigates and closes; hero logo does not cover the title", async ({ page }) => {
  await page.goto("/")
  const nav = page.getByRole("navigation", { name: "Principal" })
  await expect(nav).toBeHidden()

  const logo = await page.locator(".hero-brand-mark").boundingBox()
  const title = await page.getByRole("heading", { level: 1 }).boundingBox()
  expect(logo!.y + logo!.height).toBeLessThanOrEqual(title!.y + 20)

  await page.getByRole("button", { name: "Abrir menú" }).click()
  await expect(nav).toBeVisible()
  await nav.getByRole("link", { name: "Inmuebles" }).click()
  await expect(page).toHaveURL(/\/inmuebles/)
  await expect(nav).toBeHidden()
})

test("active filters show as removable chips", async ({ page }) => {
  await page.goto("/inmuebles?operation=Arriendo&bedrooms=2")
  const chips = page.getByRole("group", { name: "Filtros activos" })
  await expect(chips.getByRole("button", { name: "Quitar filtro Arriendo" })).toBeVisible()
  await chips.getByRole("button", { name: "Quitar filtro 2+ habitaciones" }).click()
  await expect(page).toHaveURL(/operation=Arriendo/)
  await expect(page).not.toHaveURL(/bedrooms/)
})
