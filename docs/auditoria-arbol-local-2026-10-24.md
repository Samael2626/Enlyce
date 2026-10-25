# Proteccion del arbol local - 2026-10-24

Inspeccion de solo lectura del checkout principal `D:\Proyectos\Enlyce`. No se atribuye autoria a archivos sin commit.

## Estado Git

- `master` local: `ae022a9`; `origin/master`: `d3bc6be`. `git merge-base --is-ancestor HEAD origin/master` devolvio 0. Hay 18 commits remotos pendientes de incorporar al checkout principal, sin divergencia de historia. No se hizo merge, reset ni clean.
- `git status --porcelain=v1` agrupa 482 entradas; con `-uall` expande a 1071 rutas: 133 versionadas modificadas o borradas y 938 sin seguimiento.
- Principales grupos expandidos: `.tmp` 575, `docs` sin seguimiento 275, `src` versionadas 80 y sin seguimiento 58, `frontend` versionadas 17 y sin seguimiento 6, `tests` versionadas 16 y sin seguimiento 7, `website` versionadas 6, borradas 6 y sin seguimiento 6. Las cuatro entradas `.codex-worktrees` corresponden a checkouts aislados de esta sesion; el resto no se reclasifica como propio sin evidencia.
- Los cambios locales de `PropertyMedia`, `ContactActivity`, `ServiceRating`, migraciones EF y snapshot estan mezclados. Su autoria y unidad de publicacion no estan establecidas. No se copiaron a la rama propia.

## Resolucion segura

La rama `codex/paridad-gate` se creo directamente desde `origin/master` y se publico como PR borrador. Ningun commit de la rama toma cambios del arbol principal. La integracion de los 18 commits remotos ya forma parte de esa base. El checkout principal conserva su estado original y requiere que su responsable seleccione o publique sus cambios antes de actualizarlo.

El PR permanece sin fusionar. Un push a la rama aislada no aplica migraciones a Railway ni activa la cola de fotos.
