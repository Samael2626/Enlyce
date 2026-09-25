import { useMemo, useState } from "react"
import type { FormEvent } from "react"
import { Navigate } from "react-router-dom"
import { ArrowUpRight, Check, CreditCard, Landmark, Minus, Plus, ShieldCheck } from "lucide-react"
import { useBillingPricing, useCreateCheckoutSession } from "@/hooks/useApi"
import { useAuthStore } from "@/stores/authStore"
import type { BillingPricing, CreateCheckoutSessionInput, PaymentCheckoutData } from "@/lib/types"

const formatCop = (cents: number) =>
  new Intl.NumberFormat("es-CO", {
    style: "currency",
    currency: "COP",
    maximumFractionDigits: 0,
  }).format(cents / 100)

function sendToWompi(checkout: PaymentCheckoutData) {
  const parameters = new URLSearchParams({
    "public-key": checkout.publicKey,
    currency: checkout.currency,
    "amount-in-cents": String(checkout.amountInCents),
    reference: checkout.reference,
    "signature:integrity": checkout.integritySignature,
    "redirect-url": checkout.redirectUrl,
    "customer-data:email": checkout.customerEmail,
  })
  window.location.assign(`${checkout.checkoutUrl}?${parameters.toString()}`)
}

export function BillingPage() {
  const user = useAuthStore((state) => state.user)
  const pricing = useBillingPricing()
  const checkout = useCreateCheckoutSession()
  const [totalAdvisors, setTotalAdvisors] = useState(3)
  const [form, setForm] = useState<CreateCheckoutSessionInput>({
    companyName: "",
    taxId: "",
    customerEmail: user?.email ?? "",
    additionalAdvisors: 0,
    includeSetup: true,
    includeWhatsApp: false,
    includePortal: false,
    includeAdvancedReports: false,
  })

  const totals = useMemo(() => calculateTotals(pricing.data, form, totalAdvisors), [pricing.data, form, totalAdvisors])

  if (user?.rol !== "Administrador") return <Navigate to="/dashboard" replace />

  const update = <K extends keyof CreateCheckoutSessionInput>(key: K, value: CreateCheckoutSessionInput[K]) =>
    setForm((current) => ({ ...current, [key]: value }))

  const changeAdvisors = (next: number) => {
    const maximum = 3 + (pricing.data?.maximumAdditionalAdvisors ?? 5)
    const total = Math.min(maximum, Math.max(3, next))
    setTotalAdvisors(total)
    update("additionalAdvisors", total - 3)
  }

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const result = await checkout.mutateAsync(form)
    sendToWompi(result.checkout)
  }

  if (pricing.isPending) return <p className="text-sm text-muted-foreground">Cargando precios...</p>
  if (pricing.isError) return <p role="alert" className="text-sm text-destructive">{pricing.error.message}</p>

  return (
    <div className="space-y-7 pb-10">
      <header className="billing-hero rounded-lg p-7 md:p-10">
        <div className="relative z-10 max-w-3xl">
          <span className="crm-eyebrow">Plan y facturación</span>
          <h1 className="mt-4 font-display text-[clamp(2.5rem,5vw,4.8rem)] leading-[.95] tracking-[-.05em]">
            ENLYCE crece con tu equipo.
          </h1>
          <p className="mt-5 max-w-2xl text-sm leading-6 text-white/72">
            Configura tu mensualidad. El precio definitivo se recalcula y firma en el servidor antes de salir a Wompi.
          </p>
        </div>
        <div className="billing-seal" aria-hidden="true"><Landmark /></div>
      </header>

      <form onSubmit={submit} className="grid gap-6 xl:grid-cols-[1fr_25rem]">
        <div className="space-y-6">
          <section className="crm-panel p-6 md:p-8">
            <span className="crm-eyebrow">01 · Titular</span>
            <h2 className="mt-2 font-display text-3xl tracking-tight">Datos de facturación</h2>
            <div className="mt-6 grid gap-4 md:grid-cols-2">
              <label className="text-sm font-semibold">Empresa
                <input className="billing-input" value={form.companyName} onChange={(event) => update("companyName", event.target.value)} maxLength={160} required />
              </label>
              <label className="text-sm font-semibold">NIT
                <input className="billing-input" value={form.taxId} onChange={(event) => update("taxId", event.target.value)} maxLength={32} required />
              </label>
              <label className="text-sm font-semibold md:col-span-2">Correo de facturación
                <input type="email" className="billing-input" value={form.customerEmail} onChange={(event) => update("customerEmail", event.target.value)} maxLength={254} required />
              </label>
            </div>
          </section>

          <section className="crm-panel p-6 md:p-8">
            <span className="crm-eyebrow">02 · Capacidad</span>
            <div className="mt-2 flex flex-wrap items-end justify-between gap-4">
              <div><h2 className="font-display text-3xl tracking-tight">Tamaño del equipo</h2><p className="mt-2 text-sm text-muted-foreground">El plan base incluye tres asesores.</p></div>
              <div className="billing-counter" aria-label="Cantidad total de asesores">
                <button type="button" onClick={() => changeAdvisors(totalAdvisors - 1)} disabled={totalAdvisors === 3} aria-label="Quitar asesor"><Minus /></button>
                <strong>{totalAdvisors}</strong>
                <button type="button" onClick={() => changeAdvisors(totalAdvisors + 1)} disabled={totalAdvisors === 3 + (pricing.data?.maximumAdditionalAdvisors ?? 5)} aria-label="Agregar asesor"><Plus /></button>
              </div>
            </div>
          </section>

          <section className="crm-panel p-6 md:p-8">
            <span className="crm-eyebrow">03 · Complementos</span>
            <h2 className="mt-2 font-display text-3xl tracking-tight">Sólo lo que vas a usar</h2>
            <div className="mt-6 grid gap-3 md:grid-cols-2">
              <BillingOption label="Instalación inicial" detail="Migración y capacitación · pago único" price={pricing.data!.setupInCents} checked={form.includeSetup} onChange={(value) => update("includeSetup", value)} />
              <BillingOption label="WhatsApp API" detail="Conversaciones integradas al CRM · mensual" price={pricing.data!.whatsAppInCents} checked={form.includeWhatsApp} onChange={(value) => update("includeWhatsApp", value)} />
              <BillingOption label="Portal inmobiliario" detail="Catálogo web público · mensual" price={pricing.data!.portalInCents} checked={form.includePortal} onChange={(value) => update("includePortal", value)} />
              <BillingOption label="Reportes avanzados" detail="PDF, Excel y métricas · mensual" price={pricing.data!.advancedReportsInCents} checked={form.includeAdvancedReports} onChange={(value) => update("includeAdvancedReports", value)} />
            </div>
          </section>
        </div>

        <aside className="billing-summary crm-panel h-fit overflow-hidden xl:sticky xl:top-0">
          <div className="border-b border-white/12 p-6">
            <span className="text-[.68rem] font-bold uppercase tracking-[.16em] text-white/55">Primer cobro</span>
            <p className="mt-3 font-display text-4xl text-white">{formatCop(totals.firstCharge)}</p>
            <p className="mt-2 text-xs text-white/55">COP · precio confirmado por el servidor</p>
          </div>
          <div className="space-y-3 p-6 text-sm text-white/72">
            <SummaryRow label="Plan base" value={pricing.data!.basePlanInCents} />
            {form.additionalAdvisors > 0 && <SummaryRow label={`${form.additionalAdvisors} asesor${form.additionalAdvisors > 1 ? "es" : ""} adicional${form.additionalAdvisors > 1 ? "es" : ""}`} value={form.additionalAdvisors * pricing.data!.additionalAdvisorInCents} />}
            {form.includeWhatsApp && <SummaryRow label="WhatsApp API" value={pricing.data!.whatsAppInCents} />}
            {form.includePortal && <SummaryRow label="Portal inmobiliario" value={pricing.data!.portalInCents} />}
            {form.includeAdvancedReports && <SummaryRow label="Reportes avanzados" value={pricing.data!.advancedReportsInCents} />}
            {form.includeSetup && <SummaryRow label="Instalación única" value={pricing.data!.setupInCents} />}
            <div className="mt-5 border-t border-white/12 pt-5">
              <div className="flex justify-between gap-4"><span>Mensualidad siguiente</span><strong className="text-white">{formatCop(totals.recurring)}</strong></div>
            </div>
            <div className="flex gap-3 border-l-2 border-accent bg-white/5 p-4 text-xs leading-5"><ShieldCheck className="mt-0.5 h-4 w-4 flex-none text-[#e3be65]" /><p>ENLYCE no recibe credenciales bancarias. La autorización ocurre en Wompi y tu banco.</p></div>
            {checkout.error && <p role="alert" className="rounded-sm bg-red-500/15 p-3 text-xs text-red-100">{checkout.error.message}</p>}
            <button className="billing-pay-button" disabled={checkout.isPending}>
              <CreditCard className="h-4 w-4" /> {checkout.isPending ? "Preparando pago..." : "Continuar a PSE"} <ArrowUpRight className="h-4 w-4" />
            </button>
            <p className="text-[.68rem] leading-5 text-white/45">PSE requiere autorización manual cada mes. No se realizará ningún débito automático.</p>
          </div>
        </aside>
      </form>
    </div>
  )
}

function BillingOption({ label, detail, price, checked, onChange }: { label: string; detail: string; price: number; checked: boolean; onChange: (value: boolean) => void }) {
  return (
    <label className="billing-option">
      <input type="checkbox" checked={checked} onChange={(event) => onChange(event.target.checked)} />
      <span className="billing-option-mark"><Check /></span>
      <span className="min-w-0 flex-1"><strong className="block text-sm">{label}</strong><small className="mt-1 block text-xs text-muted-foreground">{detail}</small></span>
      <strong className="text-sm tabular-nums">{formatCop(price)}</strong>
    </label>
  )
}

function SummaryRow({ label, value }: { label: string; value: number }) {
  return <div className="flex justify-between gap-4"><span>{label}</span><span className="tabular-nums">{formatCop(value)}</span></div>
}

function calculateTotals(pricing: BillingPricing | undefined, form: CreateCheckoutSessionInput, totalAdvisors: number) {
  if (!pricing) return { recurring: 0, firstCharge: 0 }
  const recurring = pricing.basePlanInCents
    + Math.max(0, totalAdvisors - 3) * pricing.additionalAdvisorInCents
    + (form.includeWhatsApp ? pricing.whatsAppInCents : 0)
    + (form.includePortal ? pricing.portalInCents : 0)
    + (form.includeAdvancedReports ? pricing.advancedReportsInCents : 0)
  return { recurring, firstCharge: recurring + (form.includeSetup ? pricing.setupInCents : 0) }
}
