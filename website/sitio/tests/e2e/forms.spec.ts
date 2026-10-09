import { expect, test, type APIRequestContext, type Page } from "@playwright/test";

const apiFixture = "http://127.0.0.1:5198";

type CapturedRequest = {
  method: string;
  path: string;
  body: Record<string, unknown>;
};

async function clearCapturedRequests(request: APIRequestContext) {
  const response = await request.delete(`${apiFixture}/__test/requests`);
  expect(response.ok()).toBeTruthy();
}

async function capturedRequests(request: APIRequestContext): Promise<CapturedRequest[]> {
  const response = await request.get(`${apiFixture}/__test/requests`);
  expect(response.ok()).toBeTruthy();
  return (await response.json()) as CapturedRequest[];
}

async function submitContactForm(page: Page, buttonName = "Enviar solicitud") {
  await page.getByLabel("Nombre", { exact: true }).fill("Ana Prueba");
  await page.getByLabel("Correo electrónico").fill("ana@example.com");
  await page.getByLabel("Teléfono").fill("3001234567");
  await page.getByRole("checkbox").check();
  await page.getByRole("button", { name: buttonName }).click();
}

test.describe("formularios de contacto", () => {
  test("envía consentimiento, publicación, ruta y campaña", async ({ page, request }) => {
    await clearCapturedRequests(request);

    await page.goto("/contacto?inmueble=apartamento-laureles&utm_campaign=e2e");
    await submitContactForm(page);

    await expect(page.getByRole("heading", { name: "Solicitud recibida" })).toBeVisible();

    const entries = await capturedRequests(request);
    const lead = entries.find(
      (entry) => entry.method === "POST" && entry.path === "/api/leads",
    );
    expect(lead, "debe capturarse el POST del lead en el fixture").toBeDefined();
    expect(lead!.body).toMatchObject({
      autorizacionDatos: true,
      sourceRoute: "/contacto",
      utmCampaign: "e2e",
    });
    expect(lead!.body.publicationId).toEqual(expect.any(String));
    expect(lead!.body.publicationId).not.toBe("apartamento-laureles");
  });

  test("registra servicio y campaña del propietario y permite completar la solicitud", async ({
    page,
    request,
  }) => {
    await clearCapturedRequests(request);

    await page.goto("/propietarios?servicio=administrar&utm_campaign=e2e");
    await submitContactForm(page, "Guardar y continuar");

    await expect(page.getByRole("heading", { name: "Ya podemos contactarte." })).toBeVisible();

    const entries = await capturedRequests(request);
    const lead = entries.find(
      (entry) => entry.method === "POST" && entry.path === "/api/leads",
    );
    expect(lead, "debe capturarse el POST del lead en el fixture").toBeDefined();
    expect(lead!.body).toMatchObject({
      sourceRoute: "/propietarios",
      utmCampaign: "e2e",
      ownerService: "Manage",
      tipoOperacion: "Arriendo",
      autorizacionDatos: true,
    });

    await page.getByLabel("Barrio o sector (opcional)").fill("Laureles");
    await page.getByLabel("Algo que debamos saber (opcional)").fill("Solicitud de prueba E2E");
    await page.getByRole("button", { name: "Guardar datos del inmueble" }).click();

    await expect(page.getByRole("heading", { name: "El asesor ya tiene el contexto del inmueble." })).toBeVisible();

    const completedEntries = await capturedRequests(request);
    const details = completedEntries.find(
      (entry) => entry.method === "PUT" && entry.path === "/api/leads/owner-details",
    );
    expect(details, "debe capturarse el PUT de detalles del propietario").toBeDefined();
    expect(details!.body.continuationToken).toEqual(expect.any(String));
    expect(details!.body).toMatchObject({
      propertyType: "Apartment",
      neighborhood: "Laureles",
      message: "Solicitud de prueba E2E",
    });
  });
});
