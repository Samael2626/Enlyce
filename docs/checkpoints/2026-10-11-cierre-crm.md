# Checkpoint — Enlyce — 2026-10-11

## Resumen
Auditoría del cierre CRM con agentes, Graphify, Vault y modo cavernícola. Backup diario de producción confirmado y restore de ensayo aislado probado. SLA de 45 minutos verificado en PostgreSQL de Railway con consultas de solo lectura.

## Cambios realizados
- `docs/checklist-producto-web-enlyce.md`: evidencia de backup/restore, estado SLA remoto y pendientes de retención/alerta.
- `docs/roadmap-crm-inmobiliario.md`: mismo ensayo remoto, regla SLA verificada y tareas pendientes.

## Estado actual
- Backup diario `Enlyce-Postgres-Backup`: ejecución programada del 11-oct, resultado 0; objeto `enlyce-20261011-020005.dump`, 62.165 bytes. ETag remoto coincide con MD5 local.
- Restore aislado PostgreSQL 17: `pg_restore` correcto, 24 tablas y ~25 filas vivas; temporal sin red/puerto/volumen persistente, eliminado al cerrar.
- Producción: migración `20261010023810_UseLeadSlaResponseMinutes` aplicada; regla global activa con 45 minutos calendario. Consulta transaccional de solo lectura; llave SSH temporal retirada.
- Pruebas backend: Domain 222, Application 51, Integration 230 verdes; prueba PostgreSQL geoespacial 1/1. Build web Next.js correcto.

## Pendientes / próximos pasos
- Crear staging sin asumir costos; ejecutar migraciones, smoke, ZAP, TRACE 405, rollback/health y fijar RPO/RTO.
- Probar alertas SLA dentro/fuera del plazo; definir excepciones por origen/operación y horario.
- Añadir alerta de fallo de backup y política de retención remota; revisar cuota Storage.
- Revisión legal, fotos/inventario reales autorizados, validación SEO/JSON-LD publicado, WCAG AA y Core Web Vitals.
- Piloto operativo con los cinco usuarios y permisos reales.
- Notificaciones reales por correo/WhatsApp y tareas SLA automáticas quedan para siguiente fase.

## Decisiones y notas
- Sin servicios pagos nuevos. No ejecutar migraciones ni cambios de producción sin gate y backup/restauración de la base objetivo.
- Vault tiene checklists atrasadas respecto al repo; esta nota registra el estado actual.

## Git
- Rama y commit base: revisar al crear el commit de esta documentación.
- Hay cambios de usuario ajenos sin confirmar; no incluirlos.

---
Tags: #checkpoint #enlyce #crm

## Ubicación en el repo
`D:\Proyectos\Enlyce\docs\checkpoints\2026-10-11-cierre-crm.md`
