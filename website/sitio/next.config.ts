import type { NextConfig } from "next";

const apiUrl = new URL(process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5019");
const isLocalApi = ["localhost", "127.0.0.1", "::1"].includes(apiUrl.hostname);

const nextConfig: NextConfig = {
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
