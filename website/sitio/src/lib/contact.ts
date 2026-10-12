// Canales publicos de contacto: una sola fuente para dock, header y footer.
export const WHATSAPP_NUMBER = "573246266362";
export const INSTAGRAM_URL =
  process.env.NEXT_PUBLIC_INSTAGRAM_URL ?? "https://instagram.com/lyc_propiedad_raiz_sas";
export const FACEBOOK_URL = process.env.NEXT_PUBLIC_FACEBOOK_URL;

export function whatsappLink(message = "Hola, quiero recibir información sobre los inmuebles de L&C Propiedad Raíz."): string {
  return `https://wa.me/${WHATSAPP_NUMBER}?text=${encodeURIComponent(message)}`;
}
