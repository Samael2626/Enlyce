import { ApiError, PublicCatalogClient, buildVisitLeadPayload, resolveApiBase } from './api.js';
import { buildWhatsAppUrl, resolveWhatsAppNumber } from './contact.js';
import { formatCurrency, formatLocation, safeMediaUrl } from './format.js';
import { FavoritesStore, FAVORITES_STORAGE_KEY, updateFavoriteButton, updateFavoriteCounts } from './favorites.js';
import { siteSettings } from './settings.js';

const params = new URLSearchParams(window.location.search);
const slug = params.get('slug')?.trim();
const client = new PublicCatalogClient(resolveApiBase(window.location.search));
const detail = document.querySelector('[data-detail]');
const loading = document.querySelector('[data-detail-loading]');
const errorPanel = document.querySelector('[data-detail-error]');
const leadForm = document.querySelector('[data-lead-form]');
const whatsappNumber = resolveWhatsAppNumber(window.location.search, siteSettings.whatsappNumber);
const favorites = new FavoritesStore();
let property;

function setText(selector, value) {
  document.querySelector(selector).textContent = value ?? '';
}

function renderGallery(photos) {
  const gallery = document.querySelector('[data-gallery]');
  const images = photos
    .map((photo) => ({ ...photo, safeUrl: safeMediaUrl(photo.url) }))
    .filter((photo) => photo.safeUrl)
    .slice(0, 5)
    .map((photo, index) => {
      const figure = document.createElement('figure');
      if (index === 0) figure.className = 'gallery-main';
      const image = document.createElement('img');
      image.src = photo.safeUrl;
      image.alt = photo.altText || property.publicTitle;
      image.loading = index === 0 ? 'eager' : 'lazy';
      figure.append(image);
      return figure;
    });

  if (images.length) gallery.replaceChildren(...images);
  else {
    const fallback = document.createElement('div');
    fallback.className = 'gallery-empty';
    fallback.textContent = 'Fotografías pendientes';
    gallery.replaceChildren(fallback);
  }
}

function renderProperty() {
  document.title = `${property.publicTitle} | L&C Propiedad Raíz`;
  document.querySelector('meta[name="description"]').content = property.publicDescription.slice(0, 155);
  setText('[data-detail-operation]', `${property.operation} · ${property.propertyType}`);
  setText('[data-detail-title]', property.publicTitle);
  setText('[data-detail-location]', formatLocation(property.location));
  setText('[data-detail-price]', formatCurrency(property.price));
  setText('[data-detail-admin]', property.administrationFee ? `Administración ${formatCurrency(property.administrationFee)}` : 'Sin administración informada');
  setText('[data-area]', `${property.features.areaSquareMeters} m²`);
  setText('[data-bedrooms]', property.features.bedrooms);
  setText('[data-bathrooms]', property.features.bathrooms);
  setText('[data-parking]', property.features.parkingSpaces);
  setText('[data-stratum]', property.features.stratum ?? '—');
  setText('[data-description]', property.publicDescription);
  setText('[data-map-location]', formatLocation(property.location));
  setText('[data-coordinates]', `${property.location.approximateLatitude.toFixed(4)}, ${property.location.approximateLongitude.toFixed(4)}`);
  setText('[data-advisor-name]', property.advisor.displayName);
  setText('[data-mobile-price]', formatCurrency(property.price));

  const favoriteButton = document.querySelector('[data-favorite]');
  updateFavoriteButton(favoriteButton, favorites.has(property.slug));

  const advisorContact = document.querySelector('[data-advisor-contact]');
  if (property.advisor.publicPhone) {
    advisorContact.textContent = property.advisor.publicPhone;
    advisorContact.href = `tel:${property.advisor.publicPhone.replace(/[^+\d]/g, '')}`;
  } else {
    advisorContact.textContent = 'Contacto mediante formulario';
    advisorContact.removeAttribute('href');
  }

  const whatsapp = document.querySelector('[data-whatsapp]');
  const whatsappUrl = buildWhatsAppUrl(whatsappNumber, property);
  if (whatsappUrl) {
    whatsapp.href = whatsappUrl;
    whatsapp.hidden = false;
  }

  const amenities = property.features.amenities.map((amenity) => {
    const item = document.createElement('li');
    item.textContent = amenity;
    return item;
  });
  document.querySelector('[data-amenities]').replaceChildren(...amenities);
  document.querySelector('[data-amenities-section]').hidden = amenities.length === 0;
  renderGallery(property.photos);

  loading.hidden = true;
  detail.hidden = false;
  document.querySelector('[data-mobile-contact]').hidden = false;
}

document.querySelector('[data-favorite]').addEventListener('click', (event) => {
  if (!property) return;
  const result = favorites.toggle(property.slug);
  updateFavoriteButton(event.currentTarget, result.isFavorite);
  updateFavoriteCounts(favorites);
});

window.addEventListener('storage', (event) => {
  if (event.key !== FAVORITES_STORAGE_KEY) return;
  favorites.reload();
  updateFavoriteCounts(favorites);
  if (property) updateFavoriteButton(document.querySelector('[data-favorite]'), favorites.has(property.slug));
});

function showError(error) {
  loading.hidden = true;
  errorPanel.hidden = false;
  const notFound = error instanceof ApiError && error.status === 404;
  setText('[data-detail-code]', notFound ? '404' : 'CONEXIÓN');
  setText('[data-detail-error-title]', notFound ? 'Este inmueble ya no está publicado.' : 'No pudimos abrir este inmueble.');
  setText('[data-detail-error-message]', notFound ? 'Puede haberse pausado o retirado. El catálogo solo muestra inventario vigente.' : error.message);
}

async function loadProperty() {
  if (!slug) {
    showError(new Error('El enlace no contiene un inmueble válido.'));
    return;
  }

  try {
    property = await client.getProperty(slug);
    renderProperty();
  } catch (error) {
    showError(error);
  }
}

leadForm.addEventListener('submit', async (event) => {
  event.preventDefault();
  const submit = document.querySelector('[data-lead-submit]');
  const feedback = document.querySelector('[data-form-feedback]');
  const data = new FormData(leadForm);
  submit.disabled = true;
  feedback.className = 'form-feedback';
  feedback.textContent = 'Guardando tu solicitud…';

  try {
    await client.createLead(buildVisitLeadPayload({
      name: data.get('name'),
      email: data.get('email'),
      phone: data.get('phone'),
      consent: data.get('consent') === 'on',
      property,
    }));
    feedback.classList.add('is-success');
    feedback.textContent = 'Solicitud recibida. Un asesor de L&C podrá continuar el proceso.';
    leadForm.reset();
  } catch (error) {
    feedback.classList.add('is-error');
    feedback.textContent = error.message || 'No pudimos guardar la solicitud. Intenta nuevamente.';
  } finally {
    submit.disabled = false;
  }
});

updateFavoriteCounts(favorites);
loadProperty();
