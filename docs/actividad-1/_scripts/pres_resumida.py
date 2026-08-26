# -*- coding: utf-8 -*-
from pathlib import Path

from pptx import Presentation
from pptx.dml.color import RGBColor
from pptx.enum.shapes import MSO_CONNECTOR, MSO_SHAPE
from pptx.enum.text import MSO_ANCHOR, PP_ALIGN
from pptx.util import Inches, Pt


OUTPUT = Path(__file__).resolve().parents[1] / "presentacion-enlyce-resumida-v2.pptx"

# Paleta tomada de la interfaz real de Enlyce.
FOREST = RGBColor(0x1A, 0x4D, 0x2E)
FOREST_DARK = RGBColor(0x10, 0x2E, 0x20)
BONE = RGBColor(0xF3, 0xEF, 0xE5)
GOLD = RGBColor(0xC9, 0xA8, 0x4C)
INK = RGBColor(0x15, 0x23, 0x31)
MUTED = RGBColor(0x6B, 0x72, 0x70)
PAPER = RGBColor(0xFC, 0xFA, 0xF5)
WHITE = RGBColor(0xFF, 0xFF, 0xFF)
ALERT = RGBColor(0xA8, 0x4A, 0x3D)

DISPLAY = "Georgia"
BODY = "Trebuchet MS"

prs = Presentation()
prs.slide_width = Inches(13.333)
prs.slide_height = Inches(7.5)
BLANK = prs.slide_layouts[6]


def shape(slide, kind, x, y, w, h, fill, line=None, radius=None):
    item = slide.shapes.add_shape(kind, Inches(x), Inches(y), Inches(w), Inches(h))
    item.fill.solid()
    item.fill.fore_color.rgb = fill
    if line is None:
        item.line.fill.background()
    else:
        item.line.color.rgb = line
        item.line.width = Pt(1)
    item.shadow.inherit = False
    if radius is not None:
        try:
            item.adjustments[0] = radius
        except (IndexError, TypeError):
            pass
    return item


def text(slide, x, y, w, h, value, size=16, color=INK, bold=False,
         font=BODY, align=PP_ALIGN.LEFT, anchor=MSO_ANCHOR.TOP):
    box = slide.shapes.add_textbox(Inches(x), Inches(y), Inches(w), Inches(h))
    frame = box.text_frame
    frame.clear()
    frame.word_wrap = True
    frame.margin_left = frame.margin_right = 0
    frame.margin_top = frame.margin_bottom = 0
    frame.vertical_anchor = anchor
    paragraph = frame.paragraphs[0]
    paragraph.alignment = align
    paragraph.space_after = Pt(0)
    run = paragraph.add_run()
    run.text = value
    run.font.name = font
    run.font.size = Pt(size)
    run.font.bold = bold
    run.font.color.rgb = color
    return box


def line(slide, x1, y1, x2, y2, color=GOLD, width=1.5):
    item = slide.shapes.add_connector(
        MSO_CONNECTOR.STRAIGHT, Inches(x1), Inches(y1), Inches(x2), Inches(y2)
    )
    item.line.color.rgb = color
    item.line.width = Pt(width)
    return item


def tag(slide, x, y, value, width=1.7, fill=GOLD, color=FOREST_DARK):
    shape(slide, MSO_SHAPE.ROUNDED_RECTANGLE, x, y, width, 0.32, fill, radius=0.16)
    text(slide, x, y + 0.02, width, 0.23, value.upper(), 8.5, color, True,
         align=PP_ALIGN.CENTER, anchor=MSO_ANCHOR.MIDDLE)


def page(slide, number, dark=False):
    text(slide, 12.22, 0.42, 0.42, 0.22, f"0{number}", 9,
         GOLD if dark else FOREST, True, align=PP_ALIGN.RIGHT)


def add_building_mark(slide, x, y, scale=1.0, color=GOLD):
    line(slide, x, y + 0.4 * scale, x + 0.55 * scale, y, color, 1.5)
    line(slide, x + 0.55 * scale, y, x + 1.1 * scale, y + 0.4 * scale, color, 1.5)
    line(slide, x + 0.12 * scale, y + 0.32 * scale, x + 0.12 * scale, y + 1.0 * scale, color, 1.5)
    line(slide, x + 0.98 * scale, y + 0.32 * scale, x + 0.98 * scale, y + 1.0 * scale, color, 1.5)
    for index in range(3):
        shape(slide, MSO_SHAPE.RECTANGLE, x + (0.28 + index * 0.22) * scale,
              y + 0.42 * scale, 0.1 * scale, 0.16 * scale, color)


def lead_card(slide, x, y, w, code, source, owner=None, accent=GOLD):
    shape(slide, MSO_SHAPE.ROUNDED_RECTANGLE, x, y, w, 0.72, WHITE, radius=0.06)
    shape(slide, MSO_SHAPE.OVAL, x + 0.16, y + 0.18, 0.3, 0.3, accent)
    text(slide, x + 0.57, y + 0.12, w - 0.72, 0.22, code, 9.5, INK, True)
    text(slide, x + 0.57, y + 0.39, w - 0.72, 0.18,
         f"{source} · {owner or 'sin asignar'}", 7.5, MUTED)


def pipeline_window(slide, x, y, w, h, compact=False):
    shape(slide, MSO_SHAPE.ROUNDED_RECTANGLE, x, y, w, h, PAPER, GOLD, 0.03)
    shape(slide, MSO_SHAPE.RECTANGLE, x, y, 0.82 if compact else 1.15, h, FOREST_DARK)
    add_building_mark(slide, x + 0.2, y + 0.24, 0.38 if compact else 0.48, GOLD)
    for index in range(4):
        shape(slide, MSO_SHAPE.ROUNDED_RECTANGLE, x + 0.2, y + 1.22 + index * 0.47,
              0.43 if compact else 0.67, 0.08, GOLD if index == 1 else FOREST,
              radius=0.04)

    content_x = x + (1.05 if compact else 1.42)
    content_w = w - (1.25 if compact else 1.67)
    text(slide, content_x, y + 0.24, content_w, 0.28, "PIPELINE", 9, FOREST, True)
    text(slide, content_x, y + 0.58, content_w, 0.32,
         "Cada lead tiene dueño y próxima acción", 12 if compact else 14, INK, True)

    stages = [("NUEVO", "12"), ("CONTACTADO", "08"), ("VISITA", "04"), ("NEGOCIACIÓN", "02")]
    gap = 0.12
    column_w = (content_w - gap * 3) / 4
    for index, (stage, count) in enumerate(stages):
        cx = content_x + index * (column_w + gap)
        shape(slide, MSO_SHAPE.ROUNDED_RECTANGLE, cx, y + 1.1, column_w, h - 1.38,
              BONE, radius=0.04)
        if compact:
            text(slide, cx + 0.05, y + 1.24, column_w - 0.1, 0.18, stage,
                 5.6, MUTED, True, align=PP_ALIGN.CENTER)
        else:
            text(slide, cx + 0.09, y + 1.25, column_w - 0.18, 0.18, stage,
                 7.3, MUTED, True)
            text(slide, cx + column_w - 0.34, y + 1.22, 0.23, 0.18, count,
                 7.3, FOREST, True, align=PP_ALIGN.RIGHT)
        if compact:
            shape(slide, MSO_SHAPE.ROUNDED_RECTANGLE, cx + 0.08, y + 1.65,
                  column_w - 0.16, 1.02, WHITE, radius=0.05)
            shape(slide, MSO_SHAPE.OVAL, cx + column_w / 2 - 0.13, y + 1.82,
                  0.26, 0.26, ALERT if index == 0 else GOLD)
            text(slide, cx + 0.08, y + 2.2, column_w - 0.16, 0.18,
                 f"L{42 + index:03}", 7.2, INK, True, align=PP_ALIGN.CENTER)
            text(slide, cx + 0.08, y + 2.44, column_w - 0.16, 0.15,
                 "sin asignar" if index == 0 else "asignado", 5.8, MUTED,
                 align=PP_ALIGN.CENTER)
        else:
            lead_card(slide, cx + 0.08, y + 1.65, column_w - 0.16,
                      f"Lead {42 + index:03}", "WhatsApp",
                      [None, "Laura", "Samuel", "Diana"][index],
                      ALERT if index == 0 else GOLD)
        if index < 3 and not compact:
            lead_card(slide, cx + 0.08, y + 2.52, column_w - 0.16,
                      f"Lead {58 + index:03}", "Referido", "Andrés", FOREST)


# 01 - Portada con producto
slide = prs.slides.add_slide(BLANK)
slide.background.fill.solid()
slide.background.fill.fore_color.rgb = BONE
shape(slide, MSO_SHAPE.RECTANGLE, 0, 0, 7.05, 7.5, FOREST_DARK)
page(slide, 1, dark=True)
tag(slide, 0.72, 0.62, "Actividad 1", 1.55)
add_building_mark(slide, 0.75, 1.55, 0.72, GOLD)
text(slide, 0.75, 2.55, 5.7, 0.92, "ENLYCE", 53, WHITE, True, DISPLAY)
text(slide, 0.78, 3.55, 5.5, 1.0,
     "El CRM que evita que una micro-inmobiliaria pierda clientes por desorden.",
     20, BONE, False, DISPLAY)
text(slide, 0.78, 5.34, 5.45, 0.58,
     "Un responsable por lead.\nUna próxima acción visible.", 14, WHITE, True)
text(slide, 0.78, 6.69, 5.5, 0.25,
     "L&C Propiedad Raíz · Medellín · Agosto de 2026", 9, GOLD)
pipeline_window(slide, 7.56, 1.05, 5.15, 5.55, compact=True)
shape(slide, MSO_SHAPE.ROUNDED_RECTANGLE, 7.9, 5.83, 4.42, 0.56, FOREST, radius=0.06)
text(slide, 8.1, 5.98, 4.02, 0.22, "ALERTA · Lead 042 lleva 26 h sin contacto",
     8.5, WHITE, True, align=PP_ALIGN.CENTER)


# 02 - El caos antes de Enlyce
slide = prs.slides.add_slide(BLANK)
slide.background.fill.solid()
slide.background.fill.fore_color.rgb = FOREST_DARK
page(slide, 2, dark=True)
tag(slide, 0.68, 0.52, "El problema", 1.5)
text(slide, 0.68, 1.15, 6.2, 1.35, "El lead existe.\nEl proceso no.", 35, WHITE, True, DISPLAY)
text(slide, 0.7, 2.74, 4.9, 0.46,
     "Cuatro asesores comparten información sin dueño ni trazabilidad.", 13, BONE)

sources = [("WA", "WhatsApp", 0.75, 3.62, FOREST),
           ("XL", "Excel", 2.72, 4.58, ALERT),
           ("WS", "Wasi", 0.98, 5.58, GOLD)]
for initials, label_value, x, y, accent in sources:
    shape(slide, MSO_SHAPE.OVAL, x, y, 0.68, 0.68, accent)
    text(slide, x, y + 0.2, 0.68, 0.22, initials, 9, WHITE, True, align=PP_ALIGN.CENTER)
    text(slide, x + 0.83, y + 0.18, 1.1, 0.24, label_value, 10.5, BONE, True)
    line(slide, x + 1.65, y + 0.34, 4.5, 4.85, GOLD, 1)

shape(slide, MSO_SHAPE.OVAL, 4.25, 4.58, 1.12, 1.12, BONE, GOLD)
text(slide, 4.25, 4.86, 1.12, 0.32, "LEAD 042", 9.5, FOREST_DARK, True, align=PP_ALIGN.CENTER)
line(slide, 5.37, 5.14, 7.14, 5.14, ALERT, 2.2)
shape(slide, MSO_SHAPE.OVAL, 7.0, 4.78, 0.72, 0.72, ALERT)
text(slide, 7.0, 4.98, 0.72, 0.22, "?", 14, WHITE, True, align=PP_ALIGN.CENTER)

shape(slide, MSO_SHAPE.RECTANGLE, 8.08, 0, 5.25, 7.5, BONE)
page(slide, 2)
text(slide, 8.7, 0.83, 3.55, 0.26, "TRES FUGAS DEL NEGOCIO", 9.5, ALERT, True)
for index, (number, title_value, body) in enumerate([
    ("01", "Nadie responde", "El lead no tiene responsable formal."),
    ("02", "Dos responden", "El cliente recibe atención duplicada."),
    ("03", "Nadie aprende", "No existe historial para medir conversión."),
]):
    y = 1.55 + index * 1.45
    text(slide, 8.7, y, 0.45, 0.26, number, 10, GOLD, True)
    text(slide, 9.34, y - 0.05, 3.15, 0.34, title_value, 17, INK, True, DISPLAY)
    text(slide, 9.34, y + 0.38, 3.1, 0.38, body, 10.5, MUTED)
text(slide, 8.7, 6.15, 3.6, 0.56,
     "La Ley 1581 añade una cuarta fuga: datos sin evidencia de autorización.",
     12, ALERT, True)


# 03 - Producto, no promesa
slide = prs.slides.add_slide(BLANK)
slide.background.fill.solid()
slide.background.fill.fore_color.rgb = PAPER
page(slide, 3)
tag(slide, 0.65, 0.45, "La solución", 1.48)
text(slide, 0.65, 1.0, 8.9, 0.58,
     "Un solo lugar. Un responsable. Cero leads invisibles.", 29, FOREST_DARK, True, DISPLAY)
text(slide, 9.56, 1.05, 2.92, 0.44,
     "No digitaliza el caos:\nlo convierte en proceso.", 11.5, MUTED, True)
pipeline_window(slide, 0.65, 1.88, 12.03, 4.68, compact=False)
shape(slide, MSO_SHAPE.ROUNDED_RECTANGLE, 1.78, 5.86, 9.96, 0.49, FOREST_DARK, radius=0.04)
events = ["CAPTURA", "CONSENTIMIENTO", "ASIGNACIÓN", "VISITA", "CIERRE"]
for index, event in enumerate(events):
    x = 2.0 + index * 1.92
    shape(slide, MSO_SHAPE.OVAL, x, 6.0, 0.18, 0.18, GOLD)
    text(slide, x + 0.28, 5.98, 1.45, 0.17, event, 6.9, WHITE, True)
text(slide, 0.65, 6.88, 12.0, 0.22,
     "La alerta aparece antes de que el lead se pierda; la auditoría explica qué ocurrió después.",
     10.5, FOREST, True, align=PP_ALIGN.CENTER)


# 04 - Modelo comercial con posicionamiento
slide = prs.slides.add_slide(BLANK)
slide.background.fill.solid()
slide.background.fill.fore_color.rgb = FOREST_DARK
page(slide, 4, dark=True)
tag(slide, 0.7, 0.52, "Modelo comercial", 1.8)
text(slide, 0.7, 1.14, 7.0, 0.76,
     "Diseñado para el espacio que otros dejan vacío.", 31, WHITE, True, DISPLAY)
text(slide, 0.72, 2.45, 2.0, 0.24, "CLIENTE IDEAL", 9, GOLD, True)
text(slide, 0.72, 2.85, 3.1, 1.18,
     "1–8 asesores\n< 50 inmuebles\nMedellín y área metropolitana", 16, BONE, True, DISPLAY)
text(slide, 4.18, 2.45, 2.0, 0.24, "OFERTA", 9, GOLD, True)
text(slide, 4.18, 2.82, 3.22, 1.22,
     "CRM multi-asesor\nOnboarding guiado\nLey 1581 integrada", 15, BONE, True, DISPLAY)
shape(slide, MSO_SHAPE.ROUNDED_RECTANGLE, 8.12, 1.02, 4.48, 3.35, BONE, radius=0.04)
text(slide, 8.52, 1.45, 3.65, 0.22, "EQUIPO DE 4 ASESORES", 9, FOREST, True)
text(slide, 8.49, 2.0, 3.7, 0.7, "$108.000", 36, FOREST_DARK, True, DISPLAY)
text(slide, 8.52, 2.77, 3.55, 0.26, "COP / mes", 10, MUTED, True)
text(slide, 8.52, 3.36, 3.5, 0.42, "$89.000 base + $19.000 por asesor extra", 10, INK)
text(slide, 8.52, 3.87, 3.5, 0.22, "Setup único: $150.000", 9, ALERT, True)
text(slide, 0.72, 4.82, 3.0, 0.24, "POSICIONAMIENTO DE PRECIO", 9, GOLD, True)
line(slide, 0.78, 5.56, 12.05, 5.56, BONE, 1.2)
positions = [(1.08, "WASI", "$0", "limitado"),
             (5.42, "ENLYCE", "$108k", "equipo de 4"),
             (10.5, "TOKKO", "$313k–667k", "mensual")]
for x, brand, price, caption in positions:
    color = GOLD if brand == "ENLYCE" else BONE
    radius = 0.32 if brand == "ENLYCE" else 0.22
    shape(slide, MSO_SHAPE.OVAL, x, 5.33, radius * 2, radius * 2, color)
    text(slide, x - 0.28, 6.04, 1.2, 0.2, brand, 8.5, color, True, align=PP_ALIGN.CENTER)
    text(slide, x - 0.55, 6.32, 1.75, 0.22, price, 11, WHITE, True, align=PP_ALIGN.CENTER)
    text(slide, x - 0.55, 6.62, 1.75, 0.18, caption, 7.5, BONE, align=PP_ALIGN.CENTER)
text(slide, 3.96, 7.05, 4.7, 0.2,
     "Ni gratuito sin equipo · ni suite sobredimensionada", 9.5, GOLD, True,
     align=PP_ALIGN.CENTER)


# 05 - Modelado y cierre con voz propia
slide = prs.slides.add_slide(BLANK)
slide.background.fill.solid()
slide.background.fill.fore_color.rgb = BONE
page(slide, 5)
tag(slide, 0.66, 0.46, "Modelado", 1.42)
text(slide, 0.66, 1.05, 7.2, 0.62,
     "Así funciona el negocio que modelé.", 30, FOREST_DARK, True, DISPLAY)
line(slide, 0.86, 2.34, 8.15, 2.34, FOREST, 2.2)
flow = [(0.84, "01", "Lead"), (2.53, "02", "Consentimiento"),
        (4.47, "03", "Asesor"), (6.03, "04", "Visita"), (7.65, "05", "Cierre")]
for x, number, title_value in flow:
    shape(slide, MSO_SHAPE.OVAL, x, 2.02, 0.64, 0.64, FOREST_DARK,
          GOLD if number == "02" else FOREST_DARK)
    text(slide, x, 2.22, 0.64, 0.19, number, 8.5,
         GOLD if number != "02" else BONE, True, align=PP_ALIGN.CENTER)
    text(slide, x - 0.35, 2.88, 1.34, 0.25, title_value, 9.5, INK, True,
         align=PP_ALIGN.CENTER)
labels = [("ACTORES", "asesor · administrador · lead · propietario", FOREST),
          ("OBJETOS", "lead · inmueble · interacción · visita · consentimiento", GOLD),
          ("REGLAS", "un responsable · avance controlado · baja lógica · auditoría", ALERT)]
for index, (label_value, detail, color) in enumerate(labels):
    y = 3.55 + index * 0.72
    text(slide, 0.72, y, 1.05, 0.2, label_value, 8.5, color, True)
    line(slide, 1.82, y + 0.08, 2.4, y + 0.08, color, 1.5)
    text(slide, 2.58, y - 0.03, 5.58, 0.28, detail, 10.5, INK)

shape(slide, MSO_SHAPE.RECTANGLE, 8.72, 0, 4.61, 7.5, FOREST_DARK)
text(slide, 9.25, 0.72, 3.45, 0.22, "EVIDENCIA DEL PRODUCTO", 9, GOLD, True)
text(slide, 9.23, 1.28, 2.0, 0.62, "172", 40, WHITE, True, DISPLAY)
text(slide, 10.7, 1.48, 1.75, 0.38, "pruebas\nen verde", 10, BONE, True)
text(slide, 9.25, 2.55, 1.42, 0.45, "100 %", 25, GOLD, True, DISPLAY)
text(slide, 10.72, 2.69, 1.72, 0.28, "consentimiento", 9.5, BONE, True)
line(slide, 9.25, 3.48, 12.55, 3.48, GOLD, 1)
text(slide, 9.25, 3.93, 3.16, 1.56,
     "Diseñé Enlyce para que una inmobiliaria pequeña pueda trabajar con el orden de una grande, sin pagar como una grande.",
     16.5, WHITE, True, DISPLAY)
text(slide, 9.25, 6.15, 3.2, 0.5,
     "El siguiente paso es validarlo con L&C y medir respuesta, conversión y adopción.", 10.5, BONE)
text(slide, 0.72, 6.69, 7.55, 0.36,
     "La regla que cruza todo: ningún lead entra sin autorización y ninguna acción desaparece sin rastro.",
     11, ALERT, True)


prs.save(OUTPUT)
print(f"Creado: {OUTPUT}")
print(f"Diapositivas: {len(prs.slides)}")
