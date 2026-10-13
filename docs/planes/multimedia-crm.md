# Multimedia en el CRM

Amplia la base existente (`IMediaStorage` + `LocalMediaStorage`, solo imagenes
y solo para publicaciones, con `MediaConstraints` y variantes webp por ImageSharp)
a multimedia en todo el CRM.

## Punto critico (persistencia)
`LocalMediaStorage` escribe en `wwwroot/media`. En Railway el filesystem del
contenedor es efimero: sin un **Volume** montado en el servicio `Enlyce`, cada
deploy borra lo subido en runtime. Resolver antes de ampliar (Fase 0), o se
pierden archivos en cada despliegue. A verificar en Railway si `Enlyce` tiene volume.

## Fase 0 — Almacenamiento persistente (fundacional, bloqueante)
- **Recomendado: object storage S3-compatible** (Cloudflare R2 o AWS S3). Adaptador `S3MediaStorage : IMediaStorage` (`AWSSDK.S3`), elegido por `Media:Provider`. `LocalMediaStorage` queda para desarrollo.
- **Alternativa minima:** Volume de Railway en `/app/wwwroot/media` + seguir con `LocalMediaStorage` (sin CDN ni URLs firmadas).
- **Config:** `Media:Provider` = `local|s3`, secretos `Media__S3__*` fuera de Git.
- **Migracion:** subir lo existente a S3 y reescribir URLs.
- **Tests:** integracion del adaptador (subir/leer/borrar) con bucket de prueba.

## Fase 1 — Dominio de medios generalizado
- **Domain:** entidad `MediaAsset` (id, `OwnerType` [Inmueble|Lead|Propietario|Interaccion|Publicacion], `OwnerId`, `Kind` [Image|Video|Document], `ContentType`, `SizeBytes`, `StorageKey`, `Variants`, `Visibility` [Public|Private], `UploadedBy`, `CreatedAt`, baja logica). Puerto `IMediaAssetRepository`.
- **Infrastructure:** tabla `MediaAssets` + config EF; `IMediaStorage` guarda por `StorageKey`.
- **Compatibilidad:** mapear fotos de publicaciones actuales a `MediaAsset` (migracion/script).
- **Tests:** asset ligado a su dueno; baja logica no borra el blob hasta purga.

## Fase 2 — Validacion, seguridad y privacidad (Ley 1581)
- Validacion de tipo real (magic bytes), no solo `Content-Type`; imagenes ya decodifican real (ImageSharp), extender a PDF/video.
- Limites por tipo (imagen <= 8 MB, PDF <= 15 MB, video <= 100 MB) y por cantidad.
- Nombre controlado por servidor (ya es el patron).
- Visibilidad: publicos en bucket/carpeta publica; **privados** (cedula, autorizacion firmada, contratos) en bucket privado con **URLs firmadas de expiracion corta** y control de acceso por recurso/rol.
- Antivirus opcional; rechazo ruidoso.
- **Tests:** tipo falso rechazado; documento privado no accesible sin firma; Asesor no ve documentos ajenos.

## Fase 3 — Adjuntos por modulo
- **Inmuebles:** galeria (ya) + planos + video tour, portada seleccionable, orden drag&drop.
- **Publicaciones:** migrar a `MediaAsset`; moderacion de fotos (aprobar antes de publicar).
- **Leads / Propietarios:** documentos privados (cedula, autorizacion de tratamiento firmada, poder).
- **Interacciones / notas:** adjuntar archivo a una nota o mensaje.
- **Api:** rutas uniformes `GET/POST/DELETE /api/{entidad}/{id}/medios`, `GET /api/medios/{id}/url-firmada` (privados). Baja logica.
- **Tests:** integracion por modulo (subir, listar, borrar logico, 403 cruzado).

## Fase 4 — Procesamiento
- Imagenes: mantener variantes webp (400/800/1600); anadir thumbnail cuadrado y strip de EXIF.
- Video: poster (primer frame) y transcodificacion opcional a MP4 H.264 (ffmpeg en worker, no bloqueante).
- PDF: miniatura de primera pagina (opcional).
- Procesamiento asincrono (cola/background service) para no bloquear la subida.

## Fase 5 — UI
- `MediaUploader` reutilizable (drag&drop, progreso, validacion, preview).
- `MediaGallery` (reordenar, marcar portada, borrar).
- Visores: lightbox, visor de PDF embebido, reproductor de video con poster.
- Integrar en `InmueblesPage`, `PublicacionesPage`, ficha de Lead/Propietario, hilo de Interacciones.

## Migraciones EF (orden)
1. `AddMediaAssets` (+ migrar fotos de publicaciones)
2. `AddMediaVisibilityAndKinds` (si no entra en la anterior)
3. Columnas de orden/portada en inmuebles/publicaciones

## Orden recomendado
Fase 0 (bloqueante) -> 1 -> 2 -> 3 (empezando por Inmuebles) -> 5 UI en paralelo a 3 -> 4 al final.

## Riesgos
- Efimero en Railway: no ampliar sin Fase 0.
- Ley 1581: documentos personales siempre privados, URLs firmadas, auditados (enlaza con la bitacora del Bloque 1), borrables a peticion del titular.
- Costo/abuso: limites de tamano y cantidad, rate-limit de subida.
- Migracion de publicaciones sin romper URLs que consume la web publica.
