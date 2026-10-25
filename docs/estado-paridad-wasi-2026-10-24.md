# Estado verificable de paridad Wasi - 2026-10-24

**Base:** checkout aislado `codex/paridad-gate`, iniciado en `origin/master` (`d3bc6be`). El checkout principal contiene cambios ajenos y no se uso para publicar. Los 28 JSON inventariados conservan estructura de interfaz, no valores ni resultados de acciones. El ensayo autenticado de importacion descrito abajo aporta evidencia de resultados en Wasi; no acredita los demas flujos ni una prueba autenticada de Enlyce en produccion.

## Verificacion de la rama integrada

- Dominio: 256 aprobadas, 0 fallidas.
- Integracion completa tras el filtro de equipo y la correccion SQL: 299 aprobadas, 1 omitida, 0 fallidas. La prueba omitida requiere configuracion PostgreSQL propia; el ensayo manual del SQL se ejecuto en PostgreSQL 17 desechable. La matriz de autorizacion incorpora las rutas publicas de CMS y blog.
- CRM: `npm run build` y `npm run test:pwa` aprobados (4/4).
- Sitio Next.js: `npm run build` aprobado; `/blog` y `/blog/[slug]` se renderizan por solicitud.
- SQL: 44 migraciones historicas en script idempotente, sin limites de transaccion internos. `prepare-atomic.mjs` verifica que cada migracion generada tenga su par y registro. Ensayo en PostgreSQL 17 desechable: primera aplicacion y segunda aplicacion idempotente correctas, 44 entradas en `__EFMigrationsHistory` y 33 tablas publicas. Un fallo inyectado devolvio codigo 1 y dejo cero tablas en otra base vacia. La primera prueba descubrio y permitio corregir un punto y coma faltante en `AddCustomerFunnelStages`. No se restauro una copia exacta de Railway; ese gate permanece abierto.
- ImageSharp 3.1.12 produjo avisos NU1902/NU1903. No se cambio la dependencia en esta entrega.

## Modulos

| Modulo | Estado en codigo | Evidencia y commit propio | Bloqueo exacto para cerrar paridad |
|---|---|---|---|
| Clientes e importacion | Parcial | Formularios, directorio e importador CSV/XLS/XLSX en `origin/master`; ensayo real en Wasi: 19 importados y 44 rechazados. Las 19 fichas nuevas fueron conciliadas. Enlyce abre las 63 filas del XLSX, prioriza Movil, reconoce Estado y admite telefonos numericos exactos; detalle abajo. | Antes de importar en Enlyce faltan cuatro etapas personalizadas en el embudo y el gate de migraciones. Demandas importadas y exportacion enviada por correo siguen sin validar. |
| Demandas y cruces | Parcial | Cruce inverso y alcance por asesor presentes en `origin/master`; integracion verde. | Falta contraste del porcentaje Wasi y cruces con inventario aliado/en red; migraciones pendientes. |
| Inmuebles y fotos | Parcial | Ficha PDF y enlace WhatsApp desde CRM: `ca2110d`, build verde. Cola de fotos probada previamente en PostgreSQL sobre el arbol local, pero no publicable: [analisis de dependencia](analisis-cola-fotos-2026-10-23.md). | `PropertyMedia` y su migracion son cambios ajenos no publicados. Falta ficha con fotos reales verificadas, operacion doble, atribucion de propietario y sindicacion. El PDF se construye con datos de la ficha publica; no adjunta archivo a WhatsApp. |
| Agenda y tareas | Parcial | Rejilla consulta todas las tareas del periodo con alcance por asesor: `30d0fbb`; admin puede filtrar tareas y visitas por asesor mientras el asesor no puede consultar a otro. Pruebas dirigidas y build verdes. | Faltan Google Calendar, eventos generales y comparacion de estados/acciones con una sesion Wasi. |
| Gestiones | Parcial | Interacciones de `origin/master` y suite integrada verde. | `ContactActivity` y la union completa de gestiones siguen en el checkout ajeno sin publicar; catalogo y fechas Wasi sin validacion dinamica. |
| Calificaciones | Faltante en rama publicada | El flujo local depende de `ServiceRating`, no presente en `origin/master`. | Entidad, modulo, migraciones y correo de invitacion ajenos no publicables de forma aislada; falta validar correo real y reglas Wasi. |
| Reportes y ACM | Parcial | ACM `/mercado` (`a9acd4b`), exportacion de propietarios (`62cb395`), edicion de negocios (`21dbc40`); suite verde. | Formulas, periodos, estados de visita y categorias de gestiones Wasi no se pueden certificar con los JSON. |
| Red inmobiliaria, MLS y muro | Faltante | El scrape prueba rutas/controles, no reglas interempresa; no se publico codigo nuevo. | Faltan identidad de agencia, acuerdos, permisos, visibilidad, moderacion y evidencia de acciones reales. |
| Foro inmobiliario | Faltante | Captura de escritorio y ruta `/usuario/muro`; no hay flujo interactivo ni codigo Enlyce. | Faltan audiencia, moderacion, permisos y estados observados. |
| Inmuebles de aliados | Faltante | Inventario Wasi muestra el acceso; Enlyce solo cruza inventario propio. | Faltan convenio, procedencia, propiedad de datos y permisos observados. |
| Alianzas y grupos | Faltante | Accesos Wasi inventariados, sin ciclo de invitacion/aceptacion verificado. | Faltan reglas interempresa y decision de alcance L&C. |
| Configuracion inicial de empresa | Faltante | Asistente visible en Wasi; Enlyce posee ajustes comerciales parciales. | Faltan pantallas completas, validaciones y estados de puesta en marcha. |
| Chat interno | Implementado, paridad parcial | SignalR, REST y UI `/chat` en `3782d64`, 11 pruebas de dominio y 6 de integracion registradas. | Falta comparar flujo, permisos y errores con sesion Wasi real; migracion pendiente de produccion. |
| Sitio publico y CMS | Parcial | CMS (`72258c5`) y blog editorial con borrador/publicacion, API admin, rutas publicas y SEO: `55929d2`; 3 pruebas de blog y builds verdes. | `AddBlogPosts` y `AddPublicWebsiteConfig` pendientes de produccion. Editor directo desde web depende de decidir dominio/sesion. El blog no se valido visualmente contra Wasi. |
| Portales externos | Faltante | Publicaciones CRM y enlace WhatsApp existentes; sin integracion con Finca Raiz o Metrocuadrado. | Faltan contratos/API, credenciales de prueba, mapeo de estados y reconciliacion de errores. |
| Movil | Parcial | PWA CRM (`5aa5159`) y fallback publico (`1abd1c3`); PWA 4/4. | Falta prueba en dispositivo real y cliente nativo equivalente a las apps Wasi. |
| Redes sociales | Sin verificar | Controles Wasi visibles y enlaces sociales configurables en CMS. | Faltan sesion real, credenciales y contrato de publicacion; no se presume automatizacion. |
| Cuenta, soporte y academia | Sin verificar | Accesos Wasi visibles en scrape; sin equivalencia interna comprobada. | Decidir si L&C requiere funciones propias del proveedor Wasi o solo soporte interno. |
| Reputacion y facturacion | Parcial | Calificaciones locales no publicadas; checkout y estado de pago de Enlyce existentes. | Faltan reglas de invitacion/resena y decision de alcance de suscripcion/factura. |
| Migraciones Railway | Parcial, sin despliegue | Script corregido para transaccion unica y ensayado dos veces en PostgreSQL 17 desechable; 44 migraciones, 33 tablas, rollback completo ante fallo inyectado. | Requiere respaldo y restauracion de la base objetivo exacta, ensayo sobre esa copia, comparacion de su `__EFMigrationsHistory`, retencion y alerta del respaldo, y autorizacion explicita. No se aplico ni desplego nada. |

## Ensayo autenticado de importacion de clientes (2026-10-24)

- Fuente: ZIP real entregado por el usuario, exportado por Wasi el 15/10/2026. Contiene un XLSX con 63 filas y 18 columnas. No se copiaron nombres, correos, telefonos ni identificaciones a este documento.
- Calidad del origen: 61 filas sin correo, 62 sin identificacion y un nombre completo repetido. No hay formulas ni vinculos externos. El XLSX trae margenes de pagina invalidos para `openpyxl`; el analisis de filas se hizo leyendo OOXML y Wasi acepto el archivo original.
- `Estado` exporta valores `Publicado` (28), `Cerrado Negativo` (1), `Seguimiento 2` (1), `Visita Inmueble` (12) y `Nuevo` (21). El importador solo acepta `Nuevo`, `En proceso`, `Convertido`, `Recuperado` y `Perdido`. Exportar e importar en la misma cuenta no conserva esas etapas personalizadas.
- El importador muestra cinco filas de vista previa y obliga a mapear columnas. Se desmarco `Incluir primera fila` para excluir encabezados. Se mapearon 15 de 18 columnas; `Ubicacion`, `Fecha Registro` e `Inmuebles enlazados` no tienen destino directo y se ignoraron. Se conservaron los valores reales de `Estado` para observar su validacion.
- Tras autorizar el usuario la importacion de las 63 filas, Wasi proceso cada cliente y mostro resultados por fila. Resultado: 19 `Cliente importado con exito` con enlace a ficha y 44 rechazados. Aparecieron 42 errores de estado, 2 de correo ya usado y 2 de telefono ya usado; una fila puede acumular varios errores. Los 19 exitos corresponden a las filas 43, 45, 46 y 48-63 del resultado Wasi.
- La operacion escribio 19 contactos en la cuenta real. No se repitio el lote ni se corrigieron etapas. Se concilio una ficha nueva mediante `Combinar` desde la ficha original, conservando su ID y sus campos; se retiraron las otras 18 fichas nuevas por sus IDs de resultado. El directorio quedo en 68 clientes y ninguno de los 19 IDs nuevos aparece en la primera pagina. No se ejecuto la exportacion por correo ni se verificaron demandas.

## Ajuste del importador Enlyce con el XLSX real (2026-10-24)

- `ClientImportTableReader` abrio el XLSX original dentro del ZIP: 63 filas y 18 columnas. No se agrego el archivo real ni valores personales al repositorio.
- El mapeo automatico ahora prioriza `Movil` como telefono principal y `Telefono` como fijo, reconoce `Estado`, `Referido` y `Observaciones Adicionales`, y evita asignar dos veces una unica columna de telefono.
- El lector marco 58 celdas telefonicas numericas. Sus valores son enteros exactos de 10 a 13 digitos; la validacion admite numeros de 7 a 15 digitos sin cero inicial y conserva el rechazo de identificaciones numericas inseguras. El ensayo local clasifico las 58 como seguras sin mostrar sus valores.
- Las cuatro etapas personalizadas `Publicado`, `Cerrado Negativo`, `Seguimiento 2` y `Visita Inmueble` deben crearse en el embudo de Enlyce antes de confirmar esta carga; el importador no inventa ni cambia etapas. `Ubicacion`, `Fecha Registro`, `Inmuebles enlazados` y `Encargado Del Cliente` no tienen un mapeo automatico equivalente.
- Verificacion de la rama: 299 pruebas de integracion aprobadas y 1 omitida; 86 pruebas de aplicacion aprobadas; build CRM y 2 pruebas del mapeo aprobados. El cambio no se desplego ni aplico migraciones en Railway.

## Orden de cierre

1. Ensayar el gate SQL sobre una restauracion exacta de Railway; la prueba en base vacia no sustituye esa verificacion.
2. Publicar `PropertyMedia` por su autor y luego portar/volver a probar la cola de fotos.
3. Separar gestiones y calificaciones locales; verificar permisos por asesor y correo real.
4. Crear las cuatro etapas de L&C y ejecutar carga controlada en Enlyce tras el gate de migraciones; completar evidencia de exportacion, demandas y otros flujos Wasi. Negociar contratos de portales/MLS.

**Conclusion:** hay avances publicables en una rama aislada, pero la paridad total permanece abierta.

## Actualización posterior del 24/10/2026

La sección inicial de verificación registra una entrega anterior. El gate Railway avanzó: el dump exacto del 24/10 fue restaurado y ensayado en PostgreSQL 18 aislado (23 a 44 migraciones, 24 a 33 tablas, segunda aplicación idempotente y rollback ante falla). Las cuatro etapas de L&C se crearon solo en esa copia. La tarea de respaldo ejecutada manualmente desde el Programador terminó con código 0 y subida correcta a las 19:53; falta comprobar la siguiente ejecución automática, retención remota y alertas. No hubo migración, carga ni despliegue en producción.

La ejecución automática del 25/10 a las 2:00 terminó con código 0 y subida de un dump de 110.901 bytes, según el Programador y el log local. Hay un monitor de fallos/omisiones en la rama, probado con casos sintéticos y lectura real. La tarea se instaló, pero la credencial local no pasó la prueba SMTP: alerta externa aún inactiva. Falta corregirla, comprobar recepción y cerrar retención remota.

El dump automático del 25/10 se restauró después en PostgreSQL 18 aislado y pasó el SQL dos veces: 23 a 44 migraciones, 24 a 33 tablas y conteos de datos 3/1/26/1 intactos. El seed creó las cuatro etapas de L&C solo en esa copia y fue idempotente. Hay una vista previa remota de retención (13 objetos conservados, cero candidatos), sin borrado. El gate de producción sigue abierto por alerta real, política de retención, decisión de reversa y autorización expresa; estos ensayos no son un despliegue.

El procesador de fotos ahora usa Magick.NET 15.0.0. Se retiraron ImageSharp y NPOI del grafo de Infrastructure; `PropertyImportTableReader` conserva lectura XLS/XLSX y rechazo estructural de fórmulas mediante ExcelDataReader y OpenMcdf. El usuario eligió esta sustitución para evitar la nueva EULA de NPOI. La auditoría actual de NuGet no reportó paquetes vulnerables en Infrastructure. Pruebas dirigidas de imágenes e importación de propiedades pasaron en Windows y Linux (23/23 en Linux); suite Windows: 256 dominio, 86 aplicación, 310 integración aprobadas y una omitida. Fotos y cola siguen apagadas por la procedencia pendiente de `PropertyMedia` y la falta de ensayo con archivos reales.

El 25/10 se observó el [cruce de demandas en Wasi](evidencia-cruce-demandas-wasi-2026-10-25.md) con sesión L&C: contadores separados para propios, aliados y red; una coincidencia en red mostró `Acierto: 56.6%`. El editor mostró rangos y características; no se guardó ni borró nada. Enlyce solo cruza inventario propio mediante una regla booleana. Fórmula, permisos y contratos siguen abiertos.

También se observaron [gestiones y tareas en Wasi](evidencia-gestiones-wasi-2026-10-25.md): lista global vacía y formularios de llamada, evento y visita abiertos/cancelados desde una ficha. Se documentaron catálogo, campos, estados y avisos; ningún guardado, permiso o envío se acreditó. La separación de `ContactActivity` y `ServiceRating` sigue pendiente.

En [blog y banners Wasi](evidencia-cms-wasi-2026-10-25.md) se inspeccionaron las listas vacías y los formularios de alta sin guardar. El blog pidió título, imagen, resumen y cuerpo; el banner, imagen, título, vínculo y estado. Falta probar publicación real y decidir si L&C requiere varios banners ordenables; no se acredita paridad CMS.

La ficha Enlyce incorporó los intervalos de aviso de tareas observados en Wasi, con cálculo desde la fecha del compromiso y opción personalizada. El build del CRM pasó; esto no prueba que el aviso se entregue por el mismo canal de Wasi ni que se notifique al cliente.

En [conexión de portales Wasi](contratos-portales-wasi-2026-10-25.md) se observaron Fincaraiz y Metrocuadrado conectados y varios portales gratuitos conectados. Se documentaron controles y contratos requeridos sin leer credenciales ni publicar anuncios. L&C debe elegir portales y proporcionar acceso/API independientes; el estado conectado en Wasi no constituye autorización de integración Enlyce.
