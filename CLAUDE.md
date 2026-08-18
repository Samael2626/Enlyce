# CLAUDE.md — Enlyce

## Que es
CRM inmobiliario para L&C Propiedad Raiz (LYC). Proyecto universitario de Ingenieria de Software.
Stack: ASP.NET Core 10 + EF Core + PostgreSQL + React/Vite.
Arquitectura: Clean Architecture / Hexagonal (ardalis).

## Comandos
```bash
dotnet build                    # Compilar todo
dotnet test                     # Correr todos los tests
dotnet run --project src/Enlyce.Api  # Levantar API
dotnet ef migrations add <Nombre> --project src/Enlyce.Infrastructure --startup-project src/Enlyce.Api
dotnet ef database update --project src/Enlyce.Infrastructure --startup-project src/Enlyce.Api
```

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
