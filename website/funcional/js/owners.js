import { PublicCatalogClient, resolveApiBase } from './api.js';
import { buildOwnerWhatsAppUrl, resolveWhatsAppNumber } from './contact.js';
import { buildOwnerLeadPayload } from './owner-inquiry.js';
import { siteSettings } from './settings.js';

const form = document.querySelector('[data-owner-form]');
const serviceSelect = form.elements.namedItem('service');
const whatsapp = document.querySelector('[data-owner-whatsapp]');
const client = new PublicCatalogClient(resolveApiBase(window.location.search));
const whatsappNumber = resolveWhatsAppNumber(window.location.search, siteSettings.whatsappNumber);

function updateWhatsApp() {
  const url = buildOwnerWhatsAppUrl(whatsappNumber, serviceSelect.value || 'Administrar');
  if (!url) return;
  whatsapp.href = url;
  whatsapp.hidden = false;
}

for (const choice of document.querySelectorAll('[data-service-choice]')) {
  choice.addEventListener('click', () => {
    serviceSelect.value = choice.dataset.serviceChoice;
    updateWhatsApp();
  });
}

serviceSelect.addEventListener('change', updateWhatsApp);

form.addEventListener('submit', async (event) => {
  event.preventDefault();
  const submit = document.querySelector('[data-owner-submit]');
  const feedback = document.querySelector('[data-owner-feedback]');
  const data = new FormData(form);

  submit.disabled = true;
  feedback.className = 'form-feedback';
  feedback.textContent = 'Guardando tu solicitud…';

  try {
    const payload = buildOwnerLeadPayload({
      name: data.get('name'),
      email: data.get('email'),
      phone: data.get('phone'),
      ownerService: data.get('service'),
      propertyType: data.get('propertyType'),
      municipality: data.get('municipality'),
      neighborhood: data.get('neighborhood'),
      consent: data.get('consent') === 'on',
    });
    await client.createLead(payload);
    feedback.classList.add('is-success');
    feedback.textContent = 'Solicitud recibida. El equipo L&C podrá revisar tu caso y contactarte.';
    form.reset();
    updateWhatsApp();
  } catch (error) {
    feedback.classList.add('is-error');
    feedback.textContent = error.message || 'No pudimos guardar la solicitud. Intenta nuevamente.';
  } finally {
    submit.disabled = false;
  }
});

updateWhatsApp();
