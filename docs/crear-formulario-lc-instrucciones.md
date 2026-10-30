# Crear el formulario de L&C

El formulario aun no esta creado en Google. El archivo `crear-formulario-lc.gs` contiene las 12 preguntas y crea el formulario y una hoja de respuestas al ejecutarlo desde tu cuenta.

1. Abre https://script.google.com/home/start y crea un proyecto.
2. Copia todo el contenido de `crear-formulario-lc.gs` en el editor, sustituyendo el codigo inicial.
3. Guarda y selecciona la funcion `createLcForm`.
4. Pulsa **Ejecutar** y revisa los permisos solicitados para crear el formulario y la hoja en tu cuenta. No requiere despliegue como aplicacion web.
5. En el registro de ejecucion, abre **EDITAR FORMULARIO** y revisa las preguntas y el acceso de los encuestados. Prueba **ENLACE PARA RESPONDER** en una ventana privada: las politicas de una cuenta corporativa pueden restringirlo.
6. Comparte solo **ENLACE PARA RESPONDER** con la propietaria. Conserva privados los enlaces de edicion y respuestas.

No recopila correos automaticamente ni publica el resumen de respuestas. No envia mensajes. Ejecutar de nuevo en el mismo proyecto devuelve los enlaces existentes; no crea otro formulario. Si una ejecucion falla a mitad, conserva el borrador y solicita revisarlo antes de continuar.

Mensaje sugerido:

> Para preparar la web de L&C, necesitamos confirmar el inventario, los permisos y las personas responsables. Te comparto este formulario corto. Si algun dato no esta definido, puedes responder «No se / Por definir». Con tus respuestas organizaremos los pendientes antes de publicar.

Referencia: https://developers.google.com/apps-script/reference/forms/form-app
