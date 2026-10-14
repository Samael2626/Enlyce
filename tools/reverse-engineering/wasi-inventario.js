// Uso: pega este archivo en la consola del navegador con Wasi abierto y guarda el JSON descargado.
(() => {
  const textOf = (element) => element
    ? (element.innerText || element.getAttribute("aria-label") || element.title || "")
      .replace(/\s+/g, " ").trim().slice(0, 100)
    : "";
  const visible = (element) => {
    const style = getComputedStyle(element);
    return style.display !== "none" && style.visibility !== "hidden" && element.getClientRects().length > 0;
  };
  const unique = (items) => [...new Set(items.filter(Boolean))];
  const normalize = (value) => value.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();
  const navigationLabels = [
    "esconder menu", "inicio", "escritorio", "foro inmobiliario", "agenda",
    "inmuebles", "lista de inmuebles", "nuevo inmueble", "inmuebles de mis aliados",
    "cruce inmuebles - clientes", "importar - exportar", "red inmobiliaria", "alianzas",
    "propietarios", "grupos", "mercado inmobiliario", "clientes", "directorio de clientes",
    "nuevo cliente", "embudo de negocios", "gestiones realizadas", "cruce clientes - inmuebles",
    "lista de clientes", "reportes", "usuarios", "sitio web", "configuracion", "mi cuenta",
    "configuracion inicial", "academia wasi", "reputacion", "invita y gana", "asistencia y soporte",
  ];
  const pageUrl = new URL(location.href);
  pageUrl.search = "";
  pageUrl.hash = "";

  const snapshot = {
    source: "Wasi UI inventory",
    capturedAt: new Date().toISOString(),
    page: { title: document.title, url: pageUrl.href },
    headingCount: [...document.querySelectorAll("h1,h2")].filter(visible).length,
    navigation: unique([...document.querySelectorAll("a[href]")]
      .filter((link) => {
        if (!visible(link)) return false;
        const label = normalize(textOf(link));
        return navigationLabels.includes(label)
          || label.startsWith("configuracion inicial ")
          || label.startsWith("reputacion: ");
      }).map((link) => {
      const target = new URL(link.href, location.href);
      if (/\/perfil\/\d+/.test(target.pathname)) return "";
      return `${textOf(link)} → ${target.origin === location.origin ? target.pathname : target.hostname}`;
    })),
    actions: unique([...document.querySelectorAll("header button, nav button, [role=toolbar] button")]
      .filter(visible).map((button) => button.getAttribute("aria-label") || button.title || "")),
    forms: [...document.querySelectorAll("form")].filter(visible).map((form) => ({
      fields: [...form.querySelectorAll("input,select,textarea")].filter(visible).map((field) => ({
        label: textOf(field.labels?.[0]) || field.getAttribute("aria-label") || field.name || "",
        type: field.type || field.tagName.toLowerCase(),
        required: field.required,
      })),
    })),
    tables: [...document.querySelectorAll("table")].filter(visible).map((table) => ({
      columns: [...table.querySelectorAll("thead th")].map(textOf),
    })),
    privacy: "No captura valores de campos, texto de filas/listados ni enlaces fuera del menu lateral.",
  };

  const blob = new Blob([JSON.stringify(snapshot, null, 2)], { type: "application/json" });
  const link = document.createElement("a");
  link.href = URL.createObjectURL(blob);
  link.download = `wasi-ui-${location.hostname}-${Date.now()}.json`;
  link.click();
  setTimeout(() => URL.revokeObjectURL(link.href), 1000);
  console.info("Inventario de interfaz descargado:", snapshot.page.url);
})();
