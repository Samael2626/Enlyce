"use client";

import { apiBaseUrl } from "@/lib/api/client";

export type FunnelEvent = "page_view" | "search" | "favorite" | "form_started" | "conversion";

type FunnelEventContext = {
  readonly path?: string;
  readonly propertySlug?: string;
};

const SESSION_KEY = "lyc:analytics:session";

function getSessionId(): string | null {
  try {
    const stored = window.sessionStorage.getItem(SESSION_KEY);
    if (stored) return stored;

    const created = window.crypto.randomUUID();
    window.sessionStorage.setItem(SESSION_KEY, created);
    return created;
  } catch {
    return null;
  }
}

export function trackFunnelEvent(event: FunnelEvent, context: FunnelEventContext = {}): void {
  if (typeof window === "undefined") return;

  const sessionId = getSessionId();
  if (!sessionId) return;

  const payload = {
    sessionId,
    event,
    path: context.path ?? window.location.pathname,
    propertySlug: context.propertySlug ?? null,
  };

  void fetch(`${apiBaseUrl}/api/public/analytics/events`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
    keepalive: true,
  }).catch(() => {
    // La analitica nunca debe romper la accion principal del visitante.
  });
}
