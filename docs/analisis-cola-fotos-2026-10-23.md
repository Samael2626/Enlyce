# Bloqueo de publicacion: fotos importadas

**Base examinada:** `origin/master` en `d3bc6be`. El checkout principal se inspecciono solo en lectura.

## Dependencia comprobada

- `origin/master` no contiene `PropertyMedia`, `IPropertyMediaRepository`, `PropertyMediaModule`, `AddPropertyMedia`, `PropertyImportPhoto` ni `QueuePropertyImportPhotos`.
- El `PropertyImportPhotoWorker` local consulta `EnlyceDbContext.PropertyMedia`, crea una entidad `PropertyMedia` y persiste su identificador con `PropertyImportPhoto.Complete`. Quitar esa dependencia deja trabajos en cola sin foto CRM visible.
- `PropertyPhoto` de `origin/master` tiene `PublicationId` y representa fotos de una publicacion; no almacena fotos de un `Inmueble` sin publicacion. Reutilizarla cambiaria semantica y visibilidad.
- `ConfirmPropertyFileImportModule`, `PropertyFileImportModule`, `EnlyceDbContext`, `DependencyInjection` y sus migraciones estan mezclados con otros cambios locales. El snapshot EF y los archivos Designer de migraciones locales tambien referencian entidades ajenas como `ServiceRating` y `ContactActivity`.
- La prueba `PropertyPhotoQueuePostgresTests` ejecuta `Database.MigrateAsync()` sobre el modelo local completo. Su pase anterior en PostgreSQL desechable valida ese arbol, no una seleccion publicable sobre `origin/master`.

## Decision

No publicar el conjunto de importacion de fotos desde el checkout principal. Copiarlo o seleccionar esos archivos incorporaria codigo ajeno o fallaria por tipos y tablas ausentes. Tampoco publicar solo la tabla de cola: sin consumidor y medios CRM seria una funcion incompleta.

## Secuencia segura

1. El autor de `PropertyMedia` separa y publica entidad, puerto, repositorio, endpoints, configuracion, DI y migracion sobre una rama limpia. Verificar permisos de asesor, alta, consulta, orden y borrado, ademas de build y pruebas.
2. Sobre esa base, portar la entidad de cola, descargador, importador, worker y migracion nueva generada desde el modelo publicado. Mantener apagado `PropertyPhotoImport:Enabled` hasta verificar la base objetivo.
3. Ejecutar en PostgreSQL desechable la migracion y las pruebas de idempotencia, concurrencia, reemplazo de URL y reintento; probar tambien la foto guardada y su visibilidad CRM/publica. Despues ejecutar suites y builds completos.

**Produccion:** no se aplicaron migraciones ni se activo el worker.
