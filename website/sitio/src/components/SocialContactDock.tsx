interface SocialLink {
  readonly label: string;
  readonly network: "facebook" | "instagram";
  readonly url: string;
  readonly icon: React.ReactNode;
}

const WHATSAPP_NUMBER = "573246266362";
const WHATSAPP_MESSAGE = "Hola, quiero recibir información sobre los inmuebles de L&C Propiedad Raíz.";
const INSTAGRAM_URL =
  process.env.NEXT_PUBLIC_INSTAGRAM_URL ?? "https://instagram.com/lyc_propiedad_raiz_sas";

export function SocialContactDock() {
  const whatsappUrl = `https://wa.me/${WHATSAPP_NUMBER}?text=${encodeURIComponent(WHATSAPP_MESSAGE)}`;
  const socialLinks: SocialLink[] = [
    {
      label: "Instagram",
      network: "instagram",
      url: INSTAGRAM_URL,
      icon: <InstagramIcon />,
    },
  ];

  if (process.env.NEXT_PUBLIC_FACEBOOK_URL) {
    socialLinks.unshift({
      label: "Facebook",
      network: "facebook",
      url: process.env.NEXT_PUBLIC_FACEBOOK_URL,
      icon: <FacebookIcon />,
    });
  }

  return (
    <aside className="social-contact-dock" aria-label="Canales de contacto">
      <div className="social-contact-links">
        {socialLinks.map((link) => (
          <a
            key={link.label}
            className={`social-contact-link social-contact-link-${link.network}`}
            href={link.url}
            target="_blank"
            rel="noreferrer"
            aria-label={`Visitar ${link.label} de L&C`}
            title={link.label}
          >
            {link.icon}
          </a>
        ))}
      </div>

      <a
        className="whatsapp-fab"
        href={whatsappUrl}
        target="_blank"
        rel="noreferrer"
        aria-label="Hablar con L&C por WhatsApp"
      >
        <WhatsAppIcon />
        <span className="social-contact-label">WhatsApp</span>
      </a>
    </aside>
  );
}

function WhatsAppIcon() {
  return (
    <svg aria-hidden="true" viewBox="0 0 32 32" focusable="false">
      <path
        fill="currentColor"
        d="M16.04 3A12.85 12.85 0 0 0 5.1 22.6L3.3 29l6.56-1.72A12.98 12.98 0 0 0 16.03 29h.01A13 13 0 0 0 16.04 3Zm0 23.8a10.8 10.8 0 0 1-5.5-1.5l-.4-.24-3.89 1.02 1.04-3.78-.26-.4a10.7 10.7 0 1 1 9 4.9Zm5.9-8.02c-.32-.16-1.9-.93-2.2-1.04-.29-.11-.5-.16-.71.16-.21.32-.82 1.04-1 1.25-.19.21-.38.24-.7.08-.32-.16-1.37-.5-2.6-1.61a9.75 9.75 0 0 1-1.8-2.24c-.18-.32-.02-.49.14-.65.14-.14.32-.37.48-.56.16-.18.21-.32.32-.53.1-.21.05-.4-.03-.56-.08-.16-.71-1.72-.98-2.35-.25-.62-.51-.53-.7-.54h-.6c-.21 0-.56.08-.85.4-.29.32-1.11 1.09-1.11 2.65s1.14 3.07 1.3 3.28c.16.21 2.24 3.42 5.42 4.8.76.32 1.35.52 1.81.67.76.24 1.45.21 2 .13.61-.09 1.9-.78 2.16-1.53.27-.75.27-1.4.19-1.53-.08-.14-.3-.22-.62-.38Z"
      />
    </svg>
  );
}

function InstagramIcon() {
  return (
    <svg aria-hidden="true" viewBox="0 0 24 24" focusable="false">
      <rect x="3" y="3" width="18" height="18" rx="5" fill="none" stroke="currentColor" strokeWidth="1.8" />
      <circle cx="12" cy="12" r="4" fill="none" stroke="currentColor" strokeWidth="1.8" />
      <circle cx="17.5" cy="6.5" r="1" fill="currentColor" />
    </svg>
  );
}

function FacebookIcon() {
  return (
    <svg aria-hidden="true" viewBox="0 0 24 24" focusable="false">
      <path fill="currentColor" d="M13.7 21v-8h2.7l.4-3.1h-3.1v-2c0-.9.3-1.5 1.6-1.5H17V3.6c-.8-.1-1.6-.2-2.4-.2-2.4 0-4.1 1.5-4.1 4.2v2.3H7.8V13h2.7v8h3.2Z" />
    </svg>
  );
}
