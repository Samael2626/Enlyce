import { useEffect, useMemo, useState } from "react"
import { ChevronLeft, ChevronRight, Download, Mail, Phone, Search, UserRound } from "lucide-react"
import { api } from "@/api/client"
import { useContact, useContacts, useCreateContact, useCreateCommercialTask, useCompleteCommercialTask, useCancelCommercialTask, useCommercialTaskHistory, useAddCommercialTaskComment, useCreateCustomerDemand, useUpdateCustomerDemand, useDeleteCustomerDemand, useCustomerDemands, useDemandMatches, useCustomerDemandCross, useLinkDemandProperty, useSetDemandPropertyStatus, useUpdateContact, useSetContactEmailPermission } from "@/hooks/useApi"
import { useAuthStore } from "@/stores/authStore"

const splitFeatures = (value: string) => value.split(",").map((item) => item.trim()).filter(Boolean)

export function ContactsPage() {
  const createContact = useCreateContact()
  const [showCreateContact, setShowCreateContact] = useState(false)
  const [includeInitialDemand, setIncludeInitialDemand] = useState(false)
  const [initialDemandError, setInitialDemandError] = useState("")
  const [newContact, setNewContact] = useState({ name: "", surname: "", email: "", phone: "", landline: "",
    customerTypes: [] as string[], customerLabels: [] as string[], address: "", country: "Colombia", department: "", city: "", dateOfBirth: "",
    identificationNumber: "", referredBy: "", acquisitionMedium: "", additionalData: "", advisorId: "",
    customerFunnel: "Clientes" })
  const pageSize = 20
  const [selectedId, setSelectedId] = useState("")
  const [search, setSearch] = useState("")
  const [createdFrom, setCreatedFrom] = useState("")
  const [createdTo, setCreatedTo] = useState("")
  const [customerTypeFilter, setCustomerTypeFilter] = useState("")
  const [customerStatusFilter, setCustomerStatusFilter] = useState("")
  const [customerFunnelFilter, setCustomerFunnelFilter] = useState("")
  const [advisorFilter, setAdvisorFilter] = useState("")
  const [crossFilters, setCrossFilters] = useState({ q: "", customerType: "", customerStatus: "", customerFunnel: "", advisorId: "",
    contactFrom: "", contactTo: "", demandFrom: "", demandTo: "" })
  const [crossSearched, setCrossSearched] = useState(false)
  const [crossPage, setCrossPage] = useState(1)
  const [page, setPage] = useState(1)
  const [selectedContactIds, setSelectedContactIds] = useState<string[]>([])
  const [exportingContacts, setExportingContacts] = useState(false)
  const [emailingContacts, setEmailingContacts] = useState(false)
  const [exportContactsError, setExportContactsError] = useState("")
  const [exportContactsNotice, setExportContactsNotice] = useState("")
  const contactsQuery = useContacts({ q: search, from: createdFrom, to: createdTo,
    customerType: customerTypeFilter, customerStatus: customerStatusFilter, customerFunnel: customerFunnelFilter,
    advisorId: advisorFilter || undefined, page, pageSize })
  const contacts = contactsQuery.data?.items ?? []
  const totalContacts = contactsQuery.data?.total ?? 0
  const pageCount = Math.max(1, Math.ceil(totalContacts / pageSize))

  useEffect(() => {
    if (contactsQuery.isFetching) return
    if (!contacts.some((contact) => contact.id === selectedId))
      setSelectedId(contacts[0]?.id ?? "")
  }, [contacts, selectedId, contactsQuery.isFetching])

  useEffect(() => setSelectedContactIds([]), [search, createdFrom, createdTo,
    customerTypeFilter, customerStatusFilter, customerFunnelFilter, advisorFilter, page])

  const detailQuery = useContact(selectedId)
  const contact = detailQuery.data?.contact
  const user = useAuthStore((state) => state.user)
  const [emailPermissionHistory, setEmailPermissionHistory] = useState<Awaited<ReturnType<typeof api.getContactEmailPermissionHistory>>>([])
  const updateContact = useUpdateContact()
  const setEmailPermission = useSetContactEmailPermission()
  useEffect(() => {
    if (user?.rol !== "Administrador" || !selectedId) return
    void api.getContactEmailPermissionHistory(selectedId).then(setEmailPermissionHistory).catch(() => setEmailPermissionHistory([]))
  }, [selectedId, user?.rol, contact?.canReceiveAutomatedEmail])
  const createTask = useCreateCommercialTask()
  const completeTask = useCompleteCommercialTask()
  const cancelTask = useCancelCommercialTask()
  const [selectedTaskId, setSelectedTaskId] = useState("")
  const taskHistory = useCommercialTaskHistory(selectedTaskId)
  const addTaskComment = useAddCommercialTaskComment()
  const createDemand = useCreateCustomerDemand()
  const updateDemand = useUpdateCustomerDemand()
  const deleteDemand = useDeleteCustomerDemand()
  const [editingDemandId, setEditingDemandId] = useState("")
  const demandsQuery = useCustomerDemands(contact?.id ?? "")
  const [selectedDemandId, setSelectedDemandId] = useState("")
  const matchesQuery = useDemandMatches(selectedDemandId)
  const linkProperty = useLinkDemandProperty()
  const setPropertyStatus = useSetDemandPropertyStatus()
  const [name, setName] = useState("")
  const [surname, setSurname] = useState("")
  const [customerTypes, setCustomerTypes] = useState<string[]>([])
  const [customerLabels, setCustomerLabels] = useState<string[]>([])
  const [customerStatus, setCustomerStatus] = useState("Nuevo")
  const [customerFunnel, setCustomerFunnel] = useState("Clientes")
  const [address, setAddress] = useState("")
  const [country, setCountry] = useState("Colombia")
  const [department, setDepartment] = useState("")
  const [city, setCity] = useState("")
  const [phone, setPhone] = useState("")
  const [landline, setLandline] = useState("")
  const [dateOfBirth, setDateOfBirth] = useState("")
  const [identificationNumber, setIdentificationNumber] = useState("")
  const [referredBy, setReferredBy] = useState("")
  const [acquisitionMedium, setAcquisitionMedium] = useState("")
  const [additionalData, setAdditionalData] = useState("")
  const [taskTitle, setTaskTitle] = useState("")
  const [taskDescription, setTaskDescription] = useState("")
  const [taskReminderAt, setTaskReminderAt] = useState("")
  const [taskComment, setTaskComment] = useState("")
  const [taskType, setTaskType] = useState("FollowUp")
  const [taskPriority, setTaskPriority] = useState("Normal")
  const [taskDueAt, setTaskDueAt] = useState(() => toLocalDateTimeInput(new Date(Date.now() + 60 * 60 * 1000)))
  const [demandOperation, setDemandOperation] = useState("Venta")
  const [demandType, setDemandType] = useState("")
  const [demandCity, setDemandCity] = useState("")
  const [demandCountry, setDemandCountry] = useState("Colombia")
  const [demandDepartment, setDemandDepartment] = useState("")
  const [demandNeighborhood, setDemandNeighborhood] = useState("")
  const [demandMaxPrice, setDemandMaxPrice] = useState("")
  const [demandMinPrice, setDemandMinPrice] = useState("")
  const [demandCurrency, setDemandCurrency] = useState("COP")
  const [demandMinArea, setDemandMinArea] = useState("")
  const [demandMaxArea, setDemandMaxArea] = useState("")
  const [demandBedrooms, setDemandBedrooms] = useState("")
  const [demandBathrooms, setDemandBathrooms] = useState("")
  const [demandParkingSpaces, setDemandParkingSpaces] = useState("")
  const [demandInternalFeatures, setDemandInternalFeatures] = useState("")
  const [demandExternalFeatures, setDemandExternalFeatures] = useState("")
  const [demandNotes, setDemandNotes] = useState("")
  const [importFile, setImportFile] = useState<File | null>(null)
  const [importColumns, setImportColumns] = useState<string[]>([])
  const [nameColumn, setNameColumn] = useState("")
  const [surnameColumn, setSurnameColumn] = useState("")
  const [emailColumn, setEmailColumn] = useState("")
  const [phoneColumn, setPhoneColumn] = useState("")
  const [canReceiveEmailColumn, setCanReceiveEmailColumn] = useState("")
  const [customerTypeColumn, setCustomerTypeColumn] = useState("")
  const [customerLabelsColumn, setCustomerLabelsColumn] = useState("")
  const [customerFunnelColumn, setCustomerFunnelColumn] = useState("")
  const [customerStatusColumn, setCustomerStatusColumn] = useState("")
  const [addressColumn, setAddressColumn] = useState("")
  const [countryColumn, setCountryColumn] = useState("")
  const [departmentColumn, setDepartmentColumn] = useState("")
  const [cityColumn, setCityColumn] = useState("")
  const [landlineColumn, setLandlineColumn] = useState("")
  const [dateOfBirthColumn, setDateOfBirthColumn] = useState("")
  const [identificationNumberColumn, setIdentificationNumberColumn] = useState("")
  const [referredByColumn, setReferredByColumn] = useState("")
  const [acquisitionMediumColumn, setAcquisitionMediumColumn] = useState("")
  const [additionalDataColumn, setAdditionalDataColumn] = useState("")
  const [importPreview, setImportPreview] = useState<Awaited<ReturnType<typeof api.previewClientImport>> | null>(null)
  const [importResult, setImportResult] = useState<Awaited<ReturnType<typeof api.confirmClientImport>> | null>(null)
  const [importError, setImportError] = useState("")
  const [importing, setImporting] = useState(false)
  const [advisors, setAdvisors] = useState<Array<{ id: string; name: string }>>([])
  const crossQuery = useCustomerDemandCross({ ...crossFilters, page: crossPage, pageSize: 10 }, crossSearched)
  const crossResult = crossQuery.data
  const [ownerAdvisorId, setOwnerAdvisorId] = useState("")
  const [importRequestId, setImportRequestId] = useState("")
  const [importRequestIdReady, setImportRequestIdReady] = useState(false)
  const updateImportMapping = (setter: (value: string) => void, value: string) => {
    setter(value)
    setImportPreview(null)
    setImportResult(null)
  }

  useEffect(() => {
    if (!importFile) {
      setImportRequestId("")
      setImportRequestIdReady(true)
      return
    }

    let cancelled = false
    setImportRequestIdReady(false)
    void importFile.arrayBuffer().then(async (fileBytes) => {
      const fileDigest = await crypto.subtle.digest("SHA-256", fileBytes)
      const fileHash = Array.from(new Uint8Array(fileDigest), (byte) => byte.toString(16).padStart(2, "0")).join("")
      const fingerprint = JSON.stringify([user?.email ?? "", fileHash, nameColumn, surnameColumn, emailColumn,
        phoneColumn, canReceiveEmailColumn, customerTypeColumn, customerStatusColumn, addressColumn, countryColumn,
        departmentColumn, cityColumn, landlineColumn, dateOfBirthColumn, identificationNumberColumn,
        referredByColumn, acquisitionMediumColumn, additionalDataColumn, customerLabelsColumn,
        customerFunnelColumn, ownerAdvisorId])
      const fingerprintDigest = await crypto.subtle.digest("SHA-256", new TextEncoder().encode(fingerprint))
      const storageKey = `enlyce:client-import:${Array.from(new Uint8Array(fingerprintDigest),
        (byte) => byte.toString(16).padStart(2, "0")).join("")}`
      let requestId: string
      try {
        requestId = sessionStorage.getItem(storageKey) ?? crypto.randomUUID()
        sessionStorage.setItem(storageKey, requestId)
      } catch {
        requestId = crypto.randomUUID()
      }
      if (!cancelled) {
        setImportRequestId(requestId)
        setImportRequestIdReady(true)
      }
    }).catch(() => {
      if (!cancelled) {
        setImportError("No se pudo preparar la clave segura del reintento. Vuelve a seleccionar el archivo.")
        setImportRequestIdReady(false)
      }
    })
    return () => { cancelled = true }
  }, [importFile, user?.email, nameColumn, surnameColumn, emailColumn, phoneColumn, canReceiveEmailColumn,
    customerTypeColumn, customerLabelsColumn, customerFunnelColumn, customerStatusColumn, addressColumn,
    countryColumn, departmentColumn, cityColumn, landlineColumn, dateOfBirthColumn,
    identificationNumberColumn, referredByColumn, acquisitionMediumColumn, additionalDataColumn, ownerAdvisorId])

  useEffect(() => {
    if (user?.rol === "Administrador")
      void api.getAdvisors().then(setAdvisors).catch(() => setAdvisors([]))
  }, [user?.rol])

  useEffect(() => {
    setName(contact?.name ?? "")
    setSurname(contact?.surname ?? "")
    setCustomerTypes(contact?.customerTypes?.length ? contact.customerTypes : contact?.customerType ? [contact.customerType] : [])
    setCustomerLabels(contact?.customerLabels ?? [])
    setCustomerStatus(contact?.customerStatus ?? "Nuevo")
    setCustomerFunnel(contact?.customerFunnel ?? "Clientes")
    setAddress(contact?.address ?? "")
    setCountry(contact?.country ?? "Colombia")
    setDepartment(contact?.department ?? "")
    setCity(contact?.city ?? "")
    setPhone(contact?.phone ?? "")
    setLandline(detailQuery.data?.landline ?? "")
    setDateOfBirth(detailQuery.data?.dateOfBirth ?? "")
    setIdentificationNumber(detailQuery.data?.identificationNumber ?? "")
    setReferredBy(detailQuery.data?.referredBy ?? "")
    setAcquisitionMedium(detailQuery.data?.acquisitionMedium ?? "")
    setAdditionalData(detailQuery.data?.additionalData ?? "")
  }, [contact?.id, contact?.name, contact?.phone, detailQuery.data?.dateOfBirth])

  useEffect(() => {
    if (!demandsQuery.data?.some((demand) => demand.id === selectedDemandId))
      setSelectedDemandId(demandsQuery.data?.[0]?.id ?? "")
  }, [demandsQuery.data, selectedDemandId])

  const timeline = useMemo(() => {
    const events = (detailQuery.data?.opportunities ?? []).flatMap((opportunity) => [
      ...opportunity.interactions.map((item) => ({
        date: item.date,
        label: item.type,
        detail: item.summary,
        context: `${opportunity.operation} · ${opportunity.stage}`,
      })),
      ...opportunity.visits.map((item) => ({
        date: item.completedAt ?? item.scheduledAt,
        label: `Visita ${item.status.toLocaleLowerCase()}`,
        detail: item.feedback,
        context: `${opportunity.operation} · inmueble ${item.propertyId.slice(0, 8)}`,
      })),
    ]).sort((left, right) => Date.parse(right.date) - Date.parse(left.date))
    return events
  }, [detailQuery.data])

  return (
    <div className="space-y-6">
      <header>
        <p className="crm-eyebrow">Relación comercial</p>
        <h1 className="mt-2 font-display text-3xl text-foreground">Contactos</h1>
        <p className="mt-2 text-sm text-muted-foreground">Personas, oportunidades y actividad en un solo expediente.</p>
        <button type="button" className="mt-4 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground" onClick={() => {
          if (!showCreateContact) {
            setInitialDemandError(""); setIncludeInitialDemand(false)
            setDemandOperation("Venta"); setDemandType(""); setDemandCity(""); setDemandCountry("Colombia")
            setDemandDepartment(""); setDemandNeighborhood(""); setDemandMinPrice(""); setDemandMaxPrice("")
            setDemandCurrency("COP"); setDemandMinArea(""); setDemandMaxArea(""); setDemandBedrooms("")
            setDemandBathrooms(""); setDemandParkingSpaces(""); setDemandInternalFeatures(""); setDemandExternalFeatures(""); setDemandNotes("")
          }
          setShowCreateContact((value) => !value)
        }}>
          {showCreateContact ? "Cerrar alta" : "Nuevo cliente"}
        </button>
      </header>

      {initialDemandError && !showCreateContact && <p role="alert" className="crm-panel p-4 text-sm text-destructive">{initialDemandError}</p>}

      {showCreateContact && <form className="crm-panel grid gap-3 p-5 sm:grid-cols-2 lg:grid-cols-3" onSubmit={async (event) => {
        event.preventDefault()
        setInitialDemandError("")
        try {
          const created = await createContact.mutateAsync({ ...newContact, advisorId: newContact.advisorId || undefined, customerStatus: "Nuevo" })
          setSearch(created.name)
          setCreatedFrom(""); setCreatedTo(""); setCustomerTypeFilter(""); setCustomerStatusFilter(""); setAdvisorFilter(""); setPage(1)
          setSelectedId(created.id)
          if (includeInitialDemand) {
            try {
              await createDemand.mutateAsync({
                contactId: created.id, operation: demandOperation, propertyType: demandType || undefined,
                city: demandCity, country: demandCountry, department: demandDepartment || undefined,
                neighborhood: demandNeighborhood || undefined, minimumPrice: demandMinPrice ? Number(demandMinPrice) : undefined,
                maximumPrice: demandMaxPrice ? Number(demandMaxPrice) : undefined, currency: demandCurrency,
                minimumArea: demandMinArea ? Number(demandMinArea) : undefined,
                maximumArea: demandMaxArea ? Number(demandMaxArea) : undefined,
                bedrooms: demandBedrooms ? Number(demandBedrooms) : undefined,
                bathrooms: demandBathrooms ? Number(demandBathrooms) : undefined,
                parkingSpaces: demandParkingSpaces ? Number(demandParkingSpaces) : undefined,
                internalFeatures: splitFeatures(demandInternalFeatures), externalFeatures: splitFeatures(demandExternalFeatures), notes: demandNotes || undefined,
              })
            } catch (error) {
              setInitialDemandError(`Cliente creado; la demanda falló: ${error instanceof Error ? error.message : "error desconocido"}. La ficha quedó abierta para completar la demanda sin duplicar el cliente.`)
              setShowCreateContact(false)
              return
            }
          }
            setNewContact({ name: "", surname: "", email: "", phone: "", landline: "", customerTypes: [], customerLabels: [],
              address: "", country: "Colombia", department: "", city: "", dateOfBirth: "", identificationNumber: "",
              referredBy: "", acquisitionMedium: "", additionalData: "", advisorId: "", customerFunnel: "Clientes" })
            setShowCreateContact(false)
            setIncludeInitialDemand(false)
        } catch {
          // El error de alta queda en createContact y se muestra bajo el formulario.
        }
      }}>
        <h2 className="font-display text-xl text-foreground sm:col-span-2 lg:col-span-3">Nuevo cliente</h2>
        {([
          ["name", "Nombre", true], ["surname", "Apellidos"], ["email", "Correo electrónico"],
          ["phone", "Teléfono móvil"], ["landline", "Teléfono fijo"], ["dateOfBirth", "Fecha de nacimiento", false, "date"],
          ["identificationNumber", "N.º de identificación"], ["customerTypes", "Tipos de cliente (separa con coma)"],
          ["customerLabels", "Etiquetas (separa con coma)"],
          ["referredBy", "Referido por"], ["acquisitionMedium", "Medio de captación"], ["country", "País"],
          ["department", "Departamento"], ["city", "Ciudad"], ["address", "Dirección"],
        ] as const).map(([key, label, required, type]) => <label key={key} className="text-xs text-muted-foreground">{label}
          {key === "customerTypes" ? <input type="text" placeholder="Comprador, Propietario" className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground"
            value={newContact.customerTypes.join(", ")} onChange={(event) => setNewContact((value) => ({ ...value,
              customerTypes: event.target.value.split(",").map((customerType) => customerType.trim()).filter(Boolean) }))} /> :
          key === "customerLabels" ? <input type="text" placeholder="VIP, Seguimiento" className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground"
            value={newContact.customerLabels.join(", ")} onChange={(event) => setNewContact((value) => ({ ...value,
              customerLabels: event.target.value.split(",").map((label) => label.trim()).filter(Boolean) }))} /> :
          <input type={type ?? "text"} required={required} maxLength={key === "address" ? 500 : 200}
            className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground"
            value={newContact[key]} onChange={(event) => setNewContact((value) => ({ ...value, [key]: event.target.value }))} />}
        </label>)}
        <label className="text-xs text-muted-foreground">Embudo
          <select className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={newContact.customerFunnel} onChange={(event) => setNewContact((value) => ({ ...value, customerFunnel: event.target.value }))}>
            <option value="Clientes">Clientes</option>
          </select>
        </label>
        {user?.rol === "Administrador" && <label className="text-xs text-muted-foreground">Asesor encargado
          <select className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={newContact.advisorId} onChange={(event) => setNewContact((value) => ({ ...value, advisorId: event.target.value }))}>
            <option value="">Sin asignar</option>{advisors.map((advisor) => <option key={advisor.id} value={advisor.id}>{advisor.name}</option>)}
          </select>
        </label>}
        <label className="flex items-center gap-2 text-sm text-foreground sm:col-span-2 lg:col-span-3">
          <input type="checkbox" checked={includeInitialDemand} onChange={(event) => setIncludeInitialDemand(event.target.checked)} />
          Registrar también la demanda inicial
        </label>
        {includeInitialDemand && <div className="grid gap-3 rounded-md border border-border p-4 sm:col-span-2 sm:grid-cols-2 lg:col-span-3 lg:grid-cols-4">
          <label className="text-xs text-muted-foreground">Operación<select className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandOperation} onChange={(event) => setDemandOperation(event.target.value)}><option value="Venta">Venta</option><option value="Arriendo">Arriendo</option></select></label>
          <label className="text-xs text-muted-foreground">Tipo de inmueble<select className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandType} onChange={(event) => setDemandType(event.target.value)}><option value="">Cualquiera</option><option value="Apartamento">Apartamento</option><option value="Casa">Casa</option><option value="Local">Local</option><option value="Oficina">Oficina</option><option value="Lote">Lote</option><option value="Bodega">Bodega</option><option value="Finca">Finca</option></select></label>
          {([["city", "Ciudad", demandCity, setDemandCity], ["country", "País", demandCountry, setDemandCountry], ["department", "Departamento", demandDepartment, setDemandDepartment], ["neighborhood", "Barrio", demandNeighborhood, setDemandNeighborhood]] as const).map(([key, label, value, setValue]) => <label key={key} className="text-xs text-muted-foreground">{label}<input required={key === "city" || key === "country"} maxLength={100} className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={value} onChange={(event) => setValue(event.target.value)} /></label>)}
          {([["minPrice", "Presupuesto mínimo", demandMinPrice, setDemandMinPrice], ["maxPrice", "Presupuesto máximo", demandMaxPrice, setDemandMaxPrice], ["minArea", "Área mínima", demandMinArea, setDemandMinArea], ["maxArea", "Área máxima", demandMaxArea, setDemandMaxArea], ["bedrooms", "Habitaciones mínimas", demandBedrooms, setDemandBedrooms], ["bathrooms", "Baños mínimos", demandBathrooms, setDemandBathrooms], ["parking", "Garajes mínimos", demandParkingSpaces, setDemandParkingSpaces]] as const).map(([key, label, value, setValue]) => <label key={key} className="text-xs text-muted-foreground">{label}<input type="number" min="0" step={key.includes("Area") || key.includes("Price") ? "0.01" : "1"} className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={value} onChange={(event) => setValue(event.target.value)} /></label>)}
          <label className="text-xs text-muted-foreground">Características internas (separa con coma)<input className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandInternalFeatures} onChange={(event) => setDemandInternalFeatures(event.target.value)} /></label>
          <label className="text-xs text-muted-foreground">Características externas (separa con coma)<input className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandExternalFeatures} onChange={(event) => setDemandExternalFeatures(event.target.value)} /></label>
          <label className="text-xs text-muted-foreground">Moneda<input required pattern="[A-Z]{3}" minLength={3} maxLength={3} className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm uppercase text-foreground" value={demandCurrency} onChange={(event) => setDemandCurrency(event.target.value.toUpperCase())} /></label>
          <label className="text-xs text-muted-foreground sm:col-span-2 lg:col-span-4">Detalles de la propiedad<textarea maxLength={2000} rows={3} className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandNotes} onChange={(event) => setDemandNotes(event.target.value)} /></label>
        </div>}
        <label className="text-xs text-muted-foreground sm:col-span-2 lg:col-span-3">Datos adicionales / observaciones
          <textarea maxLength={4000} rows={3} className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={newContact.additionalData} onChange={(event) => setNewContact((value) => ({ ...value, additionalData: event.target.value }))} />
        </label>
        {createContact.isError && <p role="alert" className="text-sm text-destructive sm:col-span-3">{createContact.error.message}</p>}
        {initialDemandError && <p role="alert" className="text-sm text-destructive sm:col-span-3">{initialDemandError}</p>}
        <div className="flex gap-2 sm:col-span-3"><button disabled={createContact.isPending} className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground disabled:opacity-50">{createContact.isPending ? "Guardando…" : "Guardar cliente"}</button>
          <button type="button" className="rounded-md border border-border px-4 py-2 text-sm text-foreground" onClick={() => setShowCreateContact(false)}>Cancelar</button></div>
      </form>}

      <section className="crm-panel p-5">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div><h2 className="font-display text-xl text-foreground">Cruce de clientes e inmuebles</h2>
            <p className="mt-1 text-sm text-muted-foreground">Busca demandas por cliente, tipo, estado, asesor y fecha. Las coincidencias cuentan inmuebles propios.</p></div>
        </div>
        <form className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4" onSubmit={(event) => { event.preventDefault(); setCrossPage(1); setCrossSearched(true) }}>
          <label className="text-xs text-muted-foreground">Nombre del cliente<input className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={crossFilters.q} onChange={(event) => setCrossFilters((value) => ({ ...value, q: event.target.value }))} /></label>
          <label className="text-xs text-muted-foreground">Tipo de cliente<input className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" placeholder="Cualquiera o personalizado" value={crossFilters.customerType} onChange={(event) => setCrossFilters((value) => ({ ...value, customerType: event.target.value }))} /></label>
          <label className="text-xs text-muted-foreground">Estado del cliente<select className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={crossFilters.customerStatus} onChange={(event) => setCrossFilters((value) => ({ ...value, customerStatus: event.target.value }))}><option value="">Todos</option><option value="Nuevo">Nuevo</option><option value="EnProceso">En proceso</option><option value="Convertido">Convertido</option><option value="Recuperado">Recuperado</option><option value="Perdido">Perdido</option></select></label>
          <label className="text-xs text-muted-foreground">Embudo<select className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={crossFilters.customerFunnel} onChange={(event) => setCrossFilters((value) => ({ ...value, customerFunnel: event.target.value }))}><option value="">Todos</option><option value="Clientes">Clientes</option></select></label>
          {user?.rol === "Administrador" && <label className="text-xs text-muted-foreground">Usuario encargado<select className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={crossFilters.advisorId} onChange={(event) => setCrossFilters((value) => ({ ...value, advisorId: event.target.value }))}><option value="">Todos</option>{advisors.map((advisor) => <option key={advisor.id} value={advisor.id}>{advisor.name}</option>)}</select></label>}
          {(["contactFrom", "contactTo", "demandFrom", "demandTo"] as const).map((key) => <label key={key} className="text-xs text-muted-foreground">{key === "contactFrom" ? "Cliente desde" : key === "contactTo" ? "Cliente hasta" : key === "demandFrom" ? "Demanda desde" : "Demanda hasta"}<input type="date" className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={crossFilters[key]} onChange={(event) => setCrossFilters((value) => ({ ...value, [key]: event.target.value }))} /></label>)}
          <div className="flex items-end"><button className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground">Buscar</button></div>
        </form>
        {crossQuery.isError && <p role="alert" className="mt-3 text-sm text-destructive">No se pudo consultar el cruce de demandas.</p>}
        {crossSearched && <div className="mt-4 space-y-2">
          {crossQuery.isPending && <p className="text-sm text-muted-foreground">Buscando demandas…</p>}
          {crossResult?.items.map((item) => <article key={item.demand.id} className="flex flex-wrap items-center justify-between gap-3 rounded-md border border-border p-3">
            <div><button type="button" className="font-semibold text-primary underline" onClick={() => {
              setSearch(`${item.contactName} ${item.contactSurname ?? ""}`.trim()); setCreatedFrom(""); setCreatedTo("");
              setCustomerTypeFilter(""); setCustomerStatusFilter(""); setAdvisorFilter(""); setPage(1); setSelectedId(item.demand.contactId)
            }}>{item.contactName} {item.contactSurname}</button>
              <p className="text-xs text-muted-foreground">{item.customerType ?? "Sin tipo"} · {item.customerStatus} · cliente {new Date(item.contactCreatedAt).toLocaleDateString("es-CO")}</p>
              <p className="mt-1 text-sm text-foreground">{item.demand.operation} · {item.demand.propertyType ?? "Cualquier inmueble"} · {item.demand.city}{item.demand.department ? `, ${item.demand.department}` : ""}, {item.demand.country}{item.demand.neighborhood ? ` · ${item.demand.neighborhood}` : ""}</p>
              <p className="text-xs text-muted-foreground">Demanda {new Date(item.demand.createdAt).toLocaleDateString("es-CO")} · {item.demand.currency} {item.demand.minimumPrice?.toLocaleString("es-CO") ?? "0"}–{item.demand.maximumPrice?.toLocaleString("es-CO") ?? "sin máximo"}</p>
            </div>
            <span className="rounded-full bg-primary/10 px-3 py-1.5 text-sm font-semibold text-primary">Propios: {item.ownPropertyMatches}</span>
          </article>)}
          {crossResult?.total === 0 && <p className="text-sm text-muted-foreground">No hay demandas que coincidan con estos filtros.</p>}
          {crossResult && crossResult.total > crossResult.pageSize && <div className="flex items-center justify-between text-sm text-muted-foreground">
            <span>{crossResult.total} demandas</span><div className="flex gap-2"><button type="button" disabled={crossPage <= 1} className="rounded border border-border px-3 py-1 disabled:opacity-40" onClick={() => setCrossPage((page) => page - 1)}>Anterior</button><button type="button" disabled={crossPage * crossResult.pageSize >= crossResult.total} className="rounded border border-border px-3 py-1 disabled:opacity-40" onClick={() => setCrossPage((page) => page + 1)}>Siguiente</button></div>
          </div>}
        </div>}
      </section>

      <section className="crm-panel p-5">
        <h2 className="font-display text-xl text-foreground">Importar clientes</h2>
        <p className="mt-1 text-sm text-muted-foreground">CSV, XLS o XLSX. Hasta 10 MB. La vista previa valida nombre, email y teléfono antes de guardar.</p>
        <form className="mt-4 flex flex-wrap items-end gap-3" onSubmit={async (event) => {
          event.preventDefault()
          if (!importFile) return
          setImporting(true)
          setImportError("")
          try {
            const preview = await api.previewClientImport(
              importFile,
              importColumns.length ? Number(nameColumn) : undefined,
              surnameColumn === "" ? undefined : Number(surnameColumn),
              emailColumn === "" ? undefined : Number(emailColumn),
              phoneColumn === "" ? undefined : Number(phoneColumn),
              canReceiveEmailColumn === "" ? undefined : Number(canReceiveEmailColumn),
              customerTypeColumn === "" ? undefined : Number(customerTypeColumn),
              customerStatusColumn === "" ? undefined : Number(customerStatusColumn),
              addressColumn === "" ? undefined : Number(addressColumn),
              countryColumn === "" ? undefined : Number(countryColumn),
              departmentColumn === "" ? undefined : Number(departmentColumn),
              cityColumn === "" ? undefined : Number(cityColumn),
              landlineColumn === "" ? undefined : Number(landlineColumn),
              dateOfBirthColumn === "" ? undefined : Number(dateOfBirthColumn),
              identificationNumberColumn === "" ? undefined : Number(identificationNumberColumn),
              referredByColumn === "" ? undefined : Number(referredByColumn),
              acquisitionMediumColumn === "" ? undefined : Number(acquisitionMediumColumn),
              additionalDataColumn === "" ? undefined : Number(additionalDataColumn),
              customerLabelsColumn === "" ? undefined : Number(customerLabelsColumn),
              customerFunnelColumn === "" ? undefined : Number(customerFunnelColumn),
            )
            setImportColumns(preview.sourceColumns)
            setImportPreview(importColumns.length ? preview : null)
            if (!importColumns.length) {
              const normalizeHeader = (value: string) => value.normalize("NFD").replace(/\p{Diacritic}/gu, "").toLowerCase().replace(/[^a-z0-9]+/g, " ").trim()
              const findColumn = (terms: string[]) => {
                const aliases = terms.map(normalizeHeader)
                const exact = preview.sourceColumns.findIndex((column) => aliases.includes(normalizeHeader(column)))
                return exact >= 0 ? exact : preview.sourceColumns.findIndex((column) => {
                  const header = normalizeHeader(column)
                  return aliases.some((term) => term.length >= 5 && term !== "telefono" && header.includes(term))
                })
              }
              const nameIndex = findColumn(["nombre", "name"])
              setNameColumn(nameIndex < 0 ? "" : String(nameIndex))
              const surnameIndex = findColumn(["apellido", "surname", "last name"])
              const emailIndex = findColumn(["email", "correo"])
              const phoneIndex = findColumn(["telefono movil", "telefono", "celular", "phone", "mobile"])
              const consentIndex = findColumn(["recibe correo", "email consent", "email permission"])
              const typeIndex = findColumn(["tipo de cliente", "tipo cliente", "customer type"])
              const labelsIndex = findColumn(["etiqueta", "labels", "tags"])
              const funnelIndex = findColumn(["embudo", "funnel"])
              const statusIndex = findColumn(["estado del cliente", "estado cliente", "customer status"])
              const addressIndex = findColumn(["direccion", "dirección", "address"])
              const countryIndex = findColumn(["pais", "país", "country"])
              const departmentIndex = findColumn(["departamento", "department"])
              const cityIndex = findColumn(["ciudad", "city"])
              const landlineIndex = findColumn(["telefono fijo", "teléfono fijo", "landline"])
              const dateOfBirthIndex = findColumn(["fecha de nacimiento", "nacimiento", "birth"])
              const identificationIndex = findColumn(["identificacion", "identificación", "documento", "identification"])
              const referredByIndex = findColumn(["referido por", "referred by"])
              const acquisitionIndex = findColumn(["medio de captacion", "medio de captación", "acquisition"])
              const additionalDataIndex = findColumn(["datos adicionales", "additional data"])
              setSurnameColumn(surnameIndex < 0 ? "" : String(surnameIndex))
              setEmailColumn(emailIndex < 0 ? "" : String(emailIndex))
              setPhoneColumn(phoneIndex < 0 ? "" : String(phoneIndex))
              setCanReceiveEmailColumn(consentIndex < 0 ? "" : String(consentIndex))
              setCustomerTypeColumn(typeIndex < 0 ? "" : String(typeIndex))
              setCustomerLabelsColumn(labelsIndex < 0 ? "" : String(labelsIndex))
              setCustomerFunnelColumn(funnelIndex < 0 ? "" : String(funnelIndex))
              setCustomerStatusColumn(statusIndex < 0 ? "" : String(statusIndex))
              setAddressColumn(addressIndex < 0 ? "" : String(addressIndex))
              setCountryColumn(countryIndex < 0 ? "" : String(countryIndex))
              setDepartmentColumn(departmentIndex < 0 ? "" : String(departmentIndex))
              setCityColumn(cityIndex < 0 ? "" : String(cityIndex))
              setLandlineColumn(landlineIndex < 0 ? "" : String(landlineIndex))
              setDateOfBirthColumn(dateOfBirthIndex < 0 ? "" : String(dateOfBirthIndex))
              setIdentificationNumberColumn(identificationIndex < 0 ? "" : String(identificationIndex))
              setReferredByColumn(referredByIndex < 0 ? "" : String(referredByIndex))
              setAcquisitionMediumColumn(acquisitionIndex < 0 ? "" : String(acquisitionIndex))
              setAdditionalDataColumn(additionalDataIndex < 0 ? "" : String(additionalDataIndex))
              setImportPreview(null)
            }
          } catch (error) {
            setImportError(error instanceof Error ? error.message : "No se pudo leer el archivo.")
          } finally {
            setImporting(false)
          }
        }}>
          <label className="text-xs text-muted-foreground">Archivo
            <input className="mt-1 block text-sm text-foreground" type="file" accept=".csv,.xls,.xlsx" required onChange={(event) => {
              setImportFile(event.target.files?.[0] ?? null)
              setImportColumns([])
              setImportPreview(null)
              setImportResult(null)
              setNameColumn("")
              setSurnameColumn("")
              setEmailColumn("")
              setPhoneColumn("")
              setCanReceiveEmailColumn("")
              setCustomerTypeColumn("")
              setCustomerLabelsColumn("")
              setCustomerFunnelColumn("")
              setCustomerStatusColumn("")
              setAddressColumn("")
              setCountryColumn("")
              setDepartmentColumn("")
              setCityColumn("")
              setLandlineColumn("")
              setDateOfBirthColumn("")
              setIdentificationNumberColumn("")
              setReferredByColumn("")
              setAcquisitionMediumColumn("")
              setAdditionalDataColumn("")
              setImportError("")
            }} />
          </label>
          {importColumns.length > 0 && <>
            <ImportColumnSelect label="Nombre *" value={nameColumn} columns={importColumns} onChange={(value) => updateImportMapping(setNameColumn, value)} />
            <ImportColumnSelect label="Apellidos" value={surnameColumn} columns={importColumns} onChange={(value) => updateImportMapping(setSurnameColumn, value)} optional />
            <ImportColumnSelect label="Email" value={emailColumn} columns={importColumns} onChange={(value) => updateImportMapping(setEmailColumn, value)} optional />
            <ImportColumnSelect label="Teléfono" value={phoneColumn} columns={importColumns} onChange={(value) => updateImportMapping(setPhoneColumn, value)} optional />
            <ImportColumnSelect label="Recibe correo" value={canReceiveEmailColumn} columns={importColumns} onChange={(value) => updateImportMapping(setCanReceiveEmailColumn, value)} optional />
            <ImportColumnSelect label="Tipo de cliente" value={customerTypeColumn} columns={importColumns} onChange={(value) => updateImportMapping(setCustomerTypeColumn, value)} optional />
            <ImportColumnSelect label="Etiquetas" value={customerLabelsColumn} columns={importColumns} onChange={(value) => updateImportMapping(setCustomerLabelsColumn, value)} optional />
            <ImportColumnSelect label="Embudo" value={customerFunnelColumn} columns={importColumns} onChange={(value) => updateImportMapping(setCustomerFunnelColumn, value)} optional />
            <ImportColumnSelect label="Estado del cliente" value={customerStatusColumn} columns={importColumns} onChange={(value) => updateImportMapping(setCustomerStatusColumn, value)} optional />
            <ImportColumnSelect label="Dirección" value={addressColumn} columns={importColumns} onChange={(value) => updateImportMapping(setAddressColumn, value)} optional />
            <ImportColumnSelect label="País" value={countryColumn} columns={importColumns} onChange={(value) => updateImportMapping(setCountryColumn, value)} optional />
            <ImportColumnSelect label="Departamento" value={departmentColumn} columns={importColumns} onChange={(value) => updateImportMapping(setDepartmentColumn, value)} optional />
            <ImportColumnSelect label="Ciudad" value={cityColumn} columns={importColumns} onChange={(value) => updateImportMapping(setCityColumn, value)} optional />
            <ImportColumnSelect label="Teléfono fijo" value={landlineColumn} columns={importColumns} onChange={(value) => updateImportMapping(setLandlineColumn, value)} optional />
            <ImportColumnSelect label="Fecha de nacimiento" value={dateOfBirthColumn} columns={importColumns} onChange={(value) => updateImportMapping(setDateOfBirthColumn, value)} optional />
            <ImportColumnSelect label="Identificación" value={identificationNumberColumn} columns={importColumns} onChange={(value) => updateImportMapping(setIdentificationNumberColumn, value)} optional />
            <ImportColumnSelect label="Referido por" value={referredByColumn} columns={importColumns} onChange={(value) => updateImportMapping(setReferredByColumn, value)} optional />
            <ImportColumnSelect label="Medio de captación" value={acquisitionMediumColumn} columns={importColumns} onChange={(value) => updateImportMapping(setAcquisitionMediumColumn, value)} optional />
            <ImportColumnSelect label="Datos adicionales" value={additionalDataColumn} columns={importColumns} onChange={(value) => updateImportMapping(setAdditionalDataColumn, value)} optional />
            <p className="text-xs text-muted-foreground">La columna se conserva para referencia, pero importar un valor afirmativo no activa correos automáticos: hace falta registrar evidencia verificable del permiso.</p>
            {user?.rol === "Administrador" && <label className="text-xs text-muted-foreground">Asignar a
              <select className="mt-1 block min-w-44 rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={ownerAdvisorId} onChange={(event) => updateImportMapping(setOwnerAdvisorId, event.target.value)}>
                <option value="">Solo administradores</option>
                {advisors.map((advisor) => <option key={advisor.id} value={advisor.id}>{advisor.name}</option>)}
              </select>
            </label>}
          </>}
          <button disabled={!importFile || importing} className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground disabled:opacity-50">
            {importing ? "Revisando…" : importColumns.length ? "Actualizar vista previa" : "Leer columnas"}
          </button>
        </form>
        {importError && <p role="alert" className="mt-3 text-sm text-destructive">{importError}</p>}
        {importPreview && <div className="mt-4 space-y-3">
          <p className="text-sm text-foreground">{importPreview.totalRows} filas · {importPreview.validRows} válidas · {importPreview.invalidRows} con errores · {importPreview.duplicateRows} duplicadas</p>
          <div className="overflow-x-auto"><table className="w-full text-left text-sm"><thead><tr className="border-b border-border text-muted-foreground"><th className="p-2">Fila</th><th className="p-2">Nombre</th><th className="p-2">Apellidos</th><th className="p-2">Email</th><th className="p-2">Teléfono</th><th className="p-2">Recibe correo</th><th className="p-2">Resultado</th></tr></thead><tbody>
            {importPreview.rows.map((row) => <tr key={row.rowNumber} className="border-b border-border"><td className="p-2">{row.rowNumber}</td><td className="p-2">{row.name || "—"}</td><td className="p-2">{row.surname || "—"}</td><td className="p-2">{row.email || "—"}</td><td className="p-2">{row.phone || "—"}</td><td className="p-2">Permiso sin activar</td><td className="p-2">{row.error || (row.duplicate ? "Email ya existe" : "Listo")}<details className="mt-1"><summary className="cursor-pointer text-primary">Campos mapeados</summary><dl className="mt-2 space-y-1">{Object.entries(row.mappedValues).map(([field, value]) => <div key={field}><dt className="inline font-medium">{field}: </dt><dd className="inline break-all">{value || "—"}</dd></div>)}</dl></details></td></tr>)}
          </tbody></table></div>
          {importPreview.totalRows > importPreview.rows.length && <p className="text-xs text-muted-foreground">Se muestran las primeras {importPreview.rows.length} filas.</p>}
          <button type="button" disabled={importing || !importRequestIdReady || importPreview.validRows === 0} className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground disabled:opacity-50" onClick={async () => {
            if (!importFile) return
            setImporting(true)
            setImportError("")
            try {
              const result = await api.confirmClientImport(importFile, Number(nameColumn),
                surnameColumn === "" ? undefined : Number(surnameColumn),
                emailColumn === "" ? undefined : Number(emailColumn),
                phoneColumn === "" ? undefined : Number(phoneColumn),
                canReceiveEmailColumn === "" ? undefined : Number(canReceiveEmailColumn),
                ownerAdvisorId || undefined,
                customerTypeColumn === "" ? undefined : Number(customerTypeColumn),
                customerStatusColumn === "" ? undefined : Number(customerStatusColumn),
                addressColumn === "" ? undefined : Number(addressColumn),
                countryColumn === "" ? undefined : Number(countryColumn),
                departmentColumn === "" ? undefined : Number(departmentColumn),
                cityColumn === "" ? undefined : Number(cityColumn), importRequestId,
                landlineColumn === "" ? undefined : Number(landlineColumn),
                dateOfBirthColumn === "" ? undefined : Number(dateOfBirthColumn),
                identificationNumberColumn === "" ? undefined : Number(identificationNumberColumn),
                referredByColumn === "" ? undefined : Number(referredByColumn),
                acquisitionMediumColumn === "" ? undefined : Number(acquisitionMediumColumn),
                additionalDataColumn === "" ? undefined : Number(additionalDataColumn),
                customerLabelsColumn === "" ? undefined : Number(customerLabelsColumn),
                customerFunnelColumn === "" ? undefined : Number(customerFunnelColumn))
              setImportResult(result)
              await contactsQuery.refetch()
            } catch (error) {
              setImportError(error instanceof Error ? error.message : "No se pudo importar el archivo.")
            } finally {
              setImporting(false)
            }
          }}>{importing ? "Importando…" : `Importar ${importPreview.validRows - importPreview.duplicateRows} clientes`}</button>
        </div>}
        {importResult && <div className="mt-3 flex flex-wrap items-center gap-3 text-sm">
          <p role="status" className="text-emerald-700">Importados: {importResult.importedRows} · con errores: {importResult.invalidRows} · duplicados: {importResult.duplicateRows}</p>
          {importResult.errors.length > 0 && <button type="button" className="rounded-md border border-border px-3 py-1 text-foreground"
            onClick={() => {
              const quote = (value: string) => `"${value.replaceAll('"', '""')}"`
              const csv = ["fila,error", ...importResult.errors.map((item) => `${item.rowNumber},${quote(item.error)}`)].join("\r\n")
              const url = URL.createObjectURL(new Blob(["\uFEFF", csv], { type: "text/csv;charset=utf-8" }))
              const anchor = document.createElement("a")
              anchor.href = url
              anchor.download = "errores-importacion-clientes.csv"
              anchor.click()
              URL.revokeObjectURL(url)
            }}>Descargar errores CSV ({importResult.errors.length})</button>}
        </div>}
      </section>

      <div className="grid gap-5 xl:grid-cols-[340px_minmax(0,1fr)]">
        <section className="crm-panel overflow-hidden">
          <div className="space-y-3 border-b border-border p-4">
            <label className="relative block">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <input
                className="w-full rounded-md border border-border bg-background py-2.5 pl-9 pr-3 text-sm outline-none focus:border-accent"
                placeholder="Buscar contacto"
                value={search}
                onChange={(event) => { setSearch(event.target.value); setPage(1) }}
              />
            </label>
            <div className="grid grid-cols-2 gap-2">
              <label className="text-xs text-muted-foreground">Desde
                <input type="date" value={createdFrom} max={createdTo || undefined} onChange={(event) => { setCreatedFrom(event.target.value); setPage(1) }} className="mt-1 block w-full rounded-sm border border-input bg-background px-2 py-2 text-foreground" />
              </label>
              <label className="text-xs text-muted-foreground">Hasta
                <input type="date" value={createdTo} min={createdFrom || undefined} onChange={(event) => { setCreatedTo(event.target.value); setPage(1) }} className="mt-1 block w-full rounded-sm border border-input bg-background px-2 py-2 text-foreground" />
              </label>
            </div>
            <label className="block text-xs text-muted-foreground">Tipo de cliente
              <input value={customerTypeFilter} onChange={(event) => { setCustomerTypeFilter(event.target.value); setPage(1) }} placeholder="Cualquiera o personalizado" className="mt-1 block w-full rounded-sm border border-input bg-background px-2 py-2 text-foreground" />
            </label>
            <label className="block text-xs text-muted-foreground">Estado del cliente
              <select value={customerStatusFilter} onChange={(event) => { setCustomerStatusFilter(event.target.value); setPage(1) }} className="mt-1 block w-full rounded-sm border border-input bg-background px-2 py-2 text-foreground">
                <option value="">Todos</option>
                <option value="Nuevo">Nuevo</option>
                <option value="EnProceso">En proceso</option>
                <option value="Convertido">Convertido</option>
                <option value="Recuperado">Recuperado</option>
                <option value="Perdido">Perdido</option>
              </select>
            </label>
            <label className="block text-xs text-muted-foreground">Embudo
              <select value={customerFunnelFilter} onChange={(event) => { setCustomerFunnelFilter(event.target.value); setPage(1) }} className="mt-1 block w-full rounded-sm border border-input bg-background px-2 py-2 text-foreground">
                <option value="">Todos</option><option value="Clientes">Clientes</option>
              </select>
            </label>
            {user?.rol === "Administrador" && <label className="block text-xs text-muted-foreground">Usuario encargado
              <select value={advisorFilter} onChange={(event) => { setAdvisorFilter(event.target.value); setPage(1) }} className="mt-1 block w-full rounded-sm border border-input bg-background px-2 py-2 text-foreground">
                <option value="">Todos</option>
                {advisors.map((advisor) => <option key={advisor.id} value={advisor.id}>{advisor.name}</option>)}
              </select>
            </label>}
            <div className="flex flex-wrap items-center justify-between gap-2 text-xs text-muted-foreground">
              <label className="flex items-center gap-2">
                <input type="checkbox" checked={contacts.length > 0 && contacts.every((item) => selectedContactIds.includes(item.id))} onChange={(event) => setSelectedContactIds(event.target.checked ? contacts.map((item) => item.id) : [])} />
                Seleccionar página
              </label>
              <button type="button" disabled={!selectedContactIds.length} className="inline-flex items-center gap-1 rounded border border-border px-2 py-1.5 font-semibold text-foreground disabled:opacity-40" onClick={() => exportContactsCsv(contacts.filter((item) => selectedContactIds.includes(item.id)))}>
                <Download className="h-3.5 w-3.5" /> Exportar ({selectedContactIds.length})
              </button>
              <button type="button" disabled={exportingContacts} className="inline-flex items-center gap-1 rounded border border-border px-2 py-1.5 font-semibold text-foreground disabled:opacity-40" onClick={async () => {
                setExportingContacts(true); setExportContactsError("")
                try {
                  const blob = await api.downloadContactsCsv({ q: search, from: createdFrom, to: createdTo,
                    customerType: customerTypeFilter, customerStatus: customerStatusFilter,
                    customerFunnel: customerFunnelFilter, advisorId: advisorFilter })
                  const url = URL.createObjectURL(blob)
                  const link = document.createElement("a"); link.href = url; link.download = "clientes-enlyce.csv"; link.click()
                  window.setTimeout(() => URL.revokeObjectURL(url), 1000)
                } catch (error) {
                  setExportContactsError(error instanceof Error ? error.message : "No se pudo exportar.")
                } finally { setExportingContacts(false) }
              }}>
                <Download className="h-3.5 w-3.5" /> {exportingContacts ? "Exportando…" : "Exportar resultados filtrados"}
              </button>
              <button type="button" disabled={emailingContacts || totalContacts === 0} className="inline-flex items-center gap-1 rounded border border-border px-2 py-1.5 font-semibold text-foreground disabled:opacity-40" onClick={async () => {
                setEmailingContacts(true); setExportContactsError(""); setExportContactsNotice("")
                try {
                  const result = await api.emailContactsCsv({ q: search, from: createdFrom, to: createdTo,
                    customerType: customerTypeFilter, customerStatus: customerStatusFilter,
                    customerFunnel: customerFunnelFilter, advisorId: advisorFilter })
                  setExportContactsNotice(`Se enviaron ${result.count} clientes al correo de tu cuenta.`)
                } catch (error) {
                  setExportContactsError(error instanceof Error ? error.message : "No se pudo enviar el archivo.")
                } finally { setEmailingContacts(false) }
              }}>
                <Mail className="h-3.5 w-3.5" /> {emailingContacts ? "Enviando…" : "Enviar resultados a mi correo"}
              </button>
            </div>
            {exportContactsError && <p role="alert" className="text-xs text-destructive">{exportContactsError}</p>}
            {exportContactsNotice && <p role="status" className="text-xs text-muted-foreground">{exportContactsNotice}</p>}
          </div>
          <div className="max-h-[70vh] overflow-y-auto">
            {contactsQuery.isPending && <p className="p-5 text-sm text-muted-foreground">Cargando contactos…</p>}
            {contactsQuery.isError && <p role="alert" className="p-5 text-sm text-destructive">No se pudieron cargar los contactos.</p>}
            {!contactsQuery.isPending && contacts.length === 0 && (
              <p className="p-5 text-sm text-muted-foreground">Todavía no hay contactos para mostrar.</p>
            )}
            {contacts.map((item) => (
              <div key={item.id} className={`flex items-start gap-3 border-b border-border px-4 py-4 transition-colors hover:bg-muted/60 ${selectedId === item.id ? "bg-muted" : ""}`}>
                <input aria-label={`Seleccionar ${[item.name, item.surname].filter(Boolean).join(" ")}`} type="checkbox" checked={selectedContactIds.includes(item.id)} onChange={(event) => setSelectedContactIds((ids) => event.target.checked ? [...ids, item.id] : ids.filter((id) => id !== item.id))} />
                <button type="button" onClick={() => setSelectedId(item.id)} aria-pressed={selectedId === item.id} className="min-w-0 flex-1 text-left">
                  <span className="block font-semibold text-foreground">{[item.name, item.surname].filter(Boolean).join(" ")}</span>
                  <span className="mt-1 block truncate text-xs text-muted-foreground">{item.email || "Sin email"}</span>
                  {item.phone && <span className="mt-1 block text-xs text-muted-foreground">{item.phone}</span>}
                  {(item.customerTypes?.length ?? 0) > 0 && <span className="mt-1 block truncate text-xs text-muted-foreground">{item.customerTypes.join(", ")}</span>}
                  {(item.customerLabels?.length ?? 0) > 0 && <span className="mt-1 block truncate text-xs text-accent">{item.customerLabels.join(", ")}</span>}
                </button>
              </div>
            ))}
          </div>
          <div className="flex items-center justify-between border-t border-border px-4 py-3 text-xs">
            <span className="text-muted-foreground">{totalContacts} contactos · página {page} de {pageCount}</span>
            <span className="flex gap-1">
              <button aria-label="Página anterior" disabled={page <= 1} onClick={() => setPage((value) => value - 1)} className="rounded border border-border p-1.5 disabled:opacity-40"><ChevronLeft className="h-4 w-4" /></button>
              <button aria-label="Página siguiente" disabled={page >= pageCount} onClick={() => setPage((value) => value + 1)} className="rounded border border-border p-1.5 disabled:opacity-40"><ChevronRight className="h-4 w-4" /></button>
            </span>
          </div>
        </section>

        <section className="space-y-5">
          {!selectedId && <div className="crm-panel p-8 text-sm text-muted-foreground">Selecciona un contacto para abrir su expediente.</div>}
          {detailQuery.isError && <div role="alert" className="crm-panel p-6 text-sm text-destructive">No se pudo cargar el expediente.</div>}
          {detailQuery.isPending && selectedId && <div className="crm-panel p-6 text-sm text-muted-foreground">Cargando expediente…</div>}
          {contact && detailQuery.data && (
            <>
              <div className="crm-panel border-t-4 border-t-accent p-6">
                <div className="flex flex-wrap items-start justify-between gap-5">
                  <div className="flex items-start gap-4">
                    <span className="rounded-full bg-accent/10 p-3 text-accent"><UserRound className="h-5 w-5" /></span>
                    <div>
                      <p className="crm-eyebrow">Ficha de contacto</p>
                      <h2 className="mt-1 font-display text-2xl text-foreground">{[contact.name, contact.surname].filter(Boolean).join(" ")}</h2>
                      {contact.email && <p className="mt-1 flex items-center gap-2 text-sm text-muted-foreground"><Mail className="h-4 w-4" />{contact.email}</p>}
                      <p className="mt-1 flex items-center gap-2 text-sm text-muted-foreground"><Phone className="h-4 w-4" />{contact.phone || "Sin teléfono"}</p>
                    </div>
                  </div>
                  <form
                    className="flex flex-wrap items-end gap-2"
                    onSubmit={(event) => {
                      event.preventDefault()
                      updateContact.mutate({ id: contact.id, name, surname: surname || undefined,
                        phone: phone || undefined, customerTypes, customerLabels, customerStatus, customerFunnel,
                        address: address || undefined, country, department: department || undefined, city: city || undefined,
                        landline: landline || undefined, dateOfBirth: dateOfBirth || undefined,
                        identificationNumber: identificationNumber || undefined, referredBy: referredBy || undefined,
                        acquisitionMedium: acquisitionMedium || undefined, additionalData: additionalData || undefined })
                    }}
                  >
                    <label className="text-xs text-muted-foreground">Nombre
                      <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={name} onChange={(event) => setName(event.target.value)} maxLength={200} required />
                    </label>
                    <label className="text-xs text-muted-foreground">Apellidos
                      <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={surname} onChange={(event) => setSurname(event.target.value)} maxLength={200} />
                    </label>
                    <label className="text-xs text-muted-foreground">Tipos de cliente (separa con coma)
                      <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={customerTypes.join(", ")}
                        onChange={(event) => setCustomerTypes(event.target.value.split(",").map((customerType) => customerType.trim()).filter(Boolean))} />
                    </label>
                    <label className="text-xs text-muted-foreground">Etiquetas (separa con coma)
                      <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={customerLabels.join(", ")}
                        onChange={(event) => setCustomerLabels(event.target.value.split(",").map((label) => label.trim()).filter(Boolean))} />
                    </label>
                    <label className="text-xs text-muted-foreground">Estado
                      <select className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={customerStatus} onChange={(event) => setCustomerStatus(event.target.value)}>
                        <option value="Nuevo">Nuevo</option><option value="EnProceso">En proceso</option><option value="Convertido">Convertido</option><option value="Recuperado">Recuperado</option><option value="Perdido">Perdido</option>
                      </select>
                    </label>
                    <label className="text-xs text-muted-foreground">Embudo
                      <select className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={customerFunnel} onChange={(event) => setCustomerFunnel(event.target.value)}>
                        <option value="Clientes">Clientes</option>
                      </select>
                    </label>
                    <label className="text-xs text-muted-foreground">País
                      <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={country} onChange={(event) => setCountry(event.target.value)} maxLength={100} />
                    </label>
                    <label className="text-xs text-muted-foreground">Departamento
                      <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={department} onChange={(event) => setDepartment(event.target.value)} maxLength={100} />
                    </label>
                    <label className="text-xs text-muted-foreground">Ciudad
                      <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={city} onChange={(event) => setCity(event.target.value)} maxLength={100} />
                    </label>
                    <label className="text-xs text-muted-foreground">Dirección
                      <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={address} onChange={(event) => setAddress(event.target.value)} maxLength={500} />
                    </label>
                    <label className="text-xs text-muted-foreground">Dirección
                      <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={address} onChange={(event) => setAddress(event.target.value)} maxLength={500} />
                    </label>
                    <label className="text-xs text-muted-foreground">Teléfono
                      <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={phone} onChange={(event) => setPhone(event.target.value)} maxLength={20} />
                    </label>
                    <label className="text-xs text-muted-foreground">Teléfono fijo
                      <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={landline} onChange={(event) => setLandline(event.target.value)} maxLength={20} />
                    </label>
                    <label className="text-xs text-muted-foreground">Fecha de nacimiento
                      <input type="date" className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={dateOfBirth} onChange={(event) => setDateOfBirth(event.target.value)} />
                    </label>
                    <label className="text-xs text-muted-foreground">Número de identificación
                      <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={identificationNumber} onChange={(event) => setIdentificationNumber(event.target.value)} maxLength={50} />
                    </label>
                    <label className="text-xs text-muted-foreground">Referido por
                      <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={referredBy} onChange={(event) => setReferredBy(event.target.value)} maxLength={200} />
                    </label>
                    <label className="text-xs text-muted-foreground">Medio de captación
                      <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={acquisitionMedium} onChange={(event) => setAcquisitionMedium(event.target.value)} maxLength={200} />
                    </label>
                    <label className="w-full text-xs text-muted-foreground">Datos adicionales / observaciones
                      <textarea className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={additionalData} onChange={(event) => setAdditionalData(event.target.value)} maxLength={4000} rows={3} />
                    </label>
                    <button disabled={updateContact.isPending} className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground disabled:opacity-50">
                      {updateContact.isPending ? "Guardando…" : "Guardar"}
                    </button>
                  </form>
                </div>
                {updateContact.isError && <p role="alert" className="mt-3 text-sm text-destructive">{updateContact.error.message}</p>}
                {updateContact.isSuccess && <p className="mt-3 text-sm text-emerald-700">Cambios guardados.</p>}
                {user?.rol === "Administrador" && <label className="mt-4 flex items-start gap-2 border-t border-border pt-4 text-sm text-foreground">
                  <input type="checkbox" checked={contact.canReceiveAutomatedEmail} disabled={setEmailPermission.isPending}
                    onChange={(event) => {
                      const allowed = event.target.checked
                      const evidenceReference = allowed ? window.prompt("Referencia de evidencia del permiso (no incluyas datos personales):")?.trim() ?? null : null
                      if (allowed && !evidenceReference) return
                      setEmailPermission.mutate({ id: contact.id, allowed, evidenceReference })
                    }} />
                  <span>Puede recibir correos automáticos. Actívalo solo con evidencia de permiso registrada.</span>
                </label>}
                {user?.rol === "Administrador" && emailPermissionHistory.length > 0 && <div className="mt-3 text-xs text-muted-foreground">
                  <p className="font-semibold">Historial de permiso de correo</p>
                  {emailPermissionHistory.map((item, index) => <p key={`${item.occurredAt}-${index}`}>
                    {new Date(item.occurredAt).toLocaleString("es-CO")} · {item.allowed ? "Concedido" : "Revocado"} · {item.source} · actor {item.actorId}
                    {item.evidenceReference ? ` · ${item.evidenceReference}` : ""}
                  </p>)}
                </div>}
              </div>

              <div className="grid gap-5 lg:grid-cols-2">
                <section className="crm-panel p-5 lg:col-span-2">
                  <div className="flex flex-wrap items-center justify-between gap-3">
                    <div>
                      <h3 className="font-display text-xl text-foreground">Seguimiento</h3>
                      <p className="mt-1 text-sm text-muted-foreground">Próximos compromisos e historial de tareas comerciales.</p>
                    </div>
                    <form
                      className="flex flex-wrap items-end gap-2"
                      onSubmit={(event) => {
                        event.preventDefault()
                        const opportunity = detailQuery.data?.opportunities[0]
                        createTask.mutate({
                          contactId: contact.id,
                          leadId: opportunity?.id,
                          advisorId: user?.rol === "Administrador" ? opportunity?.advisorId : undefined,
                          type: taskType,
                          title: taskTitle,
                          description: taskDescription || undefined,
                          dueAt: new Date(taskDueAt).toISOString(),
                          reminderAt: taskReminderAt ? new Date(taskReminderAt).toISOString() : undefined,
                          priority: taskPriority,
                        }, { onSuccess: () => { setTaskTitle(""); setTaskDescription(""); setTaskReminderAt("") } })
                      }}
                    >
                      <label className="text-xs text-muted-foreground">Compromiso
                        <input className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={taskTitle} onChange={(event) => setTaskTitle(event.target.value)} maxLength={200} placeholder="Llamar para confirmar interés" required />
                      </label>
                      <label className="text-xs text-muted-foreground">Tipo
                        <select className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={taskType} onChange={(event) => setTaskType(event.target.value)}>
                          <option value="Call">Llamada</option><option value="Message">Mensaje</option><option value="Meeting">Reunión</option><option value="FollowUp">Seguimiento</option><option value="Other">Otro</option>
                        </select>
                      </label>
                      <label className="text-xs text-muted-foreground">Fecha
                        <input type="datetime-local" className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={taskDueAt} onChange={(event) => setTaskDueAt(event.target.value)} required />
                      </label>
                      <label className="text-xs text-muted-foreground">Recordatorio opcional
                        <input type="datetime-local" className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={taskReminderAt} onChange={(event) => setTaskReminderAt(event.target.value)} />
                      </label>
                      <label className="text-xs text-muted-foreground sm:col-span-2">Notas
                        <input className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={taskDescription} onChange={(event) => setTaskDescription(event.target.value)} maxLength={2000} />
                      </label>
                      <label className="text-xs text-muted-foreground">Prioridad
                        <select className="mt-1 block rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={taskPriority} onChange={(event) => setTaskPriority(event.target.value)}>
                          <option value="Low">Baja</option><option value="Normal">Normal</option><option value="High">Alta</option><option value="Urgent">Urgente</option>
                        </select>
                      </label>
                      <button disabled={createTask.isPending || (user?.rol === "Administrador" && !detailQuery.data?.opportunities[0]?.advisorId)} className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground disabled:opacity-50">
                        {createTask.isPending ? "Creando…" : "Crear tarea"}
                      </button>
                    </form>
                  </div>
                  {createTask.isError && <p role="alert" className="mt-3 text-sm text-destructive">{createTask.error.message}</p>}
                  {user?.rol === "Administrador" && !detailQuery.data.opportunities[0]?.advisorId && <p className="mt-3 text-sm text-muted-foreground">Asigna primero una oportunidad a un asesor para crearle una tarea.</p>}
                  <div className="mt-5 grid gap-3 md:grid-cols-2 xl:grid-cols-3">
                    {detailQuery.data.tasks.map((task) => (
                      <article key={task.id} className="rounded-md border border-border p-4">
                        <div className="flex items-start justify-between gap-2">
                          <p className="font-semibold text-foreground">{task.title}</p>
                          <span className="rounded-full bg-muted px-2 py-1 text-[.65rem] font-semibold text-muted-foreground">{task.priority}</span>
                        </div>
                        <p className="mt-2 text-xs text-muted-foreground">{task.type} · {new Date(task.dueAt).toLocaleString("es-CO")}</p>
                        <p className="mt-1 text-xs text-muted-foreground">{task.status === "Pending" ? "Pendiente" : task.status === "Completed" ? "Completada" : "Cancelada"}</p>
                        {task.status === "Pending" && <><button onClick={() => completeTask.mutate(task.id)} disabled={completeTask.isPending} className="mt-3 rounded border border-border px-3 py-1.5 text-xs font-semibold text-foreground hover:bg-muted">Marcar completada</button><button onClick={() => cancelTask.mutate(task.id)} disabled={cancelTask.isPending} className="ml-2 mt-3 rounded border border-destructive/40 px-3 py-1.5 text-xs font-semibold text-destructive">Cancelar</button></>}
                        <button onClick={() => setSelectedTaskId(selectedTaskId === task.id ? "" : task.id)} className="ml-2 mt-3 rounded border border-border px-3 py-1.5 text-xs font-semibold text-foreground hover:bg-muted">Historial</button>
                        {selectedTaskId === task.id && (
                          <div className="mt-3 border-t border-border pt-3">
                            {taskHistory.data?.map((event) => <p key={event.id} className="mt-1 text-xs text-muted-foreground">{new Date(event.occurredAt).toLocaleString("es-CO")} · {event.action}{event.comment ? `: ${event.comment}` : ""}</p>)}
                            <form className="mt-2 flex gap-2" onSubmit={(event) => { event.preventDefault(); addTaskComment.mutate({ id: task.id, comment: taskComment }, { onSuccess: () => setTaskComment("") }) }}>
                              <input className="min-w-0 flex-1 rounded-md border border-border bg-background px-2 py-1.5 text-xs text-foreground" value={taskComment} onChange={(event) => setTaskComment(event.target.value)} maxLength={2000} placeholder="Agregar comentario" required />
                              <button disabled={addTaskComment.isPending} className="rounded border border-border px-2 py-1 text-xs font-semibold text-foreground">Añadir</button>
                            </form>
                          </div>
                        )}
                      </article>
                    ))}
                    {detailQuery.data.tasks.length === 0 && <p className="text-sm text-muted-foreground">Sin tareas todavía.</p>}
                  </div>
                </section>
                <section className="crm-panel p-5 lg:col-span-2">
                  <div className="flex flex-wrap items-start justify-between gap-4">
                    <div>
                      <h3 className="font-display text-xl text-foreground">Búsqueda del cliente</h3>
                      <p className="mt-1 text-sm text-muted-foreground">Registra su demanda y encuentra inmuebles disponibles que coincidan.</p>
                    </div>
                    <form
                      className="grid w-full gap-2 sm:grid-cols-2 xl:grid-cols-6"
                      onSubmit={(event) => {
                        event.preventDefault()
                        const demandInput = {
                          operation: demandOperation,
                          propertyType: demandType || undefined,
                          city: demandCity,
                          country: demandCountry,
                          department: demandDepartment || undefined,
                          neighborhood: demandNeighborhood || undefined,
                          minimumPrice: demandMinPrice ? Number(demandMinPrice) : undefined,
                          maximumPrice: demandMaxPrice ? Number(demandMaxPrice) : undefined,
                          currency: demandCurrency,
                          minimumArea: demandMinArea ? Number(demandMinArea) : undefined,
                          maximumArea: demandMaxArea ? Number(demandMaxArea) : undefined,
                          bedrooms: demandBedrooms ? Number(demandBedrooms) : undefined,
                          bathrooms: demandBathrooms ? Number(demandBathrooms) : undefined,
                          parkingSpaces: demandParkingSpaces ? Number(demandParkingSpaces) : undefined,
                          internalFeatures: splitFeatures(demandInternalFeatures),
                          externalFeatures: splitFeatures(demandExternalFeatures),
                          notes: demandNotes || undefined,
                        }
                        if (editingDemandId) {
                          updateDemand.mutate({ id: editingDemandId, data: demandInput }, {
                            onSuccess: (demand) => { setSelectedDemandId(demand.id); setEditingDemandId("") },
                          })
                        } else {
                          createDemand.mutate({ ...demandInput, contactId: contact.id,
                            leadId: detailQuery.data?.opportunities[0]?.id }, {
                              onSuccess: (demand) => { setSelectedDemandId(demand.id); setDemandCity(""); setDemandNeighborhood("") },
                            })
                        }
                      }}
                    >
                      <label className="text-xs text-muted-foreground">Operación
                        <select className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandOperation} onChange={(event) => setDemandOperation(event.target.value)}><option value="Venta">Venta</option><option value="Arriendo">Arriendo</option></select>
                      </label>
                      <label className="text-xs text-muted-foreground">Tipo de inmueble
                        <select className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandType} onChange={(event) => setDemandType(event.target.value)}><option value="">Cualquiera</option><option value="Apartamento">Apartamento</option><option value="Casa">Casa</option><option value="Local">Local</option><option value="Oficina">Oficina</option><option value="Lote">Lote</option><option value="Bodega">Bodega</option><option value="Finca">Finca</option></select>
                      </label>
                      <label className="text-xs text-muted-foreground">Ciudad
                        <input className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandCity} onChange={(event) => setDemandCity(event.target.value)} maxLength={100} required />
                      </label>
                      <label className="text-xs text-muted-foreground">País
                        <input className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandCountry} onChange={(event) => setDemandCountry(event.target.value)} maxLength={100} required />
                      </label>
                      <label className="text-xs text-muted-foreground">Departamento
                        <input className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandDepartment} onChange={(event) => setDemandDepartment(event.target.value)} maxLength={100} />
                      </label>
                      <label className="text-xs text-muted-foreground">Barrio
                        <input className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandNeighborhood} onChange={(event) => setDemandNeighborhood(event.target.value)} maxLength={100} />
                      </label>
                      <label className="text-xs text-muted-foreground">Presupuesto mínimo
                        <input type="number" min="0" className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandMinPrice} onChange={(event) => setDemandMinPrice(event.target.value)} />
                      </label>
                      <label className="text-xs text-muted-foreground">Presupuesto máximo
                        <input type="number" min="0" className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandMaxPrice} onChange={(event) => setDemandMaxPrice(event.target.value)} />
                      </label>
                      <label className="text-xs text-muted-foreground">Moneda (código ISO)
                        <input className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm uppercase text-foreground" value={demandCurrency} onChange={(event) => setDemandCurrency(event.target.value.toUpperCase())} minLength={3} maxLength={3} pattern="[A-Z]{3}" required />
                      </label>
                      <label className="text-xs text-muted-foreground">Área mínima (m²)
                        <input type="number" min="0" step="0.01" className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandMinArea} onChange={(event) => setDemandMinArea(event.target.value)} />
                      </label>
                      <label className="text-xs text-muted-foreground">Área máxima (m²)
                        <input type="number" min="0" step="0.01" className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandMaxArea} onChange={(event) => setDemandMaxArea(event.target.value)} />
                      </label>
                      <label className="text-xs text-muted-foreground">Habitaciones mínimas
                        <input type="number" min="0" max="50" className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandBedrooms} onChange={(event) => setDemandBedrooms(event.target.value)} />
                      </label>
                      <label className="text-xs text-muted-foreground">Baños mínimos
                        <input type="number" min="0" max="50" className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandBathrooms} onChange={(event) => setDemandBathrooms(event.target.value)} />
                      </label>
                      <label className="text-xs text-muted-foreground">Garajes mínimos
                        <input type="number" min="0" max="50" className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandParkingSpaces} onChange={(event) => setDemandParkingSpaces(event.target.value)} />
                      </label>
                      <label className="text-xs text-muted-foreground sm:col-span-2 xl:col-span-6">Detalles adicionales de la propiedad
                        <textarea className="mt-1 block min-h-20 w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandNotes} onChange={(event) => setDemandNotes(event.target.value)} maxLength={2000} />
                      </label>
                      <label className="text-xs text-muted-foreground sm:col-span-2 xl:col-span-3">Características internas (separa con coma)
                        <input className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandInternalFeatures} onChange={(event) => setDemandInternalFeatures(event.target.value)} placeholder="Balcón, cocina integral" />
                      </label>
                      <label className="text-xs text-muted-foreground sm:col-span-2 xl:col-span-3">Características externas (separa con coma)
                        <input className="mt-1 block w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={demandExternalFeatures} onChange={(event) => setDemandExternalFeatures(event.target.value)} placeholder="Piscina, portería" />
                      </label>
                      <div className="sm:col-span-2 xl:col-span-6">
                        <button disabled={createDemand.isPending || updateDemand.isPending} className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground disabled:opacity-50">{createDemand.isPending || updateDemand.isPending ? "Guardando…" : editingDemandId ? "Guardar cambios de búsqueda" : "Guardar búsqueda"}</button>
                        {editingDemandId && <button type="button" className="ml-2 rounded-md border border-border px-4 py-2 text-sm text-foreground" onClick={() => setEditingDemandId("")}>Cancelar edición</button>}
                      </div>
                    </form>
                  </div>
                  {(createDemand.isError || updateDemand.isError || deleteDemand.isError) && <p role="alert" className="mt-3 text-sm text-destructive">{createDemand.error?.message ?? updateDemand.error?.message ?? deleteDemand.error?.message}</p>}
                  <div className="mt-5 grid gap-5 lg:grid-cols-[minmax(220px,.65fr)_minmax(0,1fr)]">
                    <div className="space-y-2">
                      <h4 className="text-sm font-semibold text-foreground">Demandas guardadas</h4>
                      {demandsQuery.data?.map((demand) => (
                        <article key={demand.id} className={`rounded-md border border-border p-3 ${selectedDemandId === demand.id ? "bg-muted" : ""}`}>
                          <button type="button" onClick={() => setSelectedDemandId(demand.id)} aria-pressed={selectedDemandId === demand.id} className="block w-full text-left">
                            <span className="block text-sm font-semibold text-foreground">{demand.operation} · {demand.propertyType ?? "Cualquier inmueble"}</span>
                            <span className="mt-1 block text-xs text-muted-foreground">{demand.city}{demand.department ? `, ${demand.department}` : ""}, {demand.country}{demand.neighborhood ? ` · ${demand.neighborhood}` : ""}{demand.minimumPrice || demand.maximumPrice ? ` · ${demand.currency} ${demand.minimumPrice?.toLocaleString("es-CO") ?? "0"}–${demand.maximumPrice?.toLocaleString("es-CO") ?? "sin máximo"}` : ""}{demand.minimumArea || demand.maximumArea ? ` · ${demand.minimumArea ?? 0}–${demand.maximumArea ?? "sin máximo"} m²` : ""}</span>
                          </button>
                          <div className="mt-2 flex gap-2">
                            <button type="button" className="rounded border border-border px-2 py-1 text-xs font-semibold text-foreground" onClick={() => {
                              setSelectedDemandId(demand.id); setEditingDemandId(demand.id); setDemandOperation(demand.operation);
                              setDemandType(demand.propertyType ?? ""); setDemandCity(demand.city); setDemandCountry(demand.country);
                              setDemandDepartment(demand.department ?? ""); setDemandNeighborhood(demand.neighborhood ?? "");
                              setDemandMinPrice(demand.minimumPrice?.toString() ?? ""); setDemandMaxPrice(demand.maximumPrice?.toString() ?? "");
                              setDemandCurrency(demand.currency); setDemandMinArea(demand.minimumArea?.toString() ?? "");
                              setDemandMaxArea(demand.maximumArea?.toString() ?? ""); setDemandBedrooms(demand.bedrooms?.toString() ?? "");
                              setDemandBathrooms(demand.bathrooms?.toString() ?? ""); setDemandParkingSpaces(demand.parkingSpaces?.toString() ?? "");
                              setDemandInternalFeatures(demand.internalFeatures.join(", ")); setDemandExternalFeatures(demand.externalFeatures.join(", "));
                              setDemandNotes(demand.notes ?? "")
                            }}>Modificar demanda</button>
                            <button type="button" disabled={deleteDemand.isPending} className="rounded border border-destructive/40 px-2 py-1 text-xs font-semibold text-destructive disabled:opacity-40" onClick={() => {
                              if (window.confirm("¿Dar de baja esta demanda? Se conservarán sus relaciones en el historial."))
                                deleteDemand.mutate(demand.id, { onSuccess: () => { if (selectedDemandId === demand.id) setSelectedDemandId(""); if (editingDemandId === demand.id) setEditingDemandId("") } })
                            }}>Borrar demanda</button>
                          </div>
                        </article>
                      ))}
                      {demandsQuery.data?.length === 0 && <p className="text-sm text-muted-foreground">Todavía no hay una búsqueda guardada.</p>}
                    </div>
                    <div>
                      <h4 className="text-sm font-semibold text-foreground">Inmuebles compatibles</h4>
                      {matchesQuery.isPending && selectedDemandId && <p className="mt-3 text-sm text-muted-foreground">Buscando en el inventario…</p>}
                      {matchesQuery.data?.map((match) => (
                        <article key={match.propertyId} className="mt-2 flex flex-wrap items-center justify-between gap-3 rounded-md border border-border p-3">
                          <div>
                            <p className="font-semibold text-foreground">{match.name}</p>
                            <p className="mt-1 text-xs text-muted-foreground">{match.propertyType} · {match.city}{match.department ? `, ${match.department}` : ""}, {match.country}{match.neighborhood ? ` · ${match.neighborhood}` : ""} · {match.bedrooms} hab. · {match.currency} {match.price.toLocaleString("es-CO")}</p>
                          </div>
                          {match.relationshipStatus ? (
                            <select aria-label={`Estado de ${match.name}`} value={match.relationshipStatus} onChange={(event) => setPropertyStatus.mutate({ demandId: selectedDemandId, propertyId: match.propertyId, status: event.target.value })} className="rounded-md border border-border bg-background px-2 py-1.5 text-xs text-foreground">
                              <option value="Suggested">Sugerido</option><option value="Shared">Compartido</option><option value="Visited">Visitado</option><option value="Discarded">Descartado</option>
                            </select>
                          ) : <button onClick={() => linkProperty.mutate({ demandId: selectedDemandId, propertyId: match.propertyId })} disabled={linkProperty.isPending} className="rounded-md border border-border px-3 py-2 text-xs font-semibold text-foreground hover:bg-muted">Relacionar</button>}
                        </article>
                      ))}
                      {selectedDemandId && matchesQuery.data?.length === 0 && <p className="mt-3 text-sm text-muted-foreground">No encontramos inmuebles disponibles que cumplan estos criterios.</p>}
                      {(matchesQuery.isError || linkProperty.isError || setPropertyStatus.isError) && <p role="alert" className="mt-3 text-sm text-destructive">No se pudo consultar o guardar la relación con el inmueble.</p>}
                    </div>
                  </div>
                </section>
                <section className="crm-panel p-5">
                  <h3 className="font-display text-xl text-foreground">Oportunidades</h3>
                  <div className="mt-4 space-y-3">
                    {detailQuery.data?.opportunities.length === 0 && <p className="text-sm text-muted-foreground">Sin oportunidades visibles.</p>}
                    {detailQuery.data?.opportunities.map((opportunity) => (
                      <article key={opportunity.id} className="rounded-md border border-border p-3">
                        <p className="font-semibold text-foreground">{opportunity.operation} · {opportunity.stage}</p>
                        <p className="mt-1 text-xs text-muted-foreground">Origen: {opportunity.source}</p>
                        <p className="mt-1 text-xs text-muted-foreground">Creada {new Date(opportunity.createdAt).toLocaleDateString("es-CO")}</p>
                      </article>
                    ))}
                  </div>
                </section>
                <section className="crm-panel p-5">
                  <h3 className="font-display text-xl text-foreground">Actividad reciente</h3>
                  <div className="mt-4 space-y-3">
                    {timeline.length === 0 && <p className="text-sm text-muted-foreground">Todavía no hay interacciones ni visitas registradas.</p>}
                    {timeline.map((event, index) => (
                      <article key={`${event.date}-${event.label}-${index}`} className="border-l-2 border-accent/50 pl-3">
                        <p className="text-sm font-semibold text-foreground">{event.label}</p>
                        <p className="text-xs text-muted-foreground">{event.context} · {new Date(event.date).toLocaleString("es-CO")}</p>
                        {event.detail && <p className="mt-1 text-sm text-muted-foreground">{event.detail}</p>}
                      </article>
                    ))}
                  </div>
                </section>
              </div>
            </>
          )}
        </section>
      </div>
    </div>
  )
}

function ImportColumnSelect({ label, value, columns, onChange, optional = false }: {
  label: string; value: string; columns: string[]; onChange: (value: string) => void; optional?: boolean
}) {
  return <label className="text-xs text-muted-foreground">{label}
    <select className="mt-1 block min-w-40 rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground" value={value} onChange={(event) => onChange(event.target.value)}>
      {optional && <option value="">No asignar</option>}
      {columns.map((column, index) => <option key={`${index}:${column}`} value={index}>{column || `Columna ${index + 1}`}</option>)}
    </select>
  </label>
}

function toLocalDateTimeInput(date: Date) {
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60_000)
  return local.toISOString().slice(0, 16)
}

function exportContactsCsv(contacts: { name: string; email: string | null; phone?: string; createdAt: string }[]) {
  const encode = (value: string) => {
    const formulaSafe = /^[\s\u0000]*[=+@-]/.test(value) ? `'${value}` : value
    return `"${formulaSafe.replaceAll('"', '""')}"`
  }
  const rows = [
    ["nombre", "correo", "telefono", "creado"],
    ...contacts.map((item) => [item.name, item.email ?? "", item.phone ?? "", item.createdAt]),
  ].map((row) => row.map(encode).join(",")).join("\r\n")
  const url = URL.createObjectURL(new Blob(["\uFEFF", rows], { type: "text/csv;charset=utf-8" }))
  const link = document.createElement("a")
  link.href = url
  link.download = "contactos-seleccionados.csv"
  link.click()
  window.setTimeout(() => URL.revokeObjectURL(url), 1000)
}
