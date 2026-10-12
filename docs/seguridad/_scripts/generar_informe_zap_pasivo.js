// Genera el informe Word de la evaluacion pasiva con OWASP ZAP (12 oct 2026).
// Evidencia real en docs/seguridad/evidencias/2026-10-12. Reutiliza el estilo editorial del repo.
const fs = require("node:fs");
const path = require("node:path");
const {
  AlignmentType, BorderStyle, Document, ExternalHyperlink, Footer, Header, HeadingLevel,
  ImageRun, LevelFormat, PageBreak, PageNumber, Paragraph, ShadingType, Table, TableCell,
  TableRow, TextRun, WidthType, Packer,
} = require("../../actividad-8/_scripts/node_modules/docx");

const root = path.resolve(__dirname, "..");
const evidence = path.join(root, "evidencias", "2026-10-12");
const output = path.join(root, "Informe_ZAP_Pasivo_ENLYCE_2026-10-12.docx");
const colors = { ink: "172B4D", blue: "175A8E", pale: "EAF2F8", gray: "52606D", line: "D5DEE7", red: "A61B1B", amber: "8A5A00", green: "176B45" };

const p = (text, options = {}) => new Paragraph({ spacing: { after: 120, line: 276 }, children: [new TextRun({ text, font: "Aptos", size: 21, color: colors.ink, ...options.run })], ...options });
const heading = (text, level = HeadingLevel.HEADING_1) => new Paragraph({ heading: level, spacing: { before: 260, after: 130 }, children: [new TextRun({ text, font: "Aptos Display", color: level === HeadingLevel.HEADING_1 ? colors.blue : colors.ink, bold: true })] });
const bullet = (text) => new Paragraph({ numbering: { reference: "report-bullets", level: 0 }, spacing: { after: 80, line: 260 }, children: [new TextRun({ text, font: "Aptos", size: 20, color: colors.ink })] });
const caption = (text) => new Paragraph({ alignment: AlignmentType.CENTER, spacing: { before: 80, after: 160 }, children: [new TextRun({ text, font: "Aptos", size: 18, italics: true, color: colors.gray })] });
const cell = (text, width, options = {}) => new TableCell({
  width: { size: width, type: WidthType.DXA },
  margins: { top: 90, bottom: 90, left: 110, right: 110 },
  shading: { type: ShadingType.CLEAR, fill: options.fill ?? "FFFFFF" },
  children: [new Paragraph({ spacing: { after: 0, line: 240 }, children: [new TextRun({ text, font: "Aptos", size: options.size ?? 18, bold: options.bold ?? false, color: options.color ?? colors.ink })] })],
});
const table = (headers, rows, widths) => new Table({
  width: { size: widths.reduce((a, b) => a + b, 0), type: WidthType.DXA },
  columnWidths: widths,
  rows: [
    new TableRow({ tableHeader: true, children: headers.map((value, index) => cell(value, widths[index], { fill: colors.blue, color: "FFFFFF", bold: true })) }),
    ...rows.map((row, rowIndex) => new TableRow({ children: row.map((value, index) => cell(value, widths[index], { fill: rowIndex % 2 ? "F5F8FA" : "FFFFFF" })) })),
  ],
  borders: Object.fromEntries(["top", "bottom", "left", "right", "insideHorizontal", "insideVertical"].map((key) => [key, { style: BorderStyle.SINGLE, size: 5, color: colors.line }])),
});
const hyperlink = (label, url) => new ExternalHyperlink({ link: url, children: [new TextRun({ text: label, style: "Hyperlink", font: "Aptos", size: 20 })] });
const link = (label, url) => new Paragraph({ spacing: { after: 100 }, children: [hyperlink(label, url)] });
// Bloque de comandos (monoespaciado, fondo gris claro). lines: array de strings.
const code = (lines) => new Paragraph({
  spacing: { before: 40, after: 120, line: 240 },
  shading: { type: ShadingType.CLEAR, fill: "F2F4F7" },
  border: { top: { style: BorderStyle.SINGLE, size: 4, color: colors.line }, bottom: { style: BorderStyle.SINGLE, size: 4, color: colors.line }, left: { style: BorderStyle.SINGLE, size: 10, color: colors.blue }, right: { style: BorderStyle.SINGLE, size: 4, color: colors.line } },
  children: lines.flatMap((l, i) => {
    const run = new TextRun({ text: l, font: "Consolas", size: 17, color: l.startsWith("#") ? colors.gray : colors.ink });
    return i === 0 ? [run] : [new TextRun({ break: 1, text: l, font: "Consolas", size: 17, color: l.startsWith("#") ? colors.gray : colors.ink })];
  }),
});
const step = (text) => new Paragraph({ spacing: { before: 60, after: 50 }, children: [new TextRun({ text, font: "Aptos", size: 20, bold: true, color: colors.blue })] });
const expect = (text) => new Paragraph({ spacing: { after: 140 }, children: [new TextRun({ text: "Resultado esperado: ", font: "Aptos", size: 19, bold: true, color: colors.green }), new TextRun({ text, font: "Aptos", size: 19, color: colors.ink })] });
const figure = (name, width = 640) => {
  const data = fs.readFileSync(path.join(evidence, name));
  const imageWidth = data.readUInt32BE(16);
  const imageHeight = data.readUInt32BE(20);
  const height = Math.round(width * imageHeight / imageWidth);
  return new ImageRun({ type: "png", data, transformation: { width, height } });
};
const fig = (name, cap, width = 640) => {
  return [
    new Paragraph({ alignment: AlignmentType.CENTER, spacing: { before: 80, after: 0 }, children: [figure(name, width)] }),
    caption(cap),
  ];
};
// Bloque de hallazgo estructurado
const finding = (code, title, rows) => {
  const out = [heading(`${code}. ${title}`, HeadingLevel.HEADING_2)];
  for (const [k, v] of rows) {
    out.push(new Paragraph({ spacing: { after: 90, line: 266 }, children: [
      new TextRun({ text: `${k}: `, font: "Aptos", size: 20, bold: true, color: colors.blue }),
      new TextRun({ text: v, font: "Aptos", size: 20, color: colors.ink }),
    ] }));
  }
  return out;
};

const children = [];

// ---------- PORTADA ----------
children.push(new Paragraph({ spacing: { before: 1100, after: 160 }, children: [new TextRun({ text: "INFORME DE SEGURIDAD", font: "Aptos", size: 24, bold: true, color: colors.blue, characterSpacing: 30 })] }));
children.push(new Paragraph({ spacing: { after: 100 }, children: [new TextRun({ text: "Evaluacion pasiva con OWASP ZAP", font: "Aptos Display", size: 42, bold: true, color: colors.ink })] }));
children.push(new Paragraph({ spacing: { after: 300 }, children: [new TextRun({ text: "ENLYCE / L&C Propiedad Raiz  ·  Despliegue en produccion (Railway)", font: "Aptos", size: 24, color: colors.gray })] }));
children.push(table(["Campo", "Valor"], [
  ["Fecha de la evaluacion", "12 de octubre de 2026"],
  ["Tipo de prueba", "Pasiva autorizada: spider controlado + analisis pasivo. Sin Active Scan."],
  ["Alcance", "lyc-web-production.up.railway.app  y  enlyce-production.up.railway.app"],
  ["Herramienta principal", "OWASP ZAP 2.17.0 (daemon, modo protected restringido al alcance)"],
  ["Autorizacion", "Dominios propios del proyecto, puestos en alcance por el responsable"],
  ["Autor", "Equipo ENLYCE - seguridad"],
], [3200, 6400]));
children.push(p("Documento de trabajo interno. Contiene URLs, estados HTTP y capturas reales de la ejecucion. No incluye secretos, cookies, tokens ni datos personales.", { run: { size: 18, italics: true, color: colors.gray } }));

children.push(new Paragraph({ children: [new PageBreak()] }));

// ---------- 1. RESUMEN EJECUTIVO ----------
children.push(heading("1. Resumen ejecutivo"));
children.push(p("Se realizo una evaluacion de seguridad autorizada, no intrusiva, sobre los dos dominios de produccion de ENLYCE usando OWASP ZAP 2.17.0. La prueba se limito a navegacion/spider controlado y analisis pasivo de las respuestas: no se ejecuto Active Scan, ni fuzzing, ni fuerza bruta, ni envio de formularios, ni creacion o borrado de datos. Todas las solicitudes fueron GET (y un OPTIONS de verificacion CORS) con tasa limitada."));
children.push(p("Primer resultado del reconocimiento: la API real es enlyce-production, no al reves de lo supuesto. Ese host sirve la SPA (React/Vite) y expone la API bajo /api, /health. El dominio lyc-web-production es la web publica (Next.js). El 404 previo de /api/public/inmuebles quedo desactualizado: hoy responde 200."));
children.push(p("ZAP no reporto ninguna alerta de riesgo Alto. Los hallazgos se concentran en endurecimiento de cabeceras HTTP: la web publica no envia HSTS y su CSP admite unsafe-inline; la SPA de la API carece de Subresource Integrity. El control de acceso de la API resiste: todo endpoint privado respondio 401 sin cuerpo ante peticiones anonimas, y CORS no refleja origenes arbitrarios."));
children.push(heading("Conteo de alertas ZAP", HeadingLevel.HEADING_2));
children.push(table(["Riesgo", "Tipos distintos", "Instancias (por URL)"], [
  ["Alto", "0", "0"],
  ["Medio", "4", "98"],
  ["Bajo", "2", "73"],
  ["Informativo", "4", "35"],
], [3200, 3200, 3200]));
children.push(p("Los tipos distintos son clases de alerta; las instancias cuentan cada URL afectada (p. ej. HSTS ausente se repite en 71 recursos de la web publica). Totales verificados contra la API de ZAP en esta ejecucion.", { run: { size: 18, italics: true, color: colors.gray } }));
children.push(...fig("zap-report-top.png", "Figura 1. Reporte HTML generado por ZAP 2.17.0 en esta ejecucion (12 oct 2026, 15:07). Resumen de alertas: 0 Altas.", 470));

children.push(new Paragraph({ children: [new PageBreak()] }));

// ---------- 2. ALCANCE, HERRAMIENTAS Y METODOLOGIA ----------
children.push(heading("2. Alcance, herramientas y metodologia"));
children.push(heading("Alcance autorizado", HeadingLevel.HEADING_2));
children.push(bullet("https://lyc-web-production.up.railway.app y rutas del mismo origen."));
children.push(bullet("https://enlyce-production.up.railway.app y rutas del mismo origen."));
children.push(bullet("No se siguieron enlaces a terceros ni se tocaron otros hosts. El contexto de ZAP se limito por expresion regular a esos dos origenes (modo protected)."));
children.push(heading("Herramientas", HeadingLevel.HEADING_2));
children.push(bullet("OWASP ZAP 2.17.0 en modo daemon, escaner pasivo + spider tradicional (1 hilo, profundidad 5, 15 hijos, duracion 3 min, ritmo limitado)."));
children.push(bullet("curl a traves del proxy de ZAP para sembrar solicitudes GET de bajo impacto y para capturar cabeceras crudas con URL y hora del servidor."));
children.push(bullet("Chrome headless para renderizar el reporte HTML de ZAP y las respuestas capturadas como imagenes."));
children.push(heading("Metodologia", HeadingLevel.HEADING_2));
children.push(p("1) Identificacion del servicio en cada dominio con metodos de bajo impacto. 2) Descubrimiento de rutas por sitemap y por spider controlado dentro del alcance. 3) Analisis pasivo de todas las respuestas por las reglas de ZAP. 4) Verificacion manual de control de acceso (GET anonimo contra endpoints privados) y de CORS (preflight con origen hostil). 5) Clasificacion de cada resultado separando confirmado de potencial. No se ejecuto ninguna prueba que modificara estado."));
children.push(p("ZAP arranco en modo safe; como safe bloquea incluso el spider, se cambio a protected, que permite spider pero restringe toda solicitud al contexto en alcance. El Active Scan nunca se invoco."));

children.push(new Paragraph({ children: [new PageBreak()] }));

// ---------- 3. IDENTIFICACION DE SERVICIOS ----------
children.push(heading("3. Identificacion de servicios (verificacion previa)"));
children.push(p("Antes de escanear se confirmo que servicio responde en cada dominio y cual es la API, con evidencia:"));
children.push(table(["Prueba (GET)", "lyc-web-production", "enlyce-production"], [
  ["/ (raiz)", "200, HTML Next.js", "200, HTML SPA (React/Vite)"],
  ["/robots.txt", "200 text/plain", "404"],
  ["/sitemap.xml", "200 (rutas publicas)", "404"],
  ["/health", "404", "200 application/json {status:healthy}"],
  ["/api/public/inmuebles", "404", "200 application/json {page,total,items}"],
  ["/swagger/v1/swagger.json", "404", "404"],
], [3000, 3300, 3300]));
children.push(p("Conclusion: enlyce-production es la API (ASP.NET Core) que ademas sirve la SPA; lyc-web-production es la web publica. Swagger no esta expuesto en produccion en ninguno de los dos. La enumeracion del bundle JS de la SPA revelo la superficie privada de la API (ver seccion 5)."));

children.push(new Paragraph({ children: [new PageBreak()] }));

// ---------- 4. HALLAZGOS ----------
children.push(heading("4. Hallazgos"));
children.push(p("Cada hallazgo indica amenaza, severidad justificada, como se descubrio, como podria aprovecharse (sin ejecutar acciones daninas), impacto, evidencia y solucion concreta. La severidad mostrada es la de ZAP; se matiza el riesgo practico segun el contexto."));

// M-01 CSP unsafe-inline
children.push(...finding("M-01", "CSP con unsafe-inline en la web publica (Medio)", [
  ["Amenaza", "Debilitamiento de la proteccion contra Cross-Site Scripting (XSS) e inyeccion de datos. CWE-693 (Protection Mechanism Failure), WASC-15."],
  ["Severidad y justificacion", "Medio (confianza Alta de ZAP). El CSP de lyc-web declara script-src 'self' 'unsafe-inline' y style-src 'self' 'unsafe-inline'. 'unsafe-inline' en script-src anula gran parte del valor del CSP: si existiera un punto de inyeccion, el navegador ejecutaria scripts inline sin bloquearlos."],
  ["Como se descubrio", "Regla pasiva 10055 de ZAP sobre la cabecera Content-Security-Policy de las respuestas de lyc-web; confirmado leyendo la cabecera cruda. 45 instancias para script-src y 45 para style-src."],
  ["Explicacion tecnica", "Un CSP robusto evita XSS prohibiendo scripts inline y exigiendo nonce/hash. Con 'unsafe-inline', una carga como <script>...</script> reflejada o almacenada se ejecutaria pese al CSP. No se identifico ni se probo ningun punto de inyeccion real: el hallazgo es de configuracion, no una explotacion demostrada."],
  ["Impacto", "Reduce la defensa en profundidad frente a XSS. El impacto se materializa solo si coexiste con una vulnerabilidad de inyeccion."],
  ["Solucion concreta", "Eliminar 'unsafe-inline' de script-src. En Next.js, usar nonce por respuesta (CSP con 'nonce-<valor>' y 'strict-dynamic') o hashes de los estilos/estilos criticos. Para style-src, migrar a nonce/hash o a clases en lugar de estilos inline. Mantener object-src 'none' y base-uri 'self' (ya presentes)."],
]));
children.push(...fig("01-lycweb-root-headers.png", "Figura 2. Respuesta de https://lyc-web-production.up.railway.app/ con el CSP que incluye unsafe-inline (URL y Date del servidor visibles).", 600));

// M-02 SRI
children.push(...finding("M-02", "Subresource Integrity ausente en la SPA de la API (Medio)", [
  ["Amenaza", "Ejecucion de codigo de terceros alterado. Si el origen externo (o un intermediario) sirviera un recurso manipulado, el navegador lo cargaria sin verificar su integridad. CWE-345 (Insufficient Verification of Data Authenticity)."],
  ["Severidad y justificacion", "Medio (confianza Alta de ZAP). La pagina raiz de enlyce-production carga una hoja de estilos de fonts.googleapis.com y recursos locales sin atributo integrity."],
  ["Como se descubrio", "Regla pasiva 90003 de ZAP sobre las etiquetas link/script de la SPA; evidencia: el <link> a Google Fonts sin integrity."],
  ["Explicacion tecnica", "SRI permite declarar un hash esperado (integrity=sha384-...) para que el navegador rechace el recurso si no coincide. Sin SRI, un compromiso del CDN externo o un ataque en ruta podria inyectar CSS/JS. No se demostro manipulacion; es un control ausente."],
  ["Impacto", "Exposicion a cadena de suministro del front en caso de compromiso del origen externo. Impacto practico moderado por tratarse principalmente de una hoja de fuentes."],
  ["Solucion concreta", "Anadir integrity + crossorigin a las etiquetas de recursos externos. Para los bundles locales servidos por la propia app, versionarlos con hash (ya lo hace Vite) y considerar auto-alojar las fuentes para eliminar la dependencia externa. Reforzar el CSP de la API para limitar style-src/font-src a origenes concretos."],
]));
children.push(...fig("05-enlyce-root-sri.png", "Figura 3. HTML raiz de la API sin atributo integrity en el recurso externo de fuentes.", 600));

// M-03 CSP sin fallback
children.push(...finding("M-03", "CSP de la API: directiva sin fallback no definida (Medio)", [
  ["Amenaza", "Directiva de CSP sin valor por defecto que quede implicitamente permisiva. CWE-693, WASC-15."],
  ["Severidad y justificacion", "Medio segun ZAP (regla 10055). El CSP de la API es default-src 'none'; frame-ancestors 'none'; base-uri 'none'. ZAP marca que directivas que no heredan de default-src (p. ej. form-action) no estan declaradas explicitamente. Riesgo practico bajo porque default-src 'none' ya es restrictivo y la API sirve JSON o una SPA propia."],
  ["Como se descubrio", "Regla pasiva 10055 sobre la cabecera CSP de enlyce-production. 4 instancias."],
  ["Explicacion tecnica", "Algunas directivas (form-action, frame-ancestors, base-uri, navigate-to) no heredan de default-src. Si no se declaran, su comportamiento depende del navegador. Aqui frame-ancestors y base-uri si estan fijadas; faltaria declarar form-action 'none'."],
  ["Impacto", "Marginal en el estado actual; endurecimiento recomendado para evitar envios de formularios a destinos no previstos si en el futuro se sirve HTML con formularios."],
  ["Solucion concreta", "Anadir form-action 'none' (o 'self' si aplica) al CSP de la API. Revisar que default-src 'none' no rompa recursos legitimos de la SPA servida por ese host."],
]));

// L-01 HSTS
children.push(...finding("L-01", "Strict-Transport-Security ausente en la web publica (Bajo, alto valor practico)", [
  ["Amenaza", "Degradacion a HTTP y ataques en ruta (SSL stripping) en la primera visita o tras expiracion de cache. CWE-319 (Cleartext Transmission)."],
  ["Severidad y justificacion", "Bajo (confianza Alta de ZAP), pero con valor practico alto: la API (enlyce) si envia HSTS (max-age 15552000; includeSubDomains) y la web publica (lyc-web) no envia la cabecera en ninguna respuesta. 71 instancias."],
  ["Como se descubrio", "Regla pasiva 10035 sobre respuestas de lyc-web; confirmado en la respuesta cruda de /robots.txt y de la raiz, donde no aparece strict-transport-security."],
  ["Explicacion tecnica", "Sin HSTS, un atacante en la red puede intentar forzar HTTP y interceptar o modificar el trafico antes de la redireccion a HTTPS. HSTS instruye al navegador a usar siempre HTTPS para el dominio."],
  ["Impacto", "Ventana de intercepcion en redes no confiables. Afecta a toda la web publica."],
  ["Solucion concreta", "Emitir Strict-Transport-Security: max-age=31536000; includeSubDomains; preload desde el servidor/edge de la web publica (Next.js headers() o configuracion del proxy de Railway), igualando el estandar que ya aplica la API."],
]));
children.push(...fig("02-lycweb-hsts-ausente.png", "Figura 4. Respuesta de la web publica sin cabecera Strict-Transport-Security (URL y Date visibles).", 600));

// L-02 timestamp
children.push(...finding("L-02", "Divulgacion de marca de tiempo Unix (Bajo / probable falso positivo)", [
  ["Amenaza", "Posible fuga de informacion por marcas de tiempo. CWE-497. 2 instancias en lyc-web."],
  ["Severidad y justificacion", "Bajo (confianza Baja). La evidencia es el valor 2000000000, una constante numerica dentro del JavaScript empaquetado, no una fecha real del servidor."],
  ["Como se descubrio", "Regla pasiva 10096 sobre recursos JS de lyc-web."],
  ["Explicacion tecnica", "ZAP marca cualquier entero que parezca un epoch. Aqui corresponde a logica del bundle, no a un timestamp sensible."],
  ["Impacto", "Nulo o insignificante."],
  ["Solucion concreta", "Verificar manualmente y tratar como falso positivo salvo que el valor represente datos sensibles."],
]));

// Informativos agrupados
children.push(...finding("I-01..04", "Hallazgos informativos", [
  ["User Controllable HTML Attribute (potencial XSS)", "16 instancias en lyc-web, parametro operation de /inmuebles. Es un punto caliente para revision manual, no una XSS confirmada. Accion: validar y codificar la salida; revisar que el valor no llegue a un atributo HTML sin sanear."],
  ["Information Disclosure - Suspicious Comments", "2 instancias en el bundle de la API (fragmentos como leadId). Codigo minificado; accion: revisar que no se filtren comentarios con logica o rutas sensibles en el build."],
  ["Re-examine Cache-control", "13 instancias en ambos. Revisar que ningun recurso con contenido sensible sea cacheable; para estaticos, cache-control inmutable es aceptable."],
  ["Modern Web Application", "Informativo, sin accion: la SPA requiere un client spider para cobertura dinamica."],
]));

children.push(new Paragraph({ children: [new PageBreak()] }));

// ---------- 5. RESULTADOS NEGATIVOS / CONTROLES VERIFICADOS ----------
children.push(heading("5. Resultados negativos relevantes (controles que resisten)"));
children.push(p("Estos controles se verificaron de forma segura y pasaron. Un resultado negativo cubre solo lo probado en esta ejecucion; no demuestra ausencia total de vulnerabilidades."));
children.push(heading("Control de acceso de la API", HeadingLevel.HEADING_2));
children.push(p("Se enumeraron 34 rutas /api desde el bundle de la SPA y se probaron GET anonimos contra los endpoints privados. Todos respondieron 401 con cuerpo vacio (0 bytes): auth/me, asesores, pipeline, visitas, tareas, alertas, contactos, demandas, publicaciones, analytics/funnel, analytics/crm-report, configuracion/sla-leads, opportunities/export.csv, billing/pricing. Las rutas tipo coleccion que exigen subruta devolvieron 404. No hubo fuga de datos ni de mensajes de error."));
children.push(...fig("07-control-acceso-401.png", "Figura 5. Endpoints privados: 401 sin cuerpo ante peticiones anonimas (solo GET).", 560));
children.push(heading("CORS", HeadingLevel.HEADING_2));
children.push(p("Un preflight OPTIONS con Origin https://evil.example y Access-Control-Request-Method GET contra /api/public/inmuebles devolvio 204 sin cabecera Access-Control-Allow-Origin: la API no refleja origenes arbitrarios."));
children.push(...fig("06-cors-preflight-evil.png", "Figura 6. Preflight CORS con origen hostil: 204 sin Access-Control-Allow-Origin.", 560));
children.push(heading("Cabeceras de la API", HeadingLevel.HEADING_2));
children.push(p("La API (enlyce-production) envia HSTS, CSP default-src 'none', X-Content-Type-Options nosniff, X-Frame-Options DENY y Permissions-Policy restrictiva. El catalogo publico responde sin exponer propietario ni direccion exacta (en esta prueba total:0, catalogo vacio)."));
children.push(...fig("03-enlyce-api-headers.png", "Figura 7. Cabeceras de seguridad de la API en /health.", 560));
children.push(heading("Ausencia de alertas Altas", HeadingLevel.HEADING_2));
children.push(p("El analisis pasivo no genero ninguna alerta de riesgo Alto en los recursos recorridos. ZAP no observo inyeccion evidente en las respuestas analizadas. Importante: no se ejecuto Active Scan, por lo que esta evaluacion no prueba ni descarta SQLi, XSS reflejado/almacenado, traversal u otras clases que requieren pruebas activas."));

children.push(new Paragraph({ children: [new PageBreak()] }));

// ---------- 6. RECOMENDACIONES PRIORIZADAS ----------
children.push(heading("6. Recomendaciones priorizadas"));
children.push(table(["#", "Accion", "Hallazgo", "Prioridad", "Esfuerzo"], [
  ["1", "Emitir HSTS en la web publica (igualar a la API)", "L-01", "Alta", "Bajo"],
  ["2", "Quitar unsafe-inline del CSP de la web publica (nonce/hash)", "M-01", "Alta", "Medio"],
  ["3", "Anadir Subresource Integrity / auto-alojar fuentes en la SPA", "M-02", "Media", "Bajo"],
  ["4", "Declarar form-action en el CSP de la API", "M-03", "Media", "Bajo"],
  ["5", "Revisar el punto caliente XSS del parametro operation", "I-01", "Media", "Medio"],
  ["6", "Revisar cache-control en recursos sensibles", "I-03", "Baja", "Bajo"],
  ["7", "Depurar comentarios sospechosos del build", "I-02", "Baja", "Bajo"],
  ["8", "Confirmar L-02 como falso positivo", "L-02", "Baja", "Bajo"],
], [600, 4200, 1400, 1700, 1700]));

// ---------- 7. LIMITACIONES ----------
children.push(heading("7. Limitaciones"));
children.push(bullet("Evaluacion pasiva: sin Active Scan, sin fuzzing, sin fuerza bruta, sin autenticacion y sin envio de formularios. La ausencia de alertas activas no demuestra ausencia de vulnerabilidades."));
children.push(bullet("El spider tradicional cubre poco de una SPA/Next.js con rutas en cliente; parte de la superficie dinamica no se recorrio."));
children.push(bullet("El catalogo publico estaba vacio (total:0), por lo que no se pudo ejercitar a fondo la exposicion de campos de inmuebles."));
children.push(bullet("No se probaron flujos autenticados ni autorizacion horizontal con credenciales reales; solo se verifico el rechazo anonimo."));
children.push(bullet("ZAP intercepta TLS con su propio CA, por lo que la validez del certificado real del sitio no se evaluo en esta prueba por proxy."));
children.push(p("Un 0 alertas de ZAP no demuestra ausencia de vulnerabilidades. Se recomienda complementar con ZAP en staging autorizado con Active Scan, revision autenticada por rol y auditoria de dependencias.", { run: { italics: true, color: colors.gray, size: 19 } }));

// ---------- 8. FUENTES ----------
children.push(heading("8. Fuentes oficiales"));
children.push(link("OWASP ZAP - documentacion y escaneo pasivo", "https://www.zaproxy.org/docs/"));
children.push(link("OWASP Secure Headers Project", "https://owasp.org/www-project-secure-headers/"));
children.push(link("MDN - Content-Security-Policy", "https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/Content-Security-Policy"));
children.push(link("OWASP Cheat Sheet - HTTP Strict Transport Security", "https://cheatsheetseries.owasp.org/cheatsheets/HTTP_Strict_Transport_Security_Cheat_Sheet.html"));
children.push(link("MDN - Subresource Integrity", "https://developer.mozilla.org/en-US/docs/Web/Security/Subresource_Integrity"));
children.push(link("OWASP API Security Top 10 (2023)", "https://owasp.org/API-Security/editions/2023/en/0x11-t10/"));
children.push(link("CWE-693 Protection Mechanism Failure", "https://cwe.mitre.org/data/definitions/693.html"));
children.push(link("CWE-319 Cleartext Transmission of Sensitive Information", "https://cwe.mitre.org/data/definitions/319.html"));
children.push(link("CWE-345 Insufficient Verification of Data Authenticity", "https://cwe.mitre.org/data/definitions/345.html"));
children.push(p("Fuentes consultadas el 12 de octubre de 2026.", { run: { size: 18, italics: true, color: colors.gray } }));

// ---------- ANEXO A: REPRODUCCION ----------
children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(heading("Anexo A. Reproduccion paso a paso"));
children.push(p("Todos los comandos son de bajo impacto: solo GET (y un OPTIONS para CORS). No modifican estado, no autentican, no envian formularios ni crean registros. Son los mismos que generaron la evidencia de este informe. Reemplaza el valor de User-Agent si tu politica lo exige. En Windows se ejecutan en Git Bash o WSL."));
children.push(p("Variable comun usada en los ejemplos:"));
children.push(code([
  'UA="ENLYCE-AuthorizedSecurityAssessment"',
  'WEB="https://lyc-web-production.up.railway.app"',
  'API="https://enlyce-production.up.railway.app"',
]));

children.push(heading("A.1. Identificacion de servicios", HeadingLevel.HEADING_2));
children.push(step("Confirmar cual dominio es la API"));
children.push(code([
  '# La API responde JSON en /health y /api/public/inmuebles; la web no',
  'curl -sS -A "$UA" "$API/health"',
  'curl -sS -A "$UA" "$API/api/public/inmuebles"',
  'curl -sS -A "$UA" -o /dev/null -w "%{http_code}\\n" "$WEB/health"',
]));
children.push(expect("$API/health -> {\"status\":\"healthy\",...} 200; $WEB/health -> 404. enlyce-production es la API."));

children.push(heading("A.2. M-01  CSP con unsafe-inline (web publica)", HeadingLevel.HEADING_2));
children.push(code([
  'curl -sS -A "$UA" -D - -o /dev/null "$WEB/" | grep -i "content-security-policy"',
]));
children.push(expect("La cabecera incluye script-src 'self' 'unsafe-inline' y style-src 'self' 'unsafe-inline'. Ver Figura 2."));

children.push(heading("A.3. M-02  Subresource Integrity ausente (SPA de la API)", HeadingLevel.HEADING_2));
children.push(code([
  'curl -sS -A "$UA" "$API/" | grep -i -E "<link|<script"',
]));
children.push(expect("Las etiquetas link/script externas no tienen atributo integrity=. Ver Figura 3."));

children.push(heading("A.4. M-03  CSP de la API sin form-action", HeadingLevel.HEADING_2));
children.push(code([
  'curl -sS -A "$UA" -D - -o /dev/null "$API/" | grep -i "content-security-policy"',
]));
children.push(expect("CSP = default-src 'none'; frame-ancestors 'none'; base-uri 'none'. No aparece form-action."));

children.push(heading("A.5. L-01  HSTS ausente en la web publica", HeadingLevel.HEADING_2));
children.push(code([
  '# La web NO envia HSTS (sin salida); la API SI lo envia',
  'curl -sS -A "$UA" -D - -o /dev/null "$WEB/robots.txt" | grep -i "strict-transport-security" || echo "HSTS AUSENTE en WEB"',
  'curl -sS -A "$UA" -D - -o /dev/null "$API/health"      | grep -i "strict-transport-security"',
]));
children.push(expect("Primera linea: \"HSTS AUSENTE en WEB\". Segunda: strict-transport-security: max-age=15552000; includeSubDomains. Ver Figura 4."));

children.push(heading("A.6. L-02  Marca de tiempo Unix (verificar falso positivo)", HeadingLevel.HEADING_2));
children.push(code([
  '# Buscar la constante 2000000000 en el bundle; confirmar que es logica, no una fecha real',
  'curl -sS -A "$UA" "$WEB/" | grep -oE "2000000000" | head',
]));
children.push(expect("Aparece como constante numerica del JavaScript; no es un timestamp sensible del servidor."));

children.push(heading("A.7. I-01  Punto caliente XSS en parametro operation", HeadingLevel.HEADING_2));
children.push(code([
  '# Solo observacion: pedir la pagina con el parametro y revisar como se refleja en atributos HTML',
  'curl -sS -A "$UA" "$WEB/inmuebles?operation=Venta" -o /dev/null -w "%{http_code}\\n"',
]));
children.push(expect("200. Es un punto para revision manual (codificacion de salida), no una XSS confirmada. No inyectar payloads en produccion."));

children.push(heading("A.8. Control de acceso: 401 anonimo (resultado negativo)", HeadingLevel.HEADING_2));
children.push(code([
  'for P in /api/auth/me /api/asesores /api/pipeline /api/visitas /api/tareas \\',
  '         /api/alertas /api/contactos /api/demandas /api/publicaciones \\',
  '         /api/analytics/funnel /api/configuracion/sla-leads \\',
  '         /api/opportunities/export.csv /api/billing/pricing; do',
  '  printf "%-34s " "$P"',
  '  curl -sS -A "$UA" -o /dev/null -w "%{http_code} / %{size_download}b\\n" "$API$P"',
  '  sleep 0.5',
  'done',
]));
children.push(expect("Todos responden 401 con 0 bytes (sin fuga de datos ni de errores). Ver Figura 5."));

children.push(heading("A.9. CORS: origen hostil rechazado (resultado negativo)", HeadingLevel.HEADING_2));
children.push(code([
  'curl -sS -A "$UA" -X OPTIONS -D - -o /dev/null \\',
  '  -H "Origin: https://evil.example" \\',
  '  -H "Access-Control-Request-Method: GET" \\',
  '  "$API/api/public/inmuebles"',
]));
children.push(expect("204 sin cabecera Access-Control-Allow-Origin: la API no refleja origenes arbitrarios. Ver Figura 6."));

children.push(heading("A.10. Reejecutar el escaneo pasivo completo con ZAP", HeadingLevel.HEADING_2));
children.push(p("Para reproducir el analisis pasivo que consolido el conteo de alertas (modo protected, sin Active Scan):"));
children.push(code([
  '# 1) Arrancar ZAP en daemon (ajusta la ruta del jar y una API key propia)',
  'KEY="una-clave-propia"',
  'java -jar "zap-2.17.0.jar" -daemon -host 127.0.0.1 -port 8099 \\',
  '  -config api.key=$KEY',
  '',
  '# 2) Modo protected + contexto limitado a los dos origenes',
  'B="http://127.0.0.1:8099"',
  'curl -s "$B/JSON/core/action/setMode/?mode=protect&apikey=$KEY"',
  'curl -s "$B/JSON/context/action/newContext/?contextName=enlyce-scope&apikey=$KEY"',
  'curl -s "$B/JSON/context/action/includeInContext/?contextName=enlyce-scope&regex=https://lyc-web-production\\.up\\.railway\\.app.*&apikey=$KEY"',
  'curl -s "$B/JSON/context/action/includeInContext/?contextName=enlyce-scope&regex=https://enlyce-production\\.up\\.railway\\.app.*&apikey=$KEY"',
  '',
  '# 3) Spider controlado (1 hilo, ritmo limitado) sobre cada origen',
  'curl -s "$B/JSON/spider/action/setOptionThreadCount/?Integer=1&apikey=$KEY"',
  'curl -s "$B/JSON/spider/action/scan/?url=$WEB/&contextName=enlyce-scope&subtreeOnly=true&apikey=$KEY"',
  'curl -s "$B/JSON/spider/action/scan/?url=$API/&contextName=enlyce-scope&subtreeOnly=true&apikey=$KEY"',
  '',
  '# 4) (NO ejecutar Active Scan). Esperar la cola pasiva y leer alertas + reporte',
  'curl -s "$B/JSON/pscan/view/recordsToScan/?apikey=$KEY"',
  'curl -s "$B/JSON/alert/view/alertsSummary/?apikey=$KEY"',
  'curl -s "$B/JSON/reports/action/generate/?template=modern&reportFileName=enlyce-zap-report.html&reportDir=<carpeta>&apikey=$KEY"',
]));
children.push(expect("alertsSummary con 0 High. El reporte HTML (enlyce-zap-report.html) reproduce la Figura 1. Las instancias por URL pueden variar levemente segun la cobertura del spider."));
children.push(p("Nota de seguridad: mantener el alcance en los dos dominios propios, limitar la tasa y detenerse ante errores o lentitud. No ejecutar Active Scan, fuzzing ni fuerza bruta sobre produccion sin autorizacion explicita para ese tipo de prueba.", { run: { italics: true, color: colors.gray, size: 19 } }));

const border = { style: BorderStyle.SINGLE, size: 6, color: colors.blue, space: 4 };
const header = new Header({ children: [new Paragraph({ border: { bottom: border }, spacing: { after: 100 }, children: [new TextRun({ text: "ENLYCE  /  EVALUACION PASIVA OWASP ZAP", font: "Aptos", size: 16, bold: true, color: colors.blue })] })] });
const footer = new Footer({ children: [new Paragraph({ border: { top: border }, alignment: AlignmentType.RIGHT, spacing: { before: 80 }, children: [new TextRun({ text: "Informe de seguridad · 12 oct 2026 · Pagina ", font: "Aptos", size: 16, color: colors.gray }), new TextRun({ children: [PageNumber.CURRENT], font: "Aptos", size: 16, color: colors.gray })] })] });

const doc = new Document({
  numbering: { config: [{ reference: "report-bullets", levels: [{ level: 0, format: LevelFormat.BULLET, text: "•", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 420, hanging: 240 } } } }] }] },
  styles: {
    default: { document: { run: { font: "Aptos", size: 21, color: colors.ink }, paragraph: { spacing: { after: 120, line: 276 } } } },
    paragraphStyles: [
      { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", run: { font: "Aptos Display", size: 30, bold: true, color: colors.blue }, paragraph: { spacing: { before: 300, after: 140 }, outlineLevel: 0 } },
      { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", run: { font: "Aptos Display", size: 24, bold: true, color: colors.ink }, paragraph: { spacing: { before: 220, after: 100 }, outlineLevel: 1 } },
    ],
  },
  sections: [{ properties: { page: { size: { width: 11906, height: 16838 }, margin: { top: 1050, bottom: 1050, left: 1100, right: 1100, header: 400, footer: 400 } } }, headers: { default: header }, footers: { default: footer }, children }],
});

Packer.toBuffer(doc).then((buffer) => { fs.writeFileSync(output, buffer); console.log("OK ->", output); }).catch((error) => { console.error(error); process.exitCode = 1; });
