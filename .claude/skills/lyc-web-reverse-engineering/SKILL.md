---
name: lyc-web-reverse-engineering
description: Analiza sitios inmobiliarios competidores para decidir la web publica de L&C Propiedad Raiz. Usar ante pedidos de ingenieria inversa, comparacion de Fincaraiz o Metrocuadrado, arquitectura de informacion, buscador, listado, ficha, captacion o SEO de website/. No usar para el CRM privado ni para copiar identidad, textos, codigo o activos ajenos.
---

# Ingenieria inversa web LYC

Convierte evidencia observable de portales inmobiliarios en decisiones verificables para la web pública de L&C. El resultado debe servir para diseñar o implementar; un inventario de pantallas sin decisiones no basta.

## Contexto fijo

- Producto: marketplace público de L&C Propiedad Raíz.
- Territorio inicial: Medellín y área metropolitana.
- Aplicación objetivo: `website/`; `frontend/` es el CRM privado y queda fuera.
- Competidores base: Fincaraíz y Metrocuadrado. Agregar otros solo si aportan un patrón ausente.
- Diferenciador: inventario verificable, conocimiento local y contacto humano trazable hacia Enlyce.
- Reutilizar patrones funcionales; nunca copiar marca, composición exacta, textos, código, fotografías ni iconos.

## Antes de investigar

Leer solo lo necesario:

1. `website/INVESTIGACION-COMPETIDORES.md` para no repetir hallazgos.
2. `website/README.md` y los prototipos relevantes para conocer el estado visual.
3. `docs/web/contrato-publicacion-inmueble.md` y `src/Enlyce.Api/Endpoints/PublicCatalog/` para separar lo disponible de lo pendiente.

Preguntar únicamente si falta una decisión que cambie el alcance: competidor, recorrido o tipo de entrega. Si no responden, usar Fincaraíz + Metrocuadrado y los recorridos buscar, comparar, consultar y contactar.

## Método

### Observar

Revisar, con fecha y URL, al menos estas superficies cuando existan:

- portada y navegación;
- búsqueda inicial y búsqueda por código;
- resultados, filtros, orden, tarjetas, mapa, favoritos y alertas;
- ficha, galería, datos, ubicación, confianza, relacionados y CTA;
- captación de comprador y propietario;
- rutas indexables, enlaces internos y contenido local.

Usar la web real como fuente primaria. Inspeccionar HTML o cabeceras solo cuando ayude a probar SEO, renderizado o comportamiento. No evadir autenticación, CAPTCHA, límites, `robots.txt` ni controles técnicos.

### Clasificar evidencia

Cada hallazgo debe tener una etiqueta:

- `CONFIRMADO`: visible en la fuente primaria o probado.
- `INFERIDO`: conclusión razonable derivada de señales observables.
- `NO COMPROBADO`: requiere analítica, acceso interno o prueba de usuario.

No presentar popularidad, conversión, rendimiento, tecnología interna o prioridad del competidor como hechos sin evidencia.

### Comparar con L&C

Para cada patrón decidir:

- `ADOPTAR`: encaja sin cambiar su propósito.
- `ADAPTAR`: sirve, pero debe volverse local, simple o humano.
- `RECHAZAR`: añade ruido, dependencia o complejidad prematura.

Contrastar contra el código actual. Distinguir `ya soportado`, `requiere frontend`, `requiere API/modelo` y `requiere validación con usuarios`.

### Convertir en trabajo

Terminar con decisiones y criterios observables:

- rutas e inventario de páginas;
- componentes y estados vacíos/error/carga;
- eventos de captación que deben llegar al CRM;
- requisitos de datos o endpoints;
- backlog `P0`, `P1`, `P2`, ordenado por dependencia;
- criterio de aceptación por ítem, no tareas vagas como “mejorar SEO”.

Usar el formato de [entrega](references/entrega.md) para investigaciones completas. Una pregunta puntual puede responderse sin generar todo el dossier.

## Reglas de salida

- Guardar la investigación completa en `website/INGENIERIA-INVERSA-WEB-LYC.md`, salvo que el usuario indique otra ruta.
- Actualizar afirmaciones viejas que el código o la web real hayan invalidado.
- Citar URL directa junto al hallazgo; fechar datos volátiles.
- Señalar huecos sin inventar y proponer la prueba mínima para cerrarlos.
- No modificar prototipos ni implementar decisiones sin pedido explícito.
