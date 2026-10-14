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
| Alta, lista y ficha de clientes | [~] | Alta manual y edición permiten estado, embudo, apellidos, ubicación, móvil/fijo, nacimiento, identificación, referido, medio de captación y observaciones; al crear se puede registrar demanda inicial con presupuesto/moneda, área, habitaciones, baños, garajes y detalles. La [guía oficial](https://ayuda.wasi.co/es/article/agrega-tu-primer-cliente) confirma múltiples tipos de cliente personalizados, etiquetas y embudos/etapas personalizados; ENLYCE solo ofrece un tipo fijo, no tiene etiquetas de cliente y el embudo Clientes no es administrable. El medio de captación es texto libre, no catálogo. La migración de campos privados y embudo está preparada, no aplicada; contactos existentes recibirán Clientes por defecto al aplicarla. El directorio filtra tipo/estado/embudo/asesor (admin)/texto/fechas; exportación CSV respeta esos filtros. País y departamento de demanda participan en el cruce. Alta no concede permiso de correo; Enlyce exige evidencia por su flujo admin. Faltan características internas/externas estructuradas y paridad de gestiones. |
| Embudo de clientes | [~] | Pipeline Enlyce maneja oportunidades de venta/arriendo. Wasi gestiona clientes con Nuevo/En proceso/Convertido/Recuperado/Perdido y adjuntar cliente; no asumir equivalencia. |
| Gestiones realizadas | [~] | Interacciones, tareas e historial existen; el formulario de leads ahora envía el GUID del asesor autenticado (antes enviaba email y chocaba con la API). Falta comparar filtros, tipos, asuntos y presentación de la lista Wasi. |
| Demandas y cruce cliente → inmuebles | [~] | La ficha permite crear/modificar/dar de baja demandas sin borrar vínculos. Nuevo cruce global filtra por nombre, tipo/estado del cliente, asesor y fechas de alta del cliente/demanda; muestra cantidad de coincidencias en inventario activo propio. Wasi separa Propios/Aliados/En Red y muestra porcentaje; faltan red multiempresa, puntaje porcentual y características internas/externas estructuradas. La guía confirma esas características, pero los JSON raspados no contienen el formulario ni sus catálogos; falta evidencia de opciones y reglas. |
| Cruce inmueble → clientes posibles | [x] | Conteo por página y detalle existen, con el mismo alcance por asesor. |
| Inmuebles vinculados al cliente y estado del vínculo | [~] | Vínculos y estados existen; historial de cada cambio de estado sigue pendiente de aprobación/migración. |
| Lista, ficha y edición de inmuebles | [~] | Enlyce lista/crea inmuebles. Faltan edición completa y paridad de filtros; Wasi añade mapa/lista, papelera, selección masiva, orden, búsqueda y búsqueda avanzada. |
| Importación de inmuebles | [~] | Importación desde API Wasi en segundo plano existe. Wasi también importa XLS/XLSX/CSV y otras fuentes; falta cubrir formatos aplicables. |
| Exportación de inmuebles | [?] | Wasi muestra acceso de importar/exportar; el flujo de exportación no quedó observado. |
| Importación de clientes | [~] | CSV/XLS/XLSX; la [guía oficial de Wasi](https://ayuda.wasi.co/es/article/como-importar-exportar-clientes) confirma mapeo de nombre (obligatorio), apellidos, email y teléfono, con opción de ignorar columnas. No aclara si email/teléfono se pueden omitir ni cómo maneja duplicados. Enlyce agrega mapeo de tipo/estado/ubicación/permiso de correo, vista previa, validación, confirmación, deduplicación por email dentro del alcance visible, CSV de errores sin datos personales e idempotencia por contenido/mapeo. No hay evidencia de importar demandas en Wasi; la captura de L&C no muestra mapeo ni resultados. |
| Exportación de clientes | [~] | Enlyce descarga seleccionados o resultados filtrados a CSV, respetando alcance por asesor y con tope de 50.000 filas; ahora también puede enviar el CSV filtrado al correo autenticado de la cuenta, sin aceptar destinatarios arbitrarios y solo si SMTP está configurado. Wasi permite elegir grupo; faltan grupos equivalentes y un historial consultable de envíos. |
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

Sigue faltando un catálogo administrable para gestionar tipos, etiquetas, embudos/etapas y medios de captación. La migración `20261014170000_AddMultipleCustomerTypes` está preparada y copia el tipo legado a la nueva columna; no se ha aplicado. No desplegar hasta validar migraciones pendientes y el procedimiento de respaldo/restauración de la base objetivo.

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

## Ajuste importador (14 oct 2026)

- Ahora mapea nombre, apellidos, email, teléfono y «Recibe correo» por separado. Solo valores afirmativos explícitos activan correos automáticos; el permiso de tratamiento de datos no se infiere.
- El correo puede repetirse en varios contactos, igual que en Wasi. La vista previa detecta repeticiones, pero no expone emails ajenos al alcance del asesor.
- Administrador puede conceder o revocar el permiso desde la ficha; conceder exige una referencia de evidencia. El historial registra estado, fuente, actor y fecha de registro, en tabla append-only consultable solo por administradores.
- La importación registra como fuente la columna «Recibe correo» y como actor quien confirma. La fecha es la de registro en Enlyce; la fecha original del permiso no se reconstruye. Contactos previos a la migración no reciben eventos inventados.
- Migraciones preparadas, no aplicadas. Contactos existentes quedan con `CanReceiveAutomatedEmail=false`; verificar evidencia real antes de habilitar envíos.

## Fuentes oficiales Wasi

- [Importar y exportar clientes](https://ayuda.wasi.co/es/article/como-importar-exportar-clientes): formatos, mapeo de columnas, nombre requerido y exportación por grupo al correo.
- [Agregar el primer cliente](https://ayuda.wasi.co/es/article/agrega-tu-primer-cliente): formulario de demanda, características internas/externas, detalles y enlaces de inmuebles.
- [Cruce inmuebles - clientes](https://ayuda.wasi.co/es/article/cruce-inmuebles-clientes): filtros del cruce y acciones para modificar/borrar demandas; se evidencia compartir propiedad por correo.
- [Importar y exportar inmuebles](https://ayuda.wasi.co/es/article/como-importar-exportar-inmuebles): archivo Excel/CSV, mapeo de atributos y datos mínimos.
- [Software inmobiliario Wasi](https://wasi.co/softwareinmobiliario/): CRM, web inmobiliaria, red de aliados, portalización, móvil y academia.
