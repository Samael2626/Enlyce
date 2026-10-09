import { expect, test } from "@playwright/test";

test("catalog map stays in sync with accessible list and omits invalid coordinates", async ({ page }) => {
  let mockedTileRequests = 0;
  await page.route("https://tile.openstreetmap.org/**", async (route) => {
    mockedTileRequests += 1;
    await route.fulfill({
      status: 200,
      contentType: "image/png",
      body: Buffer.from("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAADUlEQVR4nGP4z8AAAAMBAQDJ/pLvAAAAAElFTkSuQmCC", "base64"),
    });
  });

  await page.goto("/inmuebles?neighborhood=MapaTest");

  const list = page.getByRole("list", { name: "Publicaciones de esta página" });
  await expect(list.locator(":scope > li")).toHaveCount(3);
  await expect(page.getByRole("status")).toContainText("1 inmueble no aparece");
  await page.getByRole("button", { name: "Mostrar mapa" }).click();

  const map = page.getByRole("region", { name: "Mapa de áreas aproximadas de los inmuebles" });
  await expect(map).toBeVisible();
  const markers = map.locator(".leaflet-marker-icon");
  await expect(markers).toHaveCount(2);
  await expect(markers.nth(0)).toHaveAttribute("title", /Apartamento amplio en Laureles/);

  const firstCard = page.locator("#catalog-property-apartamento-laureles");
  await firstCard.getByRole("link", { name: "Apartamento amplio en Laureles" }).focus();
  await expect(firstCard).toHaveAttribute("data-active", "true");

  await markers.nth(1).focus();
  await markers.nth(1).press("Enter");
  const secondCard = page.locator("#catalog-property-casa-belén");
  await expect(secondCard).toHaveAttribute("data-active", "true");
  await expect(secondCard.getByRole("link", { name: "Casa de prueba en Belén" })).toBeFocused();
  await expect(map.locator(".leaflet-interactive")).toHaveCount(2);
  await expect(map.locator(".leaflet-overlay-pane path")).toHaveCount(2);
  expect(mockedTileRequests).toBeGreaterThan(0);
});
