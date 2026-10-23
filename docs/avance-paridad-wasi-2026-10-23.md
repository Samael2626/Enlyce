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
- Arbol de trabajo verificado: integracion 305 aprobadas, 2 omitidas, 0 fallidas; dominio 234 aprobadas; aplicacion 77 aprobadas. Los tests de SQLite sin `char_length` y los contratos de catalogo fueron alineados con el estado actual. El commit aislado no incorpora archivos con cambios ajenos.

## Bloqueos de evidencia

- No existe muestra XLS/XLSX/CSV de importacion de clientes Wasi; duplicados y demandas no se pueden contrastar con ese flujo.
- El editor directo de la web requiere resolver el dominio/sesion; el enlace al CRM es la ruta funcional preparada.
- Los JSON no muestran valores, filas ni resultados de acciones; no cierran las reglas dinamicas de los modulos de Wasi.
