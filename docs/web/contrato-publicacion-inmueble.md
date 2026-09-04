# Contrato de publicación de inmuebles

## Propósito

Este contrato define los datos que la web pública de L&C puede consumir. El CRM
continúa siendo la fuente de verdad. La web no recibe datos del propietario,
notas internas, historial comercial ni dirección exacta.

El estado comercial de `Inmueble` y el estado de publicación son independientes:

- Comercial: disponible, reservado, vendido, arrendado o inactivo.
- Publicación: `Draft`, `Published`, `Paused` o `Withdrawn`.

Una propiedad puede estar disponible sin publicarse. Pausar o retirar una
publicación no modifica el estado comercial.

## Ciclo de publicación

| Estado | Significado | Transiciones permitidas |
|---|---|---|
| `Draft` | Preparación privada en el CRM | `Published` |
| `Published` | Visible en el catálogo | `Paused`, `Withdrawn` |
| `Paused` | Oculta temporalmente | `Published`, `Withdrawn` |
| `Withdrawn` | Retirada definitivamente | Ninguna |

Para publicar se exige precio positivo, ubicación pública suficiente y al menos
tres fotografías válidas. La publicación conserva un slug normalizado y único.

## Campos públicos

- Identificador de publicación y slug.
- Título y descripción preparados para la web.
- Tipo, modalidad, precio visible y moneda.
- Municipio, barrio y coordenadas aproximadas.
- Área, habitaciones, baños, parqueaderos, administración, estrato y amenidades.
- Fotografías con URL, texto alternativo, orden y portada.
- Fecha de publicación y asesor responsable con sus datos públicos de contacto.

## Campos prohibidos

- Nombre, documento, teléfono, correo o cualquier dato del propietario.
- Dirección exacta, indicaciones de acceso o coordenadas precisas.
- Notas internas, comisiones, historial de negociación y auditoría del CRM.
- Credenciales, identificadores de sesión o información de otros leads.

## Política de ubicación

La respuesta pública usa municipio, barrio y coordenadas aproximadas. La
dirección exacta se mantiene oculta por defecto y no forma parte de estos DTO.
La estrategia de aproximación y el proveedor de mapas quedan abiertos para una
fase posterior.

## Reglas de los campos

- `slug`: minúsculas ASCII, palabras separadas por guiones y estable después de
  publicarse.
- `publicTitle`: texto obligatorio de máximo 200 caracteres.
- `publicDescription`: texto público de máximo 4.000 caracteres.
- `price.amount`: mayor que cero al publicar; moneda ISO de tres letras.
- `publishedAt`: nulo antes de la primera publicación.
- `advisor`: responsable visible; nunca contiene credenciales ni datos internos.
- `photos`: mínimo tres al publicar, orden único, texto alternativo obligatorio y
  máximo una portada.

Las URLs de medios son conceptuales. El proveedor de almacenamiento, CDN,
variantes y URLs firmadas no están decididos.

## Ejemplo de listado

```json
{
  "page": 1,
  "pageSize": 12,
  "total": 1,
  "items": [
    {
      "id": "f9c7a8b8-5a28-40e5-90d8-31952a5f2921",
      "slug": "apartamento-en-laureles-medellin",
      "publicTitle": "Apartamento iluminado en Laureles",
      "propertyType": "Apartamento",
      "operation": "Venta",
      "price": { "amount": 620000000, "currency": "COP" },
      "location": {
        "municipality": "Medellín",
        "neighborhood": "Laureles",
        "approximateLatitude": 6.2443,
        "approximateLongitude": -75.5934
      },
      "areaSquareMeters": 92,
      "bedrooms": 3,
      "bathrooms": 2,
      "parkingSpaces": 1,
      "coverPhoto": {
        "url": "https://media.example.test/properties/laureles/cover.webp",
        "altText": "Sala iluminada del apartamento"
      },
      "publishedAt": "2026-09-04T18:00:00Z"
    }
  ]
}
```

## Ejemplo de detalle

```json
{
  "id": "f9c7a8b8-5a28-40e5-90d8-31952a5f2921",
  "slug": "apartamento-en-laureles-medellin",
  "publicTitle": "Apartamento iluminado en Laureles",
  "publicDescription": "Vivienda cercana a servicios y vías principales.",
  "propertyType": "Apartamento",
  "operation": "Venta",
  "price": { "amount": 620000000, "currency": "COP" },
  "administrationFee": { "amount": 420000, "currency": "COP" },
  "location": {
    "municipality": "Medellín",
    "neighborhood": "Laureles",
    "approximateLatitude": 6.2443,
    "approximateLongitude": -75.5934
  },
  "features": {
    "areaSquareMeters": 92,
    "bedrooms": 3,
    "bathrooms": 2,
    "parkingSpaces": 1,
    "stratum": 5,
    "amenities": ["Balcón", "Ascensor"]
  },
  "photos": [
    {
      "url": "https://media.example.test/properties/laureles/cover.webp",
      "altText": "Sala iluminada del apartamento",
      "order": 0,
      "isCover": true
    }
  ],
  "advisor": {
    "id": "31b84272-8124-4625-82f8-9c74945ce93d",
    "displayName": "Asesor L&C",
    "publicPhone": "+57 300 000 0000"
  },
  "publishedAt": "2026-09-04T18:00:00Z"
}
```

## Ejemplo de solicitud de visita

```json
{
  "publicationId": "f9c7a8b8-5a28-40e5-90d8-31952a5f2921",
  "name": "Persona interesada",
  "email": "persona@example.test",
  "phone": "+57 300 000 0000",
  "preferredDate": "2026-09-12T15:00:00-05:00",
  "dataProcessingConsent": true,
  "policyVersion": "1.0",
  "source": "web-property-detail"
}
```

## Ejemplo de solicitud de propietario

```json
{
  "name": "Persona propietaria",
  "email": "propietaria@example.test",
  "phone": "+57 300 000 0000",
  "need": "Administración",
  "propertyType": "Apartamento",
  "municipality": "Medellín",
  "neighborhood": "Belén",
  "message": "Deseo conocer el servicio.",
  "dataProcessingConsent": true,
  "policyVersion": "1.0",
  "source": "web-owner-form"
}
```

## Decisiones abiertas

- Proveedor de mapas y algoritmo de aproximación de coordenadas.
- Almacenamiento, CDN y procesamiento de imágenes.
- DTO definitivos, endpoints, filtros, paginación y ordenamiento.
- Datos públicos de contacto del asesor.
- Modelo definitivo de administración, estrato y amenidades.
