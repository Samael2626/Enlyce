"use client";

import Link from "next/link";
import { useState } from "react";
import { unsubscribeSearchAlert } from "@/lib/api/leads";

type Status = "idle" | "sending" | "done" | "error";

// La baja exige un clic: los escaneres de enlaces del correo no la disparan solos.
export function UnsubscribeAlert({ id, token }: { id: string; token: string }) {
  const [status, setStatus] = useState<Status>("idle");

  if (!id || !token) {
    return <p className="text-muted">El enlace está incompleto. Ábrelo de nuevo desde el correo que recibiste.</p>;
  }

  if (status === "done") {
    return (
      <div className="space-y-3" role="status">
        <p>Listo. No volverás a recibir avisos de esta alerta.</p>
        <Link href="/inmuebles" className="text-accent-strong underline">Seguir viendo inmuebles</Link>
      </div>
    );
  }

  const confirm = async () => {
    setStatus("sending");
    try {
      await unsubscribeSearchAlert(id, token);
      setStatus("done");
    } catch {
      setStatus("error");
    }
  };

  return (
    <div className="space-y-4">
      <p className="text-muted">Dejarás de recibir correos cuando publiquemos inmuebles que coincidan con esta búsqueda.</p>
      {status === "error" && (
        <p role="alert" className="text-red-700">No pudimos cancelar la alerta. El enlace puede estar vencido o incompleto.</p>
      )}
      <button
        type="button"
        onClick={confirm}
        disabled={status === "sending"}
        className="bg-ink px-5 py-3 text-sm font-bold text-white disabled:opacity-60"
      >
        {status === "sending" ? "Cancelando…" : "Cancelar alerta"}
      </button>
    </div>
  );
}
