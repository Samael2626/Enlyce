# Arquitectura web y conexión con la API

**Estado:** documento base verificado contra el código local el 2026-09-24.

**Alcance:** CRM privado, sitio web público, API compartida, medios y PostgreSQL.

**Versión analizada:** commit documental `b8fecf0` más el cierre técnico del 2026-09-24.

**Implementación del cierre:** commit `82ae9f2`.

**Diagrama:** `NO RENDERIZADO`; PlantUML no está instalado localmente.

**Advertencia:** el worktree contenía cambios sin commit en el módulo administrativo de publicaciones al crear este documento.

## Un producto, dos interfaces

ENLYCE incluye dos aplicaciones web activas con públicos distintos:

| Componente | Ruta | Tecnología | Usuario principal | Responsabilidad |
|---|---|---|---|---|
| CRM privado | `frontend/` | React 19, Vite 8, TanStack Query y Zustand | Administrador y asesor | Autenticación, leads, pipeline, inmuebles, alertas y administración de publicaciones |
| Sitio público | `website/sitio/` | Next.js 16 y React 19 | Visitante, comprador, arrendatario o propietario | Catálogo, ficha del inmueble, contacto y captación de leads |
| API | `src/Enlyce.Api/` | ASP.NET Core 10 Minimal APIs | Ambas interfaces | Autenticación, autorización, casos de uso, catálogo, medios y acceso al dominio |
| Persistencia | `src/Enlyce.Infrastructure/` | EF Core y PostgreSQL | API | Repositorios, configuraciones y migraciones |

`website/funcional/` y `website/concept-*` son prototipos o implementaciones anteriores. No son la web pública principal.

## Regla arquitectónica

Ninguna interfaz web accede directamente a PostgreSQL. Ambas consumen `Enlyce.Api` mediante HTTP. La API ejecuta los casos de uso de Application, aplica las reglas de Domain y persiste mediante los adaptadores de Infrastructure.

Fuente UML editable: [`uml/arquitectura-web-api.puml`](uml/arquitectura-web-api.puml).

## Flujo del CRM privado

1. React obtiene la URL base desde `VITE_API_URL`. El `.env` local usa `http://localhost:5019`, igual que `launchSettings.json`.
2. El valor de respaldo del cliente y el proxy de Vite apuntan al puerto local vigente `5019`.
3. El login envía `POST /api/auth/login`.
4. La API valida correo, contraseña y estado del asesor.
5. La API devuelve el JWT y crea la cookie HttpOnly `_enlyce_auth`.
6. El cliente usa `credentials: "include"`; el middleware transforma la cookie en encabezado `Authorization: Bearer` antes de la autenticación JWT.
7. Las políticas y `EndpointAccess` limitan información según rol y asesor asignado.

La respuesta también contiene el token para conservar el contrato de la API, pero el CRM no lo guarda ni lo usa. Zustand solo mantiene en memoria el usuario de la sesión; la autenticación depende de la cookie HttpOnly y se revalida con `GET /api/auth/me`. El cierre de sesión llama `POST /api/auth/logout` antes de limpiar el estado local.

### Desfases CRM/API cerrados el 2026-09-24

- Pipeline alineado con `/api/pipeline/{id}/mover-etapa` y `/api/pipeline/{id}/asignar`.
- Interacciones alineadas con `/api/interacciones/lead/{id}` y `/api/interacciones`.
- Política y datos personales alineados con `/api/politica/activa` y `/api/datos-personales/{id}`.
- La antigua acción ambigua de revocación fue reemplazada por la operación real de supresión: `DELETE /api/datos-personales/{id}`.
- API, cliente Vite y proxy local usan el puerto `5019`.

## Flujo del sitio público

### Consulta del catálogo

1. Next.js obtiene la API desde `NEXT_PUBLIC_API_URL`; el respaldo local es `http://localhost:5019`.
2. El catálogo consulta `GET /api/public/inmuebles` con filtros en la URL.
3. La ficha consulta `GET /api/public/inmuebles/{slug}`.
4. Ambos endpoints permiten acceso anónimo y responden con caché pública de cinco minutos.
5. Las fotografías se sirven desde `/media`; el sitio normaliza las URLs contra el origen actual de la API.

### Captación de un lead

1. El visitante completa el formulario y debe autorizar el tratamiento de datos.
2. El sitio consulta la política activa en `GET /api/politica/activa`.
3. Envía `POST /api/leads` con datos de contacto, origen, operación, publicación y canal `sitio_web`.
4. La API obtiene la dirección IP desde la conexión, nunca desde el cuerpo enviado por el navegador.
5. Application valida los datos, identifica contactos repetidos, registra el consentimiento y vincula la oportunidad con una publicación cuando corresponde.
6. PostgreSQL conserva el lead, consentimiento e interacciones aplicables.

El catálogo puede consultarse desde el servidor de Next.js sin depender de CORS. El formulario de contacto se ejecuta en el navegador y llama directamente a la API. La política CORS local permite los orígenes de Vite (`5173` y `4173`) y de Next.js (`3000`), tanto con `localhost` como con `127.0.0.1` cuando aplica.

La portada y las páginas dinámicas de zona se renderizan bajo demanda. Así el build de Next.js no necesita una API activa; el `sitemap` conserva una degradación controlada cuando el catálogo no responde.

## Fronteras de seguridad

- Catálogo, política activa, creación de lead, login y salud son públicos.
- CRM, pipeline, inmuebles, visitas, interacciones y administración de publicaciones requieren autenticación.
- Registrar asesores, asignar y reasignar leads requiere rol `Administrador`.
- CORS usa orígenes explícitos y credenciales; no permite cualquier origen.
- Los encabezados reenviados solo se confían cuando existe un proxy configurado.
- Los secretos JWT y la conexión PostgreSQL viven fuera del repositorio.

## Variables de configuración

| Componente | Variable | Uso |
|---|---|---|
| CRM | `VITE_API_URL` | Origen de API y medios |
| Sitio público | `NEXT_PUBLIC_API_URL` | Origen de catálogo, leads y medios |
| API | `ConnectionStrings__Default` | Conexión PostgreSQL |
| API | `JwtSettings__SecretKey` | Firma y validación JWT |
| API | `Cors__AllowTrycloudflareOrigins` | Habilitación temporal de dominios de demo |
| API | `Forwarding__Enabled` | Confianza explícita en proxy inverso |

## Estado técnico comprobado

- Dominio: 190 pruebas superadas.
- Application: 33 pruebas superadas.
- Integración: 150 pruebas superadas.
- CRM React: build de producción superado.
- Sitio Next.js: lint, typecheck y build de producción superados con API y PostgreSQL locales apagados.
- API: build correcto; 190 pruebas de dominio, 33 de Application y 150 de integración superadas.
- Deuda conocida: advertencias de nulabilidad en .NET y advertencia futura por `__dirname` en Vite.

## Decisión PlantUML

Se usará la edición **MIT, Compiled jar**. Mantiene generación de todos los diagramas UML y evita incorporar la licencia GPL al artefacto descargado. El archivo debe guardarse como `tools/plantuml/plantuml.jar`; no se versionará el binario. La fuente oficial consultada fue la página de descargas de PlantUML, versión `v1.2026.8` al 2026-09-24.

## Pendientes documentales

- Generar diagrama de secuencia de autenticación.
- Generar diagrama de secuencia de captación de lead.
- Separar módulos estables de publicaciones administrativas aún presentes como cambios sin commit.
- Documentar despliegue definitivo cuando reemplace los túneles temporales.
