---
name: enlyce-academic-docs
description: Diseña y mantiene entregables académicos de ENLYCE en Word y PDF con UML verificado, composición editorial elegante y fuentes versionables. Usar para talleres, informes, guías universitarias y rediseños visuales del proyecto; no usar para documentación técnica interna sin entregable académico.
---

# ENLYCE Academic Docs

Crear documentos que parezcan una publicación técnica cuidada, no un formulario administrativo decorado.

## Fuentes de verdad

1. Verificar hechos en `src/`, `frontend/`, `website/sitio/` y `tests/`.
2. Leer la guía o plantilla académica sin modificar el original.
3. Usar `docs/` y el vault como contexto; no sustituir evidencia de código.
4. Marcar campos personales faltantes como `PENDIENTE`; nunca inventarlos.

## Flujo obligatorio

1. Precisar pregunta, audiencia, formato y criterios de evaluación.
2. Separar funciones implementadas, parciales y planificadas.
3. Modelar cada UML con un solo propósito. Mantener el `.puml` y exportar PNG y SVG.
4. Reducir cruces: agrupar por contexto, ordenar por flujo y omitir relaciones secundarias solo si se explica la omisión.
5. Componer el Word siguiendo `references/editorial-style.md`.
6. Entregar fuente Markdown, PlantUML, imágenes, DOCX y PDF cuando aplique.
7. Validar PlantUML, OpenXML y render final. Inspeccionar todas las páginas; corregir cortes, páginas vacías, texto huérfano o diagramas ilegibles.

## Reglas UML

- Casos de uso: agrupar por capacidad; actor significa rol externo. No usar herencia de actores si crea cableado innecesario.
- Clases: preferir vista por agregados. Mostrar entidades y atributos decisivos; no convertir cada value object en caja si basta su tipo.
- Actividad y secuencia: conservar caminos alternos y respuestas relevantes.
- Una línea que no cambia la comprensión sobra. Una línea omitida que cambia el significado debe volver.
- Distinguir relación configurada en EF Core de asociación conceptual.

## Reglas del documento

- Times New Roman en todo el entregable, incluidos los diagramas.
- Usar una jerarquía tipográfica fuerte y pocos colores; no llenar cada bloque con bordes.
- Variar la composición entre portada, tablas, texto y páginas de figura.
- Dar a los UML espacio horizontal suficiente y pies de figura explicativos.
- Mantener tablas con anchos DXA fijos y filas indivisibles.
- No usar tarjetas repetitivas, iconos circulares con letra, gradientes decorativos ni azul corporativo genérico.

## Verificación mínima

```powershell
java -jar tools/plantuml/plantuml.jar -checkonly docs/actividad-2/uml/*.puml
java -jar tools/plantuml/plantuml.jar -tpng docs/actividad-2/uml/*.puml
java -jar tools/plantuml/plantuml.jar -tsvg docs/actividad-2/uml/*.puml
```

Abrir o renderizar el DOCX a PDF y revisar cada página como imagen antes de declarar terminado.
