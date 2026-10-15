# Evidencia estructural del scrape de Wasi

**Corte:** 2026-10-14  
**Fuente local:** `C:\Users\USUARIO\Downloads\Wasi\Inventarios` (28 JSON).  
**Uso:** inventario de superficies y campos visibles para la paridad ENLYCE. No copiar registros, valores de formulario ni datos personales.

## Superficies observadas

- Escritorio: `/desktop` (3 capturas).
- Agenda: `/usuario/agenda`.
- Inmuebles: `/usuario/inmuebles`, `/inmueble-editar.htm`, `/usuario/inmuebles/exportar`, `/administrador/inmuebles/aliados`.
- Cruces: `/administrador/cruces/propiedades-clientes` y `/administrador/cruces/clientes-propiedades`.
- Clientes: `/usuario/clientes`, `/usuario/clientes/directorio`, `/usuario/clientes/create`, `/usuario/clientes/exportar`, `/usuario/pipeline/1`, `/usuario/crm`.
- Red y alianzas: `/administrador/empresa/alianzas`, `/administrador/empresa/alianzas-propietario`, `/administrador/grupo`.
- Mercado y reportes: `/administrador/reportes/mercado`, `/administrador/reportes2`, `/administrador/reportes/negocios/negocios`, `/administrador/reportes2/agentes`, `/administrador/reportes2/propietarios`, `/administrador/reportes/estadisticas`, `/administrador/reportes/correos`.
- Reporte de propietarios (`/administrador/reportes2/propietarios`): el JSON `wasi-ui-wasi.co-1791954561589.json` conserva los encabezados Propietario, Visitas En Página, Llamadas, Correos, Eventos, Visitas, Contactos recibidos y Opciones. No conserva filas ni permite verificar filtros, periodos o las reglas de conteo.
- Reporte de correos (`/administrador/reportes/correos`): el JSON `wasi-ui-wasi.co-1791954587334.json` conserva las columnas Fecha, Asunto, Descripción, Usuario encargado y Estado. No conserva filas, filtros ni acciones de detalle.
- Reporte de página web (`/administrador/reportes/estadisticas`): el JSON `wasi-ui-wasi.co-1791954572426.json` conserva las columnas Título inmueble, Visitas y Opciones. No conserva filas, filtros, periodos ni la definición de visita.
- Usuarios: `/administrador/agentes` (2 capturas).

## Campos visibles en formularios

**Crear cliente:** encargado, embudo, estado, nombre, apellidos, email, permiso de correo, teléfonos fijo/móvil, nacimiento, identificación, referido, datos adicionales, país/departamento/ciudad/dirección y campos de demanda (presupuesto, área y detalles).

**Editar inmueble:** encargado, título, publicación, tipo/negocio, precios y monedas, estado físico, año, habitaciones, baños, garaje, estrato, piso y áreas, video, matrícula, disposición/orientación/balcón, pisos del edificio, publicación/red, ubicación, dirección privada, características internas/externas, datos del propietario y opciones de comisión/exclusividad.

**Otros formularios:** alianzas y alianzas de propietarios muestran nombre y ubicación; la búsqueda de gestiones muestra tipo, nombre de cliente y usuario encargado.

## Límites de la evidencia

El scrape preserva las etiquetas de campos y rutas, pero declara que no captura valores ni filas. No prueba opciones dinámicas de selectores, reglas de validación, permisos efectivos, resultados de botones, columnas/criterios de exportación ni estados después de guardar. El JSON `wasi-ui-wasi.co-1791954505967.json` registra `/usuario/clientes/exportar` y el enlace lateral «Importar clientes» apunta a esa misma ruta, pero no captura encabezados ni formularios (`forms: []`). La captura aportada confirma únicamente el selector XLS/XLSX/CSV. Esta evidencia no muestra mapeo, validaciones, confirmación ni resultado de importación. Las pantallas `/usuario/clientes/exportar` y `/usuario/inmuebles/exportar` prueban que esas superficies existen; falta recorrerlas para documentar su comportamiento.

Las capturas de formularios tampoco prueban que Wasi importe demandas o que el archivo de clientes permita mapear todos esos campos. La guía oficial de importación sigue siendo la fuente para el formato de importación.

La guía oficial de inmuebles confirma importación por XLS/XLSX/CSV con mapeo, límite recomendado de 2.000 filas, sin fórmulas, y fotos mediante URLs separadas por `|`; la pantalla no quedó recorrida en las capturas. La misma guía dice que la exportación entrega un XLS por correo, pero no define en el texto qué estados incluye ni las columnas.

## Próxima evidencia interactiva

1. Abrir cada exportación y registrar solo nombres de filtros/columnas, opciones de grupo y destino del archivo; ocultar filas y valores personales.
2. Abrir administración de agentes para ver campos, roles y estados editables.
3. Probar configuración de embudo, tipos y etiquetas con datos ficticios.
4. Comparar formularios de edición en Enlyce contra las etiquetas anteriores, campo por campo.

