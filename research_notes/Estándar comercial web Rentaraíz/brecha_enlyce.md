# Brecha de ENLYCE frente a un estándar web inmobiliario comercial

Estado auditado: 2026-09-25. Alcance: `website/sitio`, catálogo público y captación pública de `Enlyce.Api`. Rentaraíz se usa como referencia de patrón comercial, no como plantilla visual ni como licencia para copiar contenido.

## ¿Qué rutas y capacidades actuales son reales y verificadas?

### Takeaway

ENLYCE ya no es una maqueta: tiene catálogo público conectado, fichas indexables, favoritos locales, zonas, captación de compradores y propietarios, privacidad y canales sociales. La brecha principal no es “tener páginas”, sino convertir el inventario y medir el embudo con disciplina comercial.

### Cited Findings

#### CONFIRMADO

- Las rutas públicas implementadas son `/`, `/inmuebles`, `/inmuebles/[slug]`, `/zonas`, `/zonas/[slug]`, `/propietarios`, `/favoritos`, `/contacto` y `/privacidad`, además de `robots.txt`, `sitemap.xml`, error y 404. — [árbol de rutas](../../website/sitio/src/app)
- La navegación principal expone Inmuebles, Zonas, Propietarios, Favoritos y Contacto; el layout incluye salto al contenido, `lang="es-CO"`, dock social global y política de datos en el pie. — [layout](../../website/sitio/src/app/layout.tsx#L27-L77)
- Inicio consume seis publicaciones reales, ofrece entradas por venta/arriendo/propietario, muestra destacados y zonas. — [inicio](../../website/sitio/src/app/page.tsx#L10-L106)
- El catálogo usa filtros compartibles por URL, listado paginado y estados vacío/carga. El API admite operación, tipo, municipio, barrio, rangos de precio y área, habitaciones, baños, parqueaderos y orden; la UI visible del catálogo sólo expone operación, tipo, barrio y orden. — [cliente de catálogo](../../website/sitio/src/lib/api/catalog.ts#L14-L97), [filtros visibles](../../website/sitio/src/components/CatalogFilters.tsx#L32-L118), [contrato público](../../src/Enlyce.Api/Endpoints/PublicCatalog/PublicCatalogModule.cs#L77-L109)
- La ficha tiene metadatos dinámicos, canonical, Open Graph con foto, galería, precio, administración, características, amenidades, ubicación aproximada, asesor, solicitud de visita y favorito. — [ficha](../../website/sitio/src/app/inmuebles/%5Bslug%5D/page.tsx#L13-L158)
- El API público entrega listado y ficha, cachea respuestas exitosas cinco minutos, no expone dirección exacta ni datos del propietario y devuelve el mismo 404 para publicación pausada o inexistente. — [módulo público](../../src/Enlyce.Api/Endpoints/PublicCatalog/PublicCatalogModule.cs#L9-L74), [pruebas de contrato](../../tests/Enlyce.IntegrationTests/PublicCatalogEndpointTests.cs#L21-L158)
- La captación pública crea leads sin sesión, asocia opcionalmente la publicación, canal e interés del propietario; la UI exige autorización de tratamiento, conserva errores y recoge UTM en la fuente. — [endpoint de leads](../../src/Enlyce.Api/Endpoints/Leads/LeadsModule.cs#L32-L52), [cliente de leads](../../website/sitio/src/lib/api/leads.ts#L43-L99), [formulario](../../website/sitio/src/components/ContactForm.tsx#L33-L204)
- Propietarios presenta vender, arrendar, administrar y avaluar, pero deriva toda acción a Contacto. — [página de propietarios](../../website/sitio/src/app/propietarios/page.tsx#L10-L49)
- Favoritos existen sólo en `localStorage`; no requieren cuenta y sólo guardan slug/título, no una ficha sincronizada. — [almacenamiento de favoritos](../../website/sitio/src/lib/favorites.ts), [página de favoritos](../../website/sitio/src/app/favoritos/page.tsx#L7-L41)
- SEO técnico básico existe: metadata global, metadata por ficha/zona, sitemap que recorre todo el catálogo y robots. — [metadata global](../../website/sitio/src/app/layout.tsx#L10-L25), [sitemap](../../website/sitio/src/app/sitemap.ts#L8-L52), [robots](../../website/sitio/src/app/robots.ts)
- El 2026-09-25 se comprobó en ejecución local `200` para `http://localhost:5019/health`, catálogo público con 12 publicaciones y `200` para inicio, inmuebles y contacto. ESLint y `tsc --noEmit` terminaron sin errores. — [health](../../src/Enlyce.Api/Endpoints/Health/HealthCheck.cs), [catálogo](../../src/Enlyce.Api/Endpoints/PublicCatalog/PublicCatalogModule.cs)

#### PROBABLE

- La base móvil es razonable porque el buscador cambia de seis columnas a tres y luego una, y el dock social se compacta. No equivale a una auditoría visual de todos los viewports. — [CSS responsive](../../website/sitio/src/app/globals.css#L142-L150), [dock responsive](../../website/sitio/src/app/globals.css#L229-L238)
- Las páginas de zona pueden captar tráfico local porque tienen URL, metadata y listado filtrado, pero el contenido editorial actual es corto y estático. — [zona](../../website/sitio/src/app/zonas/%5Bslug%5D/page.tsx#L12-L56), [datos de zonas](../../website/sitio/src/lib/zones.ts)

#### NO VERIFICADO

- No hay prueba automatizada del frontend, E2E, accesibilidad, Lighthouse, navegación móvil ni conversión; `package.json` sólo define lint, typecheck y build. — [scripts](../../website/sitio/package.json#L5-L12)
- El build compiló el código, pero terminó en `spawn EPERM` durante la fase posterior de TypeScript; por tanto no debe registrarse como build de producción verde en este corte. El fallo parece del entorno/proceso, no prueba de defecto funcional.
- Las pruebas .NET seleccionadas no pudieron ejecutarse porque la API activa bloqueó sus DLL de salida; los contratos sí tienen pruebas en el repositorio, pero no se revalidaron en esta auditoría. — [pruebas del catálogo](../../tests/Enlyce.IntegrationTests/PublicCatalogEndpointTests.cs)
- No se verificó producción, DNS, TLS, CDN, almacenamiento de medios, métricas reales ni entrega de leads al equipo comercial.

### Inferences

- La arquitectura soporta una vitrina comercial pequeña/mediana. El cuello de botella inmediato está en descubrimiento, conversión contextual, confianza y medición, no en rehacer el stack.
- La coexistencia del panel completo y `CatalogFilters` ya muestra dos fuentes de configuración de filtros; si se amplían por separado se desincronizarán.

### Gaps

- Falta evidencia de tráfico, tasa de contacto, tiempo de respuesta, calidad del inventario y comportamiento en producción.
- Falta una suite que pruebe del buscador al lead: búsqueda → ficha → contacto → CRM.

## ¿Dónde debe aparecer el buscador completo, compacto o no aparecer?

### Takeaway

El panel completo pertenece al inicio. En Contacto distrae del objetivo y debe retirarse o moverse al final como salida secundaria. Las demás rutas necesitan variantes compactas y contextuales, no repetir el mismo bloque grande por obedecer al referente.

### Cited Findings

| Ruta | Recomendación exacta | Motivo | Evidencia actual |
|---|---|---|---|
| `/` | **Panel completo**, superpuesto al hero, como está. | Es la puerta principal de descubrimiento y permite expresar intención antes de ver inventario. | [inicio](../../website/sitio/src/app/page.tsx#L38-L63) |
| `/inmuebles` | **Filtros compactos y persistentes**, preferiblemente barra sticky en escritorio y drawer “Filtros” en móvil. No duplicar panel completo. | El usuario ya está en resultados; necesita refinar, ordenar y ver filtros activos, no volver a empezar. | [listado](../../website/sitio/src/app/inmuebles/%28listado%29/page.tsx#L15-L77), [filtros](../../website/sitio/src/components/CatalogFilters.tsx#L32-L118) |
| `/zonas/[slug]` | **Buscador compacto prellenado** debajo del encabezado, con la zona visible y opción de cambiarla. | Convierte una landing SEO local en entrada al inventario sin romper el contexto de zona. | [zona filtrada](../../website/sitio/src/app/zonas/%5Bslug%5D/page.tsx#L28-L56) |
| `/zonas` | **Buscador compacto de ubicación** encima de las tarjetas o sólo CTA “Ver inmuebles por zona”. | Aquí la intención es explorar geografía; seis controles antes de elegir zona es exceso. | [índice de zonas](../../website/sitio/src/app/zonas/page.tsx#L11-L34) |
| `/inmuebles/[slug]` | **Barra mínima “Modificar búsqueda”** colapsable, después del breadcrumb o al final de la ficha. Mantener arriba la ficha y la conversión. | El visitante puede necesitar alternativas, pero el objetivo principal es contactar por ese inmueble. | [ficha y CTA](../../website/sitio/src/app/inmuebles/%5Bslug%5D/page.tsx#L45-L158) |
| `/favoritos` | **Compacto sólo en estado vacío**; con favoritos, CTA a catálogo. | Ayuda a recuperarse del vacío sin competir con la comparación de guardados. | [favoritos](../../website/sitio/src/app/favoritos/page.tsx#L19-L40) |
| `/contacto` | **Sin panel completo antes del formulario**. Como máximo, enlace “Seguir buscando” o compacto al final después de enviar. | La ruta tiene intención de contacto; el panel actual desvía al usuario justo antes del formulario. | [contacto actual](../../website/sitio/src/app/contacto/page.tsx#L40-L80) |
| `/propietarios` | **Sin buscador de comprador**. Usar formulario corto de captación del inmueble. | Mezclar búsqueda de vivienda con captación del propietario divide el mensaje comercial. | [propietarios](../../website/sitio/src/app/propietarios/page.tsx#L17-L49) |
| `/privacidad` | **Ninguno**. | Es una ruta legal, no una landing comercial. | [privacidad](../../website/sitio/src/app/privacidad/page.tsx) |
| 404 | **Compacto** con ubicación/operación y enlace al catálogo. | Recupera sesiones que llegan por inmuebles retirados o URLs viejas. | [404 actual](../../website/sitio/src/app/not-found.tsx) |

- El panel completo actual envía barrio, operación, tipo, precio máximo y habitaciones por GET; todos corresponden a parámetros reales del API. — [panel](../../website/sitio/src/components/PropertySearchPanel.tsx#L36-L89), [request público](../../src/Enlyce.Api/Endpoints/PublicCatalog/PublicCatalogModule.cs#L77-L109)
- El catálogo ya conserva filtros en URL y reinicia paginación al cambiar un filtro, buena base para una variante compacta. — [CatalogFilters](../../website/sitio/src/components/CatalogFilters.tsx#L38-L55)

### Inferences

- La repetición de Rentaraíz es útil cuando mantiene viva la búsqueda entre páginas de descubrimiento; no debe copiarse en páginas cuya conversión es otra.
- Conviene crear una sola definición de filtros y tres presentaciones: `full`, `compact` y `drawer`. Así etiquetas, enums y query strings no divergen.

### Gaps

- No hay datos de analítica que demuestren si el buscador de Contacto ayuda o roba conversiones; la recomendación se basa en jerarquía de intención y debe validarse con eventos.
- No existe catálogo de municipios/barrios ni endpoint de facetas, por lo que la ubicación es texto libre y no puede ofrecer autocompletado fiable.

## ¿Qué brechas existen y cuál es el backlog priorizado?

### Takeaway

Primero hay que cerrar el embudo medible “encontrar → evaluar → contactar”. Después se amplían confianza, captación de propietarios y SEO. Funciones de portal grande —cuentas, alertas, comparación avanzada— van después porque tienen mayor costo operativo.

### Cited Findings

#### P0 — estándar comercial mínimo (siguiente bloque de trabajo)

1. **Unificar y completar filtros.** Extraer una definición común para panel completo/compacto; agregar precio mínimo/máximo, área, baños y parqueaderos al catálogo; mostrar chips activos, “limpiar” y cantidad de resultados. El API ya soporta esos campos, pero la UI del catálogo no. — [contrato](../../src/Enlyce.Api/Endpoints/PublicCatalog/PublicCatalogModule.cs#L77-L109), [UI actual](../../website/sitio/src/components/CatalogFilters.tsx#L57-L117)
2. **Conversión contextual en ficha.** Añadir CTA sticky móvil, WhatsApp con título/URL/código del inmueble, llamada al teléfono público del asesor, compartir y un formulario corto embebido. Hoy la ficha sólo enlaza a Contacto y el WhatsApp global no identifica la publicación. — [CTA actual](../../website/sitio/src/app/inmuebles/%5Bslug%5D/page.tsx#L140-L157), [dock global](../../website/sitio/src/components/SocialContactDock.tsx#L8-L60), [asesor disponible en API](../../src/Enlyce.Application/PublicCatalog/PublicCatalogModels.cs#L89-L93)
3. **Instrumentación del embudo.** Eventos para búsquedas, cero resultados, filtros, vista de ficha, favorito, WhatsApp, llamada, envío de lead y propietario. UTM se conserva parcialmente, pero no existe integración analítica en el código del sitio. — [UTM actual](../../website/sitio/src/lib/api/leads.ts#L47-L64), [dependencias](../../website/sitio/package.json#L13-L27)
4. **Corregir colocación del buscador.** Mantener completo en Inicio, retirar el bloque completo de Contacto y desplegar variantes contextuales según la matriz anterior. — [inicio](../../website/sitio/src/app/page.tsx#L61-L63), [contacto](../../website/sitio/src/app/contacto/page.tsx#L51-L79)
5. **QA comercial automatizado.** Playwright: home→filtro→resultados→ficha→lead, favoritos, propietario, 404, móvil; axe/Lighthouse en CI. No hay archivos ni scripts de pruebas web. — [package.json](../../website/sitio/package.json#L5-L12)

#### P1 — credibilidad, SEO y captación

6. **Confianza verificable.** Crear Nosotros/equipo, ficha real de asesores, dirección y horario, NIT/registro si corresponde, proceso de compra/arriendo, preguntas frecuentes y testimonios sólo con evidencia/autorización. El layout actual sólo declara empresa, ciudad y política. — [layout](../../website/sitio/src/app/layout.tsx#L68-L76)
7. **Captación de propietarios en la misma página.** Sustituir CTA único por formulario corto con servicio, tipo, municipio/barrio, estado de ocupación, precio esperado y mejor horario; dejar documentos/fotos para un segundo paso. La ruta actual sólo describe servicios y manda a Contacto. — [propietarios](../../website/sitio/src/app/propietarios/page.tsx#L28-L49), [contrato de lead actual](../../src/Enlyce.Api/Endpoints/Leads/LeadsModule.cs#L78-L88)
8. **SEO inmobiliario estructurado.** JSON-LD `RealEstateAgent`/`LocalBusiness`, `BreadcrumbList` y un esquema compatible para cada oferta; canonical/noindex para filtros y Favoritos; OG/Twitter completos y sitemap de imágenes. Hoy hay metadata, canonical de ficha/zona y sitemap básico, pero no datos estructurados. — [metadata de ficha](../../website/sitio/src/app/inmuebles/%5Bslug%5D/page.tsx#L13-L33), [sitemap](../../website/sitio/src/app/sitemap.ts#L25-L52), [favoritos sin metadata](../../website/sitio/src/app/favoritos/page.tsx#L1-L44)
9. **Contenido local útil.** Enriquecer zonas con rangos de precio basados en inventario, movilidad, comercio, perfil de vivienda y enlaces a filtros preaplicados. Hoy son resúmenes estáticos y listados. — [zonas](../../website/sitio/src/lib/zones.ts), [landing de zona](../../website/sitio/src/app/zonas/%5Bslug%5D/page.tsx#L28-L56)
10. **Resultados más comerciales.** Vista lista/mapa, conteo visible, filtros sticky, URL compartible, estados sin resultados con alternativas y tarjetas con más contexto. Las coordenadas aproximadas ya llegan en cada resultado, así que el mapa no requiere contrato nuevo. — [modelo de listado](../../src/Enlyce.Application/PublicCatalog/PublicCatalogModels.cs#L36-L52), [listado actual](../../website/sitio/src/app/inmuebles/%28listado%29/page.tsx#L21-L77)
11. **Accesibilidad y móvil.** Menú móvil real, objetivos táctiles, gestión de foco en drawers/galería, contraste, pruebas teclado/lector y reducción de movimiento. Ya existen etiquetas, alt, skip link y algunos estados ARIA, pero no auditoría automatizada. — [layout accesible](../../website/sitio/src/app/layout.tsx#L35-L64), [controles del catálogo](../../website/sitio/src/components/CatalogFilters.tsx#L57-L117)

#### P2 — retención y producto portal

12. **Favoritos comerciales.** Mostrar tarjetas actualizadas, total/comparación y CTA para consultar varios. Hoy sólo se persisten slug y título en el navegador. — [favoritos](../../website/sitio/src/lib/favorites.ts), [vista](../../website/sitio/src/app/favoritos/page.tsx#L19-L40)
13. **Búsquedas guardadas y alertas.** Cuenta opcional o enlace mágico, preferencias de búsqueda, consentimiento y notificaciones de nuevos inmuebles/cambios de precio. No hay contrato público para ello; las alertas actuales pertenecen al CRM autenticado. — [alertas privadas](../../src/Enlyce.Api/Endpoints/Alertas/AlertasModule.cs)
14. **Comparador.** Comparar 2–4 propiedades por precio, área, administración, habitaciones, ubicación y amenidades. Puede iniciar en cliente; compartir comparaciones o sincronizarlas requiere persistencia.
15. **Agenda real de visita.** Disponibilidad del asesor, franja, confirmación y reprogramación. El sitio sólo crea lead; Visitas está autenticado para CRM. — [lead público](../../src/Enlyce.Api/Endpoints/Leads/LeadsModule.cs#L32-L52), [visitas privadas](../../src/Enlyce.Api/Endpoints/Visitas/VisitasModule.cs#L35-L97)
16. **Operación y observabilidad.** SLA de respuesta, asignación automática, alerta por lead sin atender, frescura de publicación, enlaces/fotos rotas, métricas de API/SSR y trazabilidad de campaña. El campo fuente mezcla contexto y UTM en una cadena, suficiente para MVP pero débil para atribución. — [serialización UTM](../../website/sitio/src/lib/api/leads.ts#L49-L64)

### Inferences

- Orden recomendado de ejecución: **P0 filtros/CTA/analítica/QA → P1 confianza/propietarios/SEO/resultados → P2 retención/agenda**.
- Mapa de resultados, compartir y CTA contextual pueden salir sin esperar backend. Facetas, búsqueda por código, agenda, alertas y captación rica sí requieren contratos.
- No conviene crear cuentas públicas antes de validar que favoritos/alertas generan retorno; un enlace mágico o alertas por correo puede ofrecer valor con menos fricción.

### Gaps

- No hay benchmark cuantitativo de conversión de Rentaraíz ni acceso a su analítica; “estándar comercial” se evalúa por capacidades observables y por la brecha del embudo local.
- No se validaron obligaciones regulatorias específicas de matrícula/registro inmobiliario ni textos legales comerciales; requieren revisión jurídica colombiana.

## ¿Qué brechas necesitan contratos de backend antes de diseñar UI definitiva?

### Takeaway

No todo requiere backend. La regla es simple: si la función depende de inventario derivado, identidad, persistencia multi-dispositivo, disponibilidad o automatización comercial, primero se define contrato y modelo; si sólo reorganiza datos ya públicos, puede hacerse en frontend.

### Cited Findings

| Capacidad | ¿Backend primero? | Contrato mínimo recomendado | Evidencia/límite actual |
|---|---:|---|---|
| Autocompletar municipio/barrio y mostrar cantidades | Sí | `GET /api/public/catalog/facets?operation=&propertyType=` con valores normalizados y conteos | Hoy municipio/barrio son strings libres. — [request](../../src/Enlyce.Api/Endpoints/PublicCatalog/PublicCatalogModule.cs#L77-L109) |
| Búsqueda por código | Sí | Campo `publicReference` estable, único y no sensible; endpoint o query `reference` | Listado/ficha no exponen código público. — [modelos públicos](../../src/Enlyce.Application/PublicCatalog/PublicCatalogModels.cs#L36-L77) |
| Mapa de resultados | No inicialmente | Usar coordenadas aproximadas ya incluidas; clustering es cliente | El listado ya devuelve latitud/longitud aproximadas. — [modelo](../../src/Enlyce.Application/PublicCatalog/PublicCatalogModels.cs#L36-L52) |
| WhatsApp, llamada y compartir contextual | No | Construir URL con slug/título; usar `advisor.publicPhone` | La ficha ya entrega teléfono público del asesor. — [advisor response](../../src/Enlyce.Application/PublicCatalog/PublicCatalogModels.cs#L89-L93) |
| Formulario embebido de ficha | No | Reutilizar `POST /api/leads` con `publicationId` y canal | El endpoint ya admite ambos. — [lead request](../../src/Enlyce.Api/Endpoints/Leads/LeadsModule.cs#L78-L88) |
| Formulario rico de propietario | Sí | DTO/entidad de captación: servicio, tipo, ubicación, ocupación, precio esperado, horario, notas; consentimiento versionado | Lead actual sólo cubre datos básicos, operación, servicio y fuente. — [lead request](../../src/Enlyce.Api/Endpoints/Leads/LeadsModule.cs#L78-L88) |
| Agenda pública de visita | Sí | Disponibilidad, creación idempotente, zona horaria, confirmación/cancelación; protección antiabuso | Las visitas actuales requieren autenticación. — [Visitas](../../src/Enlyce.Api/Endpoints/Visitas/VisitasModule.cs#L35-L97) |
| Favoritos sincronizados/comparación compartida | Sí para sincronizar; no para MVP local | Colección anónima/token o cuenta ligera y endpoint bulk por slugs | Favoritos actuales sólo guardan slug/título localmente. — [favorites](../../website/sitio/src/lib/favorites.ts) |
| Búsquedas guardadas/alertas | Sí | Preferencia de filtros, consentimiento, frecuencia, verificación de correo y unsubscribe | No existe API pública; Alertas es CRM privado. — [Alertas](../../src/Enlyce.Api/Endpoints/Alertas/AlertasModule.cs) |
| Analytics de clics y embudo | No para proveedor externo; sí para analítica propia | Eventos consentidos o integración tag manager; separar UTM en campos si se quiere atribución seria | UTM actual se concatena a `fuente`. — [leads client](../../website/sitio/src/lib/api/leads.ts#L47-L64) |
| Reseñas/testimonios | Sí si serán dinámicos | Moderación, consentimiento, fuente, fecha y estado publicado | No existe contenido o contrato verificable en el sitio. |

### Inferences

- Antes de diseñar un panel “tipo portal”, deben priorizarse `facets`, `publicReference` y captación rica: desbloquean búsqueda confiable y operación comercial real.
- La agenda pública no debe reutilizar ciegamente el endpoint privado; necesita antiabuso, idempotencia y confirmación porque crea compromisos de calendario.
- Separar campaña/medio/fuente en columnas facilita reportes y evita parsear una cadena truncable.

### Gaps

- Falta decidir si ENLYCE será portal de una inmobiliaria o SaaS multiempresa/white-label también en la web pública; esto cambia tenant, branding, dominios y contratos.
- Falta definir proveedor de analítica, correo/SMS/WhatsApp, mapas/geocodificación y política de cookies antes de cerrar arquitectura operativa.
