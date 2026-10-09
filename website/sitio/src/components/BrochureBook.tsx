"use client";

import Link from "next/link";
import { useEffect, useRef, useState, useSyncExternalStore } from "react";
import type { PageFlip } from "page-flip";

const PAGE_COUNT = 10;
const subscribeToHydration = () => () => undefined;
const getClientHydrationSnapshot = () => true;
const getServerHydrationSnapshot = () => false;

export function BrochureBook() {
  const bookRef = useRef<HTMLDivElement>(null);
  const readerRef = useRef<HTMLElement>(null);
  const pageFlipRef = useRef<PageFlip | null>(null);
  const [currentPage, setCurrentPage] = useState(0);
  const isHydrated = useSyncExternalStore(
    subscribeToHydration,
    getClientHydrationSnapshot,
    getServerHydrationSnapshot,
  );

  useEffect(() => {
    const reader = readerRef.current;
    let cancelled = false;
    let instance: PageFlip | null = null;

    async function initializeBook() {
      if (!bookRef.current || pageFlipRef.current) return;

      const { PageFlip: PageFlipConstructor } = await import("page-flip");
      if (cancelled || !bookRef.current) return;

      instance = new PageFlipConstructor(bookRef.current, {
        width: 520,
        height: 700,
        size: "stretch",
        minWidth: 290,
        maxWidth: 520,
        minHeight: 390,
        maxHeight: 700,
        maxShadowOpacity: 0.42,
        showCover: true,
        mobileScrollSupport: true,
        usePortrait: true,
        flippingTime: 850,
        drawShadow: true,
        autoSize: true,
      });

      const leaves = Array.from(bookRef.current.querySelectorAll<HTMLElement>(".brochure-leaf"));
      instance.loadFromHTML(leaves);
      instance.on("flip", (event) => setCurrentPage(Number(event.data)));
      pageFlipRef.current = instance;
    }

    void initializeBook();

    function handleKeyDown(event: KeyboardEvent) {
      if (!bookRef.current?.contains(event.target as Node)) return;
      if (event.key === "ArrowRight") pageFlipRef.current?.flipNext();
      if (event.key === "ArrowLeft") pageFlipRef.current?.flipPrev();
    }

    reader?.addEventListener("keydown", handleKeyDown);
    return () => {
      cancelled = true;
      reader?.removeEventListener("keydown", handleKeyDown);
      if (instance) {
        instance.destroy();
        pageFlipRef.current = null;
      }
    };
  }, []);

  async function toggleFullscreen() {
    if (!readerRef.current) return;
    if (document.fullscreenElement) await document.exitFullscreen();
    else await readerRef.current.requestFullscreen();
  }

  return (
    <section ref={readerRef} className="brochure-reader" aria-label="Brochure institucional L&C">
      <div className="brochure-reader-bar">
        <div className="brochure-reader-edition">
          <span>Edición institucional</span>
          <strong>Medellín · 2026</strong>
        </div>
        <div className="brochure-reader-actions">
          <button type="button" onClick={toggleFullscreen} className="brochure-tool">Pantalla completa</button>
          <a href="/brochure-lyc.pdf" download className="brochure-tool">Descargar PDF</a>
          <Link href="/contacto" className="brochure-contact">Hablar con un asesor</Link>
        </div>
      </div>

      <div className="brochure-stage">
        <button
          type="button"
          className="brochure-turn brochure-turn-previous"
          onClick={() => pageFlipRef.current?.flipPrev()}
          disabled={isHydrated && currentPage === 0}
          aria-label="Página anterior"
        >
          <span aria-hidden="true">←</span>
        </button>

        <div ref={bookRef} className="brochure-book">
          <article className="brochure-leaf brochure-cover" data-density="hard">
            <div className="brochure-cover-shade" />
            <div className="brochure-cover-brand">
              <span className="brochure-kicker">L&amp;C Propiedad Raíz S.A.S.</span>
              <h2>Espacios con propósito.</h2>
              <p>Una mirada selecta al patrimonio inmobiliario de Medellín y el Valle de Aburrá.</p>
            </div>
            <div className="brochure-cover-foot">
              <span>Portafolio institucional</span>
              <span>MMXXVI</span>
            </div>
          </article>

          <article className="brochure-leaf brochure-opening">
            <span className="brochure-folio">02</span>
            <div className="brochure-opening-copy">
              <span className="brochure-kicker">Nuestra mirada</span>
              <h2>No vendemos metros cuadrados. Leemos decisiones.</h2>
              <p>Ubicación, momento, precio y propósito deben conversar. Por eso acompañamos cada operación con criterio comercial y atención cercana.</p>
            </div>
            <div className="brochure-opening-photo" role="img" aria-label="Vista urbana de Medellín" />
          </article>

          <article className="brochure-leaf brochure-services">
            <span className="brochure-folio">03</span>
            <span className="brochure-kicker">Lo que hacemos</span>
            <h2>Un servicio completo, sin ruido.</h2>
            <div className="brochure-service-list">
              <div><b>01</b><h3>Venta</h3><p>Posicionamiento, presentación, visitas y negociación.</p></div>
              <div><b>02</b><h3>Arriendo</h3><p>Selección de candidatos y acompañamiento contractual.</p></div>
              <div><b>03</b><h3>Administración</h3><p>Seguimiento del inmueble y comunicación ordenada.</p></div>
              <div><b>04</b><h3>Valoración</h3><p>Lectura comercial para decidir con contexto.</p></div>
            </div>
          </article>

          <article className="brochure-leaf brochure-photo-page brochure-photo-facade">
            <div className="brochure-photo-caption">
              <span className="brochure-kicker">Presentación que sí vende</span>
              <h2>Cada inmueble entra al mercado con una dirección.</h2>
            </div>
            <span className="brochure-folio brochure-folio-light">04</span>
          </article>

          <article className="brochure-leaf brochure-route">
            <span className="brochure-folio">05</span>
            <span className="brochure-kicker">Para propietarios</span>
            <h2>De la primera conversación al cierre.</h2>
            <ol>
              <li><b>01</b><span><strong>Entendemos</strong> el inmueble y el objetivo.</span></li>
              <li><b>02</b><span><strong>Preparamos</strong> documentación y presentación.</span></li>
              <li><b>03</b><span><strong>Activamos</strong> la estrategia comercial.</span></li>
              <li><b>04</b><span><strong>Acompañamos</strong> visitas, negociación y cierre.</span></li>
            </ol>
          </article>

          <article className="brochure-leaf brochure-interior">
            <div className="brochure-interior-photo" role="img" aria-label="Interior contemporáneo de una vivienda" />
            <div className="brochure-interior-copy">
              <span className="brochure-kicker">Para compradores y arrendatarios</span>
              <h2>Encontrar también es saber descartar.</h2>
              <p>Curamos opciones, explicamos el contexto y acompañamos cada visita para comparar con información útil.</p>
            </div>
            <span className="brochure-folio">06</span>
          </article>

          <article className="brochure-leaf brochure-territory">
            <div className="brochure-territory-map" />
            <span className="brochure-folio brochure-folio-light">07</span>
            <div className="brochure-territory-copy">
              <span className="brochure-kicker">Conocimiento local</span>
              <h2>Una ciudad cambia de calle en calle.</h2>
              <p>Medellín · Envigado · Sabaneta · Itagüí · Bello</p>
            </div>
          </article>

          <article className="brochure-leaf brochure-enlyce">
            <span className="brochure-folio">08</span>
            <div>
              <span className="brochure-kicker">Gestión respaldada por ENLYCE</span>
              <h2>La tecnología ordena. Las personas deciden.</h2>
            </div>
            <div className="brochure-enlyce-ledger">
              <p><span>01</span> Cada contacto se convierte en una oportunidad trazable.</p>
              <p><span>02</span> Visitas, conversaciones y siguientes pasos quedan organizados.</p>
              <p><span>03</span> El seguimiento no depende de la memoria ni de chats dispersos.</p>
            </div>
            <p className="brochure-enlyce-note">ENLYCE conecta la web, los asesores y cada oportunidad.</p>
          </article>

          <article className="brochure-leaf brochure-contact-page">
            <span className="brochure-folio">09</span>
            <span className="brochure-kicker">El siguiente movimiento</span>
            <h2>Conversemos sobre tu propiedad.</h2>
            <p>Cuéntanos si quieres vender, arrendar, comprar o recibir una valoración inicial.</p>
            <Link href="/contacto" className="brochure-page-cta">Iniciar conversación <span>↗</span></Link>
            <div className="brochure-contact-lines">
              <span>Medellín, Colombia</span>
              <span>L&amp;C Propiedad Raíz S.A.S.</span>
            </div>
          </article>

          <article className="brochure-leaf brochure-back-cover" data-density="hard">
            <div className="brochure-back-mark">L&amp;C</div>
            <p>Propiedad raíz con criterio.</p>
            <span>Medellín · Valle de Aburrá</span>
          </article>
        </div>

        <button
          type="button"
          className="brochure-turn brochure-turn-next"
          onClick={() => pageFlipRef.current?.flipNext()}
          disabled={isHydrated && currentPage >= PAGE_COUNT - 1}
          aria-label="Página siguiente"
        >
          <span aria-hidden="true">→</span>
        </button>
      </div>

      <div className="brochure-status" role="status">
        <span className="sr-only">Página {currentPage + 1} de {PAGE_COUNT}</span>
        <span aria-hidden="true">{String(currentPage + 1).padStart(2, "0")}</span>
        <i aria-hidden="true"><b style={{ width: `${((currentPage + 1) / PAGE_COUNT) * 100}%` }} /></i>
        <span aria-hidden="true">{String(PAGE_COUNT).padStart(2, "0")}</span>
        <p aria-hidden="true">Arrastra la esquina o usa las flechas.</p>
      </div>
    </section>
  );
}
