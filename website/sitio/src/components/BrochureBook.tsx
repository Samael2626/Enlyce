"use client";

import Link from "next/link";
import Image from "next/image";
import { useEffect, useRef, useState, useSyncExternalStore, type CSSProperties } from "react";
import type { PageFlip } from "page-flip";

const PAGE_NAVIGATION = [
  { title: "Portada", tone: "photo" },
  { title: "Nuestra mirada", tone: "paper" },
  { title: "Servicios", tone: "paper" },
  { title: "Presentación", tone: "photo" },
  { title: "Propietarios", tone: "paper" },
  { title: "Búsqueda", tone: "navy" },
  { title: "Territorio", tone: "photo" },
  { title: "ENLYCE", tone: "navy" },
  { title: "Contacto", tone: "paper" },
  { title: "Cierre", tone: "navy" },
] as const;

const PAGE_COUNT = PAGE_NAVIGATION.length;
const MIN_ZOOM = 80;
const MAX_ZOOM = 140;
const ZOOM_STEP = 10;
const WHATSAPP_URL =
  "https://wa.me/573246266362?text=Hola%2C%20quiero%20solicitar%20una%20valoraci%C3%B3n%20con%20L%26C%20Propiedad%20Ra%C3%ADz.";
const subscribeToHydration = () => () => undefined;
const getClientHydrationSnapshot = () => true;
const getServerHydrationSnapshot = () => false;

export function BrochureBook() {
  const bookRef = useRef<HTMLDivElement>(null);
  const readerRef = useRef<HTMLElement>(null);
  const pageFlipRef = useRef<PageFlip | null>(null);
  const [currentPage, setCurrentPage] = useState(0);
  const [zoom, setZoom] = useState(100);
  const [isFullscreen, setIsFullscreen] = useState(false);
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

      const prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

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
        flippingTime: prefersReducedMotion ? 1 : 720,
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
      if (event.altKey || event.ctrlKey || event.metaKey) return;

      const actions: Readonly<Record<string, () => void>> = {
        ArrowRight: () => pageFlipRef.current?.flipNext(),
        PageDown: () => pageFlipRef.current?.flipNext(),
        ArrowLeft: () => pageFlipRef.current?.flipPrev(),
        PageUp: () => pageFlipRef.current?.flipPrev(),
        Home: () => pageFlipRef.current?.flip(0),
        End: () => pageFlipRef.current?.flip(PAGE_COUNT - 1),
        "+": () => setZoom((value) => Math.min(MAX_ZOOM, value + ZOOM_STEP)),
        "=": () => setZoom((value) => Math.min(MAX_ZOOM, value + ZOOM_STEP)),
        "-": () => setZoom((value) => Math.max(MIN_ZOOM, value - ZOOM_STEP)),
        "0": () => setZoom(100),
      };
      const action = actions[event.key];
      if (!action) return;
      event.preventDefault();
      action();
    }

    function handleFullscreenChange() {
      setIsFullscreen(document.fullscreenElement === reader);
    }

    reader?.addEventListener("keydown", handleKeyDown);
    document.addEventListener("fullscreenchange", handleFullscreenChange);
    return () => {
      cancelled = true;
      reader?.removeEventListener("keydown", handleKeyDown);
      document.removeEventListener("fullscreenchange", handleFullscreenChange);
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

  function flipToPage(pageIndex: number) {
    pageFlipRef.current?.flip(pageIndex);
    readerRef.current?.focus();
  }

  function changeZoom(delta: number) {
    setZoom((value) => Math.min(MAX_ZOOM, Math.max(MIN_ZOOM, value + delta)));
  }

  const zoomStyle = { "--brochure-zoom": zoom / 100 } as CSSProperties;
  const visiblePageEnd = currentPage > 0 && currentPage < PAGE_COUNT - 1
    ? Math.min(currentPage + 1, PAGE_COUNT - 1)
    : currentPage;
  const pageStatus = visiblePageEnd === currentPage
    ? `Página ${currentPage + 1} de ${PAGE_COUNT}`
    : `Páginas ${currentPage + 1}–${visiblePageEnd + 1} de ${PAGE_COUNT}`;

  return (
    <section
      ref={readerRef}
      className="brochure-reader"
      aria-label="Lector del brochure institucional de L&C"
      tabIndex={0}
    >
      <div className="brochure-reader-bar" role="toolbar" aria-label="Herramientas del brochure">
        <div className="brochure-reader-edition">
          <Image
            src="/lyc-logo-transparent.png"
            width={1254}
            height={1254}
            alt="L&C Propiedad Raíz S.A.S."
            className="brochure-reader-logo"
          />
          <span><small>Brochure institucional</small><strong>Medellín · 2026</strong></span>
        </div>
        <div className="brochure-reader-actions">
          <div className="brochure-zoom" aria-label="Zoom del brochure">
            <button type="button" onClick={() => changeZoom(-ZOOM_STEP)} disabled={zoom === MIN_ZOOM} aria-label="Alejar">&minus;</button>
            <button type="button" onClick={() => setZoom(100)} aria-label="Restablecer zoom">{zoom}%</button>
            <button type="button" onClick={() => changeZoom(ZOOM_STEP)} disabled={zoom === MAX_ZOOM} aria-label="Acercar">+</button>
          </div>
          <button type="button" onClick={toggleFullscreen} className="brochure-tool brochure-fullscreen">
            {isFullscreen ? "Salir de pantalla completa" : "Pantalla completa"}
          </button>
          <a href="/brochure-lyc.pdf" download className="brochure-tool">Descargar PDF</a>
          <Link href="/propietarios?servicio=avaluar&utm_campaign=brochure#solicitud" className="brochure-contact">Solicitar valoración</Link>
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

        <div
          className={`brochure-book-shell${currentPage === 0 ? " brochure-book-shell-front" : ""}${currentPage === PAGE_COUNT - 1 ? " brochure-book-shell-back" : ""}`}
          style={zoomStyle}
        >
        <div ref={bookRef} className="brochure-book">
          <article className="brochure-leaf brochure-cover" data-density="hard" aria-label="Página 1: Portada">
            <div className="brochure-cover-shade" />
            <Image
              src="/lyc-logo-transparent.png"
              width={1254}
              height={1254}
              alt="L&C Propiedad Raíz S.A.S."
              className="brochure-cover-logo"
              preload
            />
            <div className="brochure-cover-brand">
              <span className="brochure-kicker">Portafolio institucional · 2026</span>
              <h2>Espacios con propósito.</h2>
              <p>Una mirada selecta al patrimonio inmobiliario de Medellín y el Valle de Aburrá.</p>
            </div>
            <div className="brochure-cover-foot">
              <span>Portafolio institucional</span>
              <span>MMXXVI</span>
            </div>
          </article>

          <article className="brochure-leaf brochure-opening" aria-label="Página 2: Nuestra mirada">
            <span className="brochure-folio">02</span>
            <div className="brochure-opening-copy">
              <span className="brochure-kicker">Nuestra mirada</span>
              <h2>No vendemos metros cuadrados. Leemos decisiones.</h2>
              <p>Ubicación, momento, precio y propósito deben conversar. Por eso acompañamos cada operación con criterio comercial y atención cercana.</p>
            </div>
            <div className="brochure-opening-photo" role="img" aria-label="Vista urbana de Medellín" />
          </article>

          <article className="brochure-leaf brochure-services" aria-label="Página 3: Servicios">
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

          <article className="brochure-leaf brochure-photo-page brochure-photo-facade" aria-label="Página 4: Presentación del inmueble">
            <div className="brochure-photo-caption">
              <span className="brochure-kicker">Presentación que sí vende</span>
              <h2>Cada inmueble entra al mercado con una dirección.</h2>
            </div>
            <span className="brochure-folio brochure-folio-light">04</span>
          </article>

          <article className="brochure-leaf brochure-route" aria-label="Página 5: Ruta para propietarios">
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

          <article className="brochure-leaf brochure-interior" aria-label="Página 6: Búsqueda de inmuebles">
            <div className="brochure-interior-photo" role="img" aria-label="Interior contemporáneo de una vivienda" />
            <div className="brochure-interior-copy">
              <span className="brochure-kicker">Para compradores y arrendatarios</span>
              <h2>Encontrar también es saber descartar.</h2>
              <p>Curamos opciones, explicamos el contexto y acompañamos cada visita para comparar con información útil.</p>
            </div>
            <span className="brochure-folio">06</span>
          </article>

          <article className="brochure-leaf brochure-territory" aria-label="Página 7: Conocimiento local">
            <div className="brochure-territory-map" />
            <span className="brochure-folio brochure-folio-light">07</span>
            <div className="brochure-territory-copy">
              <span className="brochure-kicker">Conocimiento local</span>
              <h2>Una ciudad cambia de calle en calle.</h2>
              <p>Medellín · Envigado · Sabaneta · Itagüí · Bello</p>
            </div>
          </article>

          <article className="brochure-leaf brochure-enlyce" aria-label="Página 8: Gestión con ENLYCE">
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

          <article className="brochure-leaf brochure-contact-page" aria-label="Página 9: Contacto">
            <span className="brochure-folio">09</span>
            <span className="brochure-kicker">El siguiente movimiento</span>
            <h2>Conversemos sobre tu propiedad.</h2>
            <p>Cuéntanos si quieres vender, arrendar, comprar o recibir una valoración inicial.</p>
            <Link href="/propietarios?servicio=avaluar&utm_campaign=brochure#solicitud" className="brochure-page-cta brochure-page-cta-screen">Solicitar valoración <span>↗</span></Link>
            <a href={WHATSAPP_URL} className="brochure-page-cta brochure-page-cta-print">Escribir por WhatsApp <span>↗</span></a>
            <div className="brochure-contact-lines">
              <span>Cra. 30 # 10C-228 · El Poblado</span>
              <a href={WHATSAPP_URL}>+57 324 626 6362</a>
            </div>
          </article>

          <article className="brochure-leaf brochure-back-cover" data-density="hard" aria-label="Página 10: Contraportada">
            <Image
              src="/lyc-logo-transparent.png"
              width={1254}
              height={1254}
              alt="L&C Propiedad Raíz S.A.S."
              className="brochure-back-logo"
            />
            <p>Tu hogar, nuestra prioridad.</p>
            <span>Medellín · Valle de Aburrá</span>
          </article>
        </div>
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

      <nav className="brochure-thumbnails" aria-label="Páginas del brochure">
        {PAGE_NAVIGATION.map((page, pageIndex) => (
          <button
            key={page.title}
            type="button"
            className={`brochure-thumbnail brochure-thumbnail-${page.tone}`}
            onClick={() => flipToPage(pageIndex)}
            aria-label={`Ir a la página ${pageIndex + 1}: ${page.title}`}
            aria-current={pageIndex >= currentPage && pageIndex <= visiblePageEnd ? "page" : undefined}
          >
            <span>{String(pageIndex + 1).padStart(2, "0")}</span>
            <small>{page.title}</small>
          </button>
        ))}
      </nav>

      <div className="brochure-status" role="status">
        <span className="sr-only">{pageStatus}</span>
        <span aria-hidden="true">{String(currentPage + 1).padStart(2, "0")}</span>
        <i aria-hidden="true"><b style={{ width: `${((visiblePageEnd + 1) / PAGE_COUNT) * 100}%` }} /></i>
        <span aria-hidden="true">{String(PAGE_COUNT).padStart(2, "0")}</span>
        <p aria-hidden="true">Arrastra la esquina o usa las flechas.</p>
      </div>
    </section>
  );
}
