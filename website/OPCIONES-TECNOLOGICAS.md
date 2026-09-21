# Opciones tecnologicas

Fecha de verificacion: 2026-09-04.

## Veredicto

Usar **Next.js + TypeScript para `website/` y conservar ASP.NET Core 10 + PostgreSQL como backend unico**. No crear otro backend en Python o Java: duplicaria autenticacion, reglas, despliegue y acceso a datos sin resolver ningun problema real.

## Consejo de decision

### Ronda de posiciones

- **Adversario:** dos frontends y dos runtimes aumentan operacion; el fallo fatal seria duplicar tambien el backend.
- **Arquitecto:** CRM y marketplace deben ser adaptadores separados sobre la misma API y el mismo dominio.
- **Creativo:** L&C debe apropiarse de la lectura humana de Medellin, no parecer otro catalogo naranja generico.
- **Cientifico:** el SEO y los flujos observados estan confirmados; la preferencia visual aun necesita pruebas con usuarios.
- **Filosofo:** el producto no es "una pagina bonita"; es convertir inventario y confianza en visitas trazables.
- **Pragmatico:** prototipar en HTML evita casarse temprano con un framework; luego Next.js reutiliza React.
- **Humanista:** comprador y propietario necesitan recorridos distintos, lenguaje claro y una salida rapida hacia una persona.

### Veredicto del Rey

**Posicion:** Next.js para la web publica, ASP.NET existente para negocio y datos, React/Vite separado para CRM.

**Confianza:** alta en arquitectura; media en direccion visual hasta probar los tres ensayos.

**Disenso que importa:** Razor Pages seria mas simple de operar. Se pierde esa simplicidad a cambio de mejor encaje con React, marketplace interactivo y SEO dinamico.

## Criterios

1. SEO por inmueble, barrio, tipo y modalidad.
2. Inventario dinamico con filtros, mapa, favoritos y formularios.
3. Integracion con la API existente de Enlyce.
4. Mantenimiento viable para un desarrollador con seis horas semanales.
5. Buen rendimiento movil y bajo coste de salida.

| Opcion | Ventaja fuerte | Perdida real | Encaje |
|---|---|---|---|
| **Next.js + TypeScript + API ASP.NET** | SSR/prerender, rutas dinamicas, metadatos por inmueble y React para mapa/filtros | Segundo runtime en produccion y mas conceptos que Vite | **Mejor balance; recomendada** |
| **Astro + React islands + API ASP.NET** | HTML ligero y JavaScript solo en componentes interactivos | Marketplace complejo termina usando muchas islas y coordinacion de estado | Excelente si la web queda mas editorial que marketplace |
| **Razor Pages + ASP.NET Core** | Un lenguaje, un despliegue y acceso natural a la solucion actual | UX interactiva exige mas JS manual; equipo ya tiene React en el CRM | Mejor si se prioriza simplicidad operativa absoluta |

## Por que Next.js

- Sus paginas y layouts se renderizan en servidor por defecto, con HTML inicial, prerender y render dinamico.
- Permite metadatos estaticos o generados desde cada inmueble para SEO y compartidos sociales.
- **Razon corregida 2026-09-21:** el argumento original decia que se reutilizarian componentes de `frontend/`. Es falso para `website/`: son 2.483 lineas de HTML/CSS/JS vanilla, sin `package.json` y sin una sola importacion de React. No hay ningun componente que reutilizar; migrar a Next es reescribir el laboratorio, no reciclarlo.
- Lo que si se gana no es "enlazar con el CRM" — el enlace es HTTP contra `Enlyce.Api` y se ve igual desde cualquier framework. Se gana **compartir ecosistema** con `frontend/` (React 19, TypeScript 6, Tailwind 4, react-query 5): un unico cliente tipado generado del OpenAPI para las dos apps, mismos tipos de dominio, mismos patrones, una sola cabeza de frontend que mantener con seis horas semanales. Esa es la justificacion real de las 18-26 horas de reescritura, no el SEO solo.
- Recordar que `website/` y `frontend/` siguen siendo aplicaciones separadas: el sitio publico es anonimo, el CRM es JWT con roles. Compartir tipos no es acoplar permisos.
- El mapa, filtros y favoritos permanecen interactivos, mientras las fichas pueden salir como HTML indexable.

Fuentes oficiales: [App Router](https://nextjs.org/docs/app), [renderizado y navegacion](https://nextjs.org/docs/app/getting-started/linking-and-navigating) y [metadatos](https://nextjs.org/docs/app/getting-started/metadata-and-og-images).

## Cuando elegir Astro

Elegirlo si L&C reduce el alcance a una web corporativa con catalogo pequeno y pocas interacciones. Astro genera HTML estatico por defecto y carga JavaScript solo en islas marcadas; tambien soporta renderizado bajo demanda mediante adaptadores.

Fuentes oficiales: [arquitectura de islas](https://docs.astro.build/es/concepts/islands/) y [renderizado bajo demanda](https://docs.astro.build/en/guides/on-demand-rendering/).

## Cuando elegir Razor Pages

Elegirlo si un solo despliegue y un solo lenguaje pesan mas que la experiencia de desarrollo del marketplace. Razor Pages organiza rutas por archivos `.cshtml` y esta pensado para escenarios centrados en paginas.

Fuente oficial: [Razor Pages en ASP.NET Core 10](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/?view=aspnetcore-10.0).

## Lenguajes descartados

- **C:** no es opcion sensata para esta web. Es bajo nivel y no aporta al producto.
- **Java/Spring:** capaz, pero introduce otro ecosistema cuando ya existe .NET.
- **Python/FastAPI:** Samuel lo domina, pero reemplazar o duplicar la API actual costaria tiempo y partiria el dominio.
- **Vite SPA sola:** buena para el CRM autenticado; mala base para miles de fichas publicas indexables sin agregar SSR/prerender externo.

## Arquitectura propuesta

```text
Navegador
  -> website/ Next.js (SEO, paginas, filtros, mapa)
       -> Enlyce.Api ASP.NET Core (catalogo publico, leads, visitas)
            -> Application / Domain
                 -> PostgreSQL + almacenamiento de imagenes

frontend/ React + Vite (CRM privado)
  -> misma Enlyce.Api
```

La web publica y el CRM son aplicaciones separadas. Comparten API y dominio, no interfaz ni permisos.
