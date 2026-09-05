const serviceOperations = new Map([
  ['Vender', 'Venta'],
  ['Arrendar', 'Arriendo'],
  ['Administrar', 'Arriendo'],
]);

function cleanSourcePart(value) {
  return String(value || '').replace(/[:|]/g, ' ').replace(/\s+/g, ' ').trim();
}

export function buildOwnerLeadPayload(input) {
  const service = cleanSourcePart(input.service);
  const operation = serviceOperations.get(service);
  if (!operation) throw new Error('El servicio no válido no puede enviarse.');

  const source = [
    'WebsiteOwner',
    service,
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
    tipoOperacion: operation,
  };
}
