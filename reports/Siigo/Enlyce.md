# Siigo dentro de Enlyce

Consulta: 9 oct 2026. Base: tres notas de investigación; fuentes enlazadas son oficiales de Siigo, DIAN o el código del repo. **Fuentes** describen lo verificado; **inferencia** marca recomendación o lectura. Confirmar normativa y anexos DIAN vigentes antes de implementar.

## Conclusión

**Fuentes:** Enlyce tiene CRM, inventario comercial y cobro de su propia suscripción; no tiene contabilidad ni administración integral de arriendos. DIAN permite facturar con software propio, servicio gratuito DIAN o proveedor tecnológico. Hacer software propio para el NIT de la inmobiliaria no habilita automáticamente a Enlyce para vender facturación a otras empresas.

**Inferencia:** construir capacidades propias por etapas, sin copiar toda la suite Siigo. Prioridad inmobiliaria: contrato, canon, recaudo, cartera, gastos y liquidación al propietario; después contabilidad y factura electrónica. Eliminar SaaS no elimina el costo de desarrollo, operación ni cumplimiento.

## Capacidades Siigo

**Fuentes:** [Siigo Nube y sus planes](https://www.siigo.com/software-administrativo/) y [precios/productos](https://www.siigo.com/precios-siigo/) muestran una suite empaquetada por plan y complementos. Sus capacidades documentadas cubren:

- Contabilidad, comprobantes e informes por cuenta, tercero y periodo.
- Facturas de venta, notas crédito/débito y documentos electrónicos.
- Cartera, vencimientos, seguimiento y recaudos; Siigo Pay puede cobrar tarifas.
- Compras, gastos, proveedores, pagos y cuentas por pagar.
- Bancos y conciliación; inventario y listas de precios.
- Nómina electrónica y POS como productos/planes aparte.

**Inferencia:** Siigo es administración/contabilidad general; el catálogo consultado no muestra como núcleo contratos de arriendo, liquidación periódica al propietario ni operación inmobiliaria. No prueba que ningún complemento o aliado lo haga.

## Requisitos DIAN y límite propio vs proveedor

**Fuentes:** la [DIAN explica las vías de habilitación](https://www.dian.gov.co/impuestos/factura-electronica/como-hacerlo/Paginas/ser-facturador-electronico.aspx); la [Resolución 000165 de 2023, compilada/modificada por la 000227 de 2025](https://normograma.dian.gov.co/dian/compilacion/docs/resolucion_dian_0165_2023.htm) y el [Concepto DIAN 13246 de 2025](https://normograma.dian.gov.co/dian/compilacion/docs/oficio_dian_13246_2025.htm) describen registro, pruebas de habilitación, requisitos técnicos, validación y transmisión. Para producción también se requiere numeración y asociación de prefijos. El desarrollo propio sirve para facturar el propio NIT; operar el servicio para terceros exige autorización DIAN separada como proveedor tecnológico, con requisitos técnicos, empresariales y patrimoniales.

La [solución gratuita DIAN](https://www.dian.gov.co/impuestos/factura-electronica/facturacion-gratuita/paginas/default.aspx) evita licencia, pero sus términos no ofrecen integración embebida desde otra aplicación. Nómina electrónica tiene habilitación separada; aplica solo si el negocio la necesita y debe seguir reglas y plazos propios ([DIAN: nómina electrónica](https://www.dian.gov.co/impuestos/Paginas/Sistema-de-Factura-Electronica/Documento-Soporte-de-Pago-de-Nomina-Electronica.aspx)).

**Inferencia:** Enlyce puede desarrollar y operar facturación para cada inmobiliaria bajo su propio proceso de habilitación, tras revisión profesional. No debe ofrecerse como facturador de terceros sin resolver primero autorización y requisitos de proveedor tecnológico.

## Brecha Enlyce

**Fuentes:** el código contiene leads, contactos, propiedades comerciales, tareas, visitas, pipeline y checkout de la suscripción Enlyce. No aparecen facturas/recibos, impuestos, cartera, cuentas por pagar, pagos de arriendo, conciliación, asientos, plan de cuentas, contratos ni liquidaciones de propietario. `Arrendado` es un estado comercial, no gestión de arriendo.

**Inferencia:** falta casi todo el dominio financiero y de administración inmobiliaria. Aislarlo por empresa/tenant y permisos antes de almacenar saldos.

## Fases recomendadas

1. **Cerrar CRM comercial** y definir operación objetivo: venta, arriendo o ambas.
2. **Operación de arriendo:** contratos, obligaciones recurrentes, cánones, vencimientos, pagos parciales/anticipos, mora, reversos, soportes y liquidación al propietario.
3. **Administración financiera:** terceros, facturas/cuentas por cobrar y pagar, recaudos, gastos, categorías, consecutivos, auditoría y reportes de cartera. Sin editar saldos manualmente: cada cambio debe tener documento y trazabilidad.
4. **Contabilidad:** plan de cuentas, comprobantes balanceados de partida doble, periodos, cierres y estados financieros; revisión de contador antes de uso real.
5. **DIAN:** especificación normativa vigente, registro y pruebas del software, firma, numeración, transmisión, recepción y manejo de rechazos/contingencia. Nómina solo si hay necesidad definida.
6. **Después:** importación/conciliación bancaria. POS e inventario físico quedan fuera salvo caso de negocio concreto.

## Riesgos y costos sin SaaS

**Fuentes:** DIAN permite software propio, pero exige habilitación y pruebas; la validación previa no elimina responsabilidad del emisor. La DIAN gratuita no es una API integrada. Siigo Pay cobra según condiciones; usarlo contradice el objetivo de no pagar proveedor externo.

**Inferencia:** sin SaaS se ahorra licencia, pero Enlyce asume construcción, hosting, conectividad, soporte, seguridad, protección de datos, cambios de anexos, monitoreo de rechazos, continuidad, archivo legal y formación contable. También puede haber costos de firma digital; la gratuidad anunciada para certificado aplica a usuarios de la solución DIAN gratuita, no debe suponerse para software propio. Riesgos mayores: errores tributarios, duplicados, pérdida de trazabilidad y cálculos de cartera/liquidaciones incorrectos.

**Decisión sugerida:** no prometer “reemplazo de Siigo” aún. Primero delimitar qué proceso inmobiliario y qué obligaciones legales tendrá la primera versión; luego construir y validar cada fase antes de migrar operación real.
