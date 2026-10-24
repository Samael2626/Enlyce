# Estado verificable de paridad Wasi - 2026-10-24

**Base:** checkout aislado `codex/paridad-gate`, iniciado en `origin/master` (`d3bc6be`). El checkout principal contiene cambios ajenos y no se uso para publicar. Los 28 JSON inventariados conservan estructura de interfaz, no valores ni resultados de acciones. Este corte no acredita una sesion funcional en Wasi ni una prueba autenticada en produccion.

## Verificacion de la rama integrada

- Dominio: 256 aprobadas, 0 fallidas.
- Integracion: 297 aprobadas, 1 omitida (requiere PostgreSQL), 0 fallidas. La matriz de autorizacion incorpora las rutas publicas de CMS y blog.
- CRM: `npm run build` y `npm run test:pwa` aprobados (4/4).
- Sitio Next.js: `npm run build` aprobado; `/blog` y `/blog/[slug]` se renderizan por solicitud.
- SQL: 44 migraciones historicas en script idempotente, sin `START TRANSACTION` ni `COMMIT` internos. `prepare-atomic.mjs` verifica que cada migracion generada tenga su par y registro. No se ensayo `psql` ni restauracion: Docker Desktop esta detenido y `psql` no esta disponible.
- ImageSharp 3.1.12 produjo avisos NU1902/NU1903. No se cambio la dependencia en esta entrega.

## Modulos

| Modulo | Estado en codigo | Evidencia y commit propio | Bloqueo exacto para cerrar paridad |
|---|---|---|---|
| Clientes e importacion | Parcial | Formularios, directorio e importador CSV/XLS/XLSX en `origin/master`; suite integrada verde. | No hay archivo ni carga real de importacion Wasi. Duplicados, demandas importadas y valores dinamicos siguen sin evidencia. Migraciones pendientes de produccion. |
| Demandas y cruces | Parcial | Cruce inverso y alcance por asesor presentes en `origin/master`; integracion verde. | Falta contraste del porcentaje Wasi y cruces con inventario aliado/en red; migraciones pendientes. |
| Inmuebles y fotos | Parcial | Ficha PDF y enlace WhatsApp desde CRM: `ca2110d`, build verde. Cola de fotos probada previamente en PostgreSQL sobre el arbol local, pero no publicable: [analisis de dependencia](analisis-cola-fotos-2026-10-23.md). | `PropertyMedia` y su migracion son cambios ajenos no publicados. Falta ficha con fotos reales verificadas, operacion doble, atribucion de propietario y sindicacion. El PDF se construye con datos de la ficha publica; no adjunta archivo a WhatsApp. |
| Agenda y tareas | Parcial | Rejilla consulta todas las tareas del periodo con alcance por asesor: `30d0fbb`; admin puede filtrar tareas y visitas por asesor mientras el asesor no puede consultar a otro. Pruebas dirigidas y build verdes. | Faltan Google Calendar, eventos generales y comparacion de estados/acciones con una sesion Wasi. |
| Gestiones | Parcial | Interacciones de `origin/master` y suite integrada verde. | `ContactActivity` y la union completa de gestiones siguen en el checkout ajeno sin publicar; catalogo y fechas Wasi sin validacion dinamica. |
| Calificaciones | Faltante en rama publicada | El flujo local depende de `ServiceRating`, no presente en `origin/master`. | Entidad, modulo, migraciones y correo de invitacion ajenos no publicables de forma aislada; falta validar correo real y reglas Wasi. |
| Reportes y ACM | Parcial | ACM `/mercado` (`a9acd4b`), exportacion de propietarios (`62cb395`), edicion de negocios (`21dbc40`); suite verde. | Formulas, periodos, estados de visita y categorias de gestiones Wasi no se pueden certificar con los JSON. |
| Red inmobiliaria, MLS y muro | Faltante | El scrape prueba rutas/controles, no reglas interempresa; no se publico codigo nuevo. | Faltan identidad de agencia, acuerdos, permisos, visibilidad, moderacion y evidencia de acciones reales. |
| Sitio publico y CMS | Parcial | CMS (`72258c5`) y blog editorial con borrador/publicacion, API admin, rutas publicas y SEO: `55929d2`; 3 pruebas de blog y builds verdes. | `AddBlogPosts` y `AddPublicWebsiteConfig` pendientes de produccion. Editor directo desde web depende de decidir dominio/sesion. El blog no se valido visualmente contra Wasi. |
| Portales externos | Faltante | Publicaciones CRM y enlace WhatsApp existentes; sin integracion con Finca Raiz o Metrocuadrado. | Faltan contratos/API, credenciales de prueba, mapeo de estados y reconciliacion de errores. |
| Movil | Parcial | PWA CRM (`5aa5159`) y fallback publico (`1abd1c3`); PWA 4/4. | Falta prueba en dispositivo real y cliente nativo equivalente a las apps Wasi. |
| Migraciones Railway | Preparado, no ejecutado | Script corregido para transaccion unica; migracion de blog agregada. | Requiere respaldo y restauracion de base objetivo, ensayo de error intermedio, comparacion de `__EFMigrationsHistory` y autorizacion explicita. No se aplico ni desplego nada. |

## Orden de cierre

1. Ensayar el gate SQL sobre una restauracion exacta; no usar el script en produccion mientras falte esa prueba.
2. Publicar `PropertyMedia` por su autor y luego portar/volver a probar la cola de fotos.
3. Separar gestiones y calificaciones locales; verificar permisos por asesor y correo real.
4. Conseguir evidencia o acceso Wasi para reglas dinamicas y negociar contratos de portales/MLS.

**Conclusion:** hay avances publicables en una rama aislada, pero la paridad total permanece abierta.
