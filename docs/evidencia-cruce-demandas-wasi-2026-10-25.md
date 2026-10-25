# Cruce de demandas Wasi: observacion autenticada del 25/10/2026

Sesión L&C, ruta `https://wasi.co/administrador/cruces/clientes-propiedades`. Navegación de lectura; no se guardó, borró ni importó ninguna demanda. Los resultados de clientes e inmuebles no se copiaron a Git.

## Flujo y resultado observados

- La búsqueda avanzada muestra nombre de cliente, tipo de cliente, estado, usuario encargado y rangos de fecha de registro del cliente y de la demanda.
- La tabla `Clientes con demanda de inmueble` mostró tres filas en esta sesión. Cada fila ofrece `Modificar`, `Borrar` y contadores separados `Propios`, `Aliados` y `En Red`. Esto acredita la existencia de los controles, no sus permisos ni el efecto de guardar o borrar.
- En una fila los contadores fueron 0 propios, 0 aliados y 1 en red. Pulsar `En Red 1` abrió una tabla con un inmueble y mostró `Acierto: 56.6%`. El menú de ese inmueble ofreció `Solicitar alianza`, `Detalles` y `Contactar`. Abrir `Detalles` mostró una ficha con datos del inmueble y secciones de clientes enlazados, posibles interesados y notas. No se accionó alianza ni contacto.
- `Modificar` abrió el formulario de demanda con tipo de inmueble, tipo de negocio, país, departamento, ciudad, localidad, zona/barrio, presupuesto mínimo/máximo, moneda, área mínima/máxima, alcobas, baños, garajes, características internas/externas y detalles. Los selectores de tipo de inmueble y tipo de negocio admiten selección múltiple en el DOM. No se guardó ni se probó validación de error.

## Contraste con Enlyce

- `CustomerDemand` y su API guardan operación y tipo de inmueble únicos, ubicación, rangos de precio y área, mínimos de habitaciones/baños/parqueaderos, características y notas. `Matches` devuelve un booleano para inventario propio disponible. La lista de cruce de Enlyce solo cuenta coincidencias propias; no expone porcentaje ni inventario aliado/en red.
- El `56.6%` es un resultado observado, no una fórmula deducible. Faltan varios casos controlados y la regla de Wasi antes de implementar un porcentaje equivalente.
- Para aliados y red faltan convenio, procedencia, permisos y decisión de alcance de L&C. No se debe fabricar inventario externo ni activar `Solicitar alianza` sin esos contratos.
- Siguen sin verificar permisos por rol, errores, guardado, borrado, porcentajes límite y transiciones de inmuebles relacionados. Esta observación no cierra la casilla de demandas.
