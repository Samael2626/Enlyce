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

## Brecha comprobada en Enlyce

El backend actual solo expone `GET /api/inmuebles/{id}` y `POST /api/inmuebles/`. La entidad guarda cantidad de fotos, pero no URLs/archivos; tampoco slug publico, coordenadas, amenidades, administracion, estrato, fecha de publicacion ni indicador explicito de publicacion web.

Antes del marketplace real hacen falta:

- Consulta publica paginada con filtros y orden.
- Modelo de publicaciones separado del estado operativo del inmueble.
- Galeria real y almacenamiento de medios.
- Coordenadas con politica de privacidad sobre direccion exacta.
- Slugs, metadatos SEO y datos estructurados.
- Endpoint de leads/visitas con consentimiento y origen de campana.
- Favoritos persistentes o locales y alertas opcionales.

Evidencia local: `src/Enlyce.Api/Endpoints/Inmuebles/InmueblesModule.cs`, `src/Enlyce.Domain/Entities/Inmueble.cs` y `src/Enlyce.Application/UseCases/GetInmuebleById/GetInmuebleByIdHandler.cs`.

## Confianza

- Patrones funcionales: CONFIRMADO contra los portales reales.
- Direccion de producto L&C: PROPUESTA; requiere validacion con usuarios e inventario real.
- Metricas, precios y contenido de los prototipos: SIMULADOS y marcados como tales.
