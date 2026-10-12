import type { Metadata } from "next";
import { UnsubscribeAlert } from "@/components/UnsubscribeAlert";

export const metadata: Metadata = {
  title: "Cancelar alerta",
  robots: { index: false, follow: false },
};

type PageProps = { searchParams: Promise<Record<string, string | string[] | undefined>> };

export default async function UnsubscribePage({ searchParams }: PageProps) {
  const params = await searchParams;
  const id = typeof params.id === "string" ? params.id : "";
  const token = typeof params.token === "string" ? params.token : "";

  return (
    <section className="mx-auto max-w-xl space-y-4 px-4 py-16">
      <p className="section-kicker">Alertas de búsqueda</p>
      <h1 className="text-4xl">Cancelar alerta</h1>
      <UnsubscribeAlert id={id} token={token} />
    </section>
  );
}
