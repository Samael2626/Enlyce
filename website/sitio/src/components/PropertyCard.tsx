import Image from "next/image";
import Link from "next/link";
import type { PropertyListItem } from "@/lib/api/client";
import { formatArea, formatOperation, formatPrice, propertyPath } from "@/lib/format";
import { FavoriteToggle } from "./FavoriteToggle";

export function PropertyCard({ property }: { property: PropertyListItem }) {
  return (
    <article className="group relative overflow-hidden rounded-sheet bg-surface shadow-card transition hover:shadow-raised">
      <div className="relative aspect-4/3 bg-line">
        {property.coverPhoto ? (
          <Image
            src={property.coverPhoto.url}
            alt={property.coverPhoto.altText}
            fill
            sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 33vw"
            className="object-cover"
          />
        ) : (
          <div className="flex h-full items-center justify-center text-sm text-muted">
            Sin fotografía
          </div>
        )}
        <span className="absolute left-3 top-3 rounded-full bg-ink/85 px-3 py-1 text-xs text-surface">
          {formatOperation(property.operation)}
        </span>
        <FavoriteToggle slug={property.slug} title={property.publicTitle} />
      </div>

      <div className="space-y-2 p-4">
        <h3 className="text-lg leading-snug">
          <Link href={propertyPath(property.slug)} className="after:absolute after:inset-0">
            {property.publicTitle}
          </Link>
        </h3>
        <p className="text-sm text-muted">
          {property.location.neighborhood}, {property.location.municipality}
        </p>
        <p className="font-display text-xl text-accent">
          {formatPrice(property.price.amount, property.price.currency)}
        </p>
        <dl className="flex flex-wrap gap-x-4 gap-y-1 text-sm text-muted">
          <div className="flex gap-1">
            <dt className="sr-only">Área</dt>
            <dd>{formatArea(property.areaSquareMeters)}</dd>
          </div>
          {property.bedrooms > 0 && (
            <div className="flex gap-1">
              <dt className="sr-only">Habitaciones</dt>
              <dd>{property.bedrooms} hab.</dd>
            </div>
          )}
          {property.bathrooms > 0 && (
            <div className="flex gap-1">
              <dt className="sr-only">Baños</dt>
              <dd>{property.bathrooms} baños</dd>
            </div>
          )}
          {property.parkingSpaces > 0 && (
            <div className="flex gap-1">
              <dt className="sr-only">Parqueaderos</dt>
              <dd>{property.parkingSpaces} parq.</dd>
            </div>
          )}
        </dl>
      </div>
    </article>
  );
}
