# Prompt de continuacion: paridad Wasi -> Enlyce para L&C

Trabaja hasta cerrar la paridad funcional de las funciones que L&C usa o aprueba. Usa exclusivamente `D:\Proyectos\Enlyce\.codex-worktrees\gate`, rama `codex/paridad-gate`, PR #1 en borrador. No integres, restaures ni limpies el checkout principal: contiene cambios ajenos. No declares paridad completa mientras quede una casilla aprobada abierta.

## Arranque obligatorio

1. Lee `AGENTS.md`, `docs/estado-paridad-wasi-2026-10-24.md`, `docs/checklist-cierre-paridad-wasi-2026-10-24.md` y `docs/checklist-pendientes-paridad-wasi-2026-10-24.md`. Contrasta cualquier dato fechado con el estado actual; el ultimo commit conocido al escribir este prompt es `a644e26`.
2. Confirma `git status`, rama, upstream y que PR #1 sigue en borrador. Conserva los archivos no rastreados preexistentes del worktree y todos los cambios del checkout principal.
3. Verifica acceso autenticado a Wasi y Railway antes de afirmar comportamientos o estado remoto. Observa acciones reales de Wasi; una captura, un nombre de campo o codigo local no prueban reglas, permisos, errores ni resultados. Documenta la matriz campo/rol/accion/resultado sin datos personales.

## Hechos ya comprobados: no repetir

- El ZIP real `D:\Downloads\Cliente_270047_2026-10-15 02_49_51.zip` contiene 63 clientes. El ensayo ya realizado en Wasi creo 19 fichas y rechazo 44; las 19 fueron conciliadas. **No repitas la importacion en Wasi.** Nunca guardes nombres, correos, telefonos, identificaciones, dumps, tokens ni el XLSX real en Git.
- El importador Enlyce lee el XLSX real; la exportacion por correo ahora genera XLSX y una prueba sintetica lo reabre. Falta entrega SMTP real y cotejo del archivo con Wasi.
- Un dump exacto de Railway del 24/10/2026 se restauro y ensayo en PostgreSQL 18 aislado: 23->44 migraciones, 24->33 tablas, segunda ejecucion idempotente y rollback ante falla. Este ensayo no autoriza una migracion posterior: toma y restaura un respaldo inmediato antes de cualquier pase.
- `tools/migrations/seed-lc-customer-stages.sql` creo `Publicado`, `Cerrado Negativo`, `Seguimiento 2` y `Visita Inmueble` solo en la copia aislada. No existen por este trabajo en la base de produccion. No cargues los 63 clientes hasta superar el gate de produccion y crear/verificar esas etapas en Enlyce objetivo.
- `ContactActivity`, `ServiceRating` y `PropertyMedia` del checkout principal no tienen autor atribuible por Git y comparten archivos de infraestructura y snapshot EF. Lee `docs/auditoria-modulos-ajenos-2026-10-24.md`; no copies carpetas ni snapshot mixto. Separa procedencia y dependencias antes de incorporar cada modulo.
- ImageSharp 3.1.12 mantiene avisos de seguridad. El usuario pidio evaluar otra biblioteca; `docs/evaluacion-biblioteca-imagenes-2026-10-24.md` propone prototipar Magick.NET. No actives procesamiento ni cola de fotos hasta validar sustituto, licencia, binarios Linux, limites y pruebas.

## Ejecucion

Avanza por cada casilla del checklist de pendientes. Prioriza gate Railway y clientes/demandas; despues gestiones/calificaciones, fotos, agenda/reportes, CMS/chat. Para portales, red inmobiliaria y funciones propias de Wasi, identifica contratos, credenciales de prueba o decisiones de alcance faltantes y sigue con trabajo independiente. Implementa lo comprobado en API, persistencia e interfaz; prueba casos de exito, rechazo y permisos por rol con datos sinteticos. Registra en cada casilla la URL/ruta o accion observada, condiciones, resultado y prueba Enlyce. Distingue `observado`, `implementado`, `probado en copia` y `validado en produccion`.

## Gate y entrega

- No apliques migraciones, cargues clientes ni despliegues en produccion sin respaldo inmediato restaurado, comparacion de historial/datos, ensayo sobre esa copia, plan de reversa operativo, ventana y **autorizacion expresa**. No ejecutes `pg_restore --clean` sobre la base viva. Conserva el PR en borrador.
- El backup automatico aun carece de retencion remota y alerta externa. La propuesta pendiente es 30 diarios y 12 mensuales; no borres objetos remotos sin autorizacion. Define canal y prueba una falla y una ejecucion omitida.
- Ejecuta `dotnet test Enlyce.slnx`, builds CRM y sitio, y PWA cuando cambie su codigo. No hagas commit ni push con pruebas rojas. Usa `git add` con rutas explicitas, commit y push en la rama aislada, y confirma el checkpoint del vault por entrega.
- Al cerrar cada entrega, actualiza ambos checklists con evidencia y comunica: terminado, probado solo en copia, bloqueado, riesgos y siguiente accion. No marques una casilla por existir codigo. Mantiene PR #1 en borrador hasta revision y decision explicita de integracion.
