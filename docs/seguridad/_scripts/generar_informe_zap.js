const fs = require("node:fs");
const path = require("node:path");
const {
  AlignmentType,
  BorderStyle,
  Document,
  ExternalHyperlink,
  Footer,
  Header,
  HeadingLevel,
  ImageRun,
  LevelFormat,
  PageBreak,
  PageNumber,
  Paragraph,
  ShadingType,
  Table,
  TableCell,
  TableRow,
  TextRun,
  WidthType,
  Packer,
} = require("../../actividad-8/_scripts/node_modules/docx");

const root = path.resolve(__dirname, "..");
const evidence = path.join(root, "evidencias", "2026-10-12");
const output = path.join(root, "Informe_ZAP_ENLYCE_2026-10-12.docx");
const colors = { ink: "172B4D", blue: "175A8E", pale: "EAF2F8", gray: "52606D", line: "D5DEE7", red: "A61B1B", amber: "8A5A00", green: "176B45" };

const p = (text, options = {}) => new Paragraph({ spacing: { after: 120, line: 276 }, children: [new TextRun({ text, font: "Aptos", size: 21, color: colors.ink, ...options.run })], ...options });
const heading = (text, level = HeadingLevel.HEADING_1) => new Paragraph({ heading: level, spacing: { before: 260, after: 130 }, children: [new TextRun({ text, font: "Aptos Display", color: colors.blue, bold: true })] });
const bullet = (text) => new Paragraph({ numbering: { reference: "report-bullets", level: 0 }, spacing: { after: 80, line: 260 }, children: [new TextRun({ text, font: "Aptos", size: 20, color: colors.ink })] });
const caption = (text) => new Paragraph({ alignment: AlignmentType.CENTER, spacing: { before: 80, after: 160 }, children: [new TextRun({ text, font: "Aptos", size: 18, italics: true, color: colors.gray })] });
const cell = (text, width, options = {}) => new TableCell({
  width: { size: width, type: WidthType.DXA },
  margins: { top: 90, bottom: 90, left: 110, right: 110 },
  shading: { type: ShadingType.CLEAR, fill: options.fill ?? "FFFFFF" },
  children: [new Paragraph({ spacing: { after: 0, line: 240 }, children: [new TextRun({ text, font: "Aptos", size: options.size ?? 19, bold: options.bold ?? false, color: options.color ?? colors.ink })] })],
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
const figure = (name, width = 650) => {
  const data = fs.readFileSync(path.join(evidence, name));
  const imageWidth = data.readUInt32BE(16);
  const imageHeight = data.readUInt32BE(20);
  const height = Math.round(width * imageHeight / imageWidth);
  return new ImageRun({ type: "png", data, transformation: { width, height } });
};

const children = [];
children.push(new Paragraph({ spacing: { before: 1200, after: 160 }, children: [new TextRun({ text: "INFORME DE SEGURIDAD", font: "Aptos", size: 24, bold: true, color: colors.blue, characterSpacing: 30 })] }));
children.push(new Paragraph({ spacing: { after: 180 }, children: [new TextRun({ text: "Prueba API con OWASP ZAP", font: "Aptos Display", size: 42, bold: true, color: colors.ink })] }));
children.push(new Paragraph({ spacing: { after: 360 }, children: [new TextRun({ text: "ENLYCE · ejecución local aislada · 12 de octubre de 2026", font: "Aptos", size: 23, color: colors.gray })] }));
children.push(table(["Veredicto", "Estado"], [
  ["BOLA histórica", "Corregida; la regresión local la bloquea."],
  ["ZAP API activo", "0 altas · 0 medias · 2 tipos de alerta baja · 7 informativas."],
  ["Riesgo nuevo de dependencia", "Pendiente: advisories de ImageSharp 3.1.12, incluidas altas."],
  ["Alcance", "Solo API en localhost y PostgreSQL desechable; no staging ni producción."],
], [2650, 7050]));
children.push(p("Resultado: la ruta histórica permitía enumerar una oportunidad y cambiar datos del propietario con correo e identificador. El código actual usa una continuación aleatoria, temporal y de un solo uso; el harness repitió el intento anónimo y obtuvo 404 sin revelar el ID. ZAP no confirmó vulnerabilidades altas o medias en el escaneo API de hoy. La evidencia de compilación sí reveló advisories de ImageSharp sin resolver; este informe no declara el producto libre de riesgos."));
children.push(p("Clasificación de este documento: evidencia técnica interna. No contiene credenciales, tokens ni datos reales.", { run: { size: 18, italics: true, color: colors.gray } }));
children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(heading("1. Resumen ejecutivo"));
children.push(p("Se ejecutó OWASP ZAP API Scan 2.17.0 contra la especificación OpenAPI de la API local. La definición importó 99 rutas; el escaneo procesó 256 URL. El reporte HTML registró 0 alertas altas, 0 medias, 2 tipos de alertas bajas y 7 informativas. No hubo alertas de SQL injection, XSS, traversal, XXE, ejecución de comandos ni inclusión remota."));
children.push(p("ZAP no es suficiente para validar permisos por objeto. Por eso se ejecutó también el harness ofensivo local del repositorio: la reproducción BOLA no reveló ID y la sobrescritura sin autenticación terminó en 404. El escaneo no usó datos reales, no se conectó a staging ni tocó producción."));
children.push(table(["Tema", "Resultado verificado", "Estado"], [
  ["Enumeración / modificación BOLA", "recontacto 202; ID igual no divulgado; PUT anónimo 404", "Corregido y verificado localmente"],
  ["Autorización horizontal", "asesor ajeno: lectura y cambio 403", "Verificado"],
  ["JWT", "alterado y sin firma 401; válido 200", "Verificado"],
  ["Abuso", "53/60 logins limitados; payload de 1 MB: 413", "Verificado"],
  ["Dependencias", "ImageSharp 3.1.12 con advisories moderadas y altas", "Pendiente de triage y actualización"],
], [2500, 5250, 1950]));

children.push(heading("2. Alcance y método"));
children.push(bullet("Activo: ASP.NET Core API en http://127.0.0.1:5029, con OpenAPI en /swagger/v1/swagger.json."));
children.push(bullet("Base de datos: PostgreSQL 17 temporal, nombre enlyce_zap_audit, publicada solo en 127.0.0.1:55437, sin volumen persistente."));
children.push(bullet("ZAP: imagen ghcr.io/zaproxy/zaproxy:stable; escaneo activo OpenAPI con -a. ZAP importó el contrato y lanzó las reglas API sobre las rutas descritas."));
children.push(bullet("Verificación manual automatizada: tools/security/strong-audit.mjs, con datos sintéticos para autenticación, RBAC, recontacto, carga grande, CORS, method override y concurrencia."));
children.push(bullet("Límite: no se ejecutó escaneo web autenticado con navegador, no se probó staging y la cobertura depende de lo que expone Swagger. ZAP no demuestra ausencia total de fallos."));
children.push(p("El primer intento produjo 116 respuestas 503 del webhook Wompi porque la base temporal no tenía EventsSecret. Se inyectó un secreto ficticio solo en el proceso local y se repitió el escaneo. El reporte final ya no muestra respuestas 5xx; el secreto ficticio no es válido para Wompi y no se guardó en archivos."));

children.push(heading("3. Amenaza: BOLA en la captación pública"));
children.push(p("Clase: Broken Object Level Authorization (BOLA/IDOR), alineada con OWASP API Security Top 10 API1:2023. Severidad histórica: crítica para el negocio por combinar exposición de datos personales y modificación de una oportunidad sin acreditar identidad. Estado actual: corregida y verificada contra entorno local desechable."));
children.push(heading("Cómo se descubrió", HeadingLevel.HEADING_2));
children.push(p("La revisión de seguridad previa siguió el flujo de recontacto público: comparó la respuesta al enviar un correo nuevo con la de uno ya registrado, y rastreó el identificador hasta la ruta pública de actualización de datos del propietario. La auditoría local del 26–27 de septiembre documenta el hallazgo y el flujo anterior. El harness actual ejecuta la misma clase de abuso con registros sintéticos; ZAP no fue quien descubrió BOLA."));
children.push(heading("Cómo entraba el atacante", HeadingLevel.HEADING_2));
children.push(new Paragraph({ numbering: { reference: "report-bullets", level: 0 }, spacing: { after: 80 }, children: [new TextRun({ text: "Enviar la captación pública con el correo de una persona ya registrada.", font: "Aptos", size: 20 })] }));
children.push(new Paragraph({ numbering: { reference: "report-bullets", level: 0 }, spacing: { after: 80 }, children: [new TextRun({ text: "La respuesta anterior confirmaba el registro y filtraba el ID y metadatos de la oportunidad existente.", font: "Aptos", size: 20 })] }));
children.push(new Paragraph({ numbering: { reference: "report-bullets", level: 0 }, spacing: { after: 80 }, children: [new TextRun({ text: "Con el ID obtenido y el mismo correo como prueba débil, llamar a PUT /api/leads/{id}/owner-details.", font: "Aptos", size: 20 })] }));
children.push(new Paragraph({ numbering: { reference: "report-bullets", level: 0 }, spacing: { after: 80 }, children: [new TextRun({ text: "El servidor aceptaba sobrescribir detalles del propietario. El atacante podía enumerar contactos, leer datos comerciales y contaminar o alterar el CRM.", font: "Aptos", size: 20 })] }));
children.push(p("Los pasos describen el defecto histórico documentado. No se ejecutaron contra un servidor público ni contra datos reales."));
children.push(heading("Solución aplicada", HeadingLevel.HEADING_2));
children.push(bullet("La respuesta pública no confirma si el correo ya existía y no expone ID, nombre, correo ni estado."));
children.push(bullet("Para completar una captación se emite un token aleatorio; la base conserva su hash, vence a los 20 minutos y se consume una sola vez."));
children.push(bullet("El endpoint actual es PUT /api/leads/owner-details y recibe el token, no un ID de oportunidad. Token inexistente, expirado o repetido produce la misma respuesta neutral."));
children.push(bullet("A contactos repetidos se les devuelve un token señuelo para que el formato de respuesta no sirva como oráculo de enumeración."));
children.push(p("La corrección corta la cadena en la fuente: conocer un correo deja de revelar el identificador y el identificador ya no autoriza la escritura."));

children.push(heading("4. Regresión reproducida hoy"));
children.push(p("El harness usó la base temporal y credenciales sintéticas. Resultados registrados el 12 de octubre de 2026:"));
children.push(table(["Control", "Prueba", "Respuesta"], [
  ["Recontacto público", "misma oportunidad enviada otra vez; campos comparados", "202; no ID; solo continuationToken y message"],
  ["Sobrescritura", "PUT anónimo con el identificador histórico", "404; sin modificación"],
  ["Acceso horizontal", "asesor A intenta leer/mover oportunidad de B", "403 en ambos casos"],
  ["JWT", "token alterado y alg:none", "401 / 401"],
  ["Fuerza bruta", "60 logins inválidos concurrentes", "53 respuestas 429; 7 respuestas 401"],
  ["Carrera", "30 captaciones simultáneas con la misma clave", "30 respuestas 202; 1 registro persistido"],
  ["Cuerpo grande", "captación con cuerpo de 1 MB", "413"],
  ["CORS / override", "origen hostil y method override", "origen rechazado; 405"],
], [2450, 4700, 2550]));
children.push(p("La respuesta 202 de la continuación no confirma si el token existía; esa neutralidad es intencional. El harness reporta 404 para la ruta antigua de sobrescritura, que ya no existe."));

children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(heading("5. Resultado del escaneo ZAP"));
children.push(p("La captura es una imagen de la página del reporte HTML generado por ZAP en esta ejecución. El detalle completo permanece en security-reports/zap-api-active-20261012-final.html, .json y .md, carpeta ignorada por Git."));
children.push(new Paragraph({ alignment: AlignmentType.CENTER, spacing: { before: 80, after: 0 }, children: [figure("zap-api-resumen.png", 635)] }));
children.push(caption("Figura 1. Captura del reporte ZAP API Scan, 12 oct 2026. 0 altas, 0 medias, 2 bajas."));
children.push(heading("Alertas bajas", HeadingLevel.HEADING_2));
children.push(table(["Alerta", "Instancias", "Evaluación"], [
  ["Cross-Origin-Resource-Policy ausente o inválida", "4", "Endurecimiento pendiente. Añadir same-origin si el contrato de recursos no requiere compartirlos entre orígenes."],
  ["Content-Type inesperado", "2", "Swagger UI devuelve HTML en rutas de documentación; esperado y no representa una respuesta JSON de negocio."],
], [2900, 1000, 5800]));
children.push(heading("Reglas relevantes superadas", HeadingLevel.HEADING_2));
children.push(p("120 reglas terminaron PASS. ZAP no confirmó inyección SQL, XSS reflejado o persistente, traversal, XXE, ejecución de comandos, inclusión remota ni SSRF/metadata cloud. Estos resultados cubren únicamente las peticiones y superficies recorridas en esta ejecución."));
children.push(p("El comando de ZAP devolvió código de salida 1 aunque el resumen de consola indicó FAIL-NEW=0, FAIL-INPROG=0 y WARN-NEW=2. Por ello el resultado se registra como escaneo completado con advertencias, no como pipeline verde."));

children.push(new Paragraph({ children: [new PageBreak()] }));
children.push(heading("6. Evidencia del harness"));
children.push(p("La siguiente captura es una vista del extracto del JSON del harness. El archivo completo local es security-reports/strong-audit-20261012.json; el extracto omitió el correo del administrador sintético y cualquier dato que no aporte al hallazgo."));
children.push(new Paragraph({ alignment: AlignmentType.CENTER, spacing: { before: 80, after: 0 }, children: [figure("harness-controles.png", 625)] }));
children.push(caption("Figura 2. Resultado del harness local: BOLA bloqueada y controles de sesión/autorización verificados."));

children.push(heading("7. Riesgos pendientes"));
children.push(heading("Alta: advisories de SixLabors.ImageSharp 3.1.12", HeadingLevel.HEADING_2));
children.push(p("Al compilar para levantar la API, NuGet emitió NU1903/NU1902 para SixLabors.ImageSharp 3.1.12. La salida incluye tres advisories altas y dos moderadas. Las altas describen escritura fuera de límites en rutas de procesamiento TIFF/HistogramEqualization; las moderadas incluyen asignaciones excesivas desde perfiles ICC y bucle prolongado al leer BigTIFF. GitHub Advisory Database marca 4.1.2 como versión corregida para estos avisos."));
children.push(p("Estado: pendiente de triage y actualización. El escaneo ZAP no examina la cadena de dependencias ni prueba estas rutas. La aplicabilidad real depende de los formatos aceptados y de los métodos invocados por ENLYCE; no se ejecutó una PoC contra el producto. Como el componente procesa imágenes y existen cargas públicas, la actualización y pruebas de regresión deben priorizarse antes de producción."));
children.push(heading("Baja: Cross-Origin-Resource-Policy", HeadingLevel.HEADING_2));
children.push(p("El encabezado no aparece en cuatro respuestas. No se demostró explotación; queda como defensa en profundidad pendiente. Revisar middleware global y documentar si alguna ruta necesita ser consumida desde otros orígenes."));
children.push(heading("Alcance remoto", HeadingLevel.HEADING_2));
children.push(p("ENLYCE no tiene staging remoto identificado en el runbook vigente. Este documento no sustituye ZAP en staging, configuración TLS real, verificación del proxy ni una revisión web autenticada."));

children.push(heading("8. Evidencias y fuentes"));
children.push(heading("Evidencia local", HeadingLevel.HEADING_2));
children.push(bullet("docs/seguridad/auditoria-local-2026-09-26.md: hallazgo histórico, corrección y verificación anterior."));
children.push(bullet("tools/security/strong-audit.mjs: reproducción ofensiva automatizada; resultado completo en security-reports/strong-audit-20261012.json."));
children.push(bullet("security-reports/zap-api-active-20261012-final.html, .json y .md: reporte ZAP generado en esta ejecución; carpeta local ignorada por Git."));
children.push(bullet("src/Enlyce.Api/Endpoints/Leads/LeadsModule.cs: captación y PUT /owner-details actual."));
children.push(bullet("src/Enlyce.Application/UseCases/CreateLead/CreateLeadHandler.cs y src/Enlyce.Application/Security/OwnerInquiryContinuationToken.cs: token, hash, caducidad y token señuelo."));
children.push(bullet("src/Enlyce.Infrastructure/Persistence/Repositories/LeadRepository.cs: consumo condicionado a hash, expiración y uso previo."));
children.push(bullet("src/Enlyce.Infrastructure/Enlyce.Infrastructure.csproj: referencia ImageSharp 3.1.12 verificada por restore/build."));
children.push(heading("Fuentes técnicas externas", HeadingLevel.HEADING_2));
children.push(new Paragraph({ spacing: { after: 110 }, children: [hyperlink("OWASP API Security Top 10: API1 Broken Object Level Authorization", "https://owasp.org/API-Security/editions/2023/en/0xa1-broken-object-level-authorization/")] }));
children.push(new Paragraph({ spacing: { after: 110 }, children: [hyperlink("OWASP Web Security Testing Guide: API BOLA testing", "https://owasp.org/www-project-web-security-testing-guide/latest/4-Web_Application_Security_Testing/12-API_Testing/02-API_Broken_Object_Level_Authorization/")] }));
children.push(new Paragraph({ spacing: { after: 110 }, children: [hyperlink("OWASP ZAP API Scan: alcance, ejecución y códigos de salida", "https://www.zaproxy.org/docs/docker/api-scan/")] }));
children.push(new Paragraph({ spacing: { after: 110 }, children: [hyperlink("GitHub Advisory: ImageSharp ICC CLUT allocation (GHSA-gwg2-r3hj-4w44)", "https://github.com/advisories/GHSA-gwg2-r3hj-4w44")] }));
children.push(new Paragraph({ spacing: { after: 110 }, children: [hyperlink("GitHub Advisory: ImageSharp HistogramEqualization (GHSA-j3p4-wp97-rph4)", "https://github.com/advisories/GHSA-j3p4-wp97-rph4")] }));
children.push(new Paragraph({ spacing: { after: 110 }, children: [hyperlink("GitHub Advisory: ImageSharp TIFF CCITT T4 (GHSA-j9gm-c75j-xc9q)", "https://github.com/advisories/GHSA-j9gm-c75j-xc9q")] }));
children.push(new Paragraph({ spacing: { after: 110 }, children: [hyperlink("GitHub Advisory: ImageSharp TIFF CCITT T6 (GHSA-jjfr-hcj7-qf5w)", "https://github.com/advisories/GHSA-jjfr-hcj7-qf5w")] }));
children.push(new Paragraph({ spacing: { after: 110 }, children: [hyperlink("GitHub Advisory: ImageSharp BigTIFF loop (GHSA-wmxv-xphr-5c9g)", "https://github.com/advisories/GHSA-wmxv-xphr-5c9g")] }));
children.push(p("Fuentes consultadas el 12 de octubre de 2026. La versión corregida de ImageSharp y el alcance de cada advisory deben confirmarse durante el cambio de dependencia.", { run: { size: 18, italics: true, color: colors.gray } }));

const border = { style: BorderStyle.SINGLE, size: 6, color: colors.blue, space: 4 };
const header = new Header({ children: [new Paragraph({ border: { bottom: border }, spacing: { after: 100 }, children: [new TextRun({ text: "ENLYCE  /  SEGURIDAD DE API", font: "Aptos", size: 16, bold: true, color: colors.blue })] })] });
const footer = new Footer({ children: [new Paragraph({ border: { top: border }, alignment: AlignmentType.RIGHT, spacing: { before: 80 }, children: [new TextRun({ text: "Informe local · 12 oct 2026 · Página ", font: "Aptos", size: 16, color: colors.gray }), new TextRun({ children: [PageNumber.CURRENT], font: "Aptos", size: 16, color: colors.gray })] })] });

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

Packer.toBuffer(doc).then((buffer) => fs.writeFileSync(output, buffer)).catch((error) => { console.error(error); process.exitCode = 1; });
