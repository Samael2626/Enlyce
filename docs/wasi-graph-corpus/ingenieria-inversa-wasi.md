---
title: "Ingenieria inversa de Wasi"
date: 2026-10-14
tags: [enlyce, crm, inmobiliaria, wasi]
---

# Inventario funcional de Wasi

Capturas hechas desde la cuenta autorizada de L&C. Registrar funciones y reglas; no copiar datos personales ni registros de clientes.

## Escritorio

Fuente: captura visual del 13 de octubre de 2026 y exportacion de interfaz `/desktop`.

### Navegacion observada

- Inicio: Escritorio (`/desktop`), Foro inmobiliario (`/usuario/muro`), Agenda (`/usuario/agenda`).
- Inmuebles, Red inmobiliaria, Clientes, Reportes, Usuarios, Sitio web, Configuracion y Mi Cuenta aparecen como secciones expandibles; sus enlaces visibles apuntan a `/desktop` hasta abrir el submenu.
- Configuracion inicial: `/administrador/empresa/optimizar`.
- Academia Wasi: `/academy`.
- Reputacion: `/usuario/reputacion`.
- Accesos complementarios: Academia Wasi, reputacion, invitar y soporte.

### Panel y acciones

- Accesos directos: crear inmueble y crear cliente.
- Resumen del equipo: tareas terminadas en 30 dias, desglosadas en llamadas, correos, eventos y visitas.
- Negocios realizados en 30 dias, separados en ventas y alquileres.
- Clientes nuevos en 7 dias, separados entre gestionados y en espera.
- Inmuebles activos, con distribucion por venta, alquiler y permuta.
- Foro inmobiliario: crear/publicar contenido y consultar actividad.
- Agenda semanal con selector de equipo y acceso para agregar tarea.
- Panel de nuevos clientes interesados y enlaces a sus listas.
- Carrusel/listado de inmuebles compartidos recientemente.
- Configuracion inicial con avance de tareas de puesta en marcha.

## Agenda

Ruta: `/usuario/agenda`.

- Vista semanal con columnas de lunes a domingo; el escritorio mostraba selector de equipo y cambio entre dia, semana y mes.
- El inventario detecta varias tablas de calendario y un selector mensual (octubre de 2026).
- La captura no permite inferir acciones de crear/editar, tipos de evento, asignacion ni reglas de recordatorio; hay que inspeccionar la pantalla visualmente.

## Red inmobiliaria

Submenu observado:

- Inmuebles compartidos (`/usuario/inmuebles/red`).
- Alianzas (`/administrador/empresa/alianzas`): filtro por nombre y ubicacion de la alianza; se observan enlaces a perfiles de empresas.
- Propietarios (`/administrador/empresa/alianzas-propietario`): filtro por nombre y ubicacion. El inventario no detecto tabla, aunque muestra un contador de resultados.
- Grupos (`/administrador/grupo`): existe acceso **Nuevo grupo**. No se detectaron tabla ni formulario en esta pantalla; puede abrir un modal o requerir interacción para mostrar contenido.
- Mercado inmobiliario (`/administrador/reportes/mercado`).

La ruta de Mercado inmobiliario no expone tablas, formularios ni acciones HTML detectables en el inventario. Su contenido podría ser visual (graficas/canvas) o cargar tras una interaccion; no inferir metricas todavia.

Los filtros sugieren busqueda de aliados y propietarios por ubicacion. No guardar el contador ni identificadores de perfiles: dependen de los datos de esta cuenta.

## Clientes

Submenu observado:

- Directorio de clientes (`/usuario/clientes/directorio`).
- Nuevo cliente (`/usuario/clientes/create`).
- Embudo de negocios (`/usuario/pipeline/1`).
- Gestiones realizadas (`/usuario/crm`).
- Cruce clientes - inmuebles (`/administrador/cruces/clientes-propiedades`).
- Lista de clientes (`/usuario/clientes`).
- Importar - exportar (`/usuario/clientes/exportar`).

La ruta `/usuario/clientes` muestra una tabla con nombre, contacto, informacion, calificacion y preferencia de recepcion de correo, mas columnas sin etiqueta para seleccion/acciones. No se guardan valores de filas.

#### Importar - exportar clientes

Ruta: `/usuario/clientes/exportar`. Se muestra el acceso **Importar clientes**; el inventario no detecta controles de exportacion en esta pantalla. Falta abrir la accion para identificar el flujo de importacion y localizar la exportacion.

### Directorio de clientes

Ruta: `/usuario/clientes/directorio`. Tabla de seguimiento con fecha de contacto, referido, comentario e inmueble. Los registros personales que aparecieron en el primer inventario fueron excluidos.

### Lista de clientes

Ruta: `/usuario/clientes`. Tabla con nombre, contacto, informacion, calificacion, preferencia de recepcion de correo y columnas de seleccion/acciones. No conservar valores de filas.

### Alta de cliente

Ruta: `/usuario/clientes/create`. Campos observados, sin valores:

- Asignacion y seguimiento: encargado, embudo y estado.
- Contacto: nombre, apellidos, correo, permiso para recibir informacion, telefonos y direccion.
- Perfil: fecha de nacimiento, identificacion, referido por y datos adicionales.
- Ubicacion: pais, departamento y ciudad.
- Demanda inmobiliaria: presupuesto minimo/maximo, area minima/maxima y detalles de la propiedad buscada.
- Tabla de inmuebles vinculados: inmueble y vinculo.

Los atributos HTML no marcan estos campos como requeridos; faltan validaciones y reglas de negocio. No capturar valores personales al seguir explorando.

### Embudo de negocios

Ruta: `/usuario/pipeline/1`. Se detecto la accion **Adjuntar Cliente**, pero no tablas ni formularios. Faltan captura visual de etapas, transiciones y acciones; no inferir un Kanban solo por la ruta.

### Gestiones realizadas

Ruta: `/usuario/crm`.

- Filtros: tipo de gestion, nombre del cliente y usuario encargado.
- Tabla: tipo de gestion, asunto y cliente, mas columnas de acciones/seleccion sin etiqueta.
- No se guardaron valores de campos ni filas.

### Cruce clientes - inmuebles

Ruta: `/administrador/cruces/clientes-propiedades`.

- Tabla principal: cliente, necesidad e inmuebles disponibles.
- Tabla de coincidencias: foto, codigo Wasi, inmueble y acciones.
- Parte de la demanda del cliente y presenta inventario compatible. El algoritmo y los criterios siguen pendientes.
- Los nombres visibles que aparecieron en el JSON fueron omitidos.

## Reportes

Submenu observado:

- General (`/administrador/reportes2`).
- Negocios realizados (`/administrador/reportes/negocios/negocios`).
- Usuarios (`/administrador/reportes2/agentes`).
- Propietarios (`/administrador/reportes2/propietarios`).
- Pagina web (`/administrador/reportes/estadisticas`).
- Correos enviados (`/administrador/reportes/correos`).

El reporte General presenta una tabla con tipo de inmueble y totales separados en venta, alquiler, traspaso y total. No se copiaron valores agregados de la cuenta. Falta revisar cada reporte y sus filtros/periodos.

### Negocios realizados

Ruta: `/administrador/reportes/negocios/negocios`. La tabla organiza cada negocio por inmueble, negocio, encargado y cliente. No se copiaron filas ni valores.

### Usuarios / agentes

Ruta: `/administrador/reportes2/agentes`. Tabla por usuario encargado con gestiones, ventas, alquileres, clientes, inmuebles y opciones. El encabezado anuncia ordenamiento ascendente en al menos una columna; faltan filtros y acciones por inspeccionar.

### Propietarios

Ruta: `/administrador/reportes2/propietarios`. Reporte por propietario con visitas en pagina, llamadas, correos, eventos, visitas, contactos recibidos y opciones. El JSON traia nombres de propietarios; no se guardaron.

### Pagina web

Ruta: `/administrador/reportes/estadisticas`. Tabla por titulo de inmueble con visitas y opciones. No se copiaron titulos ni valores de filas.

### Correos enviados

Ruta: `/administrador/reportes/correos`. Tabla con fecha, asunto, descripcion, usuario encargado y estado; una columna admite ordenamiento ascendente. No se copiaron filas ni contenido de correos.

## Usuarios

Submenu observado:

- Lista de usuarios (`/administrador/agentes`).
- Agregar usuario (el enlace apunta a la misma ruta; el alta puede abrirse como modal o con interaccion).

La lista contiene foto, datos generales, correo/alias Wasi, estado en el sistema y acciones. No guardar registros ni direcciones de correo.

## Inmuebles

Ruta: `/usuario/inmuebles`.

### Submenu

- Lista de inmuebles (`/usuario/inmuebles`).
- Nuevo inmueble (`/usuario/inmuebles/create`).
- Inmuebles de mis aliados (`/administrador/inmuebles/aliados`).
- Cruce inmuebles - clientes (`/administrador/cruces/propiedades-clientes`).
- Importar - exportar (`/usuario/inmuebles/exportar`).

### Lista

La tabla presenta foto, datos Wasi, estado, inmueble, fecha de publicacion, direccion, encargado, vinculo y estado de publicacion en portales. El inventario no guarda valores de filas.

### Brechas por revisar en detalle

- Campos y validaciones del alta/edicion de inmueble.
- Filtros, busqueda, orden y acciones de fila.
- Fuentes de datos y permisos de inmuebles de aliados.
- Reglas del cruce inmueble-cliente.
- Formatos y limites de importacion/exportacion.

#### Importar - exportar

Ruta: `/usuario/inmuebles/exportar`. La interfaz muestra el enlace **Importar Inmuebles** hacia `/usuario/inmuebles/importar`. El inventario no detecto formularios, tablas ni controles de exportacion; confirmar si la exportacion esta en otra pestana o se activa desde la lista.

### Formulario de inmueble nuevo

Ruta observada: `/inmueble-editar.htm`. El inventario recogio nombres de campos, nunca sus valores. El formulario agrupa:

- Control comercial: encargado, titulo, estado de publicacion, tipo de inmueble y tipo de negocio.
- Precios y costos: venta, alquiler, periodo, moneda, administracion y moneda de administracion.
- Caracteristicas: estado fisico, ano, alcobas, banos, garaje, estrato, piso, areas con unidades, balcon, pisos del edificio, disposicion y orientacion.
- Identificacion y medios: matricula inmobiliaria, video, carga de archivo/fotos.
- Ubicacion: pais, departamento, ciudad, zona, ubicacion para FincaRaiz, codigo postal y direccion privada.
- Mercadeo: caracteristicas internas/externas, portales, clubes, red inmobiliaria, Facebook e Instagram.
- Contacto asociado: nombre, apellidos, email, preferencia de correo y telefonos.
- Condiciones/comision: tipo de propiedad propia, porcentaje, nota, exclusividad y fecha de expiracion.

La mayoria de controles aparece con `required: false` en el HTML; no concluir que los campos sean opcionales sin probar validaciones al guardar. Falta revisar secciones, dependencias entre campos, catalogos, reglas de negocio y comportamiento de guardado.

### Inmuebles de mis aliados

Ruta: `/administrador/inmuebles/aliados`.

La tabla muestra foto, opcion para publicar el inmueble en mi pagina web, comision e inmueble. Sugiere un flujo de inventario compartido entre aliados con permiso de republicacion y acuerdo comercial; permisos, consentimiento y calculo de comision quedan por verificar.

### Cruce inmuebles - clientes

Ruta: `/administrador/cruces/propiedades-clientes`.

- Tabla principal: foto, inmueble y clientes posibles.
- Tabla de coincidencias: nombre, necesidad del cliente y acciones.
- La interfaz cruza inmuebles con demanda de clientes. No se ven criterios, puntaje, filtros ni automatizacion en el inventario; revisar visualmente antes de definir la regla de matching.
- No se conservaron nombres ni datos de clientes.

## Pendiente de explorar

La navegacion y estructura principal ya estan inventariadas. Falta inspeccion visual/interactiva para confirmar:

- Validaciones, dependencias y guardado de altas de inmuebles, clientes y usuarios.
- Etapas, transiciones y acciones del embudo de negocios.
- Crear, editar, asignar y reprogramar tareas/eventos en Agenda.
- Flujo real de importacion y exportacion de inmuebles y clientes.
- Filtros, periodos, orden y detalle de cada reporte.
- Contenido dinamico del reporte de mercado, que no expuso HTML tabular.
- Flujos de Sitio web, Configuracion y Mi Cuenta.
- Permisos, consentimiento y calculo de comision en inventario de aliados.
- Criterios y puntaje de los dos cruces cliente-inmueble.

## Comparacion inicial con Enlyce

Enlyce ya tiene panel, inmuebles, pipeline/oportunidades, contactos, calendario, analitica, alertas y configuracion. Wasi presenta Foro inmobiliario, Red inmobiliaria y configuracion inicial como areas visibles; falta explorar sus flujos antes de definir brechas. El Chat de Enlyce sigue siendo placeholder.

Referencias internas: `frontend/src/App.tsx`, `frontend/src/lib/constants.ts`, `docs/roadmap-crm-inmobiliario.md`.
Referencias del vault: [[Enlyce-MOC]], [[Ingenieria-Inversa-Web-LYC]].
Graphify: resumen versionado en `docs/graphify-wasi/GRAPH_REPORT.md`; subgrafo local en `docs/graphify-wasi/graphify-out/graph.json` (11 nodos, 12 relaciones) y grafo combinado local `graphify-out/graph-with-wasi.json` (2528 nodos, 5530 relaciones). La reextraccion detallada produjo 8 nodos frente a 11 anteriores; Graphify activo su proteccion contra reduccion y conservo el ultimo grafo completo. No se forzo la sustitucion.
