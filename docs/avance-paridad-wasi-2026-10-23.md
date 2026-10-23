# Avance de paridad Wasi - 2026-10-23

## Evidencia del scrape

- Localizados 28 JSON originales en `C:\Users\USUARIO\Downloads\Wasi\Inventarios` y 28 copias en `D:\Brain\10-Proyectos\Enlyce\evidencia\wasi-scrape`.
- Los 28 pares coincidieron por SHA-256 antes del renombrado. Se renombraron ambos conjuntos sin cambiar contenido; los nombres, tamaños, rutas y alcance de cada archivo constan en [el inventario](inventario-json-scrape-wasi.md).
- Las citas con nombre antiguo en la evidencia del repo y en `Ingenieria-Inversa-Wasi.md` del vault quedaron actualizadas. El scrape conserva estructura, no filas ni reglas de importación. Tarea cerrada.

## Importación de fotos de inmuebles y cola desacoplada

- La regresión aislada del descargador HTTP cubre URL insegura antes de enviar, rechazo de redirección y MIME no fotográfico, límite de 8 MB aún sin `Content-Length`, y respuesta PNG válida.
- `dotnet test tests/Enlyce.IntegrationTests/Enlyce.IntegrationTests.csproj --filter FullyQualifiedName~PublicPhotoUrlDownloaderTests --no-restore`: 7 aprobadas, 0 fallidas, 0 omitidas.
- La ruta DELETE de medios vuelve a iniciar: se cambió la inyección de `ILogger` a `ILoggerFactory` en `PropertyMediaModule.cs`, archivo ajeno aún sin seguimiento en el árbol principal.
- La cola de fotos se probó en PostgreSQL 17 desechable y aislado (`127.0.0.1:55432`, base `enlyce_photo_test`): migración, alta idempotente, concurrencia en dos contextos, reemplazo de URL y reintento después de tres fallos; 1/1 prueba aprobada (`PropertyPhotoQueuePostgresTests.cs`). La instancia temporal se detuvo y eliminó.
- **Investigación de desacoplamiento para publicación en `origin/master`:**
  - Se analizó exhaustivamente si `PropertyImportPhotoQueue` y sus pruebas podían publicarse aisladamente sin incluir cambios ajenos.
  - **Bloqueo técnico identificado:** `PropertyImportPhotoQueue` depende directamente de `PropertyMedia` (entidad `PropertyMedia`, repositorio `IPropertyMediaRepository`, endpoints `PropertyMediaModule.cs` y migración de base de datos `AddPropertyMedia`). Ninguno de estos artefactos existe en `origin/master`; son cambios locales no commiteados de otro autor en el checkout principal sucio.
  - Además, `EnlyceDbContext` en el checkout principal contiene 10 migraciones no commiteadas entremezcladas (`AddAdvisorInvitations`, `AddServiceRatings`, `AddContactActivities`, `AddContactPropertyLinks`, `TrackContactStatusHistory`, etc.).
  - Publicar `PropertyImportPhotoQueue` de forma aislada sobre `origin/master` causaría errores de compilación inmediatos por tipos y tablas inexistentes, o exigiría incorporar cambios ajenos no autorizados.
  - **Decisión:** Bloqueo documentado formalmente. No se puede publicar hasta que el autor de `PropertyMedia` estabilice y publique su módulo de forma independiente.

## Gestiones y calificaciones

- `InteraccionRepository.SearchAsync` en el árbol de trabajo une interacciones, actividades, tareas completadas y visitas con filtros y paginación SQL. La proyección a `InteractionSearchItem` ocurre después de la unión; `CompletedAt.Value` evita una proyección de cliente que antes producía HTTP 409. Los dos tests de gestiones que fallaban pasan en local.
- Las rutas públicas de calificación por token declaran acceso anónimo, límite por IP y cuerpo máximo de 16 KB. La matriz de autorización pasa.
- **Bloqueo de publicación:** Ambos módulos (`ContactActivity` y `ServiceRatingsModule`) son archivos y entidades ajenos no commiteados en el checkout principal, con migraciones pendientes no aplicables en producción sin instrucción. No pueden publicarse de forma aislada en `origin/master`.

## Alertas de publicaciones

- `PublicationAlertDispatcher` omite contactos inactivos aunque conserven permiso de correo automático y demanda activa. No envía correo ni crea vínculo para ellos; evita reactivar una alerta por un permiso anterior a la baja.
- Suite de aplicación en el árbol actual: 78 aprobadas, 0 fallidas. El parche aislado sobre `origin/master`, compilando solo el dispatcher y sus pruebas, pasó 3 de 3. La prueba nueva cubre contacto inactivo con permiso y demanda coincidente.

## Sitio público y CRM móvil instalable (PWA)

- **Sitio público (commit `1abd1c3`):** Se agregó service worker al catálogo público. Para navegaciones del mismo origen intenta siempre la red; cuando falla muestra una página estática sin conexión. No guarda respuestas de la API, fichas de inmuebles ni formularios. Pruebas: `node tests/service-worker.test.mjs` (3 aprobadas).
- **CRM móvil (commit `5aa5159`):** Paridad de aplicación móvil web (PWA). Se implementó soporte PWA completo para la aplicación del CRM en `frontend/public/manifest.json`, iconos de alta resolución (192x192 y 512x512), service worker `frontend/public/sw.js` con fallback estático `frontend/public/offline.html`, y registro en `frontend/src/main.tsx` y `frontend/index.html`.
- Pruebas PWA: `npm run test:pwa` (4/4 aprobadas: interceptación de fetch, fallback offline, bypass de API/mutaciones y scope). `npm run build` aprobado sin errores.
- **Brecha restante móvil:** Wasi ofrece aplicaciones móviles nativas publicadas en tiendas (Android/iOS). La validación en dispositivo físico y un cliente móvil nativo quedan pendientes.

## Reportes de propietarios y analítica

- **Exportación de actividad de propietarios a Excel (commit `62cb395`):**
  - Se agregó endpoint `GET /api/analytics/owners.xlsx` en `AnalyticsModule.cs`, restringido al rol `Administrador`.
  - Validación de rango (máximo 366 días) y límite seguro de 50.000 filas. Genera archivo Excel binario OpenXML (`application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`) con resumen y métricas por propietario (inmuebles activos, visitas, llamadas, correos, eventos y contactos).
  - En CRM frontend: función `downloadOwnerPerformanceReportXlsx` en `client.ts` y botón interactivo "Descargar Excel" en `AnaliticaPage.tsx`.
  - Pruebas: `OwnerReportExportEndpointTests.cs` (2/2 aprobadas, verdes: autorización y generación/descarga válida).

## Negocios realizados y edición dentro de 3 días

- **Modificación de cierres dentro de ventana de 3 días (commit `21dbc40`):**
  - La ayuda oficial de Wasi confirma que los negocios cerrados pueden modificarse durante los 3 días posteriores al cierre desde el historial.
  - Enlyce implementó en dominio `RealizedDeal.CanEditAt(DateTime nowUtc)` y `UpdateFinancials(decimal grossAmount, decimal commissionAmount, string currency, DateTime nowUtc)` en `src/Enlyce.Domain/Entities/RealizedDeal.cs`. Valida montos > 0, comisión <= monto bruto, código de moneda ISO y arroja `InvalidOperationException` si se excede la ventana de 3 días (72 horas exactas).
  - API: endpoint `PUT /api/realized-deals/{id}` en `RealizedDealsModule.cs`. Autorización verificada: solo el asesor responsable del negocio o usuarios con rol `Administrador` pueden modificarlo; retorna HTTP 409 Conflict si han transcurrido más de 3 días.
  - Repositorio: métodos `GetByIdAsync` y `UpdateAsync` en `IRealizedDealRepository` e implementados en `RealizedDealRepository.cs`.
  - Frontend: actualización de `client.ts`, hook `useApi.ts`, tipado en `types.ts` y modal interactivo de edición financiera en `AnaliticaPage.tsx` con control de fecha límite (3 días).
  - Pruebas: `RealizedDealTests.cs` (11/11 unitarias de dominio aprobadas) y `RealizedDealUpdateEndpointTests.cs` (4/4 de integración aprobadas: actualización válida por asesor dueño, actualización por admin, rechazo tras 3 días y rechazo a asesor no dueño). `npm run build` aprobado.

## Cruces de demandas y formularios públicos

- En `origin/master` se corrigió la exposición indebida de inmuebles ajenos en cruces de demanda y acciones de vínculo. El parche aislado filtra conteos, coincidencias, detalle inverso y cambios de estado con el mismo alcance por asesor; 8/8 pruebas dirigidas aprobadas.
- La consulta pública de inmuebles clasifica formularios de arriendo como Arriendo y ofrece Compra/Arriendo en fichas mixtas. Pruebas E2E de formularios 4/4, TypeScript y build aprobados.
## Chat interno en tiempo real (SignalR + UI en CRM)

- **Implementación completa (commit `3782d64`):**
  - **Dominio:** Entidad `ChatMessage` con validaciones de longitud (<= 2000 chars), sanitización, destinatario opcional (canal general vs mensaje directo 1-a-1), fecha de envío y marca de lectura (`MarkAsRead`). Puerto `IChatMessageRepository` con consultas paginadas, conteos de no leídos por usuario y marcado de lectura en lote. Pruebas de dominio: `ChatMessageTests.cs` (11/11 aprobadas).
  - **Infraestructura:** `ChatMessageConfiguration` con índices optimizados para buzones y canales, `ChatMessageRepository` con consultas asíncronas EF Core y migración generada en código `AddChatMessages`.
  - **SignalR Hub & Transporte:** `ChatHub` en `/hubs/chat` protegido con JWT (`[Authorize]`), con extracción de token desde query string para WebSockets y `CustomUserIdProvider` para mapeo por GUID de usuario. Soporta canal general de agencia (`GeneralChannel`), mensajes directos por ID de usuario, indicadores de lectura (`MessagesRead`) y eventos de escritura en vivo (`UserTyping`).
  - **API REST complementaria:** `ChatModule.cs` expone `GET /api/chat/users` (asesores con conteo de no leídos), `GET /api/chat/messages` (historial de conversación o canal), `POST /api/chat/messages` (envío con fallback a REST si reconecta WebSockets), `POST /api/chat/read` (marcar lectura) y `GET /api/chat/unread`. Pruebas de integración: `ChatEndpointTests.cs` (6/6 aprobadas).
  - **Frontend:** Hook `useChatHub.ts` con reconexión automática y listeners en tiempo real; métodos en `client.ts`; página interactiva `/chat` en `ChatPage.tsx` con lista de canales, contactos, burbujas de mensaje, indicadores de lectura («Leído»/«Enviado»), detector de escritura y conexión en vivo. `App.tsx` actualizado y build verificado sin errores.

## Bloqueos y estado de evidencia

- **Importación de clientes:** No existe muestra XLS/XLSX/CSV de importación de clientes Wasi. La validación final de duplicados y reintentos queda marcada expresamente como pendiente de evidencia externa.
- **Edición directa de la web pública:** Pendiente de decisión sobre dominio y arquitectura de sesión/SSO.
- **Cola de fotos (`PropertyImportPhotoQueue`):** Bloqueada por dependencia de `PropertyMedia` y migraciones mixtas de otro autor en el árbol sucio.
- **Gestiones y Calificaciones:** Bloqueadas por entidades y migraciones no publicadas en `origin/master` (`ContactActivity`, `ServiceRating`).
- **Red inmobiliaria y Alianzas:** Requiere arquitectura multi-tenant y reglas de negocio inter-empresa no definidas.
- **Mercado inmobiliario:** Depende de algoritmo propietario de estimación de Wasi.
