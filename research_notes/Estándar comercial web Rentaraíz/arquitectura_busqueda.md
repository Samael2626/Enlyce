# Arquitectura de información, navegación y búsqueda de Rentaraíz

## ¿En qué rutas y contextos aparece el buscador?

### Takeaway
Rentaraíz trata la búsqueda como una capacidad transversal, no como una función exclusiva del inicio. La barra completa está confirmada en siete contextos; el catálogo usa una interfaz de filtros propia. Para L&C conviene mantenerla completa sólo donde el usuario conserva intención de descubrir inmuebles y usar una versión compacta en el resto.

### Cited Findings
- **CONFIRMADO:** el componente reutilizable `app-barra-filtros` se monta en inicio (`app-vista-inicial`), contacto, equipo, quiénes somos, detalle de blog, política de privacidad y avalúos comerciales. La evidencia está en el bundle cliente servido por el sitio, donde cada componente incluye esa barra. — [Bundle oficial de Rentaraíz](https://rentaraiz.co/chunk-POCF6KRL.js)
- **CONFIRMADO:** la página de blogs no usa el mismo componente, pero implementa su propia zona de búsqueda inmobiliaria dentro del template. — [Bundle oficial de Rentaraíz](https://rentaraiz.co/chunk-POCF6KRL.js)
- **CONFIRMADO:** `/filtros` no repite la barra promocional: emplea un buscador de ubicación, botón para abrir filtros, contador de resultados, ordenamiento, grilla y paginación. — [Página de resultados](https://rentaraiz.co/filtros); [bundle oficial](https://rentaraiz.co/chunk-POCF6KRL.js)
- **CONFIRMADO:** el detalle de propiedad no monta la barra principal; prioriza galería, precio, atributos, descripción, comodidades, asesor, contacto y, cuando hay datos, inmuebles similares. — [Detalle de propiedad](https://rentaraiz.co/ver-propiedad/7138/0)
- **CONFIRMADO:** el formulario para publicar un inmueble tampoco monta la barra, lo que evita competir con la intención de captación del propietario. — [Publicar inmueble](https://rentaraiz.co/publicar-inmueble); [bundle oficial](https://rentaraiz.co/chunk-POCF6KRL.js)
- **CONFIRMADO:** el sitemap declara inicio, resultados, contacto, equipo, quiénes somos, mapa, blogs, publicar inmueble, políticas, avalúos y numerosos detalles de propiedades como rutas públicas. — [Sitemap oficial](https://rentaraiz.co/sitemap.xml)

### Inferences
- En L&C la barra completa debería permanecer en **inicio** y **contacto**, y añadirse en **quiénes somos**, **servicios/avalúos** y **contenido editorial** sólo si no desplaza la tarea principal.
- En **catálogo/listado** debe existir como filtro persistente o compacto, no como bloque hero repetido.
- En **detalle del inmueble** conviene un módulo compacto de “seguir buscando” o enlace para modificar filtros, ubicado después de los datos clave o al final; la ficha debe conservar prioridad comercial.
- En **favoritos** conviene una búsqueda compacta para recuperar descubrimiento cuando la lista está vacía o corta.
- En **publicar inmueble**, formularios de contacto de alta intención y páginas legales no conviene insertar la barra completa: una llamada secundaria al catálogo es suficiente.

### Gaps
- **NO COMPROBADO:** no se midieron analítica, conversión ni uso real de la barra en cada página; su repetición confirma una decisión de diseño, no que todas las ubicaciones conviertan mejor.
- **NO COMPROBADO:** no se verificó mediante prueba A/B si la barra completa en páginas legales o corporativas ayuda o distrae.

## ¿Qué controles ofrece y cómo conduce a resultados?

### Takeaway
La barra resuelve dos trabajos: exploración por ubicación/filtros y acceso directo por código. La exploración desemboca en `/filtros`; el código lleva a la ficha concreta. El conjunto es comercialmente completo, aunque su transferencia de estado a resultados es técnicamente débil.

### Cited Findings
- **CONFIRMADO:** el usuario elige entre `Ubicación` y `Código`; ubicación es el modo inicial. — [Bundle oficial de Rentaraíz](https://rentaraiz.co/chunk-POCF6KRL.js)
- **CONFIRMADO:** el modo ubicación ofrece autocompletado de ciudades y barrios, tipo de negocio —incluido arriendo/venta—, tipo de inmueble, rangos de precio y “Más filtros”. — [Bundle oficial de Rentaraíz](https://rentaraiz.co/chunk-POCF6KRL.js)
- **CONFIRMADO:** “Más filtros” contiene habitaciones, baños, parqueaderos, estratos y área mínima/máxima; dormitorios, baños y parqueaderos permiten valores de 1 a 5 y `+6`, mientras el estrato va de 1 a 6. — [Bundle oficial de Rentaraíz](https://rentaraiz.co/chunk-POCF6KRL.js)
- **CONFIRMADO:** el botón Buscar consulta el API, ordena inicialmente por fecha de consignación descendente y navega a `/filtros` con resultados, paginación y filtros en el estado del router; además persiste ese estado en almacenamiento local. — [Bundle oficial de Rentaraíz](https://rentaraiz.co/chunk-POCF6KRL.js)
- **CONFIRMADO:** la búsqueda por código consulta la propiedad y, si existe, navega a `/ver-propiedad/{codigo}/0`; ante código inválido o inexistente muestra error. — [Bundle oficial de Rentaraíz](https://rentaraiz.co/chunk-POCF6KRL.js)
- **CONFIRMADO:** la pantalla de resultados permite reabrir filtros, buscar ubicación, borrar filtros, ordenar por precio o antigüedad, ver tarjetas con favoritos y paginar. — [Página de resultados](https://rentaraiz.co/filtros); [bundle oficial](https://rentaraiz.co/chunk-POCF6KRL.js)
- **CONFIRMADO:** las tarjetas comunican tipo, código, barrio, descripción, habitaciones, baños, área, precio y nivel comercial; desde ellas se abre la ficha. — [Bundle oficial de Rentaraíz](https://rentaraiz.co/chunk-POCF6KRL.js)

### Inferences
- L&C debería incorporar búsqueda por código cuando el backend exponga un código público estable; sirve a clientes que llegan desde portales, redes o un asesor.
- El panel compacto debe conservar cinco entradas visibles: ubicación, operación, tipo, precio y botón; habitaciones, baños, parqueaderos, estrato y área pueden vivir en un panel avanzado.
- La ruta de resultados de L&C debería serializar filtros en query string o segmentos legibles, por ejemplo `/inmuebles?operacion=arriendo&ciudad=medellin&barrio=poblado`, para soportar volver atrás, compartir, indexar y medir campañas.
- Los filtros deben sobrevivir al regreso desde una ficha y reflejarse en chips removibles; almacenamiento local puede complementar la URL, nunca sustituirla.

### Gaps
- **NO COMPROBADO:** no se pudo completar una búsqueda interactiva con instrumentación de red para medir latencia, estados vacíos y todos los errores del API.
- **NO COMPROBADO:** no se verificó si las búsquedas por ubicación toleran sinónimos, errores ortográficos o múltiples ubicaciones simultáneas.

## ¿Qué jerarquía de navegación, SEO local y contenido repetible usa?

### Takeaway
La jerarquía comercial cubre descubrir, confiar, captar propietarios y contactar. El SEO visible se apoya sobre todo en fichas individuales con ciudad, barrio, operación y atributos; no se encontraron landing pages indexables por zona, una oportunidad clara para L&C.

### Cited Findings
- **CONFIRMADO:** las rutas públicas declaradas incluyen inicio, resultados, contacto, equipo, quiénes somos, mapa, blog/detalle, publicación de inmueble, privacidad, acoso sexual, avalúos, portafolio de asesor y propiedades prioritarias. — [Bundle oficial de Rentaraíz](https://rentaraiz.co/chunk-POCF6KRL.js)
- **CONFIRMADO:** la navegación principal agrupa `Venta`, `Arriendo`, `Nosotros`, `Clientes`, `Publica tu propiedad` y `Contáctanos`; `Nosotros` deriva a quiénes somos, expertos y brochure, mientras `Clientes` deriva a propietarios y arrendatarios. — [Sitio oficial](https://rentaraiz.co/); [bundle oficial](https://rentaraiz.co/chunk-POCF6KRL.js)
- **CONFIRMADO:** las fichas pueden mostrar breadcrumbs del tipo operación → tipo de inmueble → ciudad → sector, por ejemplo Arriendo → Apartamento → Medellín → El Poblado. — [Ficha oficial](https://rentaraiz.co/ver-propiedad/7127/0)
- **CONFIRMADO:** cada ficha posee URL estable basada en código, precio, atributos clave, código visible, descripción extensa con referencias locales, comodidades, CTA de teléfono/WhatsApp, asesor responsable y CTA institucional de contacto/publicación. — [Ficha oficial](https://rentaraiz.co/ver-propiedad/7016/0)
- **CONFIRMADO:** algunas fichas añaden inmuebles similares, prolongando el recorrido de descubrimiento. — [Ficha oficial](https://rentaraiz.co/ver-propiedad/7138/0)
- **CONFIRMADO:** contacto combina formulario, teléfonos, correo, dirección, mapa y dos CTA transversales: contactar y publicar inmueble. — [Contacto oficial](https://rentaraiz.co/contacto/)
- **CONFIRMADO:** `robots.txt` no bloquea rastreo y anuncia el sitemap; el sitemap incluye páginas corporativas y fichas, pero no muestra rutas específicas indexables por ciudad, barrio, operación o tipo. — [Robots oficial](https://rentaraiz.co/robots.txt); [sitemap oficial](https://rentaraiz.co/sitemap.xml)
- **CONFIRMADO:** el sitemap asigna la misma prioridad `0.8`, frecuencia `daily` y fecha de modificación a todas las URLs observadas, incluso páginas corporativas y fichas. — [Sitemap oficial](https://rentaraiz.co/sitemap.xml)

### Inferences
- El estándar comercial que L&C debe adoptar es una arquitectura de cuatro caminos visibles: **buscar inmueble**, **vender/arrendar mi inmueble**, **conocer la empresa/servicios** y **contactar/recibir asesoría**.
- Debe existir un patrón transversal de confianza: datos legales y de contacto, asesor identificable, WhatsApp, formulario breve y CTA para propietarios.
- L&C puede superar el SEO local de referencia creando landing pages canónicas con contenido útil y catálogo filtrado para combinaciones valiosas: operación + tipo + municipio/barrio.
- Las fichas deben incorporar breadcrumbs enlazables, metadatos únicos, datos estructurados compatibles, ubicación semántica y propiedades similares basadas en criterios claros.
- El blog debe enlazar a búsquedas y fichas relacionadas; las fichas deberían enlazar a guías de zona, financiación, proceso de compra/arriendo y captación.

### Gaps
- **NO COMPROBADO:** no se verificaron `schema.org`, canonical, Open Graph, títulos y descripciones individuales mediante una auditoría SEO de cada ruta.
- **NO COMPROBADO:** no se confirmó que todos los breadcrumbs sean enlaces rastreables; el contenido indexado los presenta como botones o texto.
- **PROBABLE:** la estrategia editorial busca capturar intención local y de inversión, porque las descripciones repiten ciudad, sector, cercanías y valorización; sin Search Console no puede afirmarse su impacto orgánico.

## ¿Qué conviene adoptar y qué no conviene copiar?

### Takeaway
Debe copiarse la lógica comercial, no la interfaz ni la implementación. Rentaraíz destaca por disponibilidad de búsqueda, profundidad de ficha, contacto inmediato y rutas para propietarios; L&C debe mejorar URLs, SEO programático, consistencia semántica y disciplina visual.

### Cited Findings
- **CONFIRMADO:** Rentaraíz combina catálogo, favoritos, publicación de inmueble, contacto, equipo, servicios, contenido y fichas con asesor en una sola experiencia pública. — [Sitio oficial](https://rentaraiz.co/); [sitemap oficial](https://rentaraiz.co/sitemap.xml)
- **CONFIRMADO:** la barra reutilizable evita que varias páginas corporativas se conviertan en callejones sin salida para quien aún busca propiedad. — [Bundle oficial de Rentaraíz](https://rentaraiz.co/chunk-POCF6KRL.js)
- **CONFIRMADO:** la implementación actual navega a `/filtros` mediante estado del router y almacenamiento local, sin plasmar la selección en una URL descriptiva. — [Bundle oficial de Rentaraíz](https://rentaraiz.co/chunk-POCF6KRL.js)
- **CONFIRMADO:** el sitemap no contiene landings de ciudad/barrio/tipo y utiliza metadatos de sitemap uniformes para rutas muy distintas. — [Sitemap oficial](https://rentaraiz.co/sitemap.xml)
- **CONFIRMADO:** varias fichas repiten bloques de precio y atributos, y algunas descripciones tienen inconsistencias entre área narrada y área mostrada; esto exige fuente de datos única y validación en L&C. — [Ficha 6799](https://rentaraiz.co/ver-propiedad/6799/0)

### Inferences
- **ADOPTAR:** buscador reusable; acceso por código; filtros progresivos; catálogo ordenable; favoritos; fichas profundas; asesor responsable; WhatsApp/teléfono/formulario; similares; captura de propietarios; mapa; equipo; avalúos; blog y políticas visibles.
- **ADOPTAR CON CAMBIOS:** repetir búsqueda en inicio, contacto, contenido, servicios y páginas corporativas, pero con variantes `hero`, `compacta` y `resultados` según intención y espacio.
- **NO COPIAR:** URLs de resultados sin filtros serializados, sitemap plano, búsqueda completa en páginas legales, texto de propiedades excesivamente repetitivo, duplicación visual de atributos ni dependencia de imágenes con texto como encabezados.
- **NO COPIAR:** marca, textos, fotografías, paleta, estructura exacta, código ni nombres comerciales. La referencia sirve para requisitos y flujos, no para clonación.
- Prioridad sugerida para L&C: **P0** catálogo/filtros compartibles + ficha completa + contacto; **P1** código público, favoritos persistentes, publicación de inmueble y SEO local; **P2** mapas, similares, portafolio de asesor, blog enlazado y avalúos.

### Gaps
- **NO COMPROBADO:** faltan métricas comparativas de velocidad, accesibilidad, Core Web Vitals, tasa de leads y tasa de búsqueda→contacto.
- **NO COMPROBADO:** no se auditó el proceso completo de publicación de inmueble ni los portales externos de propietarios/arrendatarios.
- **NO COMPROBADO:** no se determinó qué funciones requieren autenticación o sincronización con CRM más allá de lo expuesto públicamente.
