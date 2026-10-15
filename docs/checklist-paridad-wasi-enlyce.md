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
| Agenda y tareas | [~] | Tareas y visitas existen; faltan agenda día/semana/mes por equipo, Google Calendar, eventos generales y filtros Pendiente/Vencida/Terminada. |
| Alta, lista y ficha de clientes | [~] | Alta manual y edición permiten estado, embudo, apellidos, ubicación, móvil/fijo, nacimiento, identificación, referido, medio de captación y observaciones; al crear se puede registrar demanda inicial con presupuesto/moneda, área, habitaciones, baños, garajes y detalles. La [guía oficial](https://ayuda.wasi.co/es/article/agrega-tu-primer-cliente) confirma tipos personalizados, etiquetas y embudos/etapas personalizados; Enlyce ya admite múltiples tipos y etiquetas como texto libre, pero carece de catálogo reutilizable/administrable y el embudo Clientes no es administrable. El medio de captación también es texto libre. El formulario Wasi capturado muestra datos del cliente y de demanda; el inventario de campos sin valores está en [evidencia del scrape](evidencia-scrape-wasi.md). La migración de campos privados y embudo está preparada, no aplicada; contactos existentes recibirán Clientes por defecto al aplicarla. El directorio filtra tipo/estado/embudo/asesor (admin)/texto/fechas; exportación CSV respeta esos filtros. País y departamento de demanda participan en el cruce. Alta no concede permiso de correo; Enlyce exige evidencia por su flujo admin. Las características internas/externas se guardan como listas de texto y participan en el cruce; este compara sin distinguir tildes, mayúsculas ni espacios repetidos. Wasi muestra campos de características, pero el scrape no preserva sus opciones, por lo que falta verificar los catálogos; también falta paridad de gestiones. |
| Embudo de clientes | [~] | Pipeline Enlyce maneja oportunidades de venta/arriendo. Wasi gestiona clientes con Nuevo/En proceso/Convertido/Recuperado/Perdido y adjuntar cliente; no asumir equivalencia. |
| Gestiones realizadas | [~] | Interacciones, tareas e historial existen; el formulario de leads ahora envía el GUID del asesor autenticado (antes enviaba email y chocaba con la API). Falta comparar filtros, tipos, asuntos y presentación de la lista Wasi. |
| Demandas y cruce cliente → inmuebles | [~] | La ficha permite crear/modificar/dar de baja demandas sin borrar vínculos. Nuevo cruce global filtra por nombre, tipo/estado del cliente, asesor y fechas de alta del cliente/demanda; muestra cantidad de coincidencias en inventario activo propio. Las características internas/externas se almacenan en ambos lados y forman parte del cruce; tolera diferencias de tildes, mayúsculas y espacios. Wasi separa Propios/Aliados/En Red y muestra porcentaje; faltan red multiempresa y puntaje porcentual. El scrape confirma los campos de características en alta de demanda, pero excluye sus opciones; falta obtenerlas para implementar catálogos comparables. |
| Cruce inmueble → clientes posibles | [x] | Conteo por página y detalle existen, con el mismo alcance por asesor. |
| Inmuebles vinculados al cliente y estado del vínculo | [~] | Vínculos y estados existen; historial de cada cambio de estado sigue pendiente de aprobación/migración. |
| Lista, ficha y edición de inmuebles | [~] | Enlyce lista/crea inmuebles. Faltan edición completa y paridad de filtros; Wasi añade mapa/lista, papelera, selección masiva, orden, búsqueda y búsqueda avanzada. |
| Importación de inmuebles | [~] | Importación desde API Wasi en segundo plano existe, pero no es el flujo de archivo. La [guía oficial de importación/exportación](https://ayuda.wasi.co/es/article/como-importar-exportar-inmuebles) confirma XLS/XLSX/CSV, mapeo de columnas e ignorar columnas, máximo recomendado de 2.000 propiedades, rechazo de fórmulas y fotos mediante URLs separadas con `|`. Requiere título, descripción, tipo, negocio, precio de venta y/o alquiler, y estado. Enlyce no tiene importación de archivos; definir resolución de propietario (el conector Wasi actual usa propietario temporal) y cómo descargar/validar fotos URL. |
| Exportación de inmuebles | [~] | Enlyce ya envía por correo un XLSX con las propiedades activas visibles en el inventario y los campos guardados por el modelo. La guía oficial describe envío de XLS con todas las propiedades; faltan validar inclusión de inactivas, columnas exactas y exportar fotos/propietario. El inventario Enlyce aún no asigna inmuebles a asesores. |
| Importación de clientes | [~] | CSV/XLS/XLSX con mapeo de campos del perfil Enlyce. La [guía oficial de Wasi](https://ayuda.wasi.co/es/article/como-importar-exportar-clientes) confirma nombre obligatorio, apellidos/email/teléfono y opción de ignorar columnas; no verifica el resto de campos ni la deduplicación. Enlyce valida fila por fila, muestra en cada fila los valores fuente de todas las columnas mapeadas, invalida la vista previa al cambiar mapeo o asesor, omite emails duplicados dentro del alcance visible, exporta errores sin datos personales y conserva la clave para reintentar fallos; al confirmar con éxito la rota para una ejecución nueva. El índice de email existente no es único; la deduplicación organizacional requiere definir el alcance y su privacidad. No hay ID Wasi ni conciliación crear/actualizar/omitir por fila. Excel conserva máscaras simples de ceros iniciales; puede redondear números de más de 15 dígitos antes de leerlos, por lo que teléfonos e identificaciones deben guardarse como texto. La columna de permiso de correo no concede permiso sin evidencia verificable. No inferir importación de demandas. |
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
| Búsqueda y operaciones del inventario | [~] | Faltan comparar filtros avanzados, mapa/lista, selección y acciones masivas, papelera y recuperación. |
| Configuración inicial de empresa | [ ] | Wasi presenta un asistente de puesta en marcha; Enlyce no muestra equivalente. |
| Configuración comercial | [~] | Reglas de reparto y SLA existen; comparar el resto de ajustes visibles en Wasi. |
| Mi Cuenta, reputación, academia y soporte | [ ] | Accesos visibles en Wasi; sin equivalentes CRM identificados en Enlyce. |
| Chat interno | [ ] | Enlyce muestra placeholder; Wasi muestra chat y contactos de red. |
| Plan y facturación | [~] | Enlyce tiene checkout y estado de pago; comparar gestión de plan, factura y cuenta frente a Wasi. |

### Avance verificado — 2026-10-14

La fila «Alta, lista y ficha de clientes» y el punto 2 del orden de trabajo quedaron parcialmente desactualizados: Enlyce ya admite varios tipos por cliente y tipos personalizados en alta, edición e importación; el filtro del directorio y el cruce buscan tipos personalizados. Se almacenan hasta 10 valores de 30 caracteres, separados por coma o punto y coma al importar.

La guía oficial de Wasi confirma que las etiquetas y medios de captación se crean desde el flujo del cliente; los tipos pueden ser múltiples, predeterminados o personalizados; y los clientes se asignan a un embudo y una etapa. No explica el alcance de edición/eliminación de cada catálogo, permisos por rol, ni qué pasa con los clientes existentes al renombrar o retirar una opción. Enlyce aún usa texto libre para esos campos y no ofrece un administrador de catálogos. No borrar ni renombrar valores en masa por inferencia; documentar reglas de conservación antes de implementar esas operaciones.

Sigue faltando un catálogo administrable para gestionar tipos, etiquetas, embudos/etapas y medios de captación. La migración `20261014170000_AddMultipleCustomerTypes` está preparada y copia el tipo legado a la nueva columna; no se ha aplicado. No desplegar hasta validar migraciones pendientes y el procedimiento de respaldo/restauración de la base objetivo.

Las etiquetas ahora se pueden asignar y editar por cliente como lista de texto, y se incluyen en el CSV exportado. Todavía falta un catálogo global para reutilizar y administrar etiquetas, colores y opciones; la migración 20261014180000_AddCustomerLabels está preparada, no aplicada.

## Orden de trabajo acordado

1. **Importar clientes (parcial):** CSV/XLS/XLSX, API de vista previa/confirmación, contactos independientes, asignación privada, campos mapeables, CSV de filas omitidas y reintentos idempotentes por usuario/clave/hash de contenido y mapeo. La guía oficial solo confirma nombre obligatorio, apellidos/email/teléfono mapeables y columnas ignorables; obligatoriedad de email/teléfono y reglas de duplicados faltan por verificar. Enlyce deduplica por email dentro del alcance visible. Consentimiento de datos y permiso de correo no se mezclan. La bitácora registra fuente/actor/fecha de registro; la fecha original del consentimiento no se conoce si el archivo no la trae. La guía no documenta importación de demandas; no inventar ese requisito. Exportación ahora permite correo propio con CSV filtrado; faltan grupos equivalentes e historial de envíos.
2. **Directorio y ficha de cliente (en curso):** tipo/estado/embudo, apellidos, ubicación y dirección editables; directorio filtra tipo/estado/embudo/asesor/fechas y conserva alcance por asesor. Falta catálogo de tipos múltiples/personalizados, etiquetas, administración de embudos/etapas, catálogo de medios de captación, aplicar la migración de campos privados después de validar respaldo/restauración y cerrar paridad de gestiones.
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
- Auditoría de ficha (2026-10-14): la edición de contacto mostraba Dirección dos veces, ambas enlazadas al mismo campo; se quitó el control duplicado. Los catálogos y etapas configurables siguen pendientes.

## Ajuste importador (14 oct 2026)

- Ahora mapea nombre, apellidos, email, teléfono y «Recibe correo» por separado. La columna se conserva para revisión, pero importar un valor afirmativo no habilita correos automáticos: falta evidencia verificable del permiso.
- El correo puede repetirse en varios contactos, igual que en Wasi. La vista previa detecta repeticiones, pero no expone emails ajenos al alcance del asesor.
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
- [Cruce inmuebles - clientes](https://ayuda.wasi.co/es/article/cruce-inmuebles-clientes): filtros del cruce y acciones para modificar/borrar demandas; se evidencia compartir propiedad por correo.
- [Importar y exportar inmuebles](https://ayuda.wasi.co/es/article/como-importar-exportar-inmuebles): archivo Excel/CSV, mapeo de atributos y datos mínimos.
- [Software inmobiliario Wasi](https://wasi.co/softwareinmobiliario/): CRM, web inmobiliaria, red de aliados, portalización, móvil y academia.
