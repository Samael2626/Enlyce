import { useLinkDemandProperty, usePropertyDemandMatches, useSetDemandPropertyStatus } from "@/hooks/useApi"

// Cruce inverso: demandas de clientes compatibles con un inmueble
export function PropertyDemandsPanel({ propertyId, propertyName, onClose }: { propertyId: string; propertyName: string; onClose: () => void }) {
  const matches = usePropertyDemandMatches(propertyId)
  const linkProperty = useLinkDemandProperty()
  const setStatus = useSetDemandPropertyStatus()

  return (
    <section className="crm-panel p-5" aria-label="Clientes posibles">
      <div className="flex items-center justify-between gap-3">
        <h3 className="font-display text-xl text-foreground">Clientes posibles · {propertyName}</h3>
        <button onClick={onClose} className="crm-button">Cerrar</button>
      </div>
      <p className="mt-1 text-xs text-muted-foreground">Búsquedas activas que cumplen operación, tipo, ciudad, barrio, presupuesto y mínimos de habitaciones, baños y parqueaderos. Solo ves las de tus oportunidades.</p>
      {matches.isPending && <p className="mt-3 text-sm text-muted-foreground">Buscando clientes…</p>}
      {matches.data?.map(({ demand, contactName, relationshipStatus }) => (
        <article key={demand.id} className="mt-2 flex flex-wrap items-center justify-between gap-3 rounded-md border border-border p-3">
          <div>
            <p className="font-semibold text-foreground">{contactName}</p>
            <p className="mt-1 text-xs text-muted-foreground">
              {demand.operation}{demand.propertyType ? ` · ${demand.propertyType}` : ""} · {demand.city || "Cualquier ciudad"}{demand.neighborhood ? `, ${demand.neighborhood}` : ""}
              {demand.minimumPrice ? ` · Desde $${demand.minimumPrice.toLocaleString("es-CO")}` : ""}{demand.maximumPrice ? ` · Hasta $${demand.maximumPrice.toLocaleString("es-CO")}` : ""}
              {demand.bedrooms ? ` · ${demand.bedrooms}+ hab.` : ""}
            </p>
          </div>
          {relationshipStatus ? (
            <select aria-label={`Estado con ${contactName}`} value={relationshipStatus} onChange={(event) => setStatus.mutate({ demandId: demand.id, propertyId, status: event.target.value })} className="rounded-md border border-border bg-background px-2 py-1.5 text-xs text-foreground">
              <option value="Suggested">Sugerido</option><option value="Shared">Compartido</option><option value="Visited">Visitado</option><option value="Discarded">Descartado</option>
            </select>
          ) : <button onClick={() => linkProperty.mutate({ demandId: demand.id, propertyId })} disabled={linkProperty.isPending} className="rounded-md border border-border px-3 py-2 text-xs font-semibold text-foreground hover:bg-muted">Relacionar</button>}
        </article>
      ))}
      {matches.data?.length === 0 && <p className="mt-3 text-sm text-muted-foreground">Ninguna búsqueda activa coincide con este inmueble. Si no está disponible, no aparecerá en ningún cruce.</p>}
      {(matches.isError || linkProperty.isError || setStatus.isError) && <p role="alert" className="mt-3 text-sm text-destructive">No se pudo consultar o guardar la relación con el cliente.</p>}
    </section>
  )
}
