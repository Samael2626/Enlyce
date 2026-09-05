# Ingenieria inversa web LYC

Fecha de observación: 2026-09-05.

## Veredicto

L&C no debe construir un Fincaraíz pequeño. Debe tomar su recorrido eficiente de búsqueda y la ficha rica de ambos portales, reducir el ruido comercial y cerrar cada inmueble con una persona real. La ventaja defendible no es volumen: es inventario publicado desde Enlyce, lectura local de Medellín y seguimiento después del contacto.

El laboratorio ya cubre resultados, ficha y captación de propietarios. La siguiente pieza es retorno sin fricción: favoritos locales sin cuenta. Mapa, alertas y páginas editoriales llegan después. Herramientas financieras y publicación autoservicio se rechazan por ahora.

## Consejo de producto

- **Adversario:** intentar igualar el volumen y las herramientas de los portales nacionales enterraría a L&C en trabajo sin ventaja.
- **Arquitecto:** portada, resultados y ficha forman el núcleo; todo lo demás depende de ese recorrido y del mismo contrato público.
- **Creativo:** transformar “buscar por barrio” en “cómo quieres vivir Medellín” aporta identidad sin esconder los filtros concretos.
- **Científico:** los patrones están confirmados; su efecto en conversión no. Medir antes de declarar ganador al mapa, WhatsApp o cualquier concepto visual.
- **Filósofo:** la web no vende páginas; convierte inventario confiable en conversaciones y visitas.
- **Pragmático:** conectar la API existente produce valor antes que favoritos, alertas, calculadoras o un portal para propietarios.
- **Humanista:** datos claros, ubicación prudente y asesor visible reducen ansiedad; el contacto no debe exigir una cuenta.

### Veredicto del Rey

**Posición:** construir primero portada, resultados y ficha conectados al catálogo público. Mapa, favoritos y SEO local siguen después. No abrir finanzas ni publicación autoservicio.

**Confianza:** alta sobre la estructura; media sobre prioridad del mapa y estilo visual hasta probar con usuarios reales.

**Disenso que importa:** el mapa diferencia la experiencia, pero meterlo en P0 añade proveedor, privacidad, interacción móvil y coste antes de validar el embudo básico.

## Alcance y limites

Se revisaron las superficies públicas accesibles de Fincaraíz, Metrocuadrado y Ciencuadras: portada, resultados de apartamentos en venta en Medellín y fichas de inmueble. Se contrastaron contra `website/`, el contrato público y los endpoints actuales de Enlyce.

Límites:

- No se tuvo acceso a analítica, experimentos, lógica de ranking ni tasas de conversión.
- La extracción de Metrocuadrado no expuso el detalle completo de cada filtro.
- No se validó el comportamiento visual en dispositivos reales.
- La tecnología se identificó solo por huellas públicas del HTML; arquitectura interna, popularidad y eficacia comercial quedan `NO COMPROBADO`.

## Revalidación técnica del código público

Observación directa del HTML descargado el 2026-09-05:

- Fincaraíz expone `__NEXT_DATA__`, rutas `/_next/`, chunks de Webpack y JSON-LD: huellas compatibles con Next.js. `CONFIRMADO`.
- Metrocuadrado entrega chunks `/_next/static/` y `main-app`: huellas compatibles con Next.js App Router. `CONFIRMADO` para el frontend público; versión y arquitectura interna `NO COMPROBADO`.
- Ciencuadras expone `ng-version` y bundles `runtime`, `polyfills` y `main`: huellas compatibles con Angular. `CONFIRMADO`.
- Fincaraíz y Ciencuadras incluyen JSON-LD en el HTML observado. Metrocuadrado entrega título, precio y atributos de la ficha en HTML rastreable, aunque no se confirmó JSON-LD en la muestra.

Decisión: no escoger framework por imitación. Tres competidores resuelven recorridos parecidos con tecnologías distintas. Mantener HTML/CSS/JS mientras se estabilizan rutas, estados y eventos; la selección posterior debe medir SEO indexable, rendimiento, mantenimiento y ajuste con Enlyce.Api.

Para favoritos se verificó Web Storage con MDN vía Context7: `localStorage` persiste por origen entre sesiones, pero puede lanzar `SecurityError` o estar deshabilitado. L&C guarda solo slugs, limita la selección a 24 y mantiene un fallback temporal en memoria.

## Matriz de superficies

| Superficie | Fincaraíz | Metrocuadrado | Decisión L&C |
|---|---|---|---|
| Portada | Navegación por venta, arriendo, proyectos y contenido; búsqueda por tipo, ubicación o código; inventario reciente y enlaces por zona. `CONFIRMADO` | Búsqueda por negocio y código; mapa y servicios financieros como accesos principales. `CONFIRMADO` | `ADAPTAR`: búsqueda inmediata por modalidad, tipo y zona. Código como acceso secundario. Sin proyectos ni finanzas en V1. |
| Resultados | Conteo, orden, tarjetas con precio y atributos; contenido local dentro de la ruta de Medellín. `CONFIRMADO` | Filtros, mapa, guardar búsqueda y enlaces por barrio/tipo/modalidad. `CONFIRMADO` | `ADOPTAR`: filtros comparables y estado persistido en URL. `ADAPTAR`: mapa después del listado funcional. |
| Ficha | Galería/mapa, precio, administración, datos extensos, preguntas sobre campos ausentes, anunciante y formulario antes de revelar contacto. `CONFIRMADO` | Precio, administración, áreas, estrato, descripción, características, mapa, anunciante, código, reporte y contacto por WhatsApp. `CONFIRMADO` | `ADAPTAR`: ficha clara, ubicación aproximada, asesor visible y CTA directo. No bloquear el contacto con registro. |
| Retorno | Favoritos, alertas y avisos de reducción de precio promocionados desde la aplicación. `CONFIRMADO` | Guardar búsqueda visible en resultados. `CONFIRMADO` | `ADAPTAR`: favoritos locales primero; alertas solo cuando exista consentimiento y entrega real. |
| SEO local | Enlaces por ciudad, barrio, tipo, modalidad y cantidad de habitaciones; texto contextual en resultados y ficha. `CONFIRMADO` | Rutas y enlaces internos por ciudad, barrio, tipo y modalidad. `CONFIRMADO` | `ADOPTAR`: páginas indexables para combinaciones con inventario real. Evitar páginas vacías generadas en masa. |
| Captación | Formulario con nombre, teléfono, email y aceptación legal en ficha. `CONFIRMADO` | CTA de contacto/WhatsApp y advertencia sobre la conversación con el anunciante. `CONFIRMADO` | `ADAPTAR`: formulario corto + WhatsApp con inmueble y fuente trazables hacia Enlyce. |

## Recorridos reconstruidos

### Comprador que explora

Competidores:

`portada -> modalidad/tipo/zona -> resultados -> filtros/orden/mapa -> ficha -> contacto`

L&C:

`portada local -> modalidad/tipo/zona -> resultados comparables -> ficha verificada -> agendar visita o WhatsApp`

Decisión: mostrar inventario antes que discurso corporativo. La promesa de acompañamiento aparece cerca del CTA, no reemplaza los datos del inmueble.

### Comprador que llega desde Google

Competidores:

`resultado orgánico por zona -> listado o ficha -> enlaces relacionados -> contacto`

L&C:

`página indexable con inventario real -> ficha -> zona aproximada + asesor -> contacto`

Decisión: cada ficha debe entregar HTML indexable, título y descripción únicos, URL canónica y datos estructurados. El texto local debe ayudar a decidir; no escribir párrafos de relleno para Google.

### Comprador con código

Competidores:

`entrada por código -> ficha exacta -> contacto`

L&C:

`código LC visible -> búsqueda directa -> ficha publicada o respuesta neutra`

Decisión: conservar código humano corto, distinto del `Guid`. Si el inmueble no está publicado, responder como inexistente para no filtrar estado interno.

### Interesado en una ficha

Competidores:

`datos y confianza -> CTA -> formulario o WhatsApp -> anunciante`

L&C:

`galería + precio + atributos -> contexto del barrio -> asesor L&C -> visita/WhatsApp -> lead con publicación y fuente`

Decisión: el contacto debe conservar `publicationId`, `slug`, URL, campaña, modalidad, consentimiento y canal. Hoy `CreateLead` no guarda una relación explícita con la publicación.

## Arquitectura de informacion propuesta

### Rutas P0

| Ruta lógica | Propósito |
|---|---|
| `/` | Búsqueda principal, inventario reciente, zonas y propuesta de confianza. |
| `/inmuebles` | Resultados con filtros, orden y paginación reflejados en la URL. |
| `/inmuebles/{slug}` | Ficha indexable, galería, ubicación aproximada, asesor y contacto. |
| `/privacidad` | Tratamiento de datos y enlace desde todo formulario. |

### Rutas P1

| Ruta lógica | Propósito |
|---|---|
| `/venta/{tipo}/{municipio}/{barrio}` | Entrada orgánica solo cuando haya inventario suficiente. |
| `/arriendo/{tipo}/{municipio}/{barrio}` | Equivalente para arriendo. |
| `/zonas/{barrio}` | Guía local útil con propiedades relacionadas. |
| `/favoritos` | Selección local sin cuenta obligatoria. |

No crear miles de combinaciones indexables vacías. Facetas sin valor deben usar URL de consulta o canonical hacia una página consolidada.

## Contrato funcional por superficie

### Portada

- Entrada: modalidad, tipo y zona; código en acción secundaria.
- Resultado: navega a `/inmuebles` con parámetros legibles.
- Móvil: formulario por pasos cortos, CTA completo visible sin desplazamiento horizontal.
- Estados: carga de zonas, sin coincidencias y error recuperable.
- Analítica: `search_started`, `search_submitted`, parámetros no sensibles.
- Aceptación: teclado, lector de pantalla y navegación táctil pueden ejecutar la búsqueda; recargar conserva la URL de resultados.

### Resultados

- Datos de tarjeta: portada, modalidad, tipo, barrio/municipio, precio, área, habitaciones, baños, parqueaderos y código público.
- Filtros P0: los ya soportados por la API; controles mínimos significan “o más”.
- Orden P0: recientes, precio menor y precio mayor.
- Estados: esqueletos, vacío con opción de limpiar filtros, error con reintento y foto ausente.
- Móvil: filtros en panel; conteo y orden siempre accesibles; tarjeta completa abre la ficha.
- Analítica: `results_viewed`, `filter_applied`, `sort_changed`, `property_opened`.
- Aceptación: atrás/adelante restaura filtros y página; ningún borrador, pausado o retirado aparece.

### Ficha

- Datos: contrato público actual, sin propietario ni dirección exacta.
- Jerarquía: galería, título/localización, precio, atributos, descripción, amenidades, ubicación aproximada, asesor y CTA.
- CTA: “Agendar visita” primario; WhatsApp secundario con mensaje contextual.
- Estados: inmueble retirado usa 404 neutro; imágenes fallidas conservan texto alternativo; contacto fallido no pierde datos digitados.
- Móvil: barra inferior fija con contacto, sin tapar contenido ni controles.
- Analítica: `property_viewed`, `gallery_opened`, `contact_started`, `lead_submitted`, `whatsapp_clicked`.
- Aceptación: un lead exitoso queda asociado a la publicación y registra consentimiento y fuente.

## Mapa de brechas de Enlyce

| Capacidad | Estado actual | Capa responsable | Siguiente cambio |
|---|---|---|---|
| Listado público con filtros, orden y paginación | Soportada | API/Application/Web | Conectado en `website/funcional/`; falta URL productiva. |
| Ficha pública por slug | Soportada | API/Application/Web | Conectada por slug; falta renderizado inicial indexable. |
| Ubicación aproximada | Soportada | Contrato/API | Elegir proveedor y política visual del mapa. |
| Estados carga/vacío/error | Soportada | Web | Mantener cobertura al agregar superficies. |
| Código humano corto | Ausente | Dominio/API/Web | Definir formato estable e índice; no reutilizar `Guid`. |
| Lead ligado a publicación | Parcial | Domain/Application/API | Añadir referencia pública, canal, URL/campaña y versión del consentimiento. |
| Formulario real | Soportada | Web/API | Catálogo y propietarios crean leads; falta idempotencia y relación explícita con publicación. |
| Favoritos sin cuenta | Soportada | Web | Slugs locales, sincronización entre pestañas y retirados visibles; falta prueba con usuarios. |
| Alertas guardadas | Ausente | Backend/Web | Posponer hasta tener envío, baja y consentimiento verificables. |
| SEO por ficha | Parcial | Web | Renderizado indexable, canonical, metadata social y JSON-LD. |
| SEO por zona/faceta | Ausente | Web/API | Crear solo páginas con inventario y contenido útil. |
| Mapa de resultados | Parcial | API/Web | Coordenadas existen; faltan mapa, clustering y búsqueda por área visible. |

## Backlog por dependencia

### P0 - buscar, evaluar y contactar

1. **Listado conectado a la API — HECHO.** Filtros, orden y página sobreviven en la URL.
2. **Ficha real por slug — PARCIAL.** Datos y 404 funcionan; el HTML inicial todavía no contiene el inmueble para indexación.
3. **Estados operativos — HECHO.** Catálogo, ficha y favoritos cubren carga, vacío, error y publicaciones retiradas.
4. **Cerrar captación contextual.** Aceptación: visita o WhatsApp incluye publicación, fuente, campaña, canal y consentimiento; error no duplica ni borra el formulario.
5. **Aplicar SEO técnico mínimo.** Aceptación: title, description, canonical, Open Graph, sitemap y schema válido para fichas publicadas. `RealEstateListing` existe en Schema.org, pero sigue marcado como tipo nuevo y Google no ofrece un resultado enriquecido inmobiliario general; no prometerlo como ventaja visual en búsqueda.
6. **Medir el embudo.** Aceptación: búsqueda, apertura de ficha e inicio/éxito de contacto emiten eventos sin datos personales.

### P1 - comparación, confianza y retorno

1. Código humano y búsqueda directa.
2. Favoritos locales sin registro — HECHO en laboratorio; pendiente validación visual y con usuarios.
3. Mapa de resultados con sincronización tarjeta/marcador.
4. Páginas de zonas de Medellín con inventario real.
5. Propiedades relacionadas basadas en modalidad, barrio y rango de precio.

### P2 - después de validar tráfico

1. Alertas de búsqueda y bajas de precio con alta/baja verificables.
2. Comparador de inmuebles.
3. Captación autoservicio de propietarios.
4. Calculadoras o alianzas financieras.

## Adoptar, adaptar y rechazar

### Adoptar

- Búsqueda visible desde la portada.
- Resultados comparables con filtros y orden.
- Galería, precio, administración y atributos claros.
- Enlaces internos por zona y tipo cuando existe inventario.
- Contacto contextual desde la ficha.

### Adaptar

- Mapa: ubicación aproximada, no dirección exacta.
- Favoritos: locales y sin cuenta en la primera versión.
- Contenido local: decisiones de vida en Medellín, no texto SEO genérico.
- Confianza: estado verificable y asesor responsable, no etiquetas promocionales inventadas.
- WhatsApp: complemento rastreable, no único camino.

### Rechazar por ahora

- Registro obligatorio para explorar.
- Resultados patrocinados o planes de publicación.
- Simuladores de crédito, seguros y compra de cartera.
- Publicación abierta de terceros.
- Métricas de confianza sin datos reales.
- Copia de lenguaje, identidad visual o composición exacta de competidores.

## Evidencia

### Confirmado

- Fincaraíz: [portada](https://www.fincaraiz.com.co/), [resultados en Medellín](https://www.fincaraiz.com.co/venta/apartamentos/medellin/antioquia) y [ficha observada](https://www.fincaraiz.com.co/apartamento-en-venta-en-castropol-medellin/193379731).
- Metrocuadrado: [portada](https://www.metrocuadrado.com/), [resultados en Medellín](https://www.metrocuadrado.com/apartamentos/venta/medellin/colombia/) y [ficha observada](https://www.metrocuadrado.com/inmueble/venta-apartamento-medellin-san-diego-3-habitaciones-2-banos-1-garajes/17166-M6462716).
- Ciencuadras: [resultados en Medellín](https://www.ciencuadras.com/venta/venta/medellin/apartamento), [ficha observada](https://www.ciencuadras.com/inmueble/apartamento-en-venta-en-alejandria-medellin-3362949%26q) y [guía de La Candelaria](https://www.ciencuadras.com/blog/guia-de-barrio-la-candelaria-medellin).
- Persistencia local: [MDN localStorage](https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage), consultado mediante Context7 `/mdn/content`.
- SEO estructurado: [RealEstateListing en Schema.org](https://schema.org/RealEstateListing) y [galería de datos estructurados compatibles con Google](https://developers.google.com/search/docs/appearance/structured-data/search-gallery).
- L&C: `website/`, `docs/web/contrato-publicacion-inmueble.md` y `src/Enlyce.Api/Endpoints/PublicCatalog/PublicCatalogModule.cs`.

### Inferido

- Reducir ruido y priorizar contacto humano encaja mejor con una inmobiliaria local que competir por volumen nacional.
- Favoritos locales permiten probar retorno antes de asumir cuentas y autenticación.
- Las páginas por zona pueden captar búsqueda orgánica, pero solo serán útiles con inventario y contenido suficientes.

### No comprobado

- Qué concepto visual convierte mejor.
- Qué filtros usa más el comprador de L&C.
- Si mapa, WhatsApp o formulario produce más visitas calificadas.
- Cuánto inventario mínimo justifica una página indexable de barrio.

Cerrar estos huecos con cinco entrevistas de búsqueda reales y una prueba moderada de los recorridos, no con opiniones internas.
