const fs = require("fs")
const path = require("path")

const modulePath = process.argv[2] || "docx"
const {
  AlignmentType, BorderStyle, Document, Footer, HeadingLevel, ImageRun,
  PageBreak, PageNumber, PageOrientation, Packer, Paragraph, ShadingType,
  Table, TableCell, TableRow, TextRun, WidthType,
} = require(modulePath)

const root = path.resolve(__dirname, "..")
const uml = path.join(root, "uml")
const output = process.argv[3]
  ? path.resolve(process.argv[3])
  : path.join(root, "2_3_REQUERIMIENTOS_WEB_PUBLICA_ENLYCE.docx")

const colors = {
  ink: "26352F", wine: "742A3A", gold: "B08A4B", sage: "E4ECE6",
  blush: "F1E5E7", ivory: "F8F4EA", line: "C9BFAE", gray: "655F56", white: "FFFFFF",
}
const border = { style: BorderStyle.SINGLE, size: 5, color: colors.line }
const borders = { top: border, bottom: border, left: border, right: border }

function run(text, options = {}) {
  return new TextRun({ text, font: "Times New Roman", size: 21, color: colors.ink, ...options })
}

function paragraph(text, options = {}) {
  return new Paragraph({ spacing: { after: 120, line: 276 }, children: [run(text)], ...options })
}

function heading(text, level = HeadingLevel.HEADING_1) {
  return new Paragraph({
    heading: level,
    spacing: { before: level === HeadingLevel.HEADING_1 ? 250 : 170, after: 110 },
    border: { bottom: { style: BorderStyle.SINGLE, size: 9, color: colors.gold } },
    children: [run(text, { bold: true, color: colors.wine })],
  })
}

function cell(content, width, shading) {
  const children = Array.isArray(content)
    ? content
    : [paragraph(content, { spacing: { after: 0 } })]
  return new TableCell({
    width: { size: width, type: WidthType.DXA }, borders,
    margins: { top: 80, bottom: 80, left: 90, right: 90 },
    shading: shading ? { type: ShadingType.CLEAR, color: "auto", fill: shading } : undefined,
    children,
  })
}

function labelTable(rows) {
  return new Table({
    width: { size: 9360, type: WidthType.DXA }, columnWidths: [2200, 7160],
    rows: rows.map(([label, value]) => new TableRow({
      cantSplit: true,
      children: [
        cell([new Paragraph({ children: [run(label, { bold: true })] })], 2200, colors.sage),
        cell(value, 7160),
      ],
    })),
  })
}

const requirements = [
  ["RW-01", "Visitante", "Consultar portada e inmuebles destacados.", "La portada obtiene publicaciones vigentes desde la API."],
  ["RW-02", "Visitante", "Filtrar, ordenar y paginar el catálogo.", "Los filtros quedan en una URL que puede compartirse."],
  ["RW-03", "Visitante", "Consultar la ficha de un inmueble.", "Muestra descripción, precio, características, ubicación, fotos y asesor."],
  ["RW-04", "Visitante", "Visualizar fotografías publicadas.", "Las imágenes se cargan desde /media de la API."],
  ["RW-05", "Visitante", "Guardar o retirar favoritos.", "Persisten en el navegador sin exigir una cuenta."],
  ["RW-06", "Visitante", "Explorar inmuebles por zona.", "La página consulta publicaciones de la zona seleccionada."],
  ["RW-07", "Interesado", "Solicitar información sobre un inmueble.", "Crea una oportunidad o registra un recontacto."],
  ["RW-08", "Propietario", "Solicitar venta, arriendo, administración o avalúo.", "Registra el servicio y operación compatibles."],
  ["RW-09", "Titular", "Consultar y aceptar la política vigente.", "Sin autorización el formulario no permite enviar."],
  ["RW-10", "Visitante", "Recibir mensajes claros ante errores.", "Conserva los datos y muestra validaciones de la API."],
  ["RW-11", "Buscador web", "Indexar páginas públicas válidas.", "Robots, sitemap y metadatos usan URLs canónicas."],
  ["RW-12", "Visitante", "Recibir respuesta segura si un inmueble no existe.", "La web presenta una página 404."],
]

function requirementTable() {
  const widths = [850, 1500, 3100, 3910]
  const header = new TableRow({ tableHeader: true, children: ["ID", "Actor", "Requisito", "Criterio observable"].map((t, i) => cell([new Paragraph({ children: [run(t, { bold: true, color: colors.white })] })], widths[i], colors.ink)) })
  return new Table({
    width: { size: 9360, type: WidthType.DXA }, columnWidths: widths,
    rows: [header, ...requirements.map((row, index) => new TableRow({ cantSplit: true, children: row.map((t, i) => cell(t, widths[i], index % 2 ? "F7F3EB" : undefined)) }))],
  })
}

function imageBlock(file, width, height, caption) {
  return [
    new Paragraph({ alignment: AlignmentType.CENTER, spacing: { before: 100, after: 80 }, children: [new ImageRun({ data: fs.readFileSync(path.join(uml, file)), type: "png", transformation: { width, height }, altText: { title: caption, description: caption, name: file } })] }),
    new Paragraph({ alignment: AlignmentType.CENTER, spacing: { after: 150 }, children: [run(caption, { italics: true, size: 18, color: colors.gray })] }),
  ]
}

function useCase(title, rows) {
  return [heading(title, HeadingLevel.HEADING_2), labelTable(rows)]
}

const footer = new Footer({ children: [new Paragraph({ alignment: AlignmentType.CENTER, children: [run("ENLYCE WEB · Taller práctico UML · ", { size: 18, color: colors.gray }), new TextRun({ children: [PageNumber.CURRENT], font: "Times New Roman", size: 18, color: colors.gray })] })] })

const intro = [
  new Paragraph({ alignment: AlignmentType.RIGHT, spacing: { before: 160, after: 40 }, children: [run("WEB", { size: 76, bold: true, color: colors.gold })] }),
  new Paragraph({ spacing: { after: 15 }, children: [run("ENLYCE", { size: 64, bold: true })] }),
  new Paragraph({ border: { bottom: { style: BorderStyle.SINGLE, size: 18, color: colors.wine } }, spacing: { after: 260 }, children: [run("PORTAL INMOBILIARIO PÚBLICO · MODELADO UML", { size: 24, bold: true, color: colors.wine })] }),
  labelTable([
    ["Estudiante", "Samuel Andres Escobar Saldarriaga"], ["Modalidad", "Trabajo individual"],
    ["Ficha", "PENDIENTE DE COMPLETAR"], ["Celular", "PENDIENTE DE COMPLETAR"],
    ["Correo", "PENDIENTE DE COMPLETAR"], ["Fecha", "24 de septiembre de 2026"],
  ]),
  heading("Nombre del proyecto"),
  paragraph("ENLYCE WEB — Portal inmobiliario público."),
  heading("Descripción del problema"),
  paragraph("Quienes buscan comprar o arrendar vivienda necesitan explorar información confiable sin depender de mensajes dispersos. Los propietarios también requieren un canal claro para solicitar venta, arriendo, administración o avalúo. ENLYCE WEB publica el inventario aprobado, permite filtrar y consultar cada inmueble, conserva favoritos localmente y transforma formularios autorizados en oportunidades comerciales para el CRM."),
  heading("Requisitos de usuario"),
  requirementTable(),
  new Paragraph({ children: [new PageBreak()] }),
  heading("Diagrama de casos de uso"),
  ...imageBlock("casos-de-uso-web-publica.png", 545, 600, "Figura 1. Capacidades de la web pública para visitantes y propietarios."),
  heading("Descripción textual de tres casos de uso"),
  ...useCase("UW-01 — Consultar catálogo", [
    ["Actor", "Visitante."], ["Objetivo", "Encontrar inmuebles acordes con sus necesidades."],
    ["Precondiciones", "Ninguna; el catálogo es público."],
    ["Flujo principal", "Abre el catálogo, selecciona filtros, la web normaliza la URL, consulta GET /api/public/inmuebles y presenta resultados paginados."],
    ["Alternativas", "Sin resultados: estado vacío. Error de API: página de error."],
    ["Postcondición", "La búsqueda puede compartirse mediante su URL."],
  ]),
  ...useCase("UW-02 — Consultar ficha de inmueble", [
    ["Actor", "Visitante."], ["Objetivo", "Conocer la información pública de una propiedad."],
    ["Precondiciones", "Existe una publicación con el enlace solicitado."],
    ["Flujo principal", "Next.js consulta la ficha y presenta precio, características, zona, mapa aproximado, fotografías y asesor."],
    ["Alternativa", "Si no existe, presenta “Inmueble no encontrado”."],
    ["Postcondición", "Puede guardarlo, compartirlo o solicitar información."],
  ]),
  ...useCase("UW-03 — Solicitar información", [
    ["Actor", "Interesado o propietario."], ["Objetivo", "Pedir contacto de un asesor con autorización de datos."],
    ["Precondiciones", "Nombre y correo válidos; política vigente disponible."],
    ["Flujo principal", "Diligencia el formulario, acepta la política y la web envía POST /api/leads. La API crea una oportunidad o registra un recontacto y guarda el consentimiento."],
    ["Alternativas", "Sin autorización no se envía. Los datos inválidos permanecen visibles con mensajes claros."],
    ["Postcondición", "La solicitud queda trazable y el visitante recibe confirmación."],
  ]),
]

const activity = [heading("Diagrama de actividad — Solicitar información"), ...imageBlock("actividad-solicitar-informacion-web.png", 940, 660, "Figura 2. Validación, envío y registro de una solicitud web.")]
const model = [heading("Modelo de información de la web"), ...imageBlock("modelo-informacion-web-publica.png", 600, 600, "Figura 3. Información consultada y enviada por la web."), paragraph("El modelo omite identificadores técnicos y muestra únicamente los datos que comprende el visitante: catálogo, ficha, fotografías, formulario, solicitud y política.")]
const architecture = [heading("Arquitectura y conexión con la API"), ...imageBlock("arquitectura-web-publica-enlyce.png", 520, 590, "Figura 4. Conexión de Next.js con la API, PostgreSQL y las imágenes."), labelTable([
  ["Consulta", "Catálogo y ficha pasan por la API antes de leer publicaciones en PostgreSQL."],
  ["Contacto", "La API valida y registra la oportunidad comercial y el consentimiento."],
  ["Frontera", "La web nunca accede directamente a PostgreSQL ni al almacenamiento de imágenes."],
]), heading("Base de verificación", HeadingLevel.HEADING_2), paragraph("Documento contrastado contra website/sitio/src/, contratos OpenAPI y Enlyce.Api el 24 de septiembre de 2026.")]

const doc = new Document({
  creator: "Samuel Andres Escobar Saldarriaga", title: "Taller UML — Web pública de ENLYCE",
  description: "Requisitos y UML exclusivos del portal inmobiliario público de ENLYCE.",
  styles: { default: { document: { run: { font: "Times New Roman", size: 21, color: colors.ink } } }, paragraphStyles: [
    { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true, run: { font: "Times New Roman", size: 30, bold: true, color: colors.wine }, paragraph: { spacing: { before: 250, after: 110 }, outlineLevel: 0 } },
    { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true, run: { font: "Times New Roman", size: 25, bold: true, color: colors.ink }, paragraph: { spacing: { before: 170, after: 90 }, outlineLevel: 1 } },
  ] },
  sections: [
    { properties: { page: { size: { width: 12240, height: 15840 }, margin: { top: 850, right: 900, bottom: 850, left: 900 } } }, footers: { default: footer }, children: intro },
    { properties: { type: "nextPage", page: { size: { width: 12240, height: 15840, orientation: PageOrientation.LANDSCAPE }, margin: { top: 400, right: 500, bottom: 400, left: 500 } } }, footers: { default: footer }, children: activity },
    { properties: { type: "nextPage", page: { size: { width: 12240, height: 15840 }, margin: { top: 500, right: 700, bottom: 500, left: 700 } } }, footers: { default: footer }, children: model },
    { properties: { type: "nextPage", page: { size: { width: 12240, height: 15840 }, margin: { top: 500, right: 700, bottom: 500, left: 700 } } }, footers: { default: footer }, children: architecture },
  ],
})

Packer.toBuffer(doc).then((buffer) => { fs.writeFileSync(output, buffer); process.stdout.write(`${output}\n`) })
