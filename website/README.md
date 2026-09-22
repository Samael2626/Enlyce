# Web publica de L&C Propiedad Raiz

Exploracion independiente del CRM ubicado en `frontend/`.

## Abrir prototipos

Abrir `index.html` directamente o levantar un servidor local:

```powershell
python -m http.server 4173 --directory website
```

Luego visitar `http://localhost:4173`.

## Contenido

- `concept-01-editorial/`: marca premium y propiedades curadas.
- `concept-02-urban/`: marketplace, filtros y mapa como protagonistas.
- `concept-03-cercano/`: asesoria humana y captacion de propietarios.
- `sitio/`: **la aplicacion Next.js + TypeScript** (bloque 5 del plan). Es la web publica que va a produccion.
- `funcional/`: laboratorio previo en HTML/CSS/JS conectado a Enlyce.Api. Se conserva como referencia y porque todavia es el unico que crea leads.
- `assets/`: imagenes originales generadas para L&C.
- `INVESTIGACION-COMPETIDORES.md`: patrones encontrados y oportunidades.
- `OPCIONES-TECNOLOGICAS.md`: comparacion de stacks y recomendacion.

## Laboratorio funcional

Levantar primero la API en `http://localhost:5019`:

```powershell
dotnet run --project src/Enlyce.Api
```

Luego servir `website/` en el puerto autorizado por CORS:

```powershell
python -m http.server 4173 --directory website
```

Abrir `http://localhost:4173/funcional/`. Para otra API, agregar
`?api=http://host:puerto`; el origen debe estar autorizado por el backend.

Pruebas del laboratorio:

```powershell
node website/funcional.test.mjs
node website/verify.mjs
```

## Sitio Next.js (`sitio/`)

```powershell
cd website/sitio
npm install
npm run dev          # http://localhost:3000
npm run build        # requiere Enlyce.Api viva: prerenderiza inicio y zonas
npm run typecheck
npm run lint
npm run api:types    # regenera src/lib/api/schema.d.ts desde openapi.json
```

Configuracion en `.env.local` (copiar de `.env.example`):

- `NEXT_PUBLIC_API_URL`: origen de Enlyce.Api. Por defecto `http://localhost:5000`.
- `NEXT_PUBLIC_SITE_URL`: origen publico del sitio, usado por metadatos, sitemap y robots.

Para actualizar los tipos tras cambiar el backend: levantar la API, guardar
`http://localhost:5000/swagger/v1/swagger.json` como `sitio/openapi.json` y
correr `npm run api:types`.

## Estado

`sitio/` (Next.js 16 + React 19 + Tailwind 4) cubre inicio, listado con filtros
en la URL, ficha con metadatos y Open Graph por inmueble, zonas indexables,
favoritos en localStorage, propietarios, contacto, privacidad, sitemap y robots.
El formulario que crea leads en el CRM es el bloque 6 y todavia no esta.

Los tres conceptos siguen siendo simulaciones. `funcional/` consume el catalogo
publico, muestra datos sinteticos, guarda favoritos locales y crea leads desde
la ficha y el embudo de propietarios. También incluye páginas estáticas para
Laureles, El Poblado y Belén con inventario vivo. WhatsApp permanece oculto
hasta configurar un numero real.

## Direccion recomendada

Usar la estructura del concepto 02, la sobriedad del concepto 01 y el tono del concepto 03. No llevar tres estilos a produccion.
