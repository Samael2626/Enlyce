# Blog y banners Wasi: observacion autenticada del 25/10/2026

Sesión L&C. Solo se abrieron listas y formularios; no se guardó ni publicó contenido. No se copiaron imágenes ni datos de terceros a Git.

## Blog

- `Sitio web > Blog` (`/administrador/novedades`) mostró tabla con orden, título, fecha y acciones. La lista estaba vacía en esta sesión.
- `Agregar blog` abrió un formulario con título, imagen, resumen introductorio y noticia completa en editor enriquecido, más `Guardar` y `Cancelar`.
- No se vieron controles separados de borrador, fecha de publicación, slug ni SEO en ese formulario. Esto describe la pantalla observada, no el resultado de guardar ni una regla de publicación.

## Banners

- `Sitio web > Banners` (`/administrador/banners`) mostró columnas orden, título, estado y vínculo. La lista estaba vacía en esta sesión.
- `Agregar banner` abrió imagen, título, vínculo y estado `Activo`/`Inactivo`, con `Guardar`. No se guardó ni comprobó el destino del vínculo.

## Contraste y límites

- Enlyce ya tiene API y editor de blog con título, resumen, cuerpo, portada, slug, SEO y estado borrador/publicado; la configuración pública guarda una URL de banner principal. Falta probar los flujos en el entorno objetivo y decidir si L&C requiere múltiples banners con orden y estado como en la lista Wasi.
- Sin contenido existente ni acción de guardar, no se verificaron publicación, visualización pública, permisos, validaciones, ordenamiento, edición, borrado ni errores. Las migraciones de blog/CMS siguen pendientes de producción.
