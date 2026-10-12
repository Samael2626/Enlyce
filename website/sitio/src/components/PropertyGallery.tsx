"use client";

import Image from "next/image";
import { useCallback, useEffect, useRef, useState } from "react";

type Photo = { url: string; altText: string };

// Carrusel con scroll-snap (gesto nativo en movil) y visor a pantalla completa.
export function PropertyGallery({ photos }: { photos: Photo[] }) {
  const track = useRef<HTMLDivElement>(null);
  const dialog = useRef<HTMLDialogElement>(null);
  const [index, setIndex] = useState(0);
  const total = photos.length;

  const goTo = useCallback((next: number) => {
    const target = (next + total) % total;
    const el = track.current;
    if (el) el.scrollTo({ left: target * el.clientWidth, behavior: "smooth" });
    setIndex(target);
  }, [total]);

  const onScroll = () => {
    const el = track.current;
    if (el && el.clientWidth > 0) setIndex(Math.round(el.scrollLeft / el.clientWidth));
  };

  useEffect(() => {
    const onKey = (event: KeyboardEvent) => {
      if (!dialog.current?.open) return;
      if (event.key === "ArrowRight") setIndex((value) => (value + 1) % total);
      if (event.key === "ArrowLeft") setIndex((value) => (value - 1 + total) % total);
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [total]);

  if (total === 0) return null;
  const current = photos[index];

  return (
    <div className="property-gallery">
      <div className="property-gallery-frame">
        <div className="property-gallery-track" ref={track} onScroll={onScroll} role="region" aria-label="Fotos del inmueble">
          {photos.map((photo, position) => (
            <button
              key={photo.url}
              type="button"
              className="property-gallery-slide"
              onClick={() => {
                setIndex(position);
                dialog.current?.showModal();
              }}
              aria-label={`Ver foto ${position + 1} de ${total} en pantalla completa`}
            >
              <Image
                src={photo.url}
                alt={photo.altText}
                fill
                priority={position === 0}
                sizes="(max-width: 1024px) 100vw, 1024px"
                className="object-cover"
              />
            </button>
          ))}
        </div>
        {total > 1 && (
          <>
            <button type="button" className="property-gallery-nav" data-dir="prev" onClick={() => goTo(index - 1)} aria-label="Foto anterior">‹</button>
            <button type="button" className="property-gallery-nav" data-dir="next" onClick={() => goTo(index + 1)} aria-label="Foto siguiente">›</button>
          </>
        )}
        <span className="property-gallery-count" aria-live="polite">{index + 1} / {total}</span>
      </div>

      {total > 1 && (
        <ul className="property-gallery-thumbs" aria-label="Miniaturas">
          {photos.map((photo, position) => (
            <li key={photo.url}>
              <button
                type="button"
                onClick={() => goTo(position)}
                aria-current={position === index}
                aria-label={`Ir a la foto ${position + 1}`}
              >
                <Image src={photo.url} alt="" fill sizes="96px" className="object-cover" />
              </button>
            </li>
          ))}
        </ul>
      )}

      <dialog ref={dialog} className="property-viewer" aria-label="Visor de fotos" onClose={() => goTo(index)}>
        <button type="button" className="property-viewer-close" onClick={() => dialog.current?.close()} aria-label="Cerrar visor">×</button>
        <div className="property-viewer-image">
          <Image src={current.url} alt={current.altText} fill sizes="100vw" className="object-contain" />
        </div>
        {total > 1 && (
          <div className="property-viewer-bar">
            <button type="button" onClick={() => setIndex((index - 1 + total) % total)} aria-label="Foto anterior">‹</button>
            <span>{index + 1} / {total}</span>
            <button type="button" onClick={() => setIndex((index + 1) % total)} aria-label="Foto siguiente">›</button>
          </div>
        )}
      </dialog>
    </div>
  );
}
