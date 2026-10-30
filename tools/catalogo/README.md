# Preparacion privada del catalogo

## Ejecutar

Con Docker iniciado y el contenedor local disponible:

```powershell
python tools/catalogo/preparar_inventario.py

# Consulta de solo lectura con las credenciales WASI de User Secrets.
python tools/catalogo/preparar_inventario.py --source wasi --output .local/catalogo-revision/wasi
```

La fuente local usa `docker exec` y `psql` dentro de una transaccion PostgreSQL `REPEATABLE READ READ ONLY`. La fuente WASI lee CompanyId/Token de User Secrets en memoria y consulta `property/search` con `scope=1` (propios), paginacion y comprobacion de totalidad e identidad. No guarda respuestas crudas, credenciales, propietarios ni direcciones exactas. No importa inmuebles, no aplica migraciones, no cambia estados y no publica. Un error de lectura interrumpe el proceso.

Los resultados quedan fuera de Git en `.local/catalogo-revision/`:

- `revision-inventario.html`: fichas privadas con hallazgos, borrador de contenido y pendientes.
- `inventario-revision.json`: corte estructurado para continuar la revision.

El HTML es un archivo local; `noindex` no es control de acceso. No alojarlo en la web publica ni subirlo a un directorio de assets. No exporta campos de direccion exacta ni identidad/contacto de propietarios. El texto de origen requiere revision para detectar datos personales incluidos en el. El contenido comercial sigue siendo privado.

La deteccion de posibles duplicados compara direccion normalizada, ciudad, tipo y modalidad; es una senal, no una orden de fusionar. No detecta todas las variantes de una direccion. Las referencias de fotos se cuentan, pero no se descargan ni se certifican su calidad, derechos o correspondencia con el inmueble. El script lee fotos de `PropertyPhotos`; material interno adicional debe revisarse en el CRM y no se considera material aprobado automaticamente.

## Resultado comprobado el 2026-10-30

Fuente: `enlyce-db/enlyce`, base local. Hay 12 inmuebles disponibles, 12 publicaciones con estado `Published` y 36 referencias de fotos. Los 12 pertenecen al catalogo sintetico de `src/Enlyce.Api/Development/DemoCatalogSeeder.cs`, identificado por las cuentas de demostracion del propietario/asesor. El seeder reutiliza dos assets en tres posiciones por ficha. Eso no acredita fotos reales del inmueble.

Resultado: 12 de demostracion, 0 candidatos reales, 0 autorizados por la revision. No se extrapola este resultado a WASI ni a otra base. No se encontro inventario comercial real en la base consultada.

Se encontro acceso WASI configurado y se consulto esa fuente por separado: **35 inmuebles**, todos con disponibilidad declarada, **619 referencias de fotos** (7 a 37 por ficha), **2 registros con posible coincidencia de direccion/tipo/ciudad**. No se fusionaron. Hay datos para preparar un lote inicial de 10 para revision. Falta confirmar con L&C que la cuenta corresponde al inventario oficial, identificar propietario/representante y asesor activo en el CRM, y comprobar precio, ubicacion, fotos y autorizacion. No se toma el campo de visibilidad de WASI como consentimiento. La existencia de datos de propietario/usuario en WASI tampoco demuestra su identidad, facultad o vigencia; no se exportan esos datos personales.

El informe WASI incorpora enlaces HTTPS al CDN de WASI para revisar el material de origen, sin descargarlo ni publicarlo. Los primeros 10 se ordenan por menor numero de pendientes tecnicos; no equivalen a 10 aprobados. Las ofertas de venta y arriendo se separan si un mismo inmueble tiene ambas modalidades.

## Cierre temporal de lectura publica

`PublicPropertyRepository` devuelve lista vacia y detalle/enlace nulos mientras `PublicCatalog:Enabled` no sea explicitamente `true`. Por defecto queda cerrado, tambien para otros consumidores del repositorio.

El interruptor es una contencion temporal, NO un flujo de autorizacion por inmueble. **No habilitarlo para la entrega comercial hasta implementar y comprobar el registro de autorizaciones, aprobacion por version, caducidad y revocacion.** La aprobacion de la regla general en Google Forms tampoco autoriza automaticamente cada ficha. No se ha modificado configuracion ni desplegado nada en Railway.

Incluso con el interruptor activo para pruebas:

- Solo se leen publicaciones `Published`, con campos publicos y fotos minimos, un asesor activo y un inmueble activo, visible y disponible.
- Se excluyen las cuentas conocidas del seeder de demostracion.
- No existe el acceso alternativo mediante slugs `crm-{id}` ni retorno a fotos internas.
- Filtros y orden por precio y ubicacion usan los valores publicos.

## Pendientes de L&C y siguiente lote

1. Obtener acceso o exportacion del inventario real; no volver a importar el seeder como inventario comercial.
2. Repetir la revision contra esa fuente y resolver coincidencias/duplicados con una persona responsable.
3. Confirmar precio, disponibilidad y ubicacion; preparar fotos y contenido reales.
4. Registrar evidencias de permiso y aprobacion de la version de cada ficha, con responsable y vigencia.
5. Completar el flujo de aprobacion, verificar bajas y cachés y solo entonces habilitar el catalogo comercial.
