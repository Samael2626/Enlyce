# Checklist de paridad Wasi → Enlyce

**Corte:** 14 de octubre de 2026  
**Objetivo:** llevar a Enlyce las funciones de Wasi usadas por L&C, sin rehacer funciones que ya existen.  
**Fuentes:** 27 JSON de interfaz en `C:\Users\USUARIO\Downloads\Wasi\Inventarios`, capturas compartidas y código actual de Enlyce. El inventario describe pantallas y controles; no demuestra reglas internas de Wasi. No contiene datos personales.

## Estados

- [x] Cubierto: función existente en Enlyce con alcance comparable.
- [~] Parcial: existe una parte, falta completar paridad o validar comportamiento.
- [ ] Faltante: no se encontró implementación equivalente.
- [?] Sin verificar: evidencia insuficiente para definir el comportamiento esperado.

## Inventario funcional

| Área Wasi | Estado Enlyce | Evidencia / brecha |
|---|---|---|
| Escritorio y métricas del equipo | [~] | Enlyce tiene panel; verificar equivalencia de widgets, periodos y accesos directos de Wasi. |
| Foro inmobiliario | [ ] | Wasi permite publicar y consultar actividad; no hay módulo equivalente. |
| Agenda y tareas | [~] | Calendario, tareas y visitas existen; faltan eventos generales y validar vistas día/semana/mes, equipos y recordatorios de Wasi. |
| Alta, lista y ficha de clientes | [~] | Contactos, oportunidades y demandas existen. Falta cliente independiente con los campos del directorio Wasi y sus vínculos. |
| Embudo de clientes | [~] | Pipeline comercial existe; comparar etapas, transiciones y acciones reales de Wasi. |
| Gestiones realizadas | [~] | Interacciones, tareas e historial existen; falta comparar filtros, tipos, asuntos y presentación de la lista Wasi. |
| Demandas y cruce cliente → inmuebles | [x] | Demandas y coincidencias existen. |
| Cruce inmueble → clientes posibles | [x] | Conteo por página y detalle existen, con el mismo alcance por asesor. |
| Inmuebles vinculados al cliente y estado del vínculo | [~] | Vínculos y estados existen; historial de cada cambio de estado sigue pendiente de aprobación/migración. |
| Lista, ficha y edición de inmuebles | [~] | Inventario inmobiliario existe; falta comparar campos, catálogos, validaciones, filtros y acciones de Wasi. |
| Importación de inmuebles | [~] | Importación desde API Wasi en segundo plano existe. Falta paridad con carga por archivo si Wasi la ofrece. |
| Exportación de inmuebles | [?] | Wasi muestra acceso de importar/exportar; el flujo de exportación no quedó observado. |
| Importación de clientes | [ ] | Wasi ofrece carga XLS/XLSX/CSV. Faltan formato/columnas y flujo de validación; Enlyce no tiene este módulo. |
| Exportación de clientes | [~] | Enlyce exporta contactos/opciones CRM en CSV; comparar columnas, filtros y alcance con Wasi. |
| Inmuebles de aliados | [ ] | Wasi lista inmuebles compartidos y permite republicación; Enlyce carece de red multiempresa. |
| Alianzas, propietarios aliados y grupos | [ ] | Pantallas detectadas en Wasi; modelo, permisos, invitaciones y acuerdos no implementados ni totalmente observados. |
| Mercado inmobiliario | [?] | Pantalla Wasi detectada, contenido dinámico y métricas no identificados. |
| Reporte general | [~] | Analítica CRM/web existe; falta comparar inventario, negocios, gestiones y demás totales de Wasi. |
| Reportes de negocios, usuarios, propietarios y sitio web | [~] | Hay reportes CRM y web; los reportes específicos de Wasi requieren comparación de filtros y métricas. |
| Correos enviados | [ ] | Wasi muestra reporte de envíos; Enlyce aún no tiene correo saliente integrado con registro de entrega. |
| Usuarios y asesores | [~] | Autenticación y roles existen; falta comparar administración de usuarios, estados, permisos y campos de Wasi. |
| Sitio web inmobiliario | [~] | Catálogo público y publicaciones existen; faltan funciones de administración y configuración web de Wasi. |
| Configuración inicial de empresa | [ ] | Wasi presenta un asistente de puesta en marcha; Enlyce no muestra equivalente. |
| Configuración comercial | [~] | Reglas de reparto y SLA existen; comparar el resto de ajustes visibles en Wasi. |
| Mi Cuenta, reputación, academia y soporte | [ ] | Accesos visibles en Wasi; sin equivalentes CRM identificados en Enlyce. |
| Chat interno | [ ] | Enlyce muestra placeholder; Wasi muestra chat y contactos de red. |
| Plan y facturación | [~] | Enlyce tiene checkout y estado de pago; comparar gestión de plan, factura y cuenta frente a Wasi. |

## Orden de trabajo acordado

1. **Importar clientes:** identificar formato real XLS/XLSX/CSV; vista previa; crear/actualizar/omitir duplicados con coincidencia segura; guardar cliente independiente; consentimiento desconocido si no hay evidencia y bloquear comunicaciones automáticas sin autorización.
2. **Directorio y ficha de cliente:** campos, ubicación, demandas, inmuebles vinculados, embudo y permisos por asesor; migrar sin crear oportunidades ficticias.
3. **Cerrar ciclo de importación/exportación:** resultados por fila, errores descargables, reintentos seguros y exportación con filtros/alcances equivalentes.
4. **Completar paridad de inmuebles:** validar campos/formulario y determinar si hace falta importación por archivo además de la integración Wasi existente.
5. **Red inmobiliaria:** acordar modelo multiempresa, invitaciones, visibilidad, publicación, comisiones y auditoría antes de implementarla.
6. **Módulos restantes:** agenda/eventos, reportes, usuarios, sitio web, foro, chat, configuración inicial y funciones de cuenta, según uso real de L&C.

## Criterio de cierre por módulo

- [ ] Flujo Wasi observado de inicio a fin; reglas no inferidas solo de nombres de campos o capturas.
- [ ] Campos, validaciones, permisos, filtros y errores documentados sin copiar datos personales.
- [ ] Enlyce implementado en API, dominio, persistencia e interfaz cuando aplique.
- [ ] Casos de éxito, permisos, duplicados y fallos cubiertos por pruebas.
- [ ] Migraciones revisadas con respaldo y restore de la base objetivo antes de producción.
- [ ] Captura visual y diferencias restantes anotadas aquí.

## Límites de la evidencia actual

- Los JSON muestran rutas, etiquetas, tipos de control y encabezados; no revelan de forma fiable validaciones, reglas del servidor ni resultados de acciones.
- Las capturas aclaran varias pantallas de clientes, cruces, agenda, inmuebles y reportes; aún faltan recorridos interactivos de importación, alianzas, ajustes y otros flujos.
- “Todo Wasi” queda acotado a las funciones disponibles para la cuenta L&C y al inventario observable. Cada nueva sección descubierta se agrega a esta lista.
