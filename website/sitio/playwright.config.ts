import { defineConfig } from "@playwright/test"

const apiUrl = "http://127.0.0.1:5198"
const siteUrl = "http://127.0.0.1:3301"

export default defineConfig({
  testDir: "./tests/e2e",
  fullyParallel: false,
  workers: 1,
  reporter: "list",
  outputDir: "./test-results",
  use: {
    baseURL: siteUrl,
    browserName: "chromium",
    channel: "chrome",
    headless: true,
    trace: "retain-on-failure",
  },
  webServer: [
    {
      command: "node tests/e2e/mock-api.mjs",
      url: `${apiUrl}/health`,
      env: { E2E_API_PORT: "5198" },
      reuseExistingServer: false,
      timeout: 30_000,
    },
    {
      command: "npm run dev -- --hostname 127.0.0.1 --port 3301",
      url: siteUrl,
      env: {
        NEXT_PUBLIC_API_URL: apiUrl,
        NEXT_PUBLIC_MAP_TILE_URL: "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
        NEXT_PUBLIC_SITE_URL: siteUrl,
        NEXT_TELEMETRY_DISABLED: "1",
      },
      reuseExistingServer: false,
      timeout: 120_000,
    },
  ],
})
