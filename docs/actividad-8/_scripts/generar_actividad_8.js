const fs = require("fs");
const path = require("path");
const {
  AlignmentType,
  BorderStyle,
  Document,
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
  TableLayoutType,
  TableRow,
  TextRun,
  VerticalAlign,
  WidthType,
  Packer,
} = require("docx");

const ROOT = path.resolve(__dirname, "..");
const EVIDENCE = path.join(ROOT, "evidencias");
const OUTPUT = path.join(ROOT, "Actividad_8_Fundamentos_Pruebas_Samuel_Escobar.docx");

const NAVY = "10203D";
const GOLD = "C89B3C";
const CREAM = "F7F2E8";
const ICE = "EAF0F5";
const BURGUNDY = "8C2F39";
const GREEN = "267A5E";
const GRAY = "5B6573";
const LIGHT_GRAY = "F1F3F5";
const WHITE = "FFFFFF";
const BLACK = "1B1F24";
const FONT = "Times New Roman";

function run(text, options = {}) {
  return new TextRun({ text, font: FONT, size: 22, color: BLACK, ...options });
}

function p(text = "", options = {}) {
  const { bold = false, italic = false, color = BLACK, size = 22, align, before = 0, after = 120, indent, keepNext = false } = options;
  return new Paragraph({
    alignment: align,
    spacing: { before, after, line: 300 },
    indent,
    keepNext,
    children: [run(text, { bold, italics: italic, color, size })],
  });
}

function rich(parts, options = {}) {
  return new Paragraph({
    alignment: options.align,
    spacing: { before: options.before || 0, after: options.after ?? 120, line: options.line || 300 },
    indent: options.indent,
    keepNext: options.keepNext || false,
    children: parts.map((part) => run(part.text, part)),
  });
}

function heading(text, level = 1) {
  return new Paragraph({
    heading: level === 1 ? HeadingLevel.HEADING_1 : level === 2 ? HeadingLevel.HEADING_2 : HeadingLevel.HEADING_3,
    spacing: { before: level === 1 ? 220 : 140, after: 100 },
    keepNext: true,
    children: [run(text, { bold: true, color: level === 1 ? NAVY : BURGUNDY, size: level === 1 ? 30 : level === 2 ? 26 : 23 })],
  });
}

function pageBreak() {
  return new Paragraph({ children: [new PageBreak()] });
}

function bullet(text, level = 0) {
  return new Paragraph({
    numbering: { reference: "bullets", level },
    spacing: { after: 80, line: 290 },
    children: [run(text)],
  });
}

function numberItem(text, level = 0, reference = "steps") {
  return new Paragraph({
    numbering: { reference, level },
    spacing: { after: 90, line: 290 },
    children: [run(text)],
  });
}

function decimalNumbering(reference) {
  return {
    reference,
    levels: [
      { level: 0, format: LevelFormat.DECIMAL, text: "%1.", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 460, hanging: 260 } } } },
      { level: 1, format: LevelFormat.LOWER_LETTER, text: "%2)", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 800, hanging: 260 } } } },
    ],
  };
}

function callout(title, text, color = GOLD) {
  return new Table({
    width: { size: 10080, type: WidthType.DXA },
    layout: TableLayoutType.FIXED,
    columnWidths: [260, 9820],
    margins: { top: 140, bottom: 140, left: 160, right: 160 },
    rows: [
      new TableRow({
        children: [
          new TableCell({
            width: { size: 260, type: WidthType.DXA },
            shading: { fill: color, type: ShadingType.CLEAR },
            borders: noBorders(),
            children: [p(" ", { after: 0 })],
          }),
          new TableCell({
            width: { size: 9820, type: WidthType.DXA },
            shading: { fill: CREAM, type: ShadingType.CLEAR },
            borders: noBorders(),
            children: [
              rich([
                { text: `${title}. `, bold: true, color: NAVY },
                { text },
              ], { after: 0 }),
            ],
          }),
        ],
      }),
    ],
  });
}

function noBorders() {
  const border = { style: BorderStyle.NONE, size: 0, color: WHITE };
  return { top: border, bottom: border, left: border, right: border, insideHorizontal: border, insideVertical: border };
}

function tableCell(text, width, options = {}) {
  return new TableCell({
    width: { size: width, type: WidthType.DXA },
    verticalAlign: VerticalAlign.CENTER,
    shading: options.fill ? { fill: options.fill, type: ShadingType.CLEAR } : undefined,
    margins: { top: 90, bottom: 90, left: 110, right: 110 },
    children: [p(text, {
      bold: options.bold || false,
      color: options.color || BLACK,
      size: options.size || 20,
      after: 0,
      align: options.align,
    })],
  });
}

function dataTable(headers, rows, widths) {
  return new Table({
    width: { size: 10080, type: WidthType.DXA },
    layout: TableLayoutType.FIXED,
    columnWidths: widths,
    margins: { top: 80, bottom: 80, left: 100, right: 100 },
    rows: [
      new TableRow({
        tableHeader: true,
        children: headers.map((header, index) => tableCell(header, widths[index], { fill: NAVY, bold: true, color: WHITE, size: 19 })),
      }),
      ...rows.map((row, rowIndex) => new TableRow({
        children: row.map((value, index) => tableCell(value, widths[index], { fill: rowIndex % 2 === 0 ? WHITE : LIGHT_GRAY, size: 19 })),
      })),
    ],
  });
}

function imageBlock(fileName, caption, width = 620, height = 390) {
  const filePath = path.join(EVIDENCE, fileName);
  if (!fs.existsSync(filePath)) throw new Error(`No existe la evidencia: ${filePath}`);
  return [
    new Paragraph({
      alignment: AlignmentType.CENTER,
      spacing: { before: 110, after: 65 },
      children: [new ImageRun({ data: fs.readFileSync(filePath), transformation: { width, height }, type: "png" })],
    }),
    p(caption, { italic: true, color: GRAY, size: 18, align: AlignmentType.CENTER, after: 130 }),
  ];
}

function codeBlock(lines) {
  return new Table({
    width: { size: 10080, type: WidthType.DXA },
    layout: TableLayoutType.FIXED,
    columnWidths: [10080],
    rows: [new TableRow({ children: [new TableCell({
      width: { size: 10080, type: WidthType.DXA },
      shading: { fill: "182235", type: ShadingType.CLEAR },
      margins: { top: 130, bottom: 130, left: 180, right: 180 },
      borders: noBorders(),
      children: lines.map((line) => new Paragraph({
        spacing: { after: 20, line: 240 },
        children: [new TextRun({ text: line || " ", font: "Consolas", size: 17, color: "F3F5F7" })],
      })),
    })] })],
  });
}

function label(text, color = GOLD) {
  return new Paragraph({
    spacing: { before: 70, after: 70 },
    children: [run(text.toUpperCase(), { bold: true, color, size: 18, characterSpacing: 35 })],
  });
}

const children = [];

// Portada
children.push(
  p("INGENIERÍA DE SOFTWARE II", { bold: true, color: GOLD, size: 20, align: AlignmentType.CENTER, before: 780, after: 240 }),
  p("ACTIVIDAD 8", { bold: true, color: NAVY, size: 48, align: AlignmentType.CENTER, after: 30 }),
  p("FUNDAMENTOS DEL PROCESO DE PRUEBAS", { bold: true, color: NAVY, size: 31, align: AlignmentType.CENTER, after: 250 }),
  p("Análisis de vulnerabilidades web aplicado a ENLYCE", { italic: true, color: BURGUNDY, size: 25, align: AlignmentType.CENTER, after: 520 }),
  callout("Producto analizado", "ENLYCE, ecosistema inmobiliario compuesto por CRM, API y sitio web público.", GOLD),
  p("Samuel Andres Escobar Saldarriaga", { bold: true, color: NAVY, size: 26, align: AlignmentType.CENTER, before: 620, after: 65 }),
  p("Trabajo individual", { italic: true, color: GRAY, size: 20, align: AlignmentType.CENTER, after: 65 }),
  p("26 de septiembre de 2026", { color: GRAY, size: 20, align: AlignmentType.CENTER, after: 420 }),
  p("Medellín, Colombia", { color: GOLD, size: 18, bold: true, align: AlignmentType.CENTER, after: 0 }),
  pageBreak(),
);

// Resumen y alcance
children.push(
  heading("Resumen ejecutivo"),
  p("Este informe documenta la instalación, ejecución y análisis de pruebas de seguridad sobre ENLYCE en un ambiente local y aislado. La evaluación combinó revisión automatizada de dependencias, análisis dinámico de la aplicación y pruebas específicas de lógica de negocio. El resultado principal fue la confirmación de una vulnerabilidad crítica de autorización a nivel de objeto —BOLA/IDOR— en el flujo público de oportunidades de propietarios."),
  p("Las herramientas automatizadas no confirmaron inyección SQL, XSS, traversal, XXE, ejecución remota ni inyección de comandos. Sin embargo, una prueba dirigida reveló que una persona que conociera el correo de otra podía recuperar el identificador de su oportunidad y modificar datos del inmueble. Este contraste demuestra por qué una auditoría seria no puede depender de un solo escáner."),
  heading("Objetivos", 2),
  bullet("Reproducir el proceso de instalación y prueba de una aplicación web."),
  bullet("Generar y organizar un reporte técnico de vulnerabilidades."),
  bullet("Explicar una vulnerabilidad confirmada, su ataque, impacto y corrección."),
  bullet("Probar una aplicación construida en una tecnología distinta al backend principal."),
  heading("Alcance y reglas éticas", 2),
  p("Las pruebas se realizaron únicamente sobre instancias locales de ENLYCE, con base de datos temporal y registros sintéticos. No se atacaron servicios de terceros, dominios públicos ni información real de clientes. Las solicitudes destructivas se limitaron al ambiente desechable."),
  callout("Criterio de evidencia", "Un hallazgo se considera confirmado solo cuando puede reproducirse y producir un efecto observable. Una alerta automática, por sí sola, no prueba una vulnerabilidad.", GREEN),
  heading("Contenido", 2),
  p("1. Contexto técnico de ENLYCE", { color: NAVY, after: 55 }),
  p("2. Actividad 8.1 — Instalación y verificación", { color: NAVY, after: 55 }),
  p("3. Reporte consolidado de vulnerabilidades", { color: NAVY, after: 55 }),
  p("4–6. Actividad 8.2 — BOLA/IDOR, evidencia y solución", { color: NAVY, after: 55 }),
  p("7–9. Actividad 8.3 — Next.js, npm audit y OWASP ZAP", { color: NAVY, after: 55 }),
  p("10. Inyección SQL y controles adicionales", { color: NAVY, after: 55 }),
  p("11. Conclusiones, referencias y trazabilidad", { color: NAVY, after: 55 }),
  pageBreak(),
);

// Contexto
children.push(
  heading("1. Contexto técnico de ENLYCE"),
  p("ENLYCE es un CRM inmobiliario con un portal público conectado por API. El núcleo usa ASP.NET Core 10, Entity Framework Core y PostgreSQL; el sitio público está construido con Next.js, React y TypeScript. El CRM interno utiliza React con Vite. La arquitectura separa dominio, aplicación, infraestructura y API."),
  dataTable(
    ["Componente", "Tecnología", "Función evaluada"],
    [
      ["API", "ASP.NET Core 10 · Minimal APIs", "Autenticación, oportunidades, catálogo y formularios"],
      ["Persistencia", "EF Core · PostgreSQL", "Acceso a datos y resistencia a inyección SQL"],
      ["Sitio público", "Next.js 16 · React 19 · TypeScript", "Captación, navegación y dependencias"],
      ["CRM", "React · Vite", "Autorización por roles y operaciones privadas"],
    ],
    [2200, 3200, 4680],
  ),
  ...imageBlock("05-enlyce-web.png", "Figura 1. Sitio público de ENLYCE ejecutándose localmente.", 610, 390),
  heading("Entorno de prueba", 2),
  p("La API principal se verificó en el puerto 5019 y el sitio web en los puertos 3000/3100. Para los ataques se usó una API temporal en el puerto 5029 y una base PostgreSQL aislada. Esta separación evitó contaminar los datos normales de desarrollo."),
);

// 8.1 metodología
children.push(
  heading("2. Actividad 8.1 — Instalación y verificación"),
  label("Preparación"),
  p("Se instalaron y validaron las herramientas necesarias para levantar la aplicación web y ejecutar pruebas de seguridad reproducibles."),
  numberItem("Abrir una terminal en el repositorio D:\\Proyectos\\Enlyce."),
  numberItem("Verificar Node.js, npm, Docker y .NET. Para este ejercicio se confirmó Node 24.16.0, npm 11.13.0 y Docker 29.5.3."),
  numberItem("Instalar las dependencias del sitio público con npm install."),
  numberItem("Levantar el sitio Next.js con npm run dev y comprobar la interfaz en http://localhost:3000."),
  numberItem("Levantar la API y PostgreSQL aislados para que las pruebas activas no alteren datos de uso normal."),
  numberItem("Ejecutar npm audit sobre las dependencias y OWASP ZAP sobre la especificación OpenAPI."),
  numberItem("Ejecutar el arnés propio de seguridad para autenticación, autorización, concurrencia y abuso de recursos."),
  ...imageBlock("01-entorno.png", "Figura 2. Verificación del entorno y de las herramientas instaladas.", 610, 330),
  heading("Herramientas", 2),
  dataTable(
    ["Herramienta", "Tipo", "Propósito"],
    [
      ["npm audit", "SCA", "Compara dependencias con vulnerabilidades conocidas"],
      ["OWASP ZAP 2.17", "DAST", "Prueba automáticamente API y sitio en ejecución"],
      ["Arnés ENLYCE", "Prueba dirigida", "Valida controles que requieren contexto del negocio"],
      ["xUnit", "Prueba automatizada", "Ejecuta regresiones unitarias e integrales"],
    ],
    [2400, 1700, 5980],
  ),
);

// Resultados
children.push(
  heading("3. Reporte consolidado de vulnerabilidades"),
  p("La siguiente tabla resume los resultados reproducibles. La prioridad considera confidencialidad, integridad, disponibilidad y facilidad de explotación."),
  dataTable(
    ["Prioridad", "Hallazgo", "Evidencia", "Estado"],
    [
      ["Crítica", "BOLA/IDOR en enriquecimiento de oportunidad", "Lectura del ID y modificación anónima con correo conocido", "Confirmado"],
      ["Alta", "Sin límite de intentos de inicio de sesión", "60 solicitudes inválidas: 60 respuestas 401 y ninguna 429", "Confirmado"],
      ["Alta", "Condición de carrera en deduplicación", "30 solicitudes simultáneas produjeron múltiples IDs", "Confirmado"],
      ["Alta", "Abuso de tamaño y registro de datos", "Campo público de 1 MB fue recibido y procesado", "Confirmado"],
      ["Media", "Cabeceras defensivas incompletas", "Sin CSP, frame-ancestors/X-Frame-Options, nosniff ni Permissions-Policy", "Confirmado"],
      ["Control", "JWT manipulado y alg:none", "Ambos rechazados con 401", "Resiste"],
      ["Control", "Acceso horizontal entre asesores", "Operaciones ajenas rechazadas con 403", "Resiste"],
      ["Control", "Inyección SQL y familias relacionadas", "ZAP no confirmó explotación", "Sin evidencia"],
    ],
    [1350, 3300, 3730, 1700],
  ),
  ...imageBlock("04-zap-api.png", "Figura 3. Resumen del análisis activo de la API con OWASP ZAP.", 610, 360),
  callout("Lectura correcta del resultado", "Cero hallazgos altos en ZAP no significa seguridad total. ZAP evaluó patrones técnicos; la falla crítica dependía de comprender el flujo de negocio.", BURGUNDY),
);

// 8.2 vulnerabilidad
children.push(
  heading("4. Actividad 8.2 — Vulnerabilidad seleccionada"),
  heading("4.1 BOLA/IDOR: autorización rota a nivel de objeto", 2),
  p("OWASP clasifica BOLA como API1:2023. Ocurre cuando una API recibe el identificador de un objeto y ejecuta una acción sin comprobar de forma suficiente que quien hace la solicitud está autorizado sobre ese objeto. El hecho de usar un GUID no corrige el problema: el identificador puede filtrarse, reutilizarse o llegar por otra respuesta."),
  heading("4.2 Cómo funciona el ataque en ENLYCE", 2),
  numberItem("El atacante conoce o adivina el correo de una persona que ya registró una oportunidad.", 0, "attack"),
  numberItem("Envía POST /api/leads con ese correo. El sistema detecta el recontacto, pero responde con el ID, nombre, correo, estado y otros datos de la oportunidad existente.", 0, "attack"),
  numberItem("Con el ID revelado, envía PUT /api/leads/{id}/owner-details de forma anónima.", 0, "attack"),
  numberItem("La API usa la combinación GUID + correo como prueba de identidad. Como ambos datos ya están disponibles, acepta la solicitud y modifica la oportunidad.", 0, "attack"),
  codeBlock([
    "POST /api/leads                     -> 201 Created",
    "respuesta: { id, nombre, email, estado, ... }",
    "",
    "PUT /api/leads/{id}/owner-details  -> 200 OK",
    "cuerpo: { email, city, neighborhood, expectedPrice, ... }",
  ]),
  heading("4.3 Impacto", 2),
  bullet("Confidencialidad: revela datos personales y metadatos de una oportunidad ajena."),
  bullet("Integridad: permite cambiar ciudad, barrio, precio esperado, mensaje y canal de contacto."),
  bullet("Negocio: contamina el pipeline comercial y puede desviar la atención del asesor."),
  bullet("Cumplimiento: aumenta el riesgo asociado al tratamiento de datos personales."),
  callout("Severidad", "Crítica. La explotación no requiere una cuenta autenticada y combina exposición de información con modificación de datos.", BURGUNDY),
);

// Evidencia
children.push(
  heading("5. Evidencia y causa raíz"),
  ...imageBlock("03-harness.png", "Figura 4. Resultado del arnés: controles resistentes y fallas confirmadas.", 610, 370),
  heading("5.1 Evidencia en el código", 2),
  p("El endpoint de creación es anónimo y devuelve el objeto completo con su ID. Si el correo ya existe, el manejador registra el recontacto y vuelve a entregar los datos guardados."),
  codeBlock([
    "var result = await handler.HandleAsync(command);",
    "return Results.Created($\"/api/leads/{result.Id}\", result);",
    ".AllowAnonymous();",
    "",
    "return new CreateLeadResponse(",
    "    saved.Id, saved.Nombre, saved.Email.Value, saved.Estado.ToString(), ...);",
  ]),
  p("El endpoint de enriquecimiento también es anónimo y valida únicamente que el correo enviado coincida con el correo de la oportunidad identificada por el GUID."),
  codeBlock([
    "var lead = await leads.GetByIdAsync(command.LeadId);",
    "var email = Email.Create(command.Email);",
    "if (lead is null || lead.Email != email) return null;",
    "lead.EnrichOwnerInquiry(...);",
  ]),
  p("La causa raíz no es el uso de GUID. Es tratar dos datos recuperables —ID y correo— como si fueran una credencial secreta, sin sesión, token de continuación ni autorización por objeto."),
);

// Solución
children.push(
  heading("6. Solución propuesta y verificación"),
  heading("6.1 Diseño seguro", 2),
  dataTable(
    ["Comportamiento actual", "Comportamiento recomendado"],
    [
      ["Un recontacto devuelve ID y datos existentes", "Responder de forma genérica sin revelar si el correo ya existía"],
      ["El correo y GUID habilitan la edición", "Emitir token aleatorio de un solo uso, corto y vinculado al flujo"],
      ["Endpoint de edición anónimo permanente", "Exigir token válido o sesión autorizada y verificar el objeto"],
      ["Sin límite visible de solicitudes", "Aplicar rate limiting por IP, correo normalizado y operación"],
      ["Deduplicación vulnerable a carreras", "Agregar restricción única/idempotencia y manejar conflictos"],
    ],
    [5040, 5040],
  ),
  heading("6.2 Controles concretos", 2),
  bullet("Devolver 202 Accepted o una respuesta genérica en recontactos; nunca exponer el objeto existente."),
  bullet("Generar un token criptográficamente aleatorio de al menos 256 bits, guardar solo su hash, expirar en 10–15 minutos y consumirlo una sola vez."),
  bullet("Vincular el token a la oportunidad y al propósito específico de completar datos."),
  bullet("Aplicar autorización a nivel de objeto en cada operación que reciba un identificador."),
  bullet("Registrar intentos fallidos, cambios y contexto técnico sin almacenar campos sensibles completos."),
  bullet("Agregar límites de tamaño de cuerpo, validación temprana y rate limiting."),
  heading("6.3 Pruebas de regresión", 2),
  numberItem("Registrar dos veces el mismo correo y comprobar que la segunda respuesta no revela ID ni datos existentes.", 0, "regression"),
  numberItem("Intentar modificar con ID y correo sin token; debe fallar con una respuesta genérica.", 0, "regression"),
  numberItem("Comprobar que un token vencido, reutilizado o ligado a otra oportunidad sea rechazado.", 0, "regression"),
  numberItem("Enviar una ráfaga controlada y verificar respuestas 429.", 0, "regression"),
  numberItem("Ejecutar nuevamente las 400 pruebas .NET y el arnés de seguridad antes del despliegue.", 0, "regression"),
);

// 8.3 tecnología distinta
children.push(
  heading("7. Actividad 8.3 — Aplicación en otra tecnología"),
  p("Para esta evidencia se seleccionó el sitio público de ENLYCE, desarrollado con Next.js, React y TypeScript. Es una tecnología distinta al backend principal, que está construido en C# con ASP.NET Core. La prueba combinó análisis de dependencias y análisis dinámico."),
  heading("7.1 Instalación del sitio Next.js", 2),
  numberItem("Instalar Node.js y comprobar la versión con node --version.", 0, "install"),
  numberItem("Abrir una terminal en apps/website/sitio.", 0, "install"),
  numberItem("Ejecutar npm install para restaurar exactamente el árbol de dependencias definido por el proyecto.", 0, "install"),
  numberItem("Ejecutar npm run dev.", 0, "install"),
  numberItem("Abrir http://localhost:3000 y comprobar navegación, catálogo y formularios.", 0, "install"),
  codeBlock([
    "cd D:\\Proyectos\\Enlyce\\apps\\website\\sitio",
    "node --version",
    "npm --version",
    "npm install",
    "npm run dev",
  ]),
  ...imageBlock("05-enlyce-web.png", "Figura 5. Verificación funcional del sitio construido con Next.js y TypeScript.", 610, 390),
);

// npm audit
children.push(
  heading("8. Prueba de dependencias con npm audit"),
  p("npm audit envía una descripción del árbol de dependencias al registro configurado y devuelve vulnerabilidades conocidas con su nivel y posible corrección. En ENLYCE el comando terminó con código 0 y reportó cero vulnerabilidades conocidas en 501 paquetes al momento de la prueba."),
  codeBlock([
    "cd apps\\website\\sitio",
    "npm audit",
    "",
    "resultado: found 0 vulnerabilities",
  ]),
  ...imageBlock("02-npm-audit.png", "Figura 6. Resultado real de npm audit sobre el sitio público.", 610, 330),
  callout("Límite de la herramienta", "Un resultado limpio solo cubre vulnerabilidades conocidas en dependencias. No evalúa autorización, lógica de negocio, configuración del servidor ni código propio.", GOLD),
  heading("Interpretación", 2),
  p("El resultado es positivo, pero no suficiente para afirmar que la aplicación es segura. Por eso se complementó con ZAP y con el arnés dirigido. Esta defensa por capas permitió descubrir una falla crítica que no dependía de una biblioteca vulnerable."),
);

// ZAP
children.push(
  heading("9. Prueba dinámica con OWASP ZAP"),
  p("ZAP API Scan importa la definición OpenAPI y ejecuta un análisis activo ajustado a APIs. La ejecución de ENLYCE importó 157 URLs. El reporte consolidó 271 puntos de prueba, 119 reglas aprobadas, cero alertas altas, cero medias, cuatro bajas y siete informativas."),
  heading("9.1 Procedimiento", 2),
  numberItem("Exponer el documento OpenAPI de la API local.", 0, "zap"),
  numberItem("Montar la carpeta de reportes en el contenedor oficial de ZAP.", 0, "zap"),
  numberItem("Ejecutar el API Scan contra el ambiente aislado.", 0, "zap"),
  numberItem("Revisar manualmente alertas, códigos 5xx y posibles falsos positivos.", 0, "zap"),
  numberItem("Cruzar el resultado con pruebas dirigidas de autenticación y autorización.", 0, "zap"),
  codeBlock([
    "docker run --rm --network host \\",
    "  -v <reportes>:/zap/wrk/:rw -t zaproxy/zap-stable \\",
    "  zap-api-scan.py -t http://127.0.0.1:5029/openapi/v1.json \\",
    "  -f openapi -r zap-api-active.html -J zap-api-active.json",
  ]),
  ...imageBlock("04-zap-api.png", "Figura 7. Reporte generado por ZAP para la API de ENLYCE.", 610, 360),
  heading("9.2 Hallazgos de configuración", 2),
  p("El análisis pasivo del sitio identificó ausencia de Content-Security-Policy, protección contra framing, X-Content-Type-Options: nosniff y Permissions-Policy. También observó exposición de X-Powered-By. Estas fallas no equivalen a ejecución inmediata, pero reducen la defensa en profundidad."),
);

// SQL y demás pruebas
children.push(
  heading("10. Pruebas de inyección SQL y controles adicionales"),
  heading("10.1 Inyección SQL", 2),
  p("Sí se probaron ataques de inyección SQL. ZAP ejecutó reglas genéricas y específicas de PostgreSQL, incluidas variantes basadas en error, booleanos y tiempo. No se confirmó inyección SQL en los endpoints evaluados. El uso habitual de LINQ y EF Core reduce el riesgo porque los valores se parametrizan; aun así, cualquier uso futuro de SQL crudo debe revisarse."),
  dataTable(
    ["Familia de ataque", "Resultado", "Observación"],
    [
      ["SQL Injection", "Sin confirmación", "No hubo respuesta que demostrara ejecución de SQL inyectado"],
      ["PostgreSQL time-based", "Sin confirmación", "No se observó retraso atribuible al payload"],
      ["XSS", "Sin confirmación", "No se confirmó ejecución de script"],
      ["Path traversal / XXE", "Sin confirmación", "No se obtuvo lectura de archivo ni expansión de entidad"],
      ["RCE / command injection", "Sin confirmación", "No hubo evidencia de ejecución en el servidor"],
    ],
    [3000, 2200, 4880],
  ),
  heading("10.2 Controles que resistieron", 2),
  bullet("Los JWT con firma alterada y con alg:none fueron rechazados con 401."),
  bullet("Un asesor no pudo consultar, mover ni reasignar oportunidades ajenas; las respuestas fueron 403."),
  bullet("Un origen CORS no autorizado no recibió permiso de lectura."),
  bullet("La sobrescritura del método HTTP fue rechazada con 405."),
  heading("10.3 Precisión del resultado", 2),
  p("“Sin confirmación” no significa “imposible”. Significa que, con el alcance, entradas y tiempo de esta ejecución, no se obtuvo evidencia reproducible. La conclusión debe revisarse después de cada cambio en consultas, dependencias o endpoints."),
);

// Conclusiones
children.push(
  heading("11. Conclusiones y plan de mejora"),
  p("La práctica demostró que las pruebas de seguridad deben combinar herramientas y criterio. npm audit confirmó un árbol de dependencias sin vulnerabilidades conocidas; ZAP cubrió patrones técnicos y configuraciones; el arnés dirigido encontró el riesgo más grave al modelar el comportamiento de una persona atacante dentro del flujo real de propietarios."),
  heading("Prioridad inmediata", 2),
  numberItem("Cerrar BOLA/IDOR: respuesta pública opaca, token de continuación y autorización por objeto.", 0, "priority"),
  numberItem("Incorporar rate limiting en autenticación y formularios públicos.", 0, "priority"),
  numberItem("Corregir deduplicación concurrente con restricción de base de datos e idempotencia.", 0, "priority"),
  numberItem("Aplicar límites de tamaño, redacción de logs y cabeceras defensivas.", 0, "priority"),
  numberItem("Ejecutar xUnit, npm audit, ZAP y el arnés en integración continua.", 0, "priority"),
  callout("Conclusión central", "La ausencia de alertas críticas en un escáner no prueba ausencia de vulnerabilidades. La autorización debe probarse con escenarios del negocio y con datos aislados.", GREEN),
  heading("Resultado frente a la guía", 2),
  dataTable(
    ["Punto", "Evidencia entregada"],
    [
      ["8.1", "Instalación, herramientas, procedimiento y reporte consolidado"],
      ["8.2", "Definición, ataque, causa raíz, impacto, solución y regresión de BOLA/IDOR"],
      ["8.3", "Instalación y prueba del sitio Next.js/TypeScript con capturas reales"],
    ],
    [1600, 8480],
  ),
);

// Referencias y trazabilidad
children.push(
  heading("Referencias"),
  p("OWASP Foundation. (2023). API1:2023 Broken Object Level Authorization. https://api-security.owasp.org/editions/2023/en/0xa1-broken-object-level-authorization/", { size: 20 }),
  p("ZAP Project. (2026). ZAP API Scan. https://www.zaproxy.org/docs/docker/api-scan/", { size: 20 }),
  p("npm, Inc. (2026). npm-audit — npm Docs. https://docs.npmjs.com/cli/audit.html/", { size: 20 }),
  p("Microsoft. (2026). SQL Queries — Entity Framework Core. https://learn.microsoft.com/en-us/ef/core/querying/sql-queries", { size: 20 }),
  heading("Trazabilidad de evidencias", 2),
  dataTable(
    ["Evidencia", "Origen"],
    [
      ["Informe de auditoría", "docs/seguridad/auditoria-local-2026-09-26.md"],
      ["Arnés reproducible", "tools/security/strong-audit.mjs"],
      ["Reporte ZAP", "security-reports/zap-api-active.html"],
      ["Pruebas .NET", "400 de 400 aprobadas"],
      ["Auditoría npm", "npm audit: 0 vulnerabilidades"],
    ],
    [3000, 7080],
  ),
  heading("Anexo — Criterios de aceptación", 2),
  bullet("No divulgar identificadores ni datos de oportunidades preexistentes en flujos públicos."),
  bullet("Toda mutación de un objeto debe validar identidad, propósito y autorización."),
  bullet("Las ráfagas deben producir 429 antes de consumir recursos excesivos."),
  bullet("Las pruebas de seguridad deben ejecutarse sobre datos sintéticos y dejar evidencia reproducible."),
  p("Fin del informe.", { italic: true, color: GOLD, bold: true, align: AlignmentType.CENTER, before: 260 }),
);

const doc = new Document({
  creator: "Samuel Andres Escobar Saldarriaga",
  title: "Actividad 8 — Fundamentos del Proceso de Pruebas — ENLYCE",
  subject: "Pruebas de seguridad de software",
  keywords: "ENLYCE, OWASP, ZAP, BOLA, IDOR, npm audit, pruebas de seguridad",
  description: "Informe académico de pruebas de seguridad sobre ENLYCE.",
  styles: {
    default: {
      document: { run: { font: FONT, size: 22, color: BLACK }, paragraph: { spacing: { line: 300, after: 120 } } },
    },
    paragraphStyles: [
      { id: "Title", name: "Title", basedOn: "Normal", next: "Normal", run: { font: FONT, size: 48, bold: true, color: NAVY } },
      { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true, run: { font: FONT, size: 30, bold: true, color: NAVY }, paragraph: { spacing: { before: 220, after: 100 }, outlineLevel: 0 } },
      { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true, run: { font: FONT, size: 26, bold: true, color: BURGUNDY }, paragraph: { spacing: { before: 160, after: 90 }, outlineLevel: 1 } },
      { id: "Heading3", name: "Heading 3", basedOn: "Normal", next: "Normal", quickFormat: true, run: { font: FONT, size: 23, bold: true, color: BURGUNDY }, paragraph: { spacing: { before: 130, after: 80 }, outlineLevel: 2 } },
    ],
  },
  numbering: {
    config: [
      {
        reference: "bullets",
        levels: [
          { level: 0, format: LevelFormat.BULLET, text: "•", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 420, hanging: 220 } } } },
          { level: 1, format: LevelFormat.BULLET, text: "–", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 760, hanging: 220 } } } },
        ],
      },
      decimalNumbering("steps"),
      decimalNumbering("attack"),
      decimalNumbering("regression"),
      decimalNumbering("install"),
      decimalNumbering("zap"),
      decimalNumbering("priority"),
    ],
  },
  sections: [
    {
      properties: {
        page: {
          size: { width: 12240, height: 15840 },
          margin: { top: 900, right: 1080, bottom: 900, left: 1080, header: 420, footer: 420 },
        },
      },
      headers: {
        default: new Header({ children: [
          new Paragraph({
            alignment: AlignmentType.RIGHT,
            border: { bottom: { style: BorderStyle.SINGLE, size: 5, color: GOLD, space: 4 } },
            spacing: { after: 80 },
            children: [run("ENLYCE · FUNDAMENTOS DEL PROCESO DE PRUEBAS", { bold: true, color: NAVY, size: 16, characterSpacing: 20 })],
          }),
        ] }),
      },
      footers: {
        default: new Footer({ children: [
          new Paragraph({
            alignment: AlignmentType.CENTER,
            border: { top: { style: BorderStyle.SINGLE, size: 5, color: GOLD, space: 4 } },
            spacing: { before: 80 },
            children: [
              run("Samuel Andres Escobar Saldarriaga  ·  Ingeniería de Software II  ·  ", { color: GRAY, size: 16 }),
              run("Página ", { color: GRAY, size: 16 }),
              new TextRun({ children: [PageNumber.CURRENT], font: FONT, size: 16, color: GRAY }),
              run(" de ", { color: GRAY, size: 16 }),
              new TextRun({ children: [PageNumber.TOTAL_PAGES], font: FONT, size: 16, color: GRAY }),
            ],
          }),
        ] }),
      },
      children,
    },
  ],
});

Packer.toBuffer(doc).then((buffer) => {
  fs.writeFileSync(OUTPUT, buffer);
  console.log(OUTPUT);
  console.log(`${buffer.length} bytes`);
});
