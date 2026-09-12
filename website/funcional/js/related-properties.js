export function buildRelatedPropertyHref(slug, search = '') {
  const current = new URLSearchParams(search);
  const target = new URLSearchParams({ slug });
  if (current.has('api')) target.set('api', current.get('api'));
  if (current.has('whatsapp')) target.set('whatsapp', current.get('whatsapp'));
  return `inmueble.html?${target}`;
}

export async function loadRelatedProperties({ client, property, section, grid, renderCard }) {
  section.hidden = true;
  grid.replaceChildren();

  const filters = {
    operation: property.operation,
    minPrice: Math.floor(property.price.amount * 0.7),
    maxPrice: Math.ceil(property.price.amount * 1.3),
    pageSize: 4,
  };
  const currentSlug = property.slug.trim().toLowerCase();
  const related = new Map();

  function collect(items) {
    for (const item of items) {
      const key = item.slug.trim().toLowerCase();
      if (key !== currentSlug && !related.has(key)) related.set(key, item);
      if (related.size === 3) break;
    }
  }

  const nearby = await client.getProperties({ ...filters, neighborhood: property.location.neighborhood });
  collect(nearby.items);

  if (related.size < 3) {
    const municipal = await client.getProperties({ ...filters, municipality: property.location.municipality });
    collect(municipal.items);
  }

  if (related.size === 0) return;
  grid.replaceChildren(...Array.from(related.values(), renderCard));
  section.hidden = false;
}
