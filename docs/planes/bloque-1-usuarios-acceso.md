# Bloque 1 — Usuarios y acceso

Panel administrativo de usuarios para ENLYCE. Reusa el RBAC existente
(`.RequireAuthorization("Administrador")`, policy definida en `Program.cs`),
los modulos minimal API (`AdvisorsModule`, `LeadDistributionModule`) y los
metodos de dominio ya anadidos (`Asesor.CambiarPasswordHash`, `RevocarSesiones`).

## Convenciones
- Entidad/puerto en `Domain`, caso de uso en `Application`, adaptador EF en `Infrastructure`, modulo minimal API en `Api`.
- EF Fluent en `Infrastructure/Persistence/Configurations/`. Nada se elimina: baja logica.
- Identificadores en ingles; UI en espanol con acentos.
- Tests: xUnit + NSubstitute (dominio/app) + integracion con `WebApplicationFactory`.

## 1.1 ABM completo de asesores
Listar (con filtros), crear, editar nombre/rol, activar/desactivar (baja logica con `Activo`).
- **Domain:** anadir a `Asesor` los metodos `CambiarNombre`, `CambiarRol`, `Desactivar`, `Reactivar`.
- **Application:** casos `ListAdvisors`, `UpdateAdvisor`, `SetAdvisorActive`. Reusar `RegisterAsesorCommandHandler` para crear. Invariante: no desactivar al ultimo Administrador activo; no auto-desactivarse.
- **Infrastructure:** extender `IAsesorRepository` con `ObtenerPorIdAsync`.
- **Api:** ampliar `AdvisorsModule`: `GET /api/asesores/admin`, `POST`, `PUT /{id}`, `PATCH /{id}/estado`. Todo `Administrador`.
- **UI:** `UsuariosPage.tsx` en `/usuarios` (adminOnly en `AppSidebar`), tabla + modal crear/editar.
- **Tests:** dominio (invariante "ultimo admin"), integracion (403 Asesor, 200 admin, baja logica persiste).

## 1.2 Reset de contrasena desde la UI
Boton "restablecer contrasena" por asesor: contrasena temporal + forzar cambio en primer login.
- **Domain:** `CambiarPasswordHash` ya existe (revoca sesiones). Anadir flag `MustChangePassword`.
- **Application:** `ResetAdvisorPassword` (admin), `ChangeOwnPassword` (self, valida la actual).
- **Api:** `POST /api/asesores/{id}/reset-password` (admin), `POST /api/auth/change-password` (self). Si `MustChangePassword`, bloquear todo salvo change-password.
- **UI:** boton en Usuarios + pantalla de cambio forzado.
- **Tests:** reset invalida sesiones viejas (SessionVersion); login forzado a cambiar.
- **Nota:** reemplaza el hook temporal `Auth:ResetAdmin` de `Program.cs`; retirarlo al cerrar este punto.

## 1.3 Revocar sesiones / forzar logout
- **Domain:** `RevocarSesiones()` ya existe. **Api:** `POST /api/asesores/{id}/revocar-sesiones`. **UI:** boton. **Tests:** JWT viejo da 401 tras revocar.

## 1.4 Bitacora de auditoria
Registro inmutable (append-only) de acciones sensibles.
- **Domain:** entidad `AuditEntry` (actor, accion, entidad, entidadId, metadata JSON, timestamp, ip). Puerto `IAuditLog`.
- **Infrastructure:** tabla `AuditEntries` + config EF; adaptador append-only.
- **Application/Api:** emitir eventos en handlers sensibles; `GET /api/auditoria` (admin, paginado + filtros).
- **UI:** `AuditoriaPage.tsx` en `/auditoria` (admin), tabla filtrable.
- **Tests:** cada accion sensible deja entrada; no se puede editar/borrar; sin datos personales ni secretos.

## Migraciones EF (orden)
1. `AddMustChangePasswordToAsesor`
2. `AddAuditEntries`

## Fases
- **Fase A:** 1.1, 1.2, 1.3 (panel de Usuarios usable; retira hook temporal de reset).
- **Fase B:** 1.4 auditoria.

## Riesgos
- Invariante "ultimo administrador": validar en dominio con test.
- `MustChangePassword`: no bloquear el propio cambio ni health.
- Auditoria: append-only real; sin volcar datos personales ni secretos.
