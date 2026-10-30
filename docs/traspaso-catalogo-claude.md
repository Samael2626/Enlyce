# Traspaso para Claude: catalogo comercial de L&C

Fecha: 2026-10-30. Este documento permite continuar desde GitHub sin acceso a esta conversacion.

## Objetivo del usuario

Preparar una web comercial util con el inventario existente, sin publicar automaticamente lo que exista en CRM o WASI. La propietaria de L&C debe confirmar datos y decisiones que el desarrollador desconoce. Ya se preparo un checklist HTML y un generador de Google Forms de 12 preguntas; el usuario ejecuto el generador y confirmo la creacion del formulario y la hoja de respuestas. Sus enlaces y respuestas no estan en este repositorio.

## Regla de negocio

Un inmueble requiere disponibilidad confirmada, precio y condiciones correctos, ubicacion publica revisada, titulo y descripcion fieles, fotos suficientes y con permiso de uso, propietario o representante identificado, asesor responsable y autorizacion verificable para la web. Tambien requiere aprobacion final de L&C. Un registro en WASI, un campo de visibilidad o un estado `Published` no acreditan esos permisos.

Estados de preparacion propuestos: LISTO, NO LISTO, REVISAR FOTOS, REVISAR PRECIO, REVISAR LEGAL, DUPLICADO e INACTIVO. Conservar todos los bloqueos, aunque se muestre un estado principal. Una coincidencia automatica solo genera sospecha de duplicado; no autoriza fusionar registros.

LISTO, APROBADO y PUBLICADO son conceptos distintos. Los minimos de fotos y los plazos de reconfirmacion definitivos siguen pendientes de decision de L&C. El control tecnico actual usa 3 referencias de fotos; contar referencias no acredita calidad, cobertura ni derechos.

## Hallazgos comprobados

| Fuente consultada | Resultado | Limite de la evidencia |
|---|---|---|
| PostgreSQL local `enlyce-db/enlyce` | 12 inmuebles, 12 publicaciones `Published`, 36 referencias de fotos | Los 12 provienen de `DemoCatalogSeeder`; son demostraciones, no oferta comercial real |
| WASI, credenciales locales configuradas, consulta `property/search` con `scope=1` | 35 inmuebles, todos con disponibilidad declarada; 619 referencias de fotos, entre 7 y 37 por ficha | Falta confirmar con L&C que esta cuenta es su inventario oficial y reconfirmar los datos |
| Revision de coincidencias WASI | 2 registros con posible coincidencia de direccion, ciudad y tipo | No se confirmo duplicidad ni se fusionaron registros |
| Lote provisional | 10 inmuebles priorizados para completar y revisar | Orden por menor numero de pendientes tecnicos, no por prioridad comercial aprobada |

Ningun inmueble quedo autorizado por esta auditoria. Las fotos no se descargaron ni se certificaron como material valido. No se importaron registros, no se aplicaron migraciones y no se cambiaron estados en las fuentes. No se desplego la web ni la API.

Los informes completos se generaron localmente en `.local/catalogo-revision/` y `.local/catalogo-revision/wasi/`. Son privados y estan excluidos de Git. No subirlos a este repositorio publico. Tampoco copiar aqui credenciales, soportes legales, datos de propietarios ni respuestas del formulario.

## Archivos para continuar

- [Checklist para la propietaria](checklist-propietaria-lc.html): documento independiente, editable en navegador y exportable con sus respuestas.
- [Generador de Google Forms](crear-formulario-lc.gs) y [pasos de ejecucion](crear-formulario-lc-instrucciones.md).
- [Herramienta y limites de la revision](../tools/catalogo/README.md).
- [Script de revision privada](../tools/catalogo/preparar_inventario.py): lectura local o WASI; genera HTML y JSON privados.
- [Repositorio de lectura publica](../src/Enlyce.Infrastructure/Persistence/Repositories/PublicPropertyRepository.cs).
- [Pruebas del repositorio publico](../tests/Enlyce.IntegrationTests/PublicPropertyRepositoryTests.cs) y [pruebas HTTP](../tests/Enlyce.IntegrationTests/PublicCatalogEndpointTests.cs).

## Comportamiento implementado y limite critico

El catalogo devuelve lista vacia y detalle/enlace nulos si `PublicCatalog:Enabled` no es explicitamente `true`. Es un cierre temporal y deliberado. **No es un flujo de aprobacion por inmueble y no debe habilitarse para entrega comercial antes de implementarlo.**

Con la lectura habilitada para pruebas, se exigen publicaciones `Published`, datos publicos minimos, fotos/portada y asesor activo, ademas de inmueble activo y disponible. Se excluyen las cuentas conocidas del seeder. Se elimino la salida alternativa por `crm-{id}` y el retorno a fotos internas. Filtros y orden usan precio y ubicacion publicos.

La configuracion de pruebas activa expresamente la lectura para comprobar esos filtros. Eso no implica habilitacion comercial. Los cambios se integraron conservando la infraestructura remota de pruebas SQLite y los modulos remotos existentes.

## Siguiente trabajo recomendado

1. Revisar las respuestas de la propietaria y confirmar la cuenta WASI oficial y los responsables. No inventar los datos faltantes.
2. Definir y persistir autorizacion: otorgante y facultad, canal permitido, evidencia privada, vigencia y revocacion. La aprobacion interna del asesor no reemplaza la facultad del propietario o representante.
3. Implementar validacion y aprobacion de una version concreta. Cambiar precio, condiciones, ubicacion o fotos debe invalidar la aprobacion afectada. Resolver concurrencia y mantener auditoria.
4. Preparar borradores privados desde la fuente confirmada; revisar las dos coincidencias y el primer lote. No publicar por importacion ni reutilizar el seeder como inventario real.
5. Comprobar retiro en listado, detalle, busqueda, mapas, sitemap y caches. Solo despues evaluar habilitacion comercial y despliegue.

No reintroducir el retorno al CRM para llenar la web mientras faltan permisos. No habilitar el interruptor solo para que aparezcan fichas.

## Verificacion previa a la integracion remota

- Compilacion de la solucion correcta.
- Suite de trabajo local: 620 pruebas correctas y 3 omitidas.
- 27 pruebas del catalogo correctas en una copia aislada del commit.
- Informes abiertos en Chromium, con 35 fichas WASI y 619 enlaces a fotos; sin desborde en movil.
- Persisten advertencias preexistentes de dependencias y nulabilidad; no equivalen a una certificacion de seguridad para produccion.

Para volver a ejecutar la revision WASI se necesitan las credenciales en User Secrets del entorno local; no estan disponibles desde GitHub. Se pueden continuar las reglas y pruebas sin esas credenciales.

La integracion con el remoto conservo sus modulos de chat, mercado y CMS. Se comprobaron 78 pruebas de aplicacion y 256 de dominio. En integracion pasaron 296 pruebas y quedo 1 omitida; se corrigieron dos desajustes de pruebas de la fusion (campo duplicado en el contrato y ruta publica del CMS ausente en la lista esperada). Se reejecutaron las 28 pruebas afectadas del catalogo/autorizacion: 28 correctas, 0 fallos. No se modificaron permisos de rutas para hacer pasar la prueba.

## Peticion sugerida para Claude

> Lee este traspaso y los archivos enlazados. Revisa la barrera temporal de publicacion y propone el cambio minimo para registrar autorizacion verificable y aprobacion por version, con auditoria, revocacion y pruebas negativas. Conserva el inventario interno separado del catalogo publico. No habilites publicacion ni despliegues. Identifica que decisiones dependen de L&C y cuales se pueden implementar ya.
