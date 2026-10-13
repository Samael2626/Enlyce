# Planes de ENLYCE — roadmap administrativo y multimedia

Indice de planes de trabajo. Cada plan respeta la arquitectura hexagonal
(Domain / Application / Infrastructure / Api) y las convenciones del repo
(RBAC por policy `Administrador`, EF Fluent, baja logica, tests xUnit + NSubstitute).

- [Bloque 1 — Usuarios y acceso](bloque-1-usuarios-acceso.md): ABM de asesores, reset de contrasena, revocar sesiones, auditoria.
- [Bloque 2 — Configuracion del negocio](bloque-2-configuracion-negocio.md): catalogos, plantillas, marca, calendario/festivos.
- [Multimedia en el CRM](multimedia-crm.md): persistencia externa, dominio de medios, adjuntos por modulo, procesamiento y UI.

## Orden sugerido global
1. **Multimedia Fase 0 (persistencia)** — riesgo activo: el filesystem de Railway es efimero.
2. **Bloque 1 Fase A** — base de usuarios (ABM + reset + revocar sesiones). Retira el hook temporal `Auth:ResetAdmin`.
3. **Bloque 1 Fase B** — auditoria (atraviesa todo).
4. **Bloque 2** — configuracion del negocio.
5. **Multimedia Fases 1-5** — sobre persistencia ya resuelta.

Fecha: 2026-10-13.
