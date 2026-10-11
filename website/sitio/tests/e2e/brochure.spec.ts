import { expect, test } from "@playwright/test";

test("brochure carga marca, navegacion y contacto sin errores de hidratacion", async ({ page, request }) => {
  const consoleErrors: string[] = [];
  const failedResources: string[] = [];
  page.on("console", (message) => {
    if (message.type() === "error") consoleErrors.push(message.text());
  });
  page.on("response", (response) => {
    if (response.status() >= 400) failedResources.push(`${response.status()} ${response.url()}`);
  });

  await page.goto("/brochure");

  await expect(page.getByRole("heading", { name: "Brochure institucional de L&C Propiedad Raíz" })).toBeAttached();
  await expect(page.getByRole("toolbar", { name: "Herramientas del brochure" })).toBeVisible();
  await expect(page.getByAltText("L&C Propiedad Raíz S.A.S.").first()).toBeVisible();
  await expect(page.getByRole("link", { name: "Solicitar valoración" }).first()).toHaveAttribute(
    "href",
    "/propietarios?servicio=avaluar&utm_source=brochure&utm_medium=interactive&utm_campaign=brochure_2026#solicitud",
  );
  await expect(page.locator('a[href^="https://wa.me/573246266362"]').first()).toHaveAttribute(
    "href",
    /vi%20el%20brochure%20institucional%202026/,
  );

  const pdfResponse = await request.get("/brochure-lyc.pdf");
  expect(pdfResponse.ok()).toBeTruthy();
  expect(pdfResponse.headers()["content-type"]).toContain("application/pdf");
  expect((await pdfResponse.body()).subarray(0, 5).toString()).toBe("%PDF-");

  await page.getByRole("button", { name: "Ir a la página 9: Contacto" }).click();
  await expect(page.getByRole("status")).toContainText("Páginas 8–9 de 10");

  await page.locator(".brochure-reader").focus();
  await page.keyboard.press("Home");
  await expect(page.getByRole("status")).toContainText("Página 1 de 10");
  await page.keyboard.press("End");
  await expect(page.getByRole("status")).toContainText("Página 10 de 10");

  expect({ consoleErrors, failedResources }).toEqual({ consoleErrors: [], failedResources: [] });
});

test("brochure conserva portada y acciones en movil sin desborde horizontal", async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto("/brochure");

  await expect(page.getByText("Espacios con propósito.", { exact: true })).toBeVisible();
  await expect(page.getByRole("link", { name: "Descargar PDF" })).toBeVisible();
  await expect(page.getByRole("link", { name: "Solicitar valoración" }).first()).toBeVisible();

  const dimensions = await page.evaluate(() => ({
    clientWidth: document.documentElement.clientWidth,
    scrollWidth: document.documentElement.scrollWidth,
  }));
  expect(dimensions.scrollWidth).toBeLessThanOrEqual(dimensions.clientWidth);
});
