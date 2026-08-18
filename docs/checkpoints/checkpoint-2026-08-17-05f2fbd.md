# Checkpoint: Enlyce - 2026-08-17

**Commit:** 05f2fbd - feat: autenticacion JWT manual con roles Administrador y Asesor
**Autor:** Samael2626
**Timestamp:** 2026-08-17 23:07

## Archivos modificados
- src/Enlyce.Api/Endpoints/Auth/AuthModule.cs
- src/Enlyce.Api/Middleware/ExceptionHandlerMiddleware.cs
- src/Enlyce.Api/Program.cs
- src/Enlyce.Api/appsettings.json
- src/Enlyce.Application/Auth/ITokenService.cs
- src/Enlyce.Application/Auth/JwtSettings.cs
- src/Enlyce.Application/Commands/Login/LoginCommand.cs
- src/Enlyce.Application/Commands/Login/LoginCommandHandler.cs
- src/Enlyce.Application/Commands/RegisterAsesor/RegisterAsesorCommand.cs
- src/Enlyce.Application/Commands/RegisterAsesor/RegisterAsesorCommandHandler.cs
- src/Enlyce.Application/DependencyInjection.cs
- src/Enlyce.Domain/Enlyce.Domain.csproj
- src/Enlyce.Domain/Entities/Asesor.cs
- src/Enlyce.Domain/Entities/Inmueble.cs
- src/Enlyce.Domain/Entities/Lead.cs
- src/Enlyce.Domain/Entities/Propietario.cs
- src/Enlyce.Domain/Ports/IAsesorRepository.cs
- src/Enlyce.Domain/ValueObjects/Dinero.cs
- src/Enlyce.Domain/ValueObjects/Direccion.cs
- src/Enlyce.Domain/ValueObjects/Email.cs
- src/Enlyce.Domain/ValueObjects/Telefono.cs
- src/Enlyce.Infrastructure/Auth/CookieAuthenticationMiddleware.cs
- src/Enlyce.Infrastructure/Auth/JwtTokenService.cs
- src/Enlyce.Infrastructure/DependencyInjection.cs
- src/Enlyce.Infrastructure/Enlyce.Infrastructure.csproj
- src/Enlyce.Infrastructure/Migrations/EnlyceDbContextModelSnapshot.cs
- src/Enlyce.Infrastructure/Persistence/Configurations/AsesorConfiguration.cs
- src/Enlyce.Infrastructure/Persistence/Configurations/LeadConfiguration.cs
- src/Enlyce.Infrastructure/Persistence/EnlyceDbContext.cs
- src/Enlyce.Infrastructure/Persistence/Migrations/20260818032258_AddAsesores.cs
- src/Enlyce.Infrastructure/Persistence/Repositories/AsesorRepository.cs
- tests/Enlyce.Domain.Tests/Entities/AsesorTests.cs
- tests/Enlyce.IntegrationTests/AuthEndpointTests.cs
- tests/Enlyce.IntegrationTests/Enlyce.IntegrationTests.csproj
- tests/Enlyce.IntegrationTests/InmueblesEndpointTests.cs
- tests/Enlyce.IntegrationTests/LeadsEndpointTests.cs
- tests/Enlyce.IntegrationTests/TestWebApplicationFactory.cs

## Estado del proyecto
- Pendiente: (completar manualmente)
- Bloqueos: (completar manualmente)
- Proxima accion: (completar manualmente)
