# Auditoria codigo Enlyce: Siigo y contabilidad

Fecha: 2026-10-09. Alcance: codigo actual, contrastado con `graphify-out/graph.json` y archivos fuente. No se modifico codigo ni roadmap.

## Ya existe

- **Cobro de la suscripcion SaaS de Enlyce:** `Domain/Billing` define precios y seleccion del plan. `PaymentOrder` guarda empresa, NIT, correo, conceptos del plan, valor COP y estado. API `/api/billing` crea checkout/consulta orden; webhook Wompi procesa notificaciones. No confundir con contabilidad para las inmobiliarias clientes.
- **Inmuebles:** inventario comercial con propietario, modalidad venta/arriendo, precio, estado, caracteristicas, fotos/publicacion. Puede marcarse vendido o arrendado. No administra inmuebles en operacion de arriendo.
- **Personas y ventas:** `Contact`, `Lead`, `Propietario`, `Asesor`, `CustomerDemand`, interacciones/tareas/visitas e historial. Usuarios son asesores con roles Asesor/Administrador.
- **Pipeline:** contempla etapas como bajo contrato/cierre, pero son estados comerciales, no contratos ni obligaciones juridicas.

## No aparece en el modelo actual

- Facturas/recibos de clientes, notas credito, impuestos, numeracion fiscal, cartera, cuentas por cobrar/pagar, pagos de arriendo o conciliacion bancaria.
- Libro diario, plan de cuentas, asientos, periodos, balances, estados financieros o centros de costo.
- Contrato de arrendamiento, canon/administracion, deposito, fechas/vencimientos, renovacion, liquidacion a propietario, mora y comprobantes.
- Comisiones de venta/arriendo modeladas como liquidaciones contabilizables.
- Contabilidad de la inmobiliaria cliente separada por empresa, sucursal y periodo.

## Brecha y orden sugerido

1. Definir alcance: primero cartera y operacion de arriendos o contabilidad general completa. No prometer reemplazo total de Siigo sin requisitos contables/fiscales y validacion profesional.
2. Crear limites multiempresa/tenant y permisos contables antes de guardar saldos financieros.
3. Modelar contratos y obligaciones recurrentes; luego recibos, pagos, cartera, conciliacion y reportes.
4. Si el alcance incluye contabilidad: plan de cuentas, comprobantes y asientos de partida doble inmutables, periodos/cierres y reportes verificables.
5. Facturacion electronica colombiana es frente aparte: numeracion/autorizacion, impuestos, documentos electronicos y requisitos DIAN; requiere investigacion normativa vigente antes de diseno.

**Conclusion:** Enlyce tiene CRM inicial y checkout para cobrar su SaaS. No tiene sistema contable ni gestion integral de arriendos para clientes. `Propietario` e `Inmueble.Estado=Arrendado` no cubren ese proceso.

## Referencias del repo

- Graphify: `graphify-out/graph.json` (nodos `Enlyce.Domain.Billing`, `PaymentOrder`, `BillingPage.tsx`, `Inmueble`, `Contact`, `CustomerDemand`, `PropertyPublication`).
- `src/Enlyce.Domain/Billing/SaasPricing.cs`, `src/Enlyce.Domain/Entities/PaymentOrder.cs`.
- `src/Enlyce.Api/Endpoints/Billing/BillingModule.cs`.
- `src/Enlyce.Domain/Entities/{Inmueble,Propietario,Contact,Lead,Asesor,CustomerDemand}.cs`.
- `src/Enlyce.Infrastructure/Persistence/EnlyceDbContext.cs` y `Configurations/PaymentOrderConfiguration.cs`.
