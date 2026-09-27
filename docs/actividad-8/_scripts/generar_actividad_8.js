const path = require("path");
const fs = require("fs");
const {
  AlignmentType, BorderStyle, Document, Footer, Header, HeadingLevel,
  LevelFormat, PageBreak, PageNumber, Paragraph, Packer, ShadingType,
  Table, TableCell, TableLayoutType, TableRow, TextRun, VerticalAlign, WidthType,
} = require("docx");

const ROOT = path.resolve(__dirname, "..");
const OUTPUT = path.join(ROOT, "Actividad_8_Fundamentos_Pruebas_Samuel_Escobar.docx");
const FONT = "Times New Roman";
const NAVY = "10203D";
const GOLD = "C89B3C";
const CREAM = "F7F2E8";
const PALE = "E9EFF3";
const RED = "8C2F39";
const GREEN = "267A5E";
const GRAY = "5B6573";
const WHITE = "FFFFFF";
const BLACK = "1B1F24";

const line = (color = "D8DDE3", size = 6) => ({ style: BorderStyle.SINGLE, color, size });
const noLine = { style: BorderStyle.NONE, color: WHITE, size: 0 };
const noBorders = () => ({ top: noLine, bottom: noLine, left: noLine, right: noLine, insideHorizontal: noLine, insideVertical: noLine });

function text(value, options = {}) {
  return new TextRun({ text: value, font: FONT, size: 22, color: BLACK, ...options });
}

function paragraph(value = "", options = {}) {
  return new Paragraph({
    alignment: options.align,
    spacing: { before: options.before || 0, after: options.after ?? 130, line: options.line || 310 },
    keepNext: options.keepNext || false,
    children: [text(value, {
      bold: options.bold || false,
      italics: options.italic || false,
      color: options.color || BLACK,
      size: options.size || 22,
    })],
  });
}

function rich(parts, options = {}) {
  return new Paragraph({
    alignment: options.align,
    spacing: { before: options.before || 0, after: options.after ?? 130, line: options.line || 310 },
    children: parts.map((part) => text(part.value, part)),
  });
}

function heading(value, level = 1) {
  return new Paragraph({
    heading: level === 1 ? HeadingLevel.HEADING_1 : HeadingLevel.HEADING_2,
    spacing: { before: level === 1 ? 200 : 130, after: 110 },
    keepNext: true,
    children: [text(value, { bold: true, color: level === 1 ? NAVY : RED, size: level === 1 ? 32 : 26 })],
  });
}

function pageBreak() { return new Paragraph({ children: [new PageBreak()] }); }

function bullet(value) {
  return new Paragraph({
    numbering: { reference: "bullets", level: 0 },
    spacing: { after: 95, line: 300 },
    children: [text(value)],
  });
}

function number(value, reference = "steps") {
  return new Paragraph({
    numbering: { reference, level: 0 },
    spacing: { after: 100, line: 300 },
    children: [text(value)],
  });
}

function callout(title, body, color = GOLD) {
  return new Table({
    width: { size: 10080, type: WidthType.DXA },
    layout: TableLayoutType.FIXED,
    columnWidths: [260, 9820],
    rows: [new TableRow({ children: [
      new TableCell({
        width: { size: 260, type: WidthType.DXA },
        shading: { fill: color, type: ShadingType.CLEAR },
        borders: noBorders(),
        children: [paragraph("", { after: 0 })],
      }),
      new TableCell({
        width: { size: 9820, type: WidthType.DXA },
        shading: { fill: CREAM, type: ShadingType.CLEAR },
        borders: noBorders(),
        margins: { top: 150, bottom: 150, left: 180, right: 180 },
        children: [rich([
          { value: `${title}. `, bold: true, color: NAVY },
          { value: body },
        ], { after: 0 })],
      }),
    ] })],
  });
}

function cell(value, width, options = {}) {
  return new TableCell({
    width: { size: width, type: WidthType.DXA },
    verticalAlign: VerticalAlign.CENTER,
    shading: options.fill ? { fill: options.fill, type: ShadingType.CLEAR } : undefined,
    borders: { top: line(), bottom: line(), left: line(), right: line(), insideHorizontal: line(), insideVertical: line() },
    margins: { top: 110, bottom: 110, left: 130, right: 130 },
    children: [paragraph(value, {
      bold: options.bold, color: options.color, size: options.size || 20,
      after: 0, align: options.align,
    })],
  });
}

function comparison(rows) {
  const widths = [5040, 5040];
  return new Table({
    width: { size: 10080, type: WidthType.DXA },
    layout: TableLayoutType.FIXED,
    columnWidths: widths,
    rows: [
      new TableRow({ children: [
        cell("ANTES", widths[0], { fill: RED, bold: true, color: WHITE, align: AlignmentType.CENTER }),
        cell("AHORA", widths[1], { fill: GREEN, bold: true, color: WHITE, align: AlignmentType.CENTER }),
      ] }),
      ...rows.map((row, index) => new TableRow({ children: [
        cell(row[0], widths[0], { fill: index % 2 ? "F5ECEE" : WHITE }),
        cell(row[1], widths[1], { fill: index % 2 ? "EAF3EF" : WHITE }),
      ] })),
    ],
  });
}

function sectionLabel(value) {
  return paragraph(value.toUpperCase(), { bold: true, color: GOLD, size: 18, before: 60, after: 75 });
}

const content = [
  paragraph("INGENIERÍA DE SOFTWARE II", { bold: true, color: GOLD, size: 20, align: AlignmentType.CENTER, before: 900, after: 220 }),
  paragraph("ACTIVIDAD 8", { bold: true, color: NAVY, size: 50, align: AlignmentType.CENTER, after: 35 }),
  paragraph("Fundamentos del proceso de pruebas", { color: NAVY, size: 30, align: AlignmentType.CENTER, after: 330 }),
  new Table({
    width: { size: 8400, type: WidthType.DXA }, alignment: AlignmentType.CENTER,
    layout: TableLayoutType.FIXED, columnWidths: [8400],
    rows: [new TableRow({ children: [new TableCell({
      width: { size: 8400, type: WidthType.DXA },
      shading: { fill: NAVY, type: ShadingType.CLEAR }, borders: noBorders(),
      margins: { top: 330, bottom: 330, left: 420, right: 420 },
      children: [
        paragraph("UNA VULNERABILIDAD REAL EN ENLYCE", { bold: true, color: GOLD, size: 22, align: AlignmentType.CENTER, after: 100 }),
        paragraph("Cómo la encontramos, qué permitía y cómo quedó corregida", { color: WHITE, size: 29, align: AlignmentType.CENTER, after: 0 }),
      ],
    })] })],
  }),
  paragraph("Samuel Andres Escobar Saldarriaga", { bold: true, color: NAVY, size: 24, align: AlignmentType.CENTER, before: 430, after: 70 }),
  paragraph("Trabajo individual · 27 de septiembre de 2026", { color: GRAY, size: 20, align: AlignmentType.CENTER, after: 0 }),

  pageBreak(),
  sectionLabel("01 · La vulnerabilidad"),
  heading("Una persona podía ver y cambiar una oportunidad ajena"),
  paragraph("ENLYCE recibe desde la página web los datos de quienes quieren comprar, arrendar, vender o administrar un inmueble. Cada solicitud se convierte en una oportunidad dentro del CRM."),
  paragraph("El problema aparecía cuando alguien enviaba un correo que ya estaba registrado. La respuesta mostraba información de la oportunidad existente, incluido su identificador interno. Después, ese identificador y el mismo correo bastaban para cambiar los datos del inmueble sin iniciar sesión."),
  callout("En palabras simples", "el sistema trataba dos datos visibles —correo e identificador— como si fueran una contraseña. No lo eran.", RED),
  heading("Ejemplo", 2),
  paragraph("Ana había dejado su correo para vender un apartamento. Otra persona conocía ese correo, lo enviaba de nuevo y ENLYCE le devolvía la referencia interna de Ana. Con esa referencia podía cambiar el barrio, el precio o el mensaje asociado a la oportunidad."),
  heading("Nombre de la falla", 2),
  paragraph("Esta vulnerabilidad se conoce como acceso indebido a un registro. En seguridad también recibe los nombres BOLA o IDOR, pero lo importante no es la sigla: una persona podía actuar sobre información que no le pertenecía."),

  pageBreak(),
  sectionLabel("02 · Cómo la encontramos"),
  heading("Repetimos el recorrido de un atacante, usando datos de prueba"),
  paragraph("La prueba se hizo únicamente en el entorno local de ENLYCE y con información inventada. No se usaron datos de clientes reales."),
  number("Creamos una oportunidad de prueba desde el formulario público de propietarios."),
  number("Volvimos a enviar el mismo correo, como lo haría alguien que conoce el correo de otra persona."),
  number("Observamos que la respuesta revelaba el nombre, el correo, el estado y la referencia interna de la oportunidad ya existente."),
  number("Usamos esa referencia y el correo en la opción pública para completar datos del inmueble."),
  number("Revisamos el CRM y comprobamos que el barrio y el mensaje habían sido modificados."),
  callout("Prueba repetible", "convertimos el recorrido anterior en una prueba automática. Antes de la corrección falló exactamente donde debía: el sistema revelaba el registro y aceptaba el cambio."),
  heading("La señal definitiva", 2),
  comparison([
    ["La respuesta del contacto repetido incluía datos de la oportunidad.", "La prueba exigía que no apareciera ningún dato interno."],
    ["La dirección antigua aceptaba el cambio y respondía correctamente.", "La prueba exigía que esa dirección ya no existiera."],
  ]),

  pageBreak(),
  sectionLabel("03 · Qué podía causar"),
  heading("El riesgo no era teórico"),
  paragraph("El fallo permitía afectar la información comercial y personal que usa el equipo para atender a un cliente. No daba acceso completo al CRM, pero sí abría una puerta pública hacia oportunidades concretas."),
  heading("Consecuencias", 2),
  bullet("Exponer el nombre, correo, estado y referencia interna de una oportunidad existente."),
  bullet("Cambiar el tipo de inmueble, ciudad, barrio, precio esperado, mensaje o canal de contacto."),
  bullet("Confundir al asesor con información falsa y contaminar el historial comercial."),
  bullet("Hacer que una persona reciba una atención basada en datos que nunca proporcionó."),
  bullet("Debilitar la confianza en el tratamiento de datos personales realizado por ENLYCE."),
  callout("Gravedad", "se clasificó como crítica porque combinaba dos problemas: primero revelaba cómo encontrar la ficha y luego permitía modificarla.", RED),

  pageBreak(),
  sectionLabel("04 · Cómo la solucionamos"),
  heading("Quitamos la identidad de la respuesta y usamos un permiso temporal"),
  paragraph("La solución no consistió en esconder el identificador visualmente. Se cambió la forma en que funciona el proceso."),
  comparison([
    ["El sistema devolvía la ficha existente.", "La respuesta es neutra e igual para todos."],
    ["Mostraba identificador, nombre, correo y estado.", "No entrega ningún dato de la oportunidad."],
    ["Correo + identificador permitían cambiar la ficha.", "Se usa una clave aleatoria creada para esa solicitud."],
    ["La autorización no vencía ni se consumía.", "La clave dura 20 minutos y funciona una sola vez."],
    ["La dirección vulnerable seguía disponible.", "La dirección antigua fue eliminada."],
  ]),
  heading("Cómo funciona ahora", 2),
  number("La persona envía sus datos básicos.", "solution-steps"),
  number("ENLYCE responde sin decir si el correo ya existía y sin mostrar información interna.", "solution-steps"),
  number("Si la solicitud es nueva, entrega una clave aleatoria para completar los datos opcionales.", "solution-steps"),
  number("La base de datos guarda solamente una huella de esa clave, no la clave original.", "solution-steps"),
  number("Al usarla, la clave queda consumida. Repetirla no cambia nada.", "solution-steps"),
  callout("Analogía", "la clave funciona como una tarjeta temporal de hotel: abre una sola puerta, durante poco tiempo y deja de servir después de usarse.", GREEN),

  pageBreak(),
  sectionLabel("05 · Cómo comprobamos la corrección"),
  heading("Intentamos el mismo ataque otra vez"),
  paragraph("Después del cambio repetimos el recorrido original y añadimos controles para evitar que la falla regrese en una actualización futura."),
  bullet("Un correo repetido recibe una respuesta genérica: no aparecen datos ni referencias de la oportunidad anterior."),
  bullet("La dirección vulnerable con el identificador ya no existe."),
  bullet("Una clave inventada no modifica ninguna oportunidad."),
  bullet("Una clave válida completa la solicitud correcta."),
  bullet("La misma clave, usada por segunda vez, no vuelve a modificar la información."),
  new Table({
    width: { size: 10080, type: WidthType.DXA }, layout: TableLayoutType.FIXED,
    columnWidths: [3500, 6580],
    rows: [
      new TableRow({ children: [cell("COMPROBACIÓN", 3500, { fill: NAVY, bold: true, color: WHITE }), cell("RESULTADO", 6580, { fill: NAVY, bold: true, color: WHITE })] }),
      new TableRow({ children: [cell("Pruebas de seguridad nuevas", 3500), cell("3 de 3 superadas", 6580, { bold: true, color: GREEN })] }),
      new TableRow({ children: [cell("Suite completa de ENLYCE", 3500, { fill: PALE }), cell("403 de 403 superadas", 6580, { fill: PALE, bold: true, color: GREEN })] }),
      new TableRow({ children: [cell("Verificación de la página web", 3500), cell("Sin errores de tipos", 6580, { bold: true, color: GREEN })] }),
    ],
  }),
  callout("Resultado final", "el ataque documentado ya no permite descubrir ni modificar una oportunidad ajena. La prueba queda dentro del proyecto para vigilar que la protección no se pierda.", GREEN),
];

const doc = new Document({
  styles: {
    default: { document: { run: { font: FONT, size: 22, color: BLACK }, paragraph: { spacing: { line: 310 } } } },
  },
  numbering: { config: [
    { reference: "bullets", levels: [{ level: 0, format: LevelFormat.BULLET, text: "•", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 430, hanging: 230 } } } }] },
    { reference: "steps", levels: [{ level: 0, format: LevelFormat.DECIMAL, text: "%1.", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 500, hanging: 260 } } } }] },
    { reference: "solution-steps", levels: [{ level: 0, format: LevelFormat.DECIMAL, text: "%1.", alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 500, hanging: 260 } } } }] },
  ] },
  sections: [{
    properties: { page: { margin: { top: 900, right: 1080, bottom: 900, left: 1080 } } },
    headers: { default: new Header({ children: [rich([
      { value: "ENLYCE", bold: true, color: NAVY, size: 18 },
      { value: "  ·  ACTIVIDAD 8", color: GOLD, size: 18 },
    ], { align: AlignmentType.RIGHT, after: 0 })] }) },
    footers: { default: new Footer({ children: [new Paragraph({
      alignment: AlignmentType.CENTER,
      children: [text("Samuel Andres Escobar Saldarriaga  ·  ", { color: GRAY, size: 17 }), new TextRun({ children: [PageNumber.CURRENT], font: FONT, size: 17, color: GRAY })],
    })] }) },
    children: content,
  }],
});

Packer.toBuffer(doc).then((buffer) => {
  fs.writeFileSync(OUTPUT, buffer);
  console.log(OUTPUT);
});
