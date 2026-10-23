"use client";

import { useEffect, useState } from "react";

const apiUrl = (process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5019").replace(/\/$/, "");
const crmUrl = (process.env.NEXT_PUBLIC_CRM_URL ?? process.env.NEXT_PUBLIC_API_URL)?.replace(/\/$/, "");

export function PropertyAdminLink({ propertyId }: { propertyId: string }) {
  const [canManageProperties, setCanManageProperties] = useState(false);

  useEffect(() => {
    const controller = new AbortController();
    setCanManageProperties(false);
    const requestOptions = {
      credentials: "include" as RequestCredentials,
      cache: "no-store" as RequestCache,
      signal: controller.signal,
      headers: { Accept: "application/json" },
    };

    void fetch(`${apiUrl}/api/auth/me`, requestOptions)
      .then(async (response) => response.ok
        ? response.json() as Promise<{ id?: string; rol?: string }>
        : null)
      .then(async (user) => {
        if (user?.rol === "Administrador") {
          setCanManageProperties(true);
          return;
        }
        if (user?.rol !== "Asesor" || !user.id) return;

        const response = await fetch(`${apiUrl}/api/inmuebles/${encodeURIComponent(propertyId)}`, requestOptions);
        if (!response.ok) return;
        const property = await response.json() as { asesorResponsableId?: string | null };
        setCanManageProperties(property.asesorResponsableId === user.id);
      })
      .catch(() => {
        if (!controller.signal.aborted) setCanManageProperties(false);
      });

    return () => controller.abort();
  }, [propertyId]);

  if (!crmUrl) return null;

  const editUrl = new URL("/inmuebles", crmUrl);
  editUrl.searchParams.set("edit", propertyId);

  const link = (
    <a
      href={editUrl.toString()}
      className="inline-flex min-h-11 items-center rounded border border-line px-4 py-2 text-sm font-semibold hover:border-accent-strong hover:text-accent-strong"
    >
      Editar este inmueble en CRM
    </a>
  );

  if (canManageProperties) return link;

  return (
    <details className="text-sm text-muted">
      <summary className="cursor-pointer font-semibold">¿Gestionas este inmueble?</summary>
      <div className="mt-3 flex flex-wrap items-center gap-3">
        {link}
        <span>El CRM solicitará tu sesión y comprobará tus permisos.</span>
      </div>
    </details>
  );
}
