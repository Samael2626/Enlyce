---
name: enlyce-uml
description: Genera y mantiene diagramas UML de ENLYCE verificados contra el código, configuraciones EF Core, pruebas y documentación vigente. Usar para casos de uso, actividades, clases, secuencias, componentes y despliegue del proyecto; no usar para ilustraciones decorativas.
---

# ENLYCE UML

Producir diagramas técnicos que describan el sistema real. Código primero; memoria y documentos viejos después.

## Fuentes de verdad

Consultar en este orden:

1. `src/`, `frontend/` y `website/sitio/`.
2. Configuraciones EF Core en `src/Enlyce.Infrastructure/Persistence/Configurations/`.
3. Pruebas en `tests/`.
4. `CLAUDE.md`, `AGENTS.md` y documentos en `docs/`.
5. Vault `D:/Brain/10-Proyectos/Enlyce/` como contexto histórico, nunca como prueba única.

Ejecutar `git status --short` antes de modelar. Si el diagrama incluye cambios sin commit, indicarlo en el documento asociado.

## Formato

- Fuente editable: PlantUML `.puml`.
- Ubicación por defecto: `docs/uml/`; usar la carpeta de la actividad cuando el usuario la indique.
- Nombre de archivo: español sin tildes y con guiones.
- Texto visible: español con tildes.
- Identificadores PlantUML: inglés, sin tildes.
- Exportar SVG para documentación y PNG para Word cuando exista renderizador.
- No entregar una imagen sin conservar su `.puml`.

## Flujo

1. Acotar tipo, audiencia y alcance del diagrama.
2. Buscar actores, rutas, clases, estados y relaciones con `rg`.
3. Separar hechos comprobados de relaciones conceptuales.
4. Crear el `.puml` mínimo que responda la pregunta.
5. Contrastar cada actor, ruta, multiplicidad y regla con su fuente.
6. Validar y renderizar cuando PlantUML esté disponible.

Comandos esperados cuando exista `tools/plantuml/plantuml.jar`:

```powershell
java -jar tools/plantuml/plantuml.jar -checkonly docs/uml/diagrama.puml
java -jar tools/plantuml/plantuml.jar -tsvg docs/uml/diagrama.puml
java -jar tools/plantuml/plantuml.jar -tpng docs/uml/diagrama.puml
```

Si no existe renderizador, entregar el `.puml`, marcar `NO RENDERIZADO` y no fingir validación visual.

## Reglas por diagrama

### Casos de uso

- Actor = rol externo; caso = objetivo del actor.
- Dibujar límite `Sistema ENLYCE`.
- No convertir endpoints, pantallas o tablas en casos de uso.
- No incluir `Autenticar` desde todos los casos: usarlo como precondición, salvo que una relación UML aporte información real.
- En ENLYCE, `Registrar asesor` pertenece al Administrador. No modelar auto-registro.

### Actividad

- Usar carriles cuando intervengan actor, frontend, API y persistencia.
- Mostrar camino feliz, validaciones y finales alternativos.
- Para autenticación: Usuario → CRM web → API → repositorio/BD.
- No esconder respuestas 401, 403, 404 o validaciones relevantes.

### Clases

- Por defecto modelar dominio, no DTO, handler, controlador y repositorio juntos.
- Usar composición para value objects poseídos por una entidad.
- Obtener multiplicidades de navegación EF, claves foráneas y reglas de negocio.
- Si solo existe un campo `...Id` sin relación EF configurada, dibujar asociación punteada y etiquetar `conceptual; FK no configurada`.
- Separar diagrama conceptual, diagrama de implementación y modelo de datos cuando mezclarlos vuelva ilegible el resultado.

### Secuencia

- Usar nombres reales de frontend, endpoint, handler, puerto y adaptador.
- Mostrar llamadas asíncronas y respuestas HTTP importantes.
- No inventar servicios intermedios para que el diagrama parezca más empresarial.

### Componentes y despliegue

- Distinguir `frontend/` (CRM privado React/Vite) de `website/sitio/` (web pública Next.js).
- Ambos consumen `Enlyce.Api`; ninguno accede directamente a PostgreSQL.
- Mostrar `/media` como contenido servido por la API y no como acceso directo al almacenamiento.
- Marcar `website/funcional/` y `website/concept-*` como prototipos o legado, salvo evidencia contraria.

## Control de calidad

- Cada elemento debe apuntar a código, prueba o documento vigente.
- Rutas HTTP deben coincidir carácter por carácter con Minimal APIs.
- Roles y autorización deben coincidir con `RequireAuthorization`, políticas y `EndpointAccess`.
- El diagrama no puede afirmar que una función está terminada si solo existe como placeholder o cambio sin commit.
- Incluir commit corto y fecha del análisis en el documento asociado.
- Cero secretos, credenciales, datos reales de clientes o URLs temporales de túneles.
