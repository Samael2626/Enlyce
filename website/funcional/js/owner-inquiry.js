const ownerServices = new Map([
  ['Vender', { operationType: 'Venta', ownerService: 'Sell' }],
  ['Arrendar', { operationType: 'Arriendo', ownerService: 'Rent' }],
  ['Administrar', { operationType: 'Arriendo', ownerService: 'Manage' }],
]);

function cleanSourcePart(value) {
  return String(value || '').replace(/[:|]/g, ' ').replace(/\s+/g, ' ').trim();
}

export function buildOwnerLeadPayload(input) {
  const selectedService = cleanSourcePart(input.ownerService);
  const service = ownerServices.get(selectedService);
  if (!service) throw new Error('El servicio no válido no puede enviarse.');

  const source = [
    'WebsiteOwner',
    input.propertyType,
    input.municipality,
    input.neighborhood,
  ].map(cleanSourcePart).filter(Boolean).join(':');

  return {
    nombre: String(input.name || '').trim(),
    email: String(input.email || '').trim(),
    telefono: String(input.phone || '').trim(),
    fuente: source.slice(0, 100),
    autorizacionDatos: Boolean(input.consent),
    tipoOperacion: service.operationType,
    ownerService: service.ownerService,
  };
}
