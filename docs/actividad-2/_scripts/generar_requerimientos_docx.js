const fs = require("fs")
const path = require("path")

const modulePath = process.argv[2] || "docx"
const {
  AlignmentType,
  BorderStyle,
  Document,
  Footer,
  HeadingLevel,
  ImageRun,
  LevelFormat,
  PageBreak,
  PageNumber,
  PageOrientation,
  Packer,
  Paragraph,
  ShadingType,
  Table,
  TableCell,
  TableRow,
  TextRun,
  WidthType,
} = require(modulePath)

const root = path.resolve(__dirname, "..")
const uml = path.join(root, "uml")
const output = process.argv[3]
  ? path.resolve(process.argv[3])
  : path.join(root, "2_3_REQUERIMIENTOS_PROYECTO_ENLYCE.docx")

const colors = {
  navy: "26352F",
  blue: "742A3A",
  pale: "E4ECE6",
  line: "C9BFAE",
  gray: "655F56",
  white: "FFFFFF",
  warning: "FFF8DF",
  ivory: "F8F4EA",
  gold: "B08A4B",
  blush: "F1E5E7",
}

const border = { style: BorderStyle.SINGLE, size: 5, color: colors.line }
const cellBorders = { top: border, bottom: border, left: border, right: border }

function run(text, options = {}) {
  return new TextRun({ text, font: "Times New Roman", size: 21, color: "26352F", ...options })
}

function paragraph(text, options = {}) {
  return new Paragraph({
    spacing: { after: 120, line: 276 },
    children: [run(text)],
    ...options,
  })
}

function heading(text, level = HeadingLevel.HEADING_1) {
  return new Paragraph({
    heading: level,
    spacing: { before: level === HeadingLevel.HEADING_1 ? 260 : 180, after: 120 },
    border: { bottom: { style: BorderStyle.SINGLE, size: 9, color: colors.gold } },
    children: [new TextRun({ text, font: "Times New Roman", color: colors.blue, bold: true })],
  })
}

function cell(text, width, options = {}) {
  const children = Array.isArray(text) ? text : [paragraph(text, { spacing: { after: 0 } })]
  return new TableCell({
    width: { size: width, type: WidthType.DXA },
    borders: cellBorders,
    margins: { top: 90, bottom: 90, left: 100, right: 100 },
    shading: options.shading
      ? { type: ShadingType.CLEAR, color: "auto", fill: options.shading }
      : undefined,
    children,
  })
}

function labelValueTable(rows) {
  return new Table({
    width: { size: 9360, type: WidthType.DXA },
    columnWidths: [2300, 7060],
    rows: rows.map(([label, value]) => new TableRow({
      children: [
        cell([new Paragraph({ children: [run(label, { bold: true, color: colors.navy })] })], 2300, { shading: colors.pale }),
        cell(value, 7060),
      ],
    })),
  })
}

function requirementTable() {
  const data = [
    ["RU-01", "Asesor / Administrador", "Iniciar y cerrar sesión.", "Credenciales válidas crean una sesión segura; cerrar sesión elimina la cookie."],
    ["RU-02", "Administrador", "Registrar asesores.", "Solo el Administrador crea cuentas con correo único."],
    ["RU-03", "Visitante / Asesor", "Registrar una oportunidad comercial.", "Se crea el lead o se registra el recontacto si el correo ya existe."],
    ["RU-04", "Administrador", "Asignar o reasignar leads.", "El lead queda vinculado con el asesor seleccionado."],
    ["RU-05", "Asesor / Administrador", "Consultar pipeline y mover leads.", "El tablero muestra etapas y conserva el cambio."],
    ["RU-06", "Asesor", "Registrar interacciones.", "La gestión queda asociada con lead y asesor."],
    ["RU-07", "Asesor", "Programar y consultar visitas.", "La visita vincula lead, inmueble, asesor y fecha."],
    ["RU-08", "Asesor / Administrador", "Gestionar inmuebles.", "Usuarios autorizados registran y consultan inmuebles."],
    ["RU-09", "Visitante", "Consultar catálogo público.", "Catálogo y ficha se consultan sin autenticación."],
    ["RU-10", "Titular / Administrador", "Gestionar datos personales.", "Se pueden consultar o suprimir datos según autorización y rol."],
    ["RU-11", "Asesor / Administrador", "Consultar alertas.", "Cada usuario ve alertas según su alcance."],
    ["RU-12", "Todos", "Acceder solo a operaciones permitidas.", "La API responde 401 sin sesión y 403 sin permiso."],
  ]
  const widths = [850, 1900, 2700, 3910]
  const header = new TableRow({
    tableHeader: true,
    children: ["ID", "Actor", "Requisito", "Criterio observable"].map((text, i) =>
      cell([new Paragraph({ children: [run(text, { bold: true, color: colors.white })] })], widths[i], { shading: colors.navy })
    ),
  })
  return new Table({
    width: { size: 9360, type: WidthType.DXA },
    columnWidths: widths,
    rows: [header, ...data.map((row, rowIndex) => new TableRow({
      children: row.map((text, i) => cell(text, widths[i], rowIndex % 2 ? { shading: "F7F9FA" } : {})),
    }))],
  })
}

function useCase(title, rows) {
  return [
    heading(title, HeadingLevel.HEADING_2),
    labelValueTable(rows),
  ]
}

function imageParagraph(filename, width, height, caption) {
  return [
    new Paragraph({
      alignment: AlignmentType.CENTER,
      spacing: { before: 120, after: 100 },
      children: [new ImageRun({
        data: fs.readFileSync(path.join(uml, filename)),
        type: "png",
        transformation: { width, height },
        altText: { title: caption, description: caption, name: filename },
      })],
    }),
    new Paragraph({
      alignment: AlignmentType.CENTER,
      spacing: { after: 160 },
      children: [run(caption, { italics: true, color: colors.gray, size: 18 })],
    }),
  ]
}

const footer = new Footer({
  children: [new Paragraph({
    alignment: AlignmentType.CENTER,
    children: [
      run("ENLYCE · Taller práctico UML · ", { color: colors.gray, size: 18 }),
      new TextRun({ children: [PageNumber.CURRENT], font: "Times New Roman", size: 18, color: colors.gray }),
    ],
  })],
})

const intro = [
  new Paragraph({
    alignment: AlignmentType.RIGHT,
    spacing: { before: 180, after: 60 },
    children: [new TextRun({ text: "02", font: "Times New Roman", size: 78, bold: true, color: colors.gold })],
  }),
  new Paragraph({
    alignment: AlignmentType.LEFT,
    spacing: { after: 20 },
    children: [new TextRun({ text: "ENLYCE", font: "Times New Roman", size: 66, bold: true, color: colors.navy })],
  }),
  new Paragraph({
    alignment: AlignmentType.LEFT,
    border: { bottom: { style: BorderStyle.SINGLE, size: 18, color: colors.blue } },
    spacing: { after: 300 },
    children: [new TextRun({ text: "TALLER PRÁCTICO · MODELADO UML", font: "Times New Roman", size: 25, bold: true, color: colors.blue, characterSpacing: 40 })],
  }),
  labelValueTable([
    ["Estudiante", "Samuel Andres Escobar Saldarriaga"],
    ["Modalidad", "Trabajo individual"],
    ["Ficha", "PENDIENTE DE COMPLETAR"],
    ["Celular", "PENDIENTE DE COMPLETAR"],
    ["Correo", "PENDIENTE DE COMPLETAR"],
    ["Fecha", "24 de septiembre de 2026"],
  ]),
  heading("Integrante e idea del proyecto"),
  new Table({
    width: { size: 9360, type: WidthType.DXA },
    columnWidths: [2800, 1700, 2260, 2600],
    rows: [
      new TableRow({ tableHeader: true, children: ["Nombre y apellido", "Celular", "Correo", "Idea de proyecto"].map((t, i) => cell([new Paragraph({ children: [run(t, { bold: true, color: colors.white })] })], [2800,1700,2260,2600][i], { shading: colors.navy })) }),
      new TableRow({ children: [
        cell("Samuel Andres Escobar Saldarriaga", 2800),
        cell("Pendiente", 1700),
        cell("Pendiente", 2260),
        cell("ENLYCE — CRM inmobiliario", 2600),
      ] }),
    ],
  }),
  heading("Nombre del proyecto"),
  paragraph("ENLYCE — CRM inmobiliario"),
  heading("Descripción del problema"),
  paragraph("Las inmobiliarias pequeñas gestionan oportunidades, inmuebles y seguimientos mediante WhatsApp, hojas de cálculo y notas dispersas. Esto provoca pérdida de contactos, duplicidad de atención, poca visibilidad sobre el trabajo de los asesores y dificultades para demostrar el tratamiento autorizado de datos personales. ENLYCE centraliza el proceso comercial: capta interesados, organiza cada oportunidad en un pipeline, asigna responsables, registra interacciones y visitas, publica inmuebles y conserva evidencia de consentimiento."),
  heading("Requisitos de usuario"),
  requirementTable(),
  new Paragraph({ children: [new PageBreak()] }),
  heading("Diagrama de casos de uso"),
  ...imageParagraph("casos-de-uso-enlyce.png", 700, 575, "Figura 1. Capacidades de ENLYCE agrupadas por actor y contexto."),
  new Paragraph({
    shading: { type: ShadingType.CLEAR, color: "auto", fill: colors.warning },
    spacing: { before: 100, after: 180 },
    children: [run("Aclaración: la guía usa “Registrarse”. ENLYCE no permite auto-registro; solo el Administrador ejecuta Registrar asesor.", { bold: true })],
  }),
  heading("Descripción textual de tres casos de uso"),
  ...useCase("UC-01 — Autenticar", [
    ["Actor principal", "Asesor o Administrador."],
    ["Objetivo", "Acceder al CRM con la identidad y permisos correspondientes."],
    ["Precondiciones", "El asesor está registrado y activo."],
    ["Disparador", "El usuario envía correo y contraseña desde el formulario de acceso."],
    ["Flujo principal", "1. El CRM valida campos. 2. Envía POST /api/auth/login. 3. La API busca al asesor. 4. Verifica contraseña y estado. 5. Genera el JWT. 6. Crea la cookie segura _enlyce_auth. 7. Responde con nombre y rol. 8. El CRM abre el panel privado."],
    ["Alternativas", "Campos incompletos: no se envía. Credenciales inválidas: 401. Asesor inactivo: 401."],
    ["Postcondiciones", "Existe una sesión autenticada mediante cookie HttpOnly y el usuario accede según su rol."],
  ]),
  ...useCase("UC-02 — Registrar asesor", [
    ["Actor principal", "Administrador."],
    ["Objetivo", "Crear una cuenta interna para un asesor o administrador."],
    ["Precondiciones", "El actor inició sesión y posee rol Administrador."],
    ["Disparador", "El administrador diligencia nombre, correo, contraseña y rol."],
    ["Flujo principal", "1. El CRM envía POST /api/auth/register. 2. La API verifica autorización. 3. Application valida el correo único. 4. Genera el hash BCrypt. 5. Crea el asesor activo. 6. Persiste la entidad. 7. Responde 201 con identificador y nombre."],
    ["Alternativas", "Sin sesión: 401. Rol Asesor: 403. Correo repetido o datos inválidos: solicitud rechazada."],
    ["Postcondiciones", "El asesor queda registrado y puede autenticarse."],
  ]),
  new Paragraph({ children: [new PageBreak()] }),
  ...useCase("UC-03 — Registrar lead", [
    ["Actor principal", "Visitante o Asesor."],
    ["Objetivo", "Registrar una oportunidad comercial y su decisión sobre autorización de datos."],
    ["Precondiciones", "Existen nombre y correo válidos; el origen informa si hubo autorización."],
    ["Disparador", "El interesado solicita información o el asesor registra el contacto."],
    ["Flujo principal", "1. La interfaz envía POST /api/leads. 2. Application valida datos y referencias. 3. Busca contactos existentes. 4. Crea el lead cuando representa una oportunidad nueva. 5. Registra consentimiento cuando fue autorizado y existe política activa. 6. Envía confirmación. 7. La API responde 201."],
    ["Alternativas", "Datos inválidos: 400. Misma oportunidad: registra recontacto sin duplicar. Publicación diferente: crea otra oportunidad."],
    ["Postcondiciones", "Queda una oportunidad nueva o un recontacto trazable; el consentimiento queda vinculado cuando corresponde."],
  ]),
]

const activitySection = [
  heading("Diagrama de actividad — Autenticar"),
  ...imageParagraph("actividad-autenticar.png", 570, 586, "Figura 2. Actividad del caso de uso Autenticar."),
]

const classSection = [
  heading("Diagrama de clases"),
  ...imageParagraph("clases-dominio-enlyce.png", 540, 643, "Figura 3. Vista conceptual del dominio organizada por agregados."),
  paragraph("Las líneas continuas representan relaciones de agregado configuradas en EF Core. Las punteadas muestran asociaciones conceptuales mediante identificadores. Para conservar la lectura se omiten conexiones secundarias de Asesor con Interacción y Visita; sus identificadores siguen visibles en las clases."),
  heading("Base de verificación", HeadingLevel.HEADING_2),
  paragraph("Documento contrastado contra src/, configuraciones EF Core, endpoints y pruebas de ENLYCE en el commit 13624a7, el 24 de septiembre de 2026. Los cambios locales aún no versionados del módulo administrativo de publicaciones no forman parte de estos diagramas."),
]

const doc = new Document({
  creator: "Samuel Andres Escobar Saldarriaga",
  title: "Taller práctico UML — ENLYCE",
  description: "Requisitos, casos de uso, actividad de autenticación y clases del proyecto ENLYCE.",
  styles: {
    default: { document: { run: { font: "Times New Roman", size: 21, color: colors.navy }, paragraph: { spacing: { line: 276 } } } },
    paragraphStyles: [
      { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true, run: { font: "Times New Roman", size: 30, bold: true, color: colors.blue }, paragraph: { spacing: { before: 260, after: 120 }, outlineLevel: 0 } },
      { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true, run: { font: "Times New Roman", size: 25, bold: true, color: colors.navy }, paragraph: { spacing: { before: 200, after: 100 }, outlineLevel: 1 } },
    ],
  },
  numbering: {
    config: [{ reference: "bullets", levels: [{ level: 0, format: LevelFormat.BULLET, text: "•", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 720, hanging: 360 } } } }] }],
  },
  sections: [
    {
      properties: { page: { size: { width: 12240, height: 15840 }, margin: { top: 900, right: 900, bottom: 900, left: 900 } } },
      footers: { default: footer },
      children: intro,
    },
    {
      properties: { type: "nextPage", page: { size: { width: 12240, height: 15840, orientation: PageOrientation.LANDSCAPE }, margin: { top: 400, right: 600, bottom: 400, left: 600 } } },
      footers: { default: footer },
      children: activitySection,
    },
    {
      properties: { type: "nextPage", page: { size: { width: 12240, height: 15840 }, margin: { top: 500, right: 700, bottom: 500, left: 700 } } },
      footers: { default: footer },
      children: classSection,
    },
  ],
})

Packer.toBuffer(doc).then((buffer) => {
  fs.writeFileSync(output, buffer)
  process.stdout.write(`${output}\n`)
})
