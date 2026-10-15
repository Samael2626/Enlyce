# Checklist de paridad Wasi → Enlyce

**Corte:** 14 de octubre de 2026  
**Objetivo:** llevar a Enlyce las funciones de Wasi usadas por L&C, sin rehacer funciones que ya existen.  
**Referencias externas para todo Enlyce:** ver [radar de repositorios externos](radar-repositorios-externos.md), que cubre CRM, inmuebles, red, agenda, web, finanzas y operacion.
**Fuentes:** 27 JSON de interfaz en `C:\Users\USUARIO\Downloads\Wasi\Inventarios`, capturas compartidas, ayuda oficial de Wasi y código actual de Enlyce. Algunos JSON iniciales incluyen texto de filas; no copiar esos datos a esta checklist ni al vault.

## Estados

- [x] Cubierto: función existente en Enlyce con alcance comparable.
- [~] Parcial: existe una parte, falta completar paridad o validar comportamiento.
- [ ] Faltante: no se encontró implementación equivalente.
- [?] Sin verificar: evidencia insuficiente para definir el comportamiento esperado.

## Inventario funcional

| Área Wasi | Estado Enlyce | Evidencia / brecha |
|---|---|---|
| Escritorio y métricas del equipo | [~] | Enlyce tiene panel; verificar equivalencia de widgets, periodos y accesos directos de Wasi. |
| Foro inmobiliario | [ ] | Wasi permite publicar y consultar actividad; no hay módulo equivalente. |
| Agenda y tareas | [~] | Tareas y visitas existen; la agenda navega y representa eventos en vistas mes/semana/día, permite saltar de un día del mes a su vista diaria y conserva acciones de completar/cancelar/reprogramar. La rejilla carga la página actual de hasta 100 tareas y las visitas del periodo; las tareas adicionales se recorren con la paginación de la lista. Faltan compartir agenda por equipo, integración Google Calendar, eventos generales y paridad de estados de visitas. |
| Alta, lista y ficha de clientes | [~] | Alta manual y edición permiten etapa configurable, embudo, apellidos, ubicación, móvil/fijo, nacimiento, identificación, referido, medio de captación y observaciones; al crear se puede registrar demanda inicial con presupuesto/moneda, área, habitaciones, baños, garajes y detalles. La [guía oficial](https://ayuda.wasi.co/es/article/agrega-tu-primer-cliente) confirma tipos personalizados, etiquetas y embudos/etapas personalizados; Enlyce admite tipos múltiples, etiquetas y medio de captación como texto; existe catálogo global reutilizable para tipos, etiquetas, embudos y medios. Solo administradores agregan opciones; etapas existentes pueden listarse, se pueden agregar etapas y elegir la predeterminada para leads entrantes; el tablero permite mover y adjuntar contactos existentes. La migración de etapas y embudos está preparada, no aplicada. El formulario Wasi capturado muestra datos del cliente y de demanda; el inventario de campos sin valores está en [evidencia del scrape](evidencia-scrape-wasi.md). La migración de campos privados y embudo está preparada, no aplicada; contactos existentes conservarán su embudo/estado y reciben etapas iniciales al aplicarla. El directorio filtra tipo/estado/embudo/etiqueta/asesor (admin)/texto/fechas; CSV descarga y correo conservan esos filtros. País y departamento de demanda participan en el cruce. Alta no concede permiso de correo; Enlyce exige evidencia por su flujo admin. Las características internas/externas se guardan como listas de texto y participan en el cruce; este compara sin distinguir tildes, mayúsculas ni espacios repetidos. Wasi muestra campos de características, pero el scrape no preserva sus opciones, por lo que falta verificar los catálogos; también falta paridad de gestiones. |
| Embudo de clientes | [~] | Su pipeline de oportunidades sigue siendo distinto del embudo de contactos de Wasi. La [guía oficial](https://ayuda.wasi.co/es/article/crea-tu-embudo-de-negocios-personalizado) confirma varios embudos con nombre personalizado, etapas creadas por la empresa, adjuntar clientes existentes y elegir embudo/etapa predeterminados para contactos de web/portales. Enlyce ya tiene etapas por embudo, conserva los cinco estados iniciales para Clientes, permite crear etapas y elegir una predeterminada para nuevos leads web; alta, edición, filtros e importación usan los catálogos de etapas. El tablero permite mover clientes entre etapas y adjuntar clientes existentes, respetando el alcance por asesor. La migración aditiva está preparada, no aplicada. Falta verificar reglas de editar/borrar/reordenar, permisos, límites de nombres y reasignación al retirar una etapa. |
| Gestiones realizadas | [~] | La captura Wasi confirma filtros por tipo, nombre de cliente, rango de fecha y usuario encargado; la tabla muestra tipo, asunto y cliente, con 10 filas por página. Enlyce tiene filtro por tipo/cliente/fechas y selector de asesor para admin, aplica alcance del asesor en API y ahora pagina de a 10. Enlyce además muestra fecha en la tabla. Falta comparar catálogo completo de tipos, semántica del rango y acciones disponibles sobre gestiones. |
| Demandas y cruce cliente → inmuebles | [~] | La ficha permite crear/modificar/dar de baja demandas sin borrar vínculos. Nuevo cruce global filtra por nombre, tipo/estado del cliente, asesor y fechas de alta del cliente/demanda; muestra cantidad de coincidencias en inventario activo propio. Las características internas/externas se almacenan en ambos lados y forman parte del cruce; tolera diferencias de tildes, mayúsculas y espacios. Wasi separa Propios/Aliados/En Red y muestra porcentaje; faltan red multiempresa y puntaje porcentual. El scrape confirma los campos de características en alta de demanda, pero excluye sus opciones; falta obtenerlas para implementar catálogos comparables. |
| Cruce inmueble → clientes posibles | [x] | Conteo por página y detalle existen, con el mismo alcance por asesor. |
| Inmuebles vinculados al cliente y estado del vínculo | [~] | Vínculos y estados existen; historial de cada cambio de estado sigue pendiente de aprobación/migración. |
| Lista, ficha y edición de inmuebles | [~] | Enlyce filtra texto y código Wasi, tipo, modalidad, estado comercial, ubicación, precio, área, habitaciones, baños, parqueaderos, año, etiqueta y asesor responsable; permite ordenar por fecha y precio. Ficha permite editar año, estado físico, etiquetas y encargado. `EstadoInmueble` y `PropertyPublication.Status` separan disponibilidad comercial de publicación. Año, estado físico, etiquetas y asesor responsable tienen migraciones aditivas preparadas, no aplicadas. Asesor al crear queda asignado al usuario asesor; administrador puede elegir/quitar encargado. Inmuebles previos quedan sin asignar hasta que se editen; no hay evidencia para reconstruir responsable histórico. Las etiquetas son texto libre por inmueble, no catálogo compartido ni creación inline equivalente a Wasi. Faltan mapa/lista y más acciones masivas. Borrado permanente sigue pendiente por dependencias y retención. |
| Importación de inmuebles | [~] | Importación desde API Wasi en segundo plano existe, pero no es el flujo de archivo. La [guía oficial de importación/exportación](https://ayuda.wasi.co/es/article/como-importar-exportar-inmuebles) confirma XLS/XLSX/CSV, mapeo de columnas e ignorar columnas, máximo recomendado de 2.000 propiedades, rechazo de fórmulas y fotos mediante URLs separadas con `|`. Requiere título, descripción, tipo, negocio, precio de venta y/o alquiler, y estado. Enlyce no tiene importación de archivos; definir resolución de propietario (el conector Wasi actual usa propietario temporal) y cómo descargar/validar fotos URL. |
| Exportación de inmuebles | [~] | Enlyce ya envía por correo un XLSX con las propiedades activas visibles en el inventario y los campos guardados por el modelo. La guía oficial describe envío de XLS con todas las propiedades; faltan validar inclusión de inactivas, columnas exactas y exportar fotos/propietario/encargado. |
| Importación de clientes | [~] | CSV/XLS/XLSX con mapeo de campos del perfil Enlyce. La [guía oficial de Wasi](https://ayuda.wasi.co/es/article/como-importar-exportar-clientes) confirma nombre obligatorio, apellidos/email/teléfono y opción de ignorar columnas; no verifica otros campos ni cómo importa duplicados. Enlyce valida el archivo completo, permite recorrer vista previa en páginas de hasta 100 filas, muestra los valores fuente mapeados, invalida la vista previa al cambiar mapeo o asesor, omite emails duplicados dentro del alcance visible, exporta errores sin datos personales y conserva la clave de reintento idempotente. Cada página vuelve a leer y validar el archivo completo; evaluar cache temporal si archivos reales hacen lenta la paginación. Wasi permite varios contactos con el mismo correo en el directorio, pero su guía no aclara si el importador también los conserva; confirmar esta regla antes de cambiar la deduplicación. No hay ID Wasi ni conciliación crear/actualizar/omitir por fila. Excel conserva máscaras simples de ceros iniciales; puede redondear números de más de 15 dígitos antes de leerlos, por lo que teléfonos e identificaciones deben guardarse como texto. La columna de permiso de correo no concede permiso sin evidencia verificable. No inferir importación de demandas. |
| Exportación de clientes | [~] | El scrape confirma la pantalla `/usuario/clientes/exportar`; Enlyce descarga seleccionados o resultados filtrados a CSV, respetando alcance por asesor y con tope de 50.000 filas, y envía el resultado filtrado al correo autenticado. La guía oficial confirma elegir un grupo y enviar el archivo al correo registrado, pero no define las opciones de grupo ni columnas. No hay evidencia de historial de exportaciones en Wasi; no se exige como paridad. Falta recorrer la pantalla y recibir un ejemplo sin datos personales. Ver [evidencia del scrape](evidencia-scrape-wasi.md). |
| Inmuebles de aliados | [ ] | Wasi lista inmuebles compartidos y permite republicación; Enlyce carece de red multiempresa. |
| Alianzas, propietarios aliados y grupos | [ ] | Faltan modelo multiempresa, permisos, invitaciones, solicitudes recibidas/aceptadas, acuerdos y grupos. |
| Mercado inmobiliario | [?] | Pantalla Wasi detectada, contenido dinámico y métricas no identificados. |
| Reporte general | [~] | Analítica CRM/web existe; faltan totales comparables de inventario/negocios/gestiones y exportación Wasi a Excel/PDF. |
| Reportes de negocios, usuarios, propietarios y sitio web | [~] | Hay reportes CRM y web; los reportes específicos de Wasi requieren comparación de filtros y métricas. |
| Correos enviados | [ ] | Wasi muestra reporte de envíos; Enlyce aún no tiene correo saliente integrado con registro de entrega. |
| Usuarios y asesores | [~] | Autenticación y roles existen; falta comparar administración de usuarios, estados, permisos y campos de Wasi. |
| Sitio web inmobiliario | [~] | Catálogo público y publicaciones existen; faltan funciones de administración y configuración web de Wasi. |
| Portales inmobiliarios | [~] | Enlyce administra publicaciones, pero no está probada publicación directa/sincronización con portales. Wasi conecta portales externos. |
| Compartir fichas por PDF, WhatsApp y correo | [~] | Revisar generación/compartición de fichas; correo y WhatsApp no están integrados como canales CRM. |
| Marketing web (plantillas, dominio, formularios, blog y banners) | [~] | Catálogo y captación pública existen; faltan editor de sitio, dominios, plantillas, blog y banners equivalentes. |
| Aplicación móvil | [ ] | Wasi ofrece aplicaciones Android/iOS; Enlyce no tiene app móvil identificada. |
| Publicación en redes sociales | [~] | Wasi muestra controles para Facebook e Instagram en inmuebles; paridad de publicación no comprobada. |
| Control de portales y publicación masiva | [~] | Enlyce gestiona publicaciones propias; falta verificar integración, sincronización, errores y publicación masiva en portales Wasi. |
| Búsqueda y operaciones del inventario | [~] | Filtros y orden: ver fila de inmuebles; etiqueta se busca por coincidencia exacta. Faltan asesor, mapa/lista y acciones masivas restantes. Selección por página y envío/restauración masiva a papelera implementados (hasta 100 IDs). Borrado permanente requiere definir dependencias y retención. |
| Configuración inicial de empresa | [ ] | Wasi presenta un asistente de puesta en marcha; Enlyce no muestra equivalente. |
| Configuración comercial | [~] | Reglas de reparto y SLA existen; comparar el resto de ajustes visibles en Wasi. |
| Mi Cuenta, reputación, academia y soporte | [ ] | Accesos visibles en Wasi; sin equivalentes CRM identificados en Enlyce. |
| Chat interno | [ ] | Enlyce muestra placeholder; Wasi muestra chat y contactos de red. |
| Plan y facturación | [~] | Enlyce tiene checkout y estado de pago; comparar gestión de plan, factura y cuenta frente a Wasi. |

### Avance verificado — 2026-10-14

La fila «Alta, lista y ficha de clientes» y el punto 2 del orden de trabajo quedaron parcialmente desactualizados: Enlyce ya admite varios tipos por cliente y tipos personalizados en alta, edición e importación; el filtro del directorio y el cruce buscan tipos personalizados. Se almacenan hasta 10 valores de 30 caracteres, separados por coma o punto y coma al importar.

La guía oficial de Wasi confirma que las etiquetas y medios de captación se crean desde el flujo del cliente; los tipos pueden ser múltiples, predeterminados o personalizados; y los clientes se asignan a un embudo y una etapa. No explica el alcance de edición/eliminación de cada catálogo, permisos por rol, ni qué pasa con los clientes existentes al renombrar o retirar una opción. Enlyce aún usa texto libre para esos campos y no ofrece un administrador de catálogos. No borrar ni renombrar valores en masa por inferencia; documentar reglas de conservación antes de implementar esas operaciones.

El catálogo administrable de tipos, etiquetas, embudos y medios ya existe (ver sección siguiente). Las etapas configurables y el valor predeterminado para leads entrantes están implementados en código, pero requieren aplicar `20261015052059_AddCustomerFunnelStages`; no se ha aplicado. Reglas verificadas de renombrado/borrado siguen pendientes. La migración `20261014170000_AddMultipleCustomerTypes` está preparada y copia el tipo legado a la nueva columna; no se ha aplicado. No desplegar hasta validar migraciones pendientes y el procedimiento de respaldo/restauración de la base objetivo.

Las etiquetas ahora se pueden asignar y editar por cliente como lista de texto, y se incluyen en el CSV exportado. Falta administrar colores para etiquetas si Wasi los admite en clientes; la guía disponible solo confirma creación personalizada, no colores. La migración 20261014180000_AddCustomerLabels está preparada, no aplicada.

## Catálogos de clasificación de clientes (15 oct 2026)

- Se agregó un catálogo global aditivo para tipos, etiquetas, embudos y medios de captación. Administradores pueden agregar opciones; asesores reciben sugerencias en alta, edición, filtros y cruce. Los campos conservan escritura libre y los valores existentes no se reescriben.
- No hay renombrar ni borrar: la ayuda de Wasi no documenta qué ocurre con clasificaciones ya asignadas. Enlyce ahora guarda etapas por embudo y la etapa predeterminada de leads entrantes; la migración correspondiente no está aplicada. La guía tampoco confirma roles permitidos para editar catálogos.
- Migración `20261015010253_AddCustomerCatalogItems` preparada, no aplicada. Crea una tabla e índice únicos; no modifica `Contacts`. Aplicar solo tras validar respaldo y restauración de la base objetivo.
- Alta y edición ahora muestran tipos y etiquetas múltiples como chips seleccionables. Administradores pueden agregar el valor escrito al catálogo global desde el mismo control; asesores pueden asignar valores personalizados solo al contacto. Los colores siguen sin definirse.
- Wasi confirma embudos de clientes con etapas propias y una selección predeterminada para contactos web/portales. Enlyce persiste etapas por embudo y configura una etapa predeterminada para leads nuevos; el tablero permite mover y adjuntar clientes existentes respetando visibilidad por asesor. `20261015052059_AddCustomerFunnelStages` también migra estados y embudos actuales. Está preparada, no aplicada. Aún faltan reglas de edición, borrado, reordenamiento, permisos y reasignación al retirar etapas.

## Orden de trabajo acordado

### Encargado del inmueble (2026-10-15)

- Wasi muestra `Encargado del inmueble`. Enlyce guarda `AsesorResponsableId` separado del asesor que publica; al crear, el asesor conectado queda asignado y el administrador puede elegir o quitar responsable.
- El inventario filtra por encargado para administradores y muestra el nombre. Inmuebles existentes quedan sin asignar porque no hay fuente fiable para recuperar esa relación.
- `20261015045612_AgregarAsesorResponsableInmueble` agrega columna nullable, índice y FK con `SET NULL`; preparada, no aplicada.

### Avance de campos físicos de inmuebles (2026-10-15)

- El formulario Wasi de alta muestra `Año construcción`; Enlyce ahora lo almacena como `AnioConstruccion` nullable, permite capturarlo/editarlo, lo devuelve en la ficha y permite filtrar inventario con `Construido desde el año`.
- `20261015043830_AgregarAnioConstruccionInmueble`, `20261015044422_AgregarEstadoFisicoInmueble` y `20261015044838_AgregarEtiquetasInmueble` agregan columnas nullable o arreglos vacíos. Preparadas; no aplicadas. No completan registros antiguos sin fuente fiable.
- Wasi muestra año, estado físico, estado de publicación y estado de disponibilidad comercial como campos separados. En Enlyce, `EstadoInmueble` separa disponibilidad de `PropertyPublication.Status`; el estado físico ahora también se guarda por separado. El scrape no preserva las opciones del selector físico ni prueba que `property/search` entregue año o estado físico; no inferir ni completar registros antiguos.
- Estado físico y etiquetas se guardan como texto porque faltan los catálogos y reglas reales de Wasi. La captura de inventario muestra etiqueta por inmueble y acción `+ Nueva`; no prueba alcance, colores, renombrado ni borrado. Enlyce permite editar etiquetas, mostrarlas y filtrar por una etiqueta exacta, pero aún no comparte un catálogo entre inmuebles ni ofrece creación inline.

1. **Importar clientes (parcial):** CSV/XLS/XLSX, API de vista previa/confirmación, contactos independientes, asignación privada, campos mapeables, CSV de filas omitidas y reintentos idempotentes por usuario/clave/hash de contenido y mapeo. La guía oficial solo confirma nombre obligatorio, apellidos/email/teléfono mapeables y columnas ignorables; obligatoriedad de email/teléfono y reglas de duplicados faltan por verificar. Enlyce deduplica por email dentro del alcance visible. Consentimiento de datos y permiso de correo no se mezclan. La bitácora registra fuente/actor/fecha de registro; la fecha original del consentimiento no se conoce si el archivo no la trae. La guía no documenta importación de demandas; no inventar ese requisito. Exportación ahora permite correo propio con CSV filtrado; faltan grupos equivalentes e historial de envíos.
2. **Directorio y ficha de cliente (en curso):** tipo/etapa/embudo, apellidos, ubicación y dirección editables; directorio filtra tipo/etapa/embudo/asesor/fechas y conserva alcance por asesor. Catálogos para tipos, etiquetas, embudos, etapas y medios ya están en código; chips múltiples en alta/edición permiten asignar valores personalizados y administradores pueden crear opciones globales inline. Falta aplicar migraciones tras validar respaldo/restauración, agregar tablero para adjuntar contactos, verificar reglas de edición/borrado/reordenamiento de etapas y cerrar paridad de gestiones.
3. **Cerrar ciclo de importación/exportación:** resultados por fila, errores descargables, reintentos seguros y exportación con filtros/alcances equivalentes.
4. **Completar paridad de inmuebles:** validar campos/formulario y determinar si hace falta importación por archivo además de la integración Wasi existente.
5. **Red inmobiliaria:** acordar modelo multiempresa, invitaciones, visibilidad, publicación, comisiones y auditoría antes de implementarla.
6. **Módulos restantes:** agenda/eventos, reportes, usuarios, sitio web/portales, foro, chat, app móvil, configuración inicial y funciones de cuenta. No sacar funciones de alcance; completar el inventario hasta cubrir Wasi.

## Criterio de cierre por módulo

- [ ] Flujo Wasi observado de inicio a fin; reglas no inferidas solo de nombres de campos o capturas.
- [ ] Campos, validaciones, permisos, filtros y errores documentados sin copiar datos personales.
- [ ] Enlyce implementado en API, dominio, persistencia e interfaz cuando aplique.
- [ ] Casos de éxito, permisos, duplicados y fallos cubiertos por pruebas.
- [ ] Migraciones revisadas con respaldo y restore de la base objetivo antes de producción.
- [ ] Captura visual y diferencias restantes anotadas aquí.

## Gate operativo para migraciones

- `tools/backup/Backup-Enlyce.ps1` sube dumps PostgreSQL custom a un bucket privado y conserva siete días localmente; el almacenamiento remoto se acumula sin borrado automático.
- La verificación automática comprueba firma `PGDMP` y tamaño. El README exige `pg_restore --list` y una restauración de ensayo en una base vacía compatible; no encontré evidencia de que esa restauración se haya ejecutado sobre una copia exacta de la base objetivo.
- No hay alerta externa de fallos. Revisar el log local y la fecha del último objeto no equivale a una alerta operativa.
- Por tanto, las migraciones de paridad siguen preparadas pero no aplicadas a producción hasta registrar el ensayo de restauración y acordar retención remota y alerta de fallo.
- Fuente de estado: [procedimiento de backups](../tools/backup/README.md).

## Límites de la evidencia actual

- Los JSON muestran rutas, etiquetas, tipos de control y encabezados; no revelan de forma fiable validaciones, reglas del servidor ni resultados de acciones.
- Las capturas aclaran pantallas de clientes, cruces, agenda, inmuebles y reportes; faltan recorridos interactivos de importación, alianzas, ajustes y otros flujos.
- El inventario combina funciones visibles en la cuenta L&C y catálogo público oficial; confirmar disponibilidad por plan/país antes de replicar integraciones externas.

## Hallazgos de auditoría del código

- El importador crea contactos independientes sin oportunidades y admite email opcional; el acceso combina propietario asesor y oportunidades asignadas.
- El matcher cubre algunos criterios, pero no replica todos los campos ni filtros Wasi. El cruce cliente→inmuebles es parcial mientras falten aliados y red.
- El importador Wasi de inmuebles es API en segundo plano; no equivale al importador Excel/CSV documentado por Wasi.
- Chat sigue como placeholder. Faltan UI equivalente para red, usuarios y reportes; el importador de clientes ya tiene UI básica.
- Enlyce ofrece calendario con tareas/visitas, analítica CRM/web y checkout; no prueba paridad con agenda/equipos, reportes y ciclo de cuenta Wasi.
- Auditoría de ficha (2026-10-14): la edición de contacto mostraba Dirección dos veces, ambas enlazadas al mismo campo; se quitó el control duplicado. Catálogos están en código; etapas configurables ya están implementadas pero su migración aún no se aplica.

## Ajuste importador (14 oct 2026)

- Ahora mapea nombre, apellidos, email, teléfono y «Recibe correo» por separado. La columna se conserva para revisión, pero importar un valor afirmativo no habilita correos automáticos: falta evidencia verificable del permiso.
- En Enlyce el correo puede repetirse en varios contactos. La vista previa detecta repeticiones, pero no expone emails ajenos al alcance del asesor. La evidencia disponible no confirma si el importador Wasi conserva esos duplicados; no atribuirle a Wasi el comportamiento de Enlyce.
- Administrador puede conceder o revocar el permiso desde la ficha; conceder exige una referencia de evidencia. El historial registra estado, fuente, actor y fecha de registro, en tabla append-only consultable solo por administradores.
- La importación no crea eventos de permiso a partir de «Recibe correo». Cuando un administrador concede permiso desde la ficha, la bitácora registra fuente, referencia de evidencia, actor y fecha de registro. La fecha original no se reconstruye; contactos previos a la migración no reciben eventos inventados.
- Migraciones preparadas, no aplicadas. Contactos existentes quedan con `CanReceiveAutomatedEmail=false`; verificar evidencia real antes de habilitar envíos.

## Caracteristicas de inmuebles y demandas

- Las capturas de Wasi muestran selectores multiples separados para caracteristicas internas y externas en el alta de inmuebles y en el registro de demanda. El JSON de interfaz confirma `select-multiple` en ambos campos; no trae el catalogo de opciones ni reglas de seleccion.
- Enlyce guarda ambas listas en inmuebles y demandas, permite capturarlas en los formularios y exige que todas las caracteristicas solicitadas existan en el inmueble para considerarlo compatible. La comparacion ignora mayusculas/minusculas; acepta texto libre separado por coma mientras falte el catalogo real de Wasi.
- Migracion `20261014162300_AddDemandAndPropertyFeatures` preparada, no aplicada. Agrega cuatro columnas `text[]` con arreglo vacio para filas existentes. No desplegar antes de probar respaldo/restauracion de la base objetivo.
- Falta conseguir evidencia sin datos personales del catalogo Wasi de caracteristicas internas/externas y confirmar si cada caracteristica es requisito excluyente o preferencia. El criterio actual de cruce es provisional de Enlyce, no una regla verificada de Wasi.

## Fuentes oficiales Wasi

- Importacion de clientes (2026-10-14): la guia oficial confirma enlace a la ficha al finalizar. Enlyce devuelve enlaces paginados para todos los clientes importados y persiste esos IDs con los errores del lote para repetir la respuesta idempotente sin migracion ni datos personales adicionales.
- Exportacion de clientes: la guia dice que el usuario elige un grupo y Wasi envia el archivo al correo registrado. No define las opciones de grupo ni las columnas exportadas. Enlyce ya descarga seleccionados/filtros y envia filtros al correo autenticado. No hay evidencia de historial de exportaciones en Wasi; no exigirlo como paridad. Falta captura de la ventana del grupo y del correo recibido.
- Auditoria de importacion Enlyce (2026-10-14): corregidos preview incompleto (ahora muestra los valores fuente mapeados por fila), preview obsoleto al cambiar mapeos/asesor, y nombre asignado a columna 0 sin encabezado claro. El índice de email no es único; el alcance de deduplicación organizacional necesita decisión por privacidad. No hay ID Wasi ni resolución crear/actualizar/omitir por fila. El indicador de permiso importado no es evidencia auditable; por eso no activa correo automático. El admin puede importar sin asignación y los asesores no ven esos contactos. Excel conserva máscaras simples de ceros iniciales; redondea identificaciones numéricas largas antes de importarse. Resolver lo restante antes de declarar paridad.

- [Importar y exportar clientes](https://ayuda.wasi.co/es/article/como-importar-exportar-clientes): formatos, mapeo de columnas, nombre requerido y exportación por grupo al correo.
- [Importar y exportar inmuebles](https://ayuda.wasi.co/es/article/como-importar-exportar-inmuebles): formatos XLS/XLSX/CSV, mapeo e ignorar columnas, campos mínimos, máximo de filas recomendado, fórmulas, URLs de fotos y exportación XLS por correo.
- [Agregar el primer cliente](https://ayuda.wasi.co/es/article/agrega-tu-primer-cliente): formulario de demanda, características internas/externas, detalles y enlaces de inmuebles.
- [Crear embudo de negocios personalizado](https://ayuda.wasi.co/es/article/crea-tu-embudo-de-negocios-personalizado): embudos y etapas propias, adjuntar clientes existentes y definir embudo/etapa predeterminados para contactos web/portales.
- [Cruce inmuebles - clientes](https://ayuda.wasi.co/es/article/cruce-inmuebles-clientes): filtros del cruce y acciones para modificar/borrar demandas; se evidencia compartir propiedad por correo.
- [Importar y exportar inmuebles](https://ayuda.wasi.co/es/article/como-importar-exportar-inmuebles): archivo Excel/CSV, mapeo de atributos y datos mínimos.
- [Software inmobiliario Wasi](https://wasi.co/softwareinmobiliario/): CRM, web inmobiliaria, red de aliados, portalización, móvil y academia.
