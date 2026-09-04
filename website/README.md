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
- `assets/`: imagenes originales generadas para L&C.
- `INVESTIGACION-COMPETIDORES.md`: patrones encontrados y oportunidades.
- `OPCIONES-TECNOLOGICAS.md`: comparacion de stacks y recomendacion.

## Estado

Prototipos HTML/CSS/JS. Busqueda, filtros, favoritos y formularios simulan interaccion; todavia no consumen la API ni guardan datos.

## Direccion recomendada

Usar la estructura del concepto 02, la sobriedad del concepto 01 y el tono del concepto 03. No llevar tres estilos a produccion.
