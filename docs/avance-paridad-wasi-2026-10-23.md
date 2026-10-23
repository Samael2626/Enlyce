# Avance de paridad Wasi - 2026-10-23

## Evidencia del scrape

- Localizados 28 JSON originales en `C:\Users\USUARIO\Downloads\Wasi\Inventarios` y 28 copias en `D:\Brain\10-Proyectos\Enlyce\evidencia\wasi-scrape`.
- Los 28 pares coincidieron por SHA-256 antes del renombrado. Se renombraron ambos conjuntos sin cambiar contenido; los nombres, tamanos, rutas y alcance de cada archivo constan en [el inventario](inventario-json-scrape-wasi.md).
- Las citas con nombre antiguo en la evidencia del repo y en `Ingenieria-Inversa-Wasi.md` del vault quedaron actualizadas. El scrape conserva estructura, no filas ni reglas de importacion.

## Importacion de fotos de inmuebles

- La regresion aislada del descargador HTTP cubre URL insegura antes de enviar, rechazo de redireccion y MIME no fotografico, limite de 8 MB aun sin `Content-Length`, y respuesta PNG valida.
- `dotnet test tests/Enlyce.IntegrationTests/Enlyce.IntegrationTests.csproj --filter FullyQualifiedName~PublicPhotoUrlDownloaderTests --no-restore`: 7 aprobadas, 0 fallidas, 0 omitidas.
- La ruta DELETE de medios vuelve a iniciar: se cambio la inyeccion de `ILogger` a `ILoggerFactory` en `PropertyMediaModule.cs`, archivo ajeno aun sin seguimiento. Las 7 pruebas del descargador dependen de `PublicPhotoUrlDownloader.cs`, tambien ajeno sin seguimiento; no se incluyen en el commit aislado.
- No se probo descarga real ni la cola con PostgreSQL. `QueuePropertyImportPhotos` sigue sin aplicarse y el worker sigue apagado por defecto. La activacion en la base objetivo requiere el respaldo y restauracion previstos en el roadmap; no se aplicaron migraciones ni se desplego.

## Gestiones y calificaciones

- `InteraccionRepository.SearchAsync` vuelve a unir interacciones, actividades, tareas completadas y visitas con filtros y paginacion SQL. La proyeccion a `InteractionSearchItem` ocurre despues de la union; `CompletedAt.Value` evita una proyeccion de cliente que antes producia HTTP 409. Los dos tests de gestiones que fallaban ahora pasan.
- Las rutas publicas de calificacion por token declaran acceso anonimo, limite por IP y cuerpo maximo de 16 KB. La matriz de autorizacion pasa. El archivo `ServiceRatingsModule.cs` ya existia sin seguimiento; la correccion funcional queda en el arbol de trabajo, fuera del commit aislado.
- Arbol de trabajo verificado: integracion 305 aprobadas, 2 omitidas, 0 fallidas; dominio 234 aprobadas; aplicacion 78 aprobadas. Los tests de SQLite sin `char_length` y los contratos de catalogo fueron alineados con el estado actual. El commit aislado no incorpora archivos con cambios ajenos.

## Alertas de publicaciones

- `PublicationAlertDispatcher` omite contactos inactivos aunque conserven permiso de correo automatico y demanda activa. No envia correo ni crea vinculo para ellos; evita reactivar una alerta por un permiso anterior a la baja.
- Suite de aplicacion en el arbol actual: 78 aprobadas, 0 fallidas. El parche aislado sobre `HEAD`, compilando solo el dispatcher y sus pruebas, paso 3 de 3. La prueba nueva cubre contacto inactivo con permiso y demanda coincidente.

## Web instalable: fallback sin conexion

- Se agrego un service worker al sitio publico. Para navegaciones del mismo origen intenta siempre la red; cuando falla muestra una pagina estatica sin conexion. No guarda respuestas de la API, fichas de inmuebles ni formularios.
- `node tests/service-worker.test.mjs`: 3 aprobadas, 0 fallidas. `npm run typecheck` y `npm run build`: aprobados en checkout aislado. El tipo de etiquetas de la ficha se tolera como opcional porque el contrato base aun no lo declara; esto permite compilar el checkout sin incorporar el cambio ajeno de `client.ts`. El lint global conserva un error previo en `PropertyAdminLink.tsx`; los archivos nuevos no lo causan.
- La paridad movil completa sigue pendiente: funciones CRM en movil, validacion en dispositivo y recorrido de la app Wasi. El editor web sigue a la espera de la decision de dominio. No se desplego.
- Los arreglos de gestiones y calificaciones siguen sin commit en el arbol principal: la consulta de gestiones depende de una extension ajena de `InteraccionRepository`, y el modulo de calificaciones entero es ajeno sin seguimiento. Sus correcciones puntuales no se pueden publicar solas desde `HEAD`.

## Bloqueos de evidencia

- No existe muestra XLS/XLSX/CSV de importacion de clientes Wasi; duplicados y demandas no se pueden contrastar con ese flujo.
- El editor directo de la web requiere resolver el dominio/sesion; el enlace al CRM es la ruta funcional preparada.
- Los JSON no muestran valores, filas ni resultados de acciones; no cierran las reglas dinamicas de los modulos de Wasi.
