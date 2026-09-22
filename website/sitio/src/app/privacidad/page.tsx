import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Política de tratamiento de datos personales",
  description:
    "Cómo L&C Propiedad Raíz S.A.S. recolecta, usa y protege los datos personales, conforme a la Ley 1581 de 2012.",
  robots: { index: true, follow: false },
};

export default function PrivacidadPage() {
  return (
    <div className="mx-auto max-w-3xl space-y-6 px-4 py-10">
      <h1 className="text-4xl">Política de tratamiento de datos personales</h1>

      <p className="text-muted">
        L&amp;C Propiedad Raíz S.A.S. trata los datos personales conforme a la Ley 1581 de 2012 y
        sus decretos reglamentarios.
      </p>

      <section className="space-y-2">
        <h2 className="text-2xl">Qué datos recogemos</h2>
        <p>
          Nombre, teléfono, correo electrónico y la información del inmueble que nos compartes al
          solicitar una visita o al ofrecer una propiedad.
        </p>
      </section>

      <section className="space-y-2">
        <h2 className="text-2xl">Para qué los usamos</h2>
        <p>
          Para responder tu solicitud, agendar visitas, hacer seguimiento comercial y cumplir
          obligaciones legales y contractuales.
        </p>
      </section>

      <section className="space-y-2">
        <h2 className="text-2xl">Tus derechos</h2>
        <p>
          Puedes conocer, actualizar, rectificar y suprimir tus datos, y revocar la autorización
          otorgada. Para ejercerlos, escríbenos por los canales de contacto.
        </p>
      </section>

      {/* Contenido pendiente de revision legal de L&C antes de produccion. */}
      <p className="rounded-sheet border border-line p-4 text-sm text-muted">
        Este texto es un borrador de trabajo. La versión definitiva debe ser aprobada por L&amp;C
        antes de publicar el sitio.
      </p>
    </div>
  );
}
