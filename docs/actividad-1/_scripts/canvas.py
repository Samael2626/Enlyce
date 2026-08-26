# -*- coding: utf-8 -*-
import openpyxl
from acentos import fix
from openpyxl.styles import Font, Alignment, PatternFill, Border, Side

OUT = r"D:/Proyectos/Enlyce/docs/actividad-1/canvas-modelo-negocio.xlsx"

wb = openpyxl.Workbook()
ws = wb.active
ws.title = "Business Model Canvas"

NAVY   = "0F2540"
ACCENT = "1F6FEB"
LIGHT  = "F4F7FB"
BORDER = Side(style="thin", color="9BB0C9")
BOX = Border(left=BORDER, right=BORDER, top=BORDER, bottom=BORDER)

def box(rng, titulo, cuerpo, fill=LIGHT):
    ws.merge_cells(rng)
    first = rng.split(":")[0]
    c = ws[first]
    c.value = fix(titulo) + "\n\n" + fix(cuerpo)
    c.alignment = Alignment(wrap_text=True, vertical="top", horizontal="left")
    c.fill = PatternFill("solid", fgColor=fill)
    c.font = Font(name="Calibri", size=9, color="17293D")
    for row in ws[rng]:
        for cell in row:
            cell.border = BOX

# ---- titulo
ws.merge_cells("A1:J1")
t = ws["A1"]
t.value = fix("BUSINESS MODEL CANVAS  |  ENLYCE - CRM Inmobiliario para micro-inmobiliarias colombianas\n"
           "Cliente ancla: L&C Propiedad Raiz (Medellin)  -  Agosto 2026  -  Ingenieria de Software")
t.font = Font(name="Calibri", size=13, bold=True, color="FFFFFF")
t.fill = PatternFill("solid", fgColor=NAVY)
t.alignment = Alignment(horizontal="center", vertical="center", wrap_text=True)
ws.row_dimensions[1].height = 46

for col in "ABCDEFGHIJ":
    ws.column_dimensions[col].width = 19
for r in range(2, 34):
    ws.row_dimensions[r].height = 15

# ---- 9 bloques
box("A2:B17", "1. SOCIOS CLAVE",
"""- L&C Propiedad Raiz: cliente ancla, valida requisitos y aporta datos reales.
- Microsoft Azure / .NET: hosting e infraestructura del stack.
- Meta - WhatsApp Business API: canal principal de captacion de leads.
- Proveedor de correo transaccional (SendGrid): alertas y notificaciones.
- Universidad: soporte academico, asesoria y validacion del proyecto.
- Comunidad open source: PostgreSQL, EF Core, React/Vite.
- SIC (marco regulatorio): registro RNBD y politica de tratamiento.""")

box("C2:D9", "2. ACTIVIDADES CLAVE",
"""- Desarrollo del CRM (Clean Architecture, 172 tests automatizados).
- Onboarding e implantacion en menos de 24 horas.
- Configuracion del pipeline y de las etapas por inmobiliaria.
- Capacitacion de asesores (2 h) y soporte en espanol.
- Mantener el cumplimiento de la Ley 1581 (consentimiento, politica, auditoria).
- Evolucion del producto con feedback del cliente ancla.""")

box("C10:D17", "6. RECURSOS CLAVE",
"""- Plataforma propia: ASP.NET Core 10 + EF Core + PostgreSQL + React/Vite.
- Arquitectura hexagonal/limpia: dominio sin dependencias, portable y testeable.
- Modulo de cumplimiento Ley 1581 (consentimientos, politica versionada, auditoria).
- Base de leads, inmuebles y propietarios de LYC.
- Conocimiento del mercado inmobiliario de Medellin.
- Equipo de desarrollo y su tiempo (96 h/semestre).""")

box("E2:F17", "3. PROPUESTA DE VALOR",
""""El CRM con el que una micro-inmobiliaria colombiana deja de perder leads."

- Multi-asesor desde el dia 1: cada lead tiene dueno, nadie se pisa el cliente.
- Pipeline Kanban visible: Contacto > Visita > Propuesta > Negociacion > Cierre.
- Alertas de seguimiento: el lead sin gestion se vuelve visible, no se pierde.
- Cumplimiento de la Ley 1581 integrado, no como un anexo: consentimiento obligatorio,
  politica versionada, registro de auditoria y derechos del titular.
- Baja logica con auditoria: nada se borra, todo queda trazado.
- Precio intermedio (~$158.000/mes) entre el gratuito sin valor y el costoso inaccesible.
- Operativo en menos de 24 horas, 100 % en espanol colombiano.""", "E8F1FF")

box("G2:H9", "4. RELACION CON CLIENTES",
"""- Acompanamiento personal en la implantacion (setup + capacitacion).
- Soporte directo por WhatsApp y correo, en horario laboral colombiano.
- Co-creacion con el cliente ancla: sus necesidades marcan el roadmap.
- Autoservicio en la aplicacion: el asesor gestiona su propio pipeline.
- Actualizaciones continuas sin costo ni interrupcion del servicio.""")

box("G10:H17", "7. CANALES",
"""- Aplicacion web responsive (React/Vite) - canal principal de uso.
- WhatsApp Business - captacion de leads y soporte.
- Correo transaccional - alertas, seguimientos y onboarding.
- Demostracion personalizada (reunion presencial o remota).
- Referidos del cliente ancla y del gremio inmobiliario local.
- Vitrina academica del proyecto universitario.""")

box("I2:J17", "5. SEGMENTOS DE CLIENTES",
"""SEGMENTO PRINCIPAL
- Inmobiliarias independientes de Medellin y su area metropolitana.
- De 1 a 8 asesores comerciales, menos de 50 inmuebles activos.
- Hoy operan con Wasi gratis, Excel y WhatsApp sin orden.
- Facturacion anual entre $100 M y $2.000 M COP.

SEGMENTOS SECUNDARIOS
- Asesores independientes que van a formar equipo.
- Franquicias locales de inmobiliarias grandes que necesitan autonomia.
- Constructoras pequenas con venta directa.

NO ES CLIENTE
- Inmobiliarias de mas de 15 asesores (les sirve Tokko).""")

box("A18:E27", "9. ESTRUCTURA DE COSTOS",
"""- Hosting Azure App Service + PostgreSQL administrado: $120.000 - $200.000 / mes.
- Dominio y certificados: ~$4.000 / mes (SSL gratuito con Let's Encrypt).
- WhatsApp Business API: variable, por conversacion (Meta).
- Correo transaccional y monitoreo: $0 - $35.000 / mes (planes gratuitos iniciales).
- Desarrollo y mantenimiento: 96 h/semestre (costo hundido - proyecto academico).
- Soporte y capacitacion: horas de asesor por cliente nuevo.

Costo fijo mensual estimado: $125.000 - $240.000. Es un modelo dirigido por el valor
(no por el costo): margen positivo desde el segundo cliente.""", "FFF6E5")

box("F18:J27", "8. FUENTES DE INGRESO",
"""1. Suscripcion base por equipo (hasta 3 asesores): $89.000 / mes.
2. Asesor adicional: $19.000 / mes cada uno (maximo 8).
3. Implantacion inicial (migracion + capacitacion): $150.000, pago unico.
4. Complemento WhatsApp Business API: $45.000 / mes.
5. Complemento portal web publico de inmuebles: $35.000 / mes.
6. Complemento de reportes avanzados (PDF/Excel): $25.000 / mes.

Caso tipico LYC (4 asesores): $158.000 / mes recurrentes + $150.000 de implantacion.
Meta del semestre: 10 clientes -> MRR de $1.580.000 COP.""", "E8F7EC")

# ---- pie
ws.merge_cells("A29:J29")
p = ws["A29"]
p.value = fix("Restriccion transversal: la Ley 1581 de 2012 exige autorizacion previa, expresa e informada del titular. "
           "En Enlyce ningun lead se crea sin consentimiento registrado, y toda operacion queda en el log de auditoria.")
p.font = Font(name="Calibri", size=9, italic=True, color="0F2540")
p.fill = PatternFill("solid", fgColor="FDECEC")
p.alignment = Alignment(wrap_text=True, vertical="center")
ws.row_dimensions[29].height = 30

ws.sheet_view.showGridLines = False
ws.page_setup.orientation = "landscape"
ws.page_setup.fitToWidth = 1
ws.page_setup.fitToHeight = 1
ws.sheet_properties.pageSetUpPr.fitToPage = True
wb.save(OUT)
print("ok", OUT)
