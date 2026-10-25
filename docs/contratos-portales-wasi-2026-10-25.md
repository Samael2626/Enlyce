# Portales Wasi: configuracion observada y contratos pendientes

Sesión L&C del 25/10/2026, `Sitio web > Conexión portales` (`/administrador/portal/configurar`). Inspección de lectura; no se modificaron conexiones, inmuebles ni credenciales. No se copiaron valores secretos a Git.

## Estado observado

- La pantalla separa portales y portales gratuitos, con columnas `Portal`, `Estado`, `Listados`, `Destacados`, `Fallos` y `Contactos`. Los contadores son operativos y pueden cambiar; no se usan como reglas de negocio.
- `Fincaraiz` y `Metrocuadrado` figuraban conectados. La configuración de Fincaraiz mostró `ID cliente`, `Guardar`, `Publicar inmuebles`, `Lista de inmuebles` y datos de agentes. La de Metrocuadrado mostró `Usuario`, `Clave` y lista de inmuebles. Solo se observaron controles, no una publicación ni sincronización exitosa.
- En la sección de portales gratuitos figuraban conectados `Los Compradores`, `Luxury Estate`, `Doomos`, `Clasf` y `Arriendo.com`. También había portales desconectados. El estado conectado en Wasi no demuestra que L&C necesite un conector propio para cada uno.

## Decisiones y material externo requerido

| Portal o grupo | Material necesario para Enlyce | Decisión L&C |
|---|---|---|
| Fincaraiz | Contrato/API directa o canal autorizado, entorno y credencial de prueba propios, catálogo de campos, límites, estados, errores, baja y conciliación de anuncios. | Confirmar si se mantiene publicación directa y quién posee el contrato e identificador. |
| Metrocuadrado | Contrato/API y credenciales de prueba propias, seguridad de secretos, mapeo de alta/cambio/baja y conciliación. | Confirmar necesidad y titularidad del acceso. |
| Portales gratuitos conectados | Términos y método admitido por cada portal, periodicidad, retirada de anuncios y derechos sobre fotos/textos. | Elegir cuáles usa realmente L&C; excluir los demás por decisión explícita. |

No reutilizar automáticamente claves de Wasi ni asumir que su conexión concede acceso API independiente. Antes de implementar: obtener sandbox/contrato, observar publicación y fallo reales sin alterar anuncios, definir propietario de datos, reintentos e idempotencia, y aprobar el alcance por portal.
