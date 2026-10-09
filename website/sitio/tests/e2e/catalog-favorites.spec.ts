import { expect, test } from "@playwright/test"

test("catalogue opens a property and favorites persist across pages", async ({ page }) => {
  await page.goto("/inmuebles")

  await expect(page.getByRole("heading", { name: "Propiedades para vivir e invertir" })).toBeVisible()
  await page.getByRole("link", { name: "Apartamento amplio en Laureles" }).click()

  await expect(page).toHaveURL(/\/inmuebles\/apartamento-laureles$/)
  await expect(page.getByRole("heading", { name: "Apartamento amplio en Laureles" })).toBeVisible()
  await page.getByRole("button", { name: "Guardar" }).click()
  await expect(page.getByRole("button", { name: "Guardado" })).toHaveAttribute("aria-pressed", "true")

  await page.getByRole("link", { name: "Favoritos" }).click()
  await expect(page.getByRole("heading", { name: "Tus favoritos" })).toBeVisible()
  await expect(page.getByRole("link", { name: "Apartamento amplio en Laureles" })).toBeVisible()
})
