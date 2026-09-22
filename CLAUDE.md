# CLAUDE.md — Enlyce

## Que es
CRM inmobiliario para L&C Propiedad Raiz (LYC). Proyecto universitario de Ingenieria de Software.
Stack: ASP.NET Core 10 + EF Core + PostgreSQL + React/Vite.
Arquitectura: Clean Architecture / Hexagonal (ardalis).

## Comandos
```bash
dotnet build                                            # Compilar todo
dotnet test                                             # Correr todos los tests (355: 182 dominio + 33 aplicacion + 140 integracion)
dotnet run --project src/Enlyce.Api --no-launch-profile  # Levantar API (puerto 5000, Postgres en 5435)
dotnet ef migrations add <Nombre> --project src/Enlyce.Infrastructure --startup-project src/Enlyce.Api
dotnet ef database update --project src/Enlyce.Infrastructure --startup-project src/Enlyce.Api
```

## Docker
```bash
docker start enlyce-db   # PostgreSQL 17-alpine, puerto 5435->5432
docker stop enlyce-db
```

## Secretos locales (antes de levantar la API)
`Enlyce.Api.csproj` ya tiene `UserSecretsId`. Cada clon necesita sus propios valores, fuera de Git:
```powershell
dotnet user-secrets set "JwtSettings:SecretKey" "<CLAVE_ALEATORIA_DE_32_BYTES_O_MAS>" --project src/Enlyce.Api/Enlyce.Api.csproj
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5435;Database=enlyce;Username=postgres;Password=<CONTRASENA_LOCAL_POSTGRES>" --project src/Enlyce.Api/Enlyce.Api.csproj
```
La contrasena en la segunda linea debe coincidir con la del contenedor PostgreSQL local. En produccion, configurar `JwtSettings__SecretKey` y `ConnectionStrings__Default` mediante secretos del entorno; User Secrets solo se carga en Development. Nunca copiar valores reales a `appsettings.json`, Git ni el vault.

## Convenciones
- Identificadores, clases, metodos: **ingles sin acentos**.
- Comentarios y docstrings: **espanol sin acentos**.
- Textos de UI: **espanol con acentos**.
- Cero caracteres CJK en cualquier lugar.
- Domain layer: **cero dependencias** externas. Solo .NET primitives.
- Entity Framework: Fluent API en `Infrastructure/Persistence/Configurations/`, nunca decoradores en las entidades de Domain.
- Tests: xUnit + NSubstitute para mocks.
- Commits: espanol sin acentos.

## Arquitectura
```
src/
  Enlyce.Domain/       # Cero dependencias. Entities, ValueObjects, Ports, Errors.
  Enlyce.Application/  # Use cases. Solo depende de Domain.
  Enlyce.Infrastructure/  # Adaptadores: EF Core, email, etc.
  Enlyce.Api/          # Minimal APIs. Composition root.
tests/
  Enlyce.Domain.Tests/
  Enlyce.Application.Tests/
  Enlyce.IntegrationTests/
```

## Reglas duras
- `Domain/` no importa nada de fuera.
- `Application/` solo depende de `Domain/`.
- `Infrastructure/` implementa los puertos de `Domain/`.
- `Api/` es el punto de entrada, wiring via DI nativo de .NET.
- Nada se elimina: baja logica con auditoria.
- Todo lead requiere autorizacion de tratamiento de datos (Ley 1581).

## Estado actual
- 355 tests pasando (182 dominio + 33 aplicacion + 140 integracion)
- PostgreSQL: container Docker `enlyce-db` en puerto 5435
- Modulos: Leads, Inmuebles, Propietarios, Pipeline, Interacciones, Visitas, Alertas, Auth (JWT+roles+bcrypt), Datos personales (Ley 1581), Medios de publicacion (IMediaStorage + ImageSharp)
- Actividad 1: docs/actividad-1/ (modelo-negocio.md, 1_MODELO_DEL_NEGOCIO_RESPUESTAS.docx, canvas-modelo-negocio.xlsx, presentacion-enlyce.pptx). Generadores en docs/actividad-1/_scripts/
