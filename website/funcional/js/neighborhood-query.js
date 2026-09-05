export function buildNeighborhoodFilters(neighborhood, municipality = 'Medellín') {
  const normalizedNeighborhood = neighborhood?.trim();
  const normalizedMunicipality = municipality?.trim();
  if (!normalizedNeighborhood || !normalizedMunicipality) {
    throw new TypeError('Barrio y municipio son obligatorios.');
  }

  return {
    page: 1,
    pageSize: 12,
    sort: 'publishedAtDesc',
    municipality: normalizedMunicipality,
    neighborhood: normalizedNeighborhood,
  };
}
