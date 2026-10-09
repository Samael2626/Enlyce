import { expect, test } from "@playwright/test"

const apiUrl = "http://127.0.0.1:5019"

type PropertyPage = {
  items: Array<{ slug: string; publicTitle: string }>
}

test("sitio local navega catalogo, detalle y rutas de contacto; sitemap refleja API", async ({
  page,
  request,
}) => {
  await page.route("**/api/public/analytics/events", (route) =>
    route.fulfill({ status: 202, body: "" }),
  )

  const apiResponse = await request.get(`${apiUrl}/api/public/inmuebles`)
  expect(apiResponse.ok()).toBeTruthy()
  const catalog = (await apiResponse.json()) as PropertyPage
  expect(catalog.items.length).toBeGreaterThan(0)
  const property = catalog.items[0]

  await page.goto("/inmuebles")
  await expect(page.getByRole("heading", { name: "Propiedades para vivir e invertir" })).toBeVisible()
  const listing = page.getByRole("link", { name: property.publicTitle, exact: true })
  await expect(listing).toBeVisible()
  await listing.click()
  await expect(page).toHaveURL(new RegExp(`/inmuebles/${property.slug}$`))
  await expect(page.getByRole("heading", { name: property.publicTitle, exact: true })).toBeVisible()

  await page.goto("/contacto")
  await expect(page.getByRole("heading", { name: "Contacto", exact: true })).toBeVisible()

  await page.goto("/propietarios")
  await expect(page.getByRole("heading", { name: "Tu propiedad, bien representada." })).toBeVisible()

  const sitemapResponse = await request.get("http://127.0.0.1:3000/sitemap.xml")
  expect(sitemapResponse.ok()).toBeTruthy()
  expect(await sitemapResponse.text()).toContain(`/inmuebles/${property.slug}`)
})
