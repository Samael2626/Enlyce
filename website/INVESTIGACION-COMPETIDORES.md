# Investigacion de competidores

Fecha de verificacion: 2026-09-04.

## Respuesta

Fincaraiz y Metrocuadrado venden velocidad de busqueda y volumen de inventario. L&C no debe imitarlos como portal nacional. Debe ganar en Medellin con inventario verificable, lectura local de barrios, respuesta humana y continuidad entre web publica y CRM.

## Patrones confirmados

### Fincaraiz

- Navegacion principal por proyectos, venta, arriendo y contenido.
- CTA visible para publicar propiedad e ingreso de usuarios.
- Busqueda inicial por tipo de propiedad, ubicacion/palabra clave y codigo.
- La portada mezcla ultimos ingresos, proyectos y accesos SEO por ciudad, barrio, modalidad y tipo.
- La aplicacion promociona filtros, favoritos, alertas de nuevas oportunidades y bajas de precio.
- Las fichas incluyen precio, ubicacion, area, habitaciones y banos.

Fuente primaria: [Fincaraiz](https://www.fincaraiz.com.co/).

### Metrocuadrado

- Busqueda por negocio, inmueble, ubicacion y codigo; ofrece busqueda avanzada.
- El mapa tiene peso propio como forma de explorar zonas.
- Agrega herramientas alrededor de la vivienda: credito, gastos notariales, seguros y cartera hipotecaria.
- Captura solicitudes para distribuirlas a inmobiliarias aliadas.
- Monetiza la publicacion con planes y cupos; solicita autorizaciones de tratamiento y contacto comercial.

Fuentes primarias: [inicio de Metrocuadrado](https://www.metrocuadrado.com/), [publicacion de inmuebles](https://www.metrocuadrado.com/publicar-inmuebles/) y [solicitud de inmueble](https://www.metrocuadrado.com/publicar-inmuebles/solicitud-inmuebles/).

## Lo que L&C debe adoptar

1. Busqueda inmediata: comprar/arrendar, tipo, zona y codigo.
2. Resultados con filtros, orden, vista de mapa y tarjetas comparables.
3. Ficha completa: galeria, precio, administracion, area, alcobas, banos, parqueaderos, mapa, caracteristicas y CTA de visita.
4. Favoritos y alertas de cambios relevantes.
5. Embudo separado para propietarios: vender, arrendar, administrar o valorar.
6. Paginas indexables por zona, modalidad y tipo de inmueble.
7. Formularios con autorizacion de tratamiento de datos y trazabilidad hacia el CRM.

## Lo que L&C no debe copiar

- Portada saturada por inventario nacional irrelevante.
- Herramientas financieras antes de tener buen catalogo y fichas.
- Registro obligatorio para explorar.
- Metricas de confianza inventadas.
- Identidad visual, textos, codigo o estructura exacta de los competidores.

## Diferenciador propuesto

`Marketplace local + asesoria real`.

- Medellin y area metropolitana como territorio experto.
- Propiedades publicadas con estado verificable desde Enlyce.
- Zonas explicadas por estilo de vida, no solo por nombre.
- Agenda de visita conectada al pipeline del CRM.
- Propietario puede entender el estado de su proceso sin perseguir al asesor.

## Estado comprobado en Enlyce

La brecha original de catálogo ya cerró. Enlyce expone `GET /api/public/inmuebles`
con filtros, orden y paginación, además de `GET /api/public/inmuebles/{slug}`.
El contrato incluye fotos, ubicación aproximada, amenidades, administración,
estrato, fecha de publicación y asesor, sin datos del propietario ni dirección
exacta.

Todavía faltan en la web productiva:

- Consumir la API desde resultados y ficha reales.
- Estados de carga, vacío, error, foto ausente y publicación retirada.
- Metadatos SEO, canonical, sitemap y datos estructurados.
- Asociar cada lead o visita con publicación, campaña, canal y consentimiento.
- Código público corto, favoritos, mapa y alertas opcionales.

Evidencia local: `src/Enlyce.Api/Endpoints/PublicCatalog/PublicCatalogModule.cs`,
`src/Enlyce.Application/PublicCatalog/` y
`docs/web/contrato-publicacion-inmueble.md`.

## Confianza

- Patrones funcionales: CONFIRMADO contra los portales reales.
- Direccion de producto L&C: PROPUESTA; requiere validacion con usuarios e inventario real.
- Metricas, precios y contenido de los prototipos: SIMULADOS y marcados como tales.
