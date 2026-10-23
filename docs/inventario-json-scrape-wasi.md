# Inventario de JSON del scrape de Wasi

**Corte:** 2026-10-23. **Contenido:** estructura de interfaz (rutas, encabezados, controles y columnas); los archivos no conservan valores de formularios ni texto de filas.

Cada fila corresponde a dos archivos identicos: el original en `C:\Users\USUARIO\Downloads\Wasi\Inventarios\<nombre>` y la copia en `D:\Brain\10-Proyectos\Enlyce\evidencia\wasi-scrape\<nombre>`. El tamano es en bytes por archivo. Se conserva el identificador del nombre antiguo para rastrear citas previas; el nombre nuevo describe la pantalla y distingue capturas repetidas.

| ID anterior | Nombre nuevo (`.json`) | Bytes | Ruta Wasi | Informacion guardada |
|---|---|---:|---|---|
| 1791953789437 | `wasi-escritorio-resumen-01.json` | 1281 | `/desktop` | Configuracion inicial y encabezados del resumen del equipo. |
| 1791953952270 | `wasi-escritorio-foro-agenda-02.json` | 2010 | `/desktop` | Encabezados de foro, agenda y solicitudes; navegacion lateral. |
| 1791954022045 | `wasi-escritorio-navegacion-03.json` | 1200 | `/desktop` | Navegacion del escritorio, incluido enlace al muro; sin tabla ni formulario. |
| 1791954119105 | `wasi-agenda-calendario.json` | 2133 | `/usuario/agenda` | Cabeceras de agenda semanal y calendario mensual. |
| 1791954201244 | `wasi-inmuebles-lista.json` | 1786 | `/usuario/inmuebles` | Columnas y controles de la lista de inmuebles propios. |
| 1791954226414 | `wasi-inmuebles-editar.json` | 9405 | `/inmueble-editar.htm` | Etiquetas y tipos de campos de edicion del inmueble, ubicacion y propietario. |
| 1791954254998 | `wasi-inmuebles-aliados.json` | 1874 | `/administrador/inmuebles/aliados` | Columnas de inmuebles aliados, publicacion web y comision. |
| 1791954274748 | `wasi-cruce-inmuebles-clientes.json` | 2069 | `/administrador/cruces/propiedades-clientes` | Tablas de inmueble, clientes posibles y necesidades. |
| 1791954292347 | `wasi-inmuebles-exportar.json` | 1674 | `/usuario/inmuebles/exportar` | Ruta y navegacion de exportacion; sin formulario capturado. |
| 1791954345638 | `wasi-alianzas-empresas.json` | 2047 | `/administrador/empresa/alianzas` | Controles de busqueda de empresas aliadas. |
| 1791954361449 | `wasi-alianzas-propietarios.json` | 2113 | `/administrador/empresa/alianzas-propietario` | Controles de busqueda de propietarios aliados. |
| 1791954377333 | `wasi-alianzas-grupos.json` | 1526 | `/administrador/grupo` | Navegacion y acciones de grupos. |
| 1791954393379 | `wasi-reportes-mercado.json` | 1594 | `/administrador/reportes/mercado` | Navegacion y acciones del reporte de mercado. |
| 1791954415144 | `wasi-clientes-directorio.json` | 5207 | `/usuario/clientes/directorio` | Filtros del directorio y tabla de contacto/referido/comentario/inmueble. |
| 1791954429119 | `wasi-clientes-crear.json` | 4427 | `/usuario/clientes/create` | Etiquetas de alta de cliente y demanda; tabla de inmuebles vinculados. |
| 1791954462902 | `wasi-clientes-embudo.json` | 2319 | `/usuario/pipeline/1` | Navegacion y controles del embudo de clientes. |
| 1791954474775 | `wasi-clientes-gestiones.json` | 2013 | `/usuario/crm` | Filtros y columnas de tipo de gestion, asunto y cliente. |
| 1791954484463 | `wasi-cruce-clientes-inmuebles.json` | 2340 | `/administrador/cruces/clientes-propiedades` | Tablas de demanda e inmuebles disponibles por cliente. |
| 1791954494326 | `wasi-clientes-lista.json` | 1747 | `/usuario/clientes` | Columnas de nombre, contacto, calificacion y permiso de correo. |
| 1791954505967 | `wasi-clientes-importar-exportar.json` | 1743 | `/usuario/clientes/exportar` | Ruta y enlaces de importar/exportar; sin formulario ni columnas de archivo. |
| 1791954520009 | `wasi-reportes-generales.json` | 1731 | `/administrador/reportes2` | Tabla agregada de inmuebles por tipo y negocio. |
| 1791954530739 | `wasi-reportes-negocios.json` | 1929 | `/administrador/reportes/negocios/negocios` | Columnas de inmueble, negocio, encargado y cliente. |
| 1791954542552 | `wasi-reportes-usuarios.json` | 1888 | `/administrador/reportes2/agentes` | Columnas de gestiones, ventas, alquiler, clientes e inmuebles por usuario. |
| 1791954561589 | `wasi-reportes-propietarios.json` | 2518 | `/administrador/reportes2/propietarios` | Columnas de visitas, llamadas, correos, eventos y contactos por propietario. |
| 1791954572426 | `wasi-reportes-web.json` | 1854 | `/administrador/reportes/estadisticas` | Columnas de titulo de inmueble y visitas web. |
| 1791954587334 | `wasi-reportes-correos.json` | 1885 | `/administrador/reportes/correos` | Columnas de fecha, asunto, descripcion, encargado y estado del correo. |
| 1791954610577 | `wasi-usuarios-lista-01.json` | 1526 | `/administrador/agentes` | Columnas de foto, datos generales, correo/alias, estado y acciones. |
| 1791954625968 | `wasi-usuarios-lista-02.json` | 1526 | `/administrador/agentes` | Segunda captura de las mismas columnas de usuarios. |

Los 28 pares tenian el mismo SHA-256 antes del cambio de nombre. El renombrado no altera el contenido. El scrape no prueba reglas del servidor, resultados de acciones, opciones dinamicas ni datos de importacion de clientes.
