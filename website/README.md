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
- `funcional/`: listado y ficha en HTML/CSS/JS conectados a Enlyce.Api.
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

## Estado

Los tres conceptos siguen siendo simulaciones. `funcional/` consume el catalogo
publico y crea leads desde la ficha; no incluye datos sinteticos ni WhatsApp
hasta configurar un numero real.

## Direccion recomendada

Usar la estructura del concepto 02, la sobriedad del concepto 01 y el tono del concepto 03. No llevar tres estilos a produccion.
