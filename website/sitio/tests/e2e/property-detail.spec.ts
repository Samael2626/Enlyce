import { expect, test } from "@playwright/test"

const apiFixture = "http://127.0.0.1:5198"

test("ficha: galeria con visor, calculadora, compartir y similares", async ({ page }) => {
  await page.goto("/inmuebles/apartamento-laureles")

  await expect(page.getByText("1 / 3").first()).toBeVisible()
  await page.getByRole("button", { name: "Foto siguiente" }).first().click()
  await expect(page.getByText("2 / 3").first()).toBeVisible()

  await page.getByRole("button", { name: "Ver foto 2 de 3 en pantalla completa" }).click()
  const viewer = page.getByRole("dialog", { name: "Visor de fotos" })
  await expect(viewer).toBeVisible()
  await page.keyboard.press("ArrowRight")
  await expect(viewer.getByText("3 / 3")).toBeVisible()
  await viewer.getByRole("button", { name: "Cerrar visor" }).click()
  await expect(viewer).toBeHidden()

  // 420M con 30% inicial = 294M financiados.
  const calculator = page.getByRole("region", { name: "Simula tu crédito" })
  await expect(calculator.getByText("$ 294.000.000")).toBeVisible()
  await expect(calculator.getByText("Cuota mensual estimada")).toBeVisible()

  await expect(page.getByRole("button", { name: "Compartir" })).toBeVisible()
  const similar = page.getByRole("region", { name: "Inmuebles parecidos" })
  await expect(similar.getByRole("link", { name: /Apartamento cerca al Estadio/ })).toBeVisible()
  await expect(similar.getByRole("link", { name: /Apartamento amplio en Laureles/ })).toHaveCount(0)
})

test("alerta de busqueda crea lead con los criterios en la ruta", async ({ page, request }) => {
  await request.delete(`${apiFixture}/__test/requests`)
  await page.goto("/inmuebles?operation=Arriendo&bedrooms=2")

  await page.getByRole("button", { name: "Crear alerta" }).click()
  await page.getByLabel("Nombre").fill("Ana Prueba")
  await page.getByLabel("Correo").fill("ana@example.com")
  await page.getByLabel(/Autorizo el tratamiento/).check()
  await page.getByRole("button", { name: "Avisarme" }).click()
  await expect(page.getByText("Alerta creada.")).toBeVisible()

  const captured = await (await request.get(`${apiFixture}/__test/requests`)).json()
  const lead = captured.find((entry: { path: string }) => entry.path === "/api/leads")
  expect(lead.body).toMatchObject({
    fuente: "alerta_busqueda",
    tipoOperacion: "Arriendo",
    autorizacionDatos: true,
    sourceRoute: "/inmuebles?operation=Arriendo&bedrooms=2",
  })
})
