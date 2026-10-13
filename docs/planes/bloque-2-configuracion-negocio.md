# Bloque 2 — Configuracion del negocio

Amplia la `SettingsPage` actual (ya tiene reparto de leads y SLA) con mas
configuracion administrativa. Reusa el patron de settings tipo
`LeadDistributionModule` / `LeadSlaModule` y el RBAC `Administrador`.

## 2.1 Catalogos editables
Gestionar fuentes/UTM, etapas del pipeline, tipos de inmueble, zonas, motivos de descarte (hoy strings dispersos).
- **Domain:** entidad generica `CatalogItem` (tipo, codigo, etiqueta, orden, activo) con `CatalogType` enum. Si un catalogo tiene reglas propias, entidad dedicada.
- **Infrastructure:** tabla `CatalogItems` + seed inicial (migracion `HasData`) con los valores actuales para no romper datos existentes.
- **Api:** `GET/POST/PUT/PATCH /api/configuracion/catalogos/{tipo}` (admin). Baja logica.
- **UI:** panel por catalogo dentro de `SettingsPage`.
- **Tests:** no duplicar codigo activo; desactivar no rompe registros que lo referencian.

## 2.2 Plantillas (correo / WhatsApp / notas)
Plantillas con variables (`{{nombre}}`, `{{inmueble}}`).
- **Domain:** entidad `MessageTemplate` (canal, clave, asunto, cuerpo, activo, version).
- **Infrastructure:** tabla + config; el servicio de correo existente las consume.
- **Api:** CRUD `/api/configuracion/plantillas` (admin) + endpoint de previsualizacion que resuelve variables.
- **UI:** editor en `SettingsPage` con vista previa.
- **Tests:** render de variables; plantilla faltante da fallback ruidoso.

## 2.3 Parametros de marca y empresa
Nombre, logo, colores, datos fiscales, `PublicSite:BaseUrl` editable.
- **Domain/Infra:** `OrganizationSettings` (singleton tipo settings, como reparto/SLA).
- **Api:** `GET/PUT /api/configuracion/organizacion` (admin). **UI:** panel en Settings. Correos y fichas leen de aqui.

## 2.4 Horarios y festivos (alimenta SLA)
Jornada laboral + festivos colombianos; el calculo de SLA los descuenta.
- **Domain:** `BusinessCalendar` (horarios por dia, lista de festivos). Integrar en el calculo de `LeadSla`.
- **Api:** `/api/configuracion/calendario` (admin). **Tests:** SLA no corre en festivo ni fuera de horario.

## Migraciones EF (orden)
1. `AddCatalogItems` + seed
2. `AddMessageTemplates`
3. `AddOrganizationSettings`
4. `AddBusinessCalendar`

## Fase
- **Fase C:** 2.3 organizacion, 2.1 catalogos, 2.2 plantillas, 2.4 calendario.

## Riesgos
- Seed de catalogos: migrar los strings actuales sin romper leads/inmuebles existentes (mapa de equivalencias).
