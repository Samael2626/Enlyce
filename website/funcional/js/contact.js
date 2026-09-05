export function resolveWhatsAppNumber(search = '', configuredNumber = '') {
  const temporaryNumber = new URLSearchParams(search).get('whatsapp');
  const digits = (temporaryNumber || configuredNumber).replace(/\D/g, '');
  return digits.length >= 8 && digits.length <= 15 ? digits : '';
}

export function buildWhatsAppUrl(number, property) {
  if (!number) return '';

  const message = `Hola, quiero información sobre ${property.publicTitle} (Ref. ${property.slug}).`;
  return `https://wa.me/${number}?text=${encodeURIComponent(message)}`;
}
