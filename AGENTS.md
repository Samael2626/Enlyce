# AGENTS.md — Enlyce

## Proyecto
CRM inmobiliario. ASP.NET Core 10 + EF Core + PostgreSQL.
Clean Architecture / Hexagonal. Minimal APIs.

## Developer
Solo, senior Python/FastAPI aprendiendo C#/.NET.
6h/semana, ~16 semanas.

## Stack
- Backend: ASP.NET Core 10 Minimal API
- ORM: EF Core + Fluent API (Npgsql)
- BD: PostgreSQL
- Frontend: React + Vite
- Tests: xUnit + NSubstitute
- Validacion: FluentValidation

## Estructura
- `src/Enlyce.Domain/` — entidades, value objects, puertos, errores. Cero dependencias.
- `src/Enlyce.Application/` — casos de uso, abstractions (CQRS sin MediatR).
- `src/Enlyce.Infrastructure/` — EF Core, repos, email sender, DI extensions.
- `src/Enlyce.Api/` — Minimal APIs, Program.cs, middleware.
- `tests/` — unit + integration tests.

## Convenciones de codigo
- Inglés sin acentos en codigo.
- Espanol sin acentos en comentarios/commits.
- Espanol con acentos en UI.
- Fluent API en Infrastructure, nunca decoradores en Domain entities.
- CQRS manual: `ICommandHandler<TCommand, TResult>`. Sin MediatR.
- DI nativo de .NET: `IServiceCollection` extension methods.
