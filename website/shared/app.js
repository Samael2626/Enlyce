const toast = document.querySelector('[data-toast]');
let toastTimer;

function showToast(message) {
  if (!toast) return;
  toast.textContent = message;
  toast.classList.add('is-visible');
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => toast.classList.remove('is-visible'), 2800);
}

document.querySelectorAll('[data-search]').forEach((form) => {
  form.addEventListener('submit', (event) => {
    event.preventDefault();
    const location = new FormData(form).get('location') || 'Medellín';
    showToast(`Demo: buscando inmuebles en ${location}.`);
  });
});

document.querySelectorAll('[data-lead]').forEach((form) => {
  form.addEventListener('submit', (event) => {
    event.preventDefault();
    showToast('Solicitud guardada en esta demostración.');
    form.reset();
  });
});

document.querySelectorAll('.favorite').forEach((button) => {
  button.addEventListener('click', () => {
    const active = button.getAttribute('aria-pressed') === 'true';
    button.setAttribute('aria-pressed', String(!active));
    button.textContent = active ? '♡' : '♥';
    showToast(active ? 'Inmueble eliminado de favoritos.' : 'Inmueble guardado en favoritos.');
  });
});

document.querySelectorAll('[data-chip]').forEach((button) => {
  button.addEventListener('click', () => {
    const active = button.getAttribute('aria-pressed') === 'true';
    button.setAttribute('aria-pressed', String(!active));
  });
});

document.querySelectorAll('[data-menu-button]').forEach((button) => {
  button.addEventListener('click', () => {
    const menu = document.querySelector(`#${button.getAttribute('aria-controls')}`);
    const open = button.getAttribute('aria-expanded') === 'true';
    button.setAttribute('aria-expanded', String(!open));
    menu?.classList.toggle('is-open', !open);
  });
});

document.querySelectorAll('[data-scroll-to]').forEach((button) => {
  button.addEventListener('click', () => {
    document.querySelector(button.dataset.scrollTo)?.scrollIntoView({ behavior: 'smooth' });
  });
});

document.querySelectorAll('[data-year]').forEach((node) => {
  node.textContent = new Date().getFullYear();
});
