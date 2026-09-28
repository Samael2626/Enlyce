"use client";

import { usePathname, useSearchParams } from "next/navigation";
import { useEffect } from "react";
import { trackFunnelEvent } from "@/lib/analytics";

const trackedLocations = new Set<string>();

export function AnalyticsPageView() {
  const pathname = usePathname();
  const searchParams = useSearchParams();

  useEffect(() => {
    const location = `${pathname}?${searchParams.toString()}`;
    if (trackedLocations.has(location)) return;
    trackedLocations.add(location);

    const propertyMatch = pathname.match(/^\/inmuebles\/([^/]+)$/);
    trackFunnelEvent("page_view", {
      path: pathname,
      propertySlug: propertyMatch?.[1],
    });
  }, [pathname, searchParams]);

  return null;
}
