import { Link, Navigate, useSearchParams } from "react-router-dom"
import { AlertTriangle, CheckCircle2, Clock3, XCircle } from "lucide-react"
import { usePaymentOrderStatus } from "@/hooks/useApi"
import { useAuthStore } from "@/stores/authStore"

const stateCopy = {
  Pending: { title: "Pago en validación", body: "Wompi todavía está procesando la respuesta del banco.", icon: Clock3, tone: "text-warning" },
  Approved: { title: "Pago aprobado", body: "La orden quedó confirmada. La activación de la suscripción será el siguiente paso.", icon: CheckCircle2, tone: "text-success" },
  Declined: { title: "Pago rechazado", body: "El banco rechazó la operación. Puedes intentarlo con una orden nueva.", icon: XCircle, tone: "text-destructive" },
  Voided: { title: "Pago anulado", body: "La transacción fue anulada y no activará servicios.", icon: AlertTriangle, tone: "text-destructive" },
  Error: { title: "Pago con error", body: "Wompi no pudo completar la operación. No se activó ningún servicio.", icon: AlertTriangle, tone: "text-destructive" },
} as const

const paymentStatusLabel = {
  Pending: "Pendiente",
  Approved: "Aprobado",
  Declined: "Rechazado",
  Voided: "Anulado",
  Error: "Error",
} as const

export function BillingResultPage() {
  const user = useAuthStore((state) => state.user)
  const [search] = useSearchParams()
  const reference = search.get("reference")
  const order = usePaymentOrderStatus(reference)

  if (user?.rol !== "Administrador") return <Navigate to="/dashboard" replace />
  if (!reference) return <ResultCard title="Referencia ausente" body="No podemos identificar la orden devuelta por Wompi." icon={AlertTriangle} tone="text-destructive" />
  if (order.isPending) return <ResultCard title="Consultando la orden" body="Esperando la confirmación firmada de Wompi." icon={Clock3} tone="text-warning" />
  if (order.isError) return <ResultCard title="No pudimos consultar el pago" body={order.error.message} icon={AlertTriangle} tone="text-destructive" />

  const copy = stateCopy[order.data.status]
  return <ResultCard {...copy} reference={order.data.reference} statusHistory={order.data.statusHistory} />
}

function ResultCard({ title, body, icon: Icon, tone, reference, statusHistory }: { title: string; body: string; icon: React.ElementType; tone: string; reference?: string; statusHistory?: import("@/lib/types").PaymentOrderStatus["statusHistory"] }) {
  return (
    <div className="mx-auto max-w-2xl pt-12">
      <section className="crm-panel overflow-hidden">
        <div className="billing-result-head p-8"><Icon className={`h-10 w-10 ${tone}`} /><span className="crm-eyebrow">Resultado del pago</span><h1 className="mt-3 font-display text-4xl tracking-tight">{title}</h1></div>
        <div className="space-y-5 p-8"><p className="text-sm leading-6 text-muted-foreground">{body}</p>{reference && <p className="border-l-2 border-accent pl-4 font-mono text-xs text-muted-foreground">{reference}</p>}<Link to="/facturacion" className="crm-button">Volver a facturación</Link></div>
        {statusHistory && <section aria-labelledby="payment-history-title" className="space-y-3 px-8 pb-8">
          <h2 id="payment-history-title" className="text-sm font-semibold">Historial del pago</h2>
          {statusHistory.length === 0 ? <p className="text-sm text-muted-foreground">Wompi aún no reporta cambios de estado.</p> :
            <ol className="space-y-2" aria-label="Cambios de estado recibidos de Wompi">
              {statusHistory.map((change, index) => <li key={`${change.occurredAtUtc}-${index}`} className="flex flex-wrap items-baseline justify-between gap-x-4 gap-y-1 border-l-2 border-line pl-3 text-sm">
                <span>{paymentStatusLabel[change.previousStatus]} → {paymentStatusLabel[change.newStatus]}</span>
                <time className="text-xs text-muted-foreground" dateTime={change.occurredAtUtc}>{new Intl.DateTimeFormat("es-CO", { dateStyle: "medium", timeStyle: "short" }).format(new Date(change.occurredAtUtc))}</time>
              </li>)}
            </ol>}
        </section>}
      </section>
    </div>
  )
}
