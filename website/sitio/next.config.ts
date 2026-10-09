import type { NextConfig } from "next";

const apiUrl = new URL(process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5019");
const mapTileUrl = process.env.NEXT_PUBLIC_MAP_TILE_URL?.trim() || "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
const mapTileOrigin = new URL(mapTileUrl.replace(/\{[^}]+\}/g, "0")).origin;
const isLocalApi = ["localhost", "127.0.0.1", "::1"].includes(apiUrl.hostname);
const developmentScriptSource = process.env.NODE_ENV === "development" ? " 'unsafe-eval'" : "";

const nextConfig: NextConfig = {
  poweredByHeader: false,
  async headers() {
    const connectSource = apiUrl.origin;
    const csp = [
      "default-src 'self'",
      `script-src 'self' 'unsafe-inline'${developmentScriptSource}`,
      "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",
      "font-src 'self' https://fonts.gstatic.com",
      `img-src 'self' data: blob: ${connectSource} ${mapTileOrigin}`,
      `connect-src 'self' ${connectSource}`,
      "object-src 'none'",
      "base-uri 'self'",
      "form-action 'self'",
      "frame-ancestors 'none'",
    ].join("; ");

    return [{
      source: "/:path*",
      headers: [
        { key: "Content-Security-Policy", value: csp },
        { key: "X-Frame-Options", value: "DENY" },
        { key: "X-Content-Type-Options", value: "nosniff" },
        { key: "Referrer-Policy", value: "strict-origin-when-cross-origin" },
        { key: "Permissions-Policy", value: "camera=(), microphone=(), geolocation=(), payment=()" },
      ],
    }];
  },
  images: {
    // Las fotos las sirve Enlyce.Api bajo /media; en produccion sera el CDN.
    remotePatterns: [
      {
        protocol: apiUrl.protocol.replace(":", "") as "http" | "https",
        hostname: apiUrl.hostname,
        port: apiUrl.port || undefined,
        pathname: "/media/**",
      },
    ],
    // Next 16 bloquea optimizar imagenes de IPs privadas por riesgo de SSRF.
    // Solo se abre cuando la API es la local de desarrollo; con un host real
    // queda cerrado sin tocar nada.
    dangerouslyAllowLocalIP: isLocalApi,
  },
};

export default nextConfig;
