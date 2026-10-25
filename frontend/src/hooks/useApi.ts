import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import { useAuthStore } from "@/stores/authStore"

// Auth
export function useLogin() {
  const setUser = useAuthStore((s) => s.setUser)

  return useMutation({
    mutationFn: async ({ email, password }: { email: string; password: string }) => {
      await api.login(email, password)
      return api.getMe()
    },
    onSuccess: (data) => {
      setUser({ id: data.id, nombre: data.nombre, rol: data.rol, email: data.email })
    },
  })
}

export function useMe() {
  const setUser = useAuthStore((s) => s.setUser)
  const logout = useAuthStore((s) => s.logout)

  return useQuery({
    queryKey: ["auth", "me"],
    queryFn: async () => {
      try {
        const data = await api.getMe()
        setUser({ id: data.id, nombre: data.nombre, rol: data.rol, email: data.email })
        return data
      } catch (error) {
        logout()
        throw error
      }
    },
    retry: false,
    staleTime: 5 * 60 * 1000,
  })
}

export function useLogout() {
  const queryClient = useQueryClient()
  const logout = useAuthStore((s) => s.logout)

  return useMutation({
    mutationFn: () => api.logout(),
    onSettled: () => {
      logout()
      queryClient.clear()
    },
  })
}

// Pipeline
export function usePipeline(filters: Parameters<typeof api.getPipeline>[0] = {}) {
  return useQuery({
    queryKey: ["pipeline", filters],
    queryFn: () => api.getPipeline(filters),
  })
}

export function useFirstResponseMetrics(from: string, to: string) {
  return useQuery({
    queryKey: ["analytics", "first-response", from, to],
    queryFn: () => api.getFirstResponseMetrics(from, to),
  })
}

export function useCrmReportMetrics(from: string, to: string) {
  return useQuery({
    queryKey: ["analytics", "crm-report", from, to],
    queryFn: () => api.getCrmReportMetrics(from, to),
  })
}

export function useInventoryAnalytics() {
  return useQuery({
    queryKey: ["analytics", "inventory"],
    queryFn: () => api.getInventoryAnalytics(),
  })
}

export function useMarketComparativeAnalysis(filters: Parameters<typeof api.getMarketComparativeAnalysis>[0]) {
  return useQuery({
    queryKey: ["analytics", "market-analysis", filters],
    queryFn: () => api.getMarketComparativeAnalysis(filters),
  })
}

export function useRealizedDealsReport(filters: Parameters<typeof api.getRealizedDealsReport>[0]) {
  return useQuery({
    queryKey: ["analytics", "realized-deals", filters],
    queryFn: () => api.getRealizedDealsReport(filters),
  })
}

export function useAdvisorPerformanceReport(from: string, to: string) {
  return useQuery({
    queryKey: ["analytics", "advisors", from, to],
    queryFn: () => api.getAdvisorPerformanceReport(from, to),
  })
}

export function useOwnerPerformanceReport(from: string, to: string) {
  return useQuery({
    queryKey: ["analytics", "owners", from, to],
    queryFn: () => api.getOwnerPerformanceReport(from, to),
  })
}

export function useSentEmailReport(filters: Parameters<typeof api.getSentEmailReport>[0]) {
  return useQuery({
    queryKey: ["analytics", "sent-emails", filters],
    queryFn: () => api.getSentEmailReport(filters),
  })
}

export function useWebPropertyVisitsReport(from: string, to: string) {
  return useQuery({
    queryKey: ["analytics", "web-properties", from, to],
    queryFn: () => api.getWebPropertyVisitsReport(from, to),
  })
}

export function useCreateRealizedDeal() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: api.createRealizedDeal.bind(api),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["analytics", "realized-deals"] })
      queryClient.invalidateQueries({ queryKey: ["analytics", "inventory"] })
      queryClient.invalidateQueries({ queryKey: ["inmuebles", "list"] })
    },
  })
}

export function useUpdateRealizedDeal() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: import("@/lib/types").UpdateRealizedDealInput }) =>
      api.updateRealizedDeal(id, input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["analytics", "realized-deals"] }),
  })
}

export function useMoveLeadInPipeline() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      leadId,
      nuevaEtapa,
      reason,
    }: {
      leadId: string
      nuevaEtapa: string
      reason?: string
    }) => api.moveLeadInPipeline(leadId, nuevaEtapa, reason),
    onMutate: async ({ leadId, nuevaEtapa }) => {
      await queryClient.cancelQueries({ queryKey: ["pipeline"] })
      const previousPipeline = queryClient.getQueryData<any>(["pipeline"])

      if (previousPipeline?.leads) {
        queryClient.setQueryData(["pipeline"], {
          ...previousPipeline,
          leads: previousPipeline.leads.map((lead: any) =>
            lead.id === leadId ? { ...lead, etapaPipeline: nuevaEtapa } : lead
          ),
        })
      }

      return { previousPipeline }
    },
    onError: (_err, _variables, context) => {
      if (context?.previousPipeline) {
        queryClient.setQueryData(["pipeline"], context.previousPipeline)
      }
    },
    onSettled: (_result, _error, variables) => {
      queryClient.invalidateQueries({ queryKey: ["pipeline"] })
      queryClient.invalidateQueries({ queryKey: ["leads", variables.leadId, "history"] })
    },
  })
}

export function useAssignLead() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      leadId,
      asesorId,
      reason,
    }: {
      leadId: string
      asesorId: string
      reason: string
    }) => api.assignLeadToAsesor(leadId, asesorId, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["pipeline"] })
    },
  })
}

export function useBulkAssignLeads() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ leadIds, advisorId, reason }: { leadIds: string[]; advisorId: string; reason: string }) =>
      api.bulkAssignLeads(leadIds, advisorId, reason),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["pipeline"] }),
  })
}

export function useLeadAssignmentHistory(leadId: string) {
  return useQuery({
    queryKey: ["leads", leadId, "assignment-history"],
    queryFn: () => api.getLeadAssignmentHistory(leadId),
    enabled: !!leadId,
  })
}

export function useLeadHistory(leadId: string) {
  return useQuery({
    queryKey: ["leads", leadId, "history"],
    queryFn: () => api.getLeadHistory(leadId),
    enabled: !!leadId,
  })
}

export function useAdvisors(enabled = true) {
  return useQuery({ queryKey: ["advisors"], queryFn: () => api.getAdvisors(), enabled })
}

// Leads
export function useLead(id: string) {
  return useQuery({
    queryKey: ["leads", id],
    queryFn: () => api.getLeadById(id),
    enabled: !!id,
  })
}

export function useCreateLead() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: api.createLead.bind(api),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["pipeline"] })
      queryClient.invalidateQueries({ queryKey: ["leads"] })
    },
  })
}

export function useContacts(filters: Parameters<typeof api.getContacts>[0] = {}) {
  return useQuery({
    queryKey: ["contacts", "list", filters],
    queryFn: () => api.getContacts(filters),
  })
}

export function useCreateContact() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: api.createContact,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["contacts"] }),
  })
}

export function useContact(id: string) {
  return useQuery({
    queryKey: ["contacts", id],
    queryFn: () => api.getContact(id),
    enabled: !!id,
  })
}

export function useSetContactEmailPermission() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, allowed, evidenceReference }: { id: string; allowed: boolean; evidenceReference: string | null }) =>
      api.setContactEmailPermission(id, allowed, evidenceReference),
    onSuccess: (_result, variables) => queryClient.invalidateQueries({ queryKey: ["contacts", variables.id] }),
  })
}

export function useUpdateContact() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, name, surname, phone, customerType, customerTypes, customerLabels, customerStatus, customerFunnel, address, country, department, city,
      landline, dateOfBirth, identificationNumber, referredBy, acquisitionMedium, additionalData }: {
      id: string; name: string; surname?: string; phone?: string; customerType?: string; customerTypes?: string[]; customerLabels?: string[]; customerStatus?: string; customerFunnel?: string;
      address?: string; country?: string; department?: string; city?: string; landline?: string; dateOfBirth?: string;
      identificationNumber?: string; referredBy?: string; acquisitionMedium?: string; additionalData?: string
    }) => api.updateContact(id, { name, surname, phone, customerType, customerTypes, customerLabels, customerStatus, customerFunnel, address, country, department,
      city, landline, dateOfBirth, identificationNumber, referredBy, acquisitionMedium, additionalData }),
    onSuccess: (_contact, variables) => {
      queryClient.invalidateQueries({ queryKey: ["contacts"] })
      queryClient.invalidateQueries({ queryKey: ["contacts", variables.id] })
    },
  })
}

export function useCreateCommercialTask() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: import("@/lib/types").CreateCommercialTaskInput) => api.createCommercialTask(data),
    onSuccess: (_task, variables) => {
      queryClient.invalidateQueries({ queryKey: ["contacts", variables.contactId] })
      queryClient.invalidateQueries({ queryKey: ["tasks"] })
    },
  })
}

export function useCommercialTasks(filters: Parameters<typeof api.getCommercialTasks>[0] = {}) {
  return useQuery({
    queryKey: ["tasks", "list", filters],
    queryFn: () => api.getCommercialTasks(filters),
  })
}

export function useCalendarCommercialTasks(filters: Parameters<typeof api.getCalendarCommercialTasks>[0]) {
  return useQuery({
    queryKey: ["tasks", "calendar", filters],
    queryFn: () => api.getCalendarCommercialTasks(filters),
  })
}

export function useBulkCompleteCommercialTasks() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (ids: string[]) => api.bulkCompleteTasks(ids),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["tasks"] })
      queryClient.invalidateQueries({ queryKey: ["contacts"] })
    },
  })
}

export function useBulkCancelCommercialTasks() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (ids: string[]) => api.bulkCancelTasks(ids),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["tasks"] })
      queryClient.invalidateQueries({ queryKey: ["contacts"] })
    },
  })
}

export function useCommercialTaskAlerts(through: string) {
  return useQuery({
    queryKey: ["tasks", "alerts", through],
    queryFn: () => api.getCommercialTaskAlerts(through),
  })
}

export function useCompleteCommercialTask() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.completeCommercialTask(id),
    onSuccess: (task) => {
      queryClient.invalidateQueries({ queryKey: ["tasks"] })
      queryClient.invalidateQueries({ queryKey: ["contacts", task.contactId] })
    },
  })
}

export function useCancelCommercialTask() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.cancelCommercialTask(id),
    onSuccess: (task) => {
      queryClient.invalidateQueries({ queryKey: ["tasks"] })
      queryClient.invalidateQueries({ queryKey: ["contacts", task.contactId] })
    },
  })
}

export function useRescheduleCommercialTask() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, dueAt, reminderAt }: { id: string; dueAt: string; reminderAt?: string }) => api.rescheduleCommercialTask(id, dueAt, reminderAt),
    onSuccess: (task) => {
      queryClient.invalidateQueries({ queryKey: ["tasks"] })
      queryClient.invalidateQueries({ queryKey: ["contacts", task.contactId] })
    },
  })
}

export function useCommercialTaskHistory(taskId: string) {
  return useQuery({
    queryKey: ["tasks", taskId, "history"],
    queryFn: () => api.getCommercialTaskHistory(taskId),
    enabled: !!taskId,
  })
}

export function useAddCommercialTaskComment() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, comment }: { id: string; comment: string }) => api.addCommercialTaskComment(id, comment),
    onSuccess: (_event, variables) => queryClient.invalidateQueries({ queryKey: ["tasks", variables.id, "history"] }),
  })
}

export function useCustomerDemands(contactId: string) {
  return useQuery({
    queryKey: ["demands", contactId],
    queryFn: () => api.getCustomerDemands(contactId),
    enabled: !!contactId,
  })
}

export function useCustomerDemandCross(filters: Parameters<typeof api.getCustomerDemandCross>[0], enabled: boolean) {
  return useQuery({
    queryKey: ["demand-cross", filters],
    queryFn: () => api.getCustomerDemandCross(filters),
    enabled,
  })
}

export function useCreateCustomerDemand() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: import("@/lib/types").CreateCustomerDemandInput) => api.createCustomerDemand(data),
    onSuccess: (_demand, variables) => queryClient.invalidateQueries({ queryKey: ["demands", variables.contactId] }),
  })
}

export function useUpdateCustomerDemand() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: import("@/lib/types").UpdateCustomerDemandInput }) =>
      api.updateCustomerDemand(id, data),
    onSuccess: (_demand, variables) => {
      queryClient.invalidateQueries({ queryKey: ["demands"] })
      queryClient.invalidateQueries({ queryKey: ["demand-matches"] })
    },
  })
}

export function useDeleteCustomerDemand() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: api.deleteCustomerDemand,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["demands"] })
      queryClient.invalidateQueries({ queryKey: ["demand-matches"] })
    },
  })
}

export function useDemandMatches(demandId: string) {
  return useQuery({
    queryKey: ["demand-matches", demandId],
    queryFn: () => api.getDemandMatches(demandId),
    enabled: !!demandId,
  })
}

export function usePropertyDemandMatches(propertyId: string) {
  return useQuery({
    queryKey: ["property-demands", propertyId],
    queryFn: () => api.getPropertyDemandMatches(propertyId),
    enabled: !!propertyId,
  })
}

export function usePropertyDemandCounts(propertyIds: string[]) {
  return useQuery({
    queryKey: ["property-demand-counts", propertyIds],
    queryFn: () => api.getPropertyDemandCounts(propertyIds),
    enabled: propertyIds.length > 0,
  })
}

// El seguimiento es el mismo en ambos sentidos: refrescar las dos vistas
function invalidateDemandLinks(queryClient: ReturnType<typeof useQueryClient>, demandId: string, propertyId: string) {
  queryClient.invalidateQueries({ queryKey: ["demand-matches", demandId] })
  queryClient.invalidateQueries({ queryKey: ["property-demands", propertyId] })
}

export function useLinkDemandProperty() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ demandId, propertyId }: { demandId: string; propertyId: string }) => api.linkDemandProperty(demandId, propertyId),
    onSuccess: (_result, variables) => invalidateDemandLinks(queryClient, variables.demandId, variables.propertyId),
  })
}

export function useSetDemandPropertyStatus() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ demandId, propertyId, status }: { demandId: string; propertyId: string; status: string }) => api.setDemandPropertyStatus(demandId, propertyId, status),
    onSuccess: (_result, variables) => invalidateDemandLinks(queryClient, variables.demandId, variables.propertyId),
  })
}

// Interacciones
export function useInteracciones(leadId: string) {
  return useQuery({
    queryKey: ["interacciones", leadId],
    queryFn: () => api.getInteracciones(leadId),
    enabled: !!leadId,
  })
}

export function useRegistrarInteraccion() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: api.registrarInteraccion,
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["interacciones", variables.leadId],
      })
    },
  })
}

// Visitas
export function useVisitas(advisorId?: string) {
  return useQuery({
    queryKey: ["visitas", advisorId],
    queryFn: () => api.getVisitas(advisorId),
  })
}

export function useVisita(id: string) {
  return useQuery({
    queryKey: ["visitas", id],
    queryFn: () => api.getVisitaById(id),
    enabled: !!id,
  })
}

export function useRegistrarVisita() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: api.registrarVisita,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["visitas"] })
    },
  })
}

export function useManageVisit() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, action, value }: { id: string; action: "complete" | "cancel" | "reschedule"; value?: string }) => {
      if (action === "complete") return api.completeVisit(id, value)
      if (action === "cancel") return api.cancelVisit(id)
      return api.rescheduleVisit(id, value ?? "")
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["visitas"] })
      queryClient.invalidateQueries({ queryKey: ["contacts"] })
    },
  })
}

// Alertas
export function useAlertas() {
  return useQuery({
    queryKey: ["alertas"],
    queryFn: () => api.getAlertas(),
  })
}

export function useLeadSlaAlerts() {
  return useQuery({
    queryKey: ["alertas", "sla"],
    queryFn: () => api.getLeadSlaAlerts(),
  })
}

// Inmuebles
export function useInmuebles(filters: NonNullable<Parameters<typeof api.getInmuebles>[0]>) {
  return useQuery({ queryKey: ["inmuebles", "list", filters], queryFn: () => api.getInmuebles({ ...filters, pageSize: filters.pageSize ?? 20 }) })
}

export function useInmueble(id: string) {
  return useQuery({
    queryKey: ["inmuebles", id],
    queryFn: () => api.getInmuebleById(id),
    enabled: !!id,
  })
}

export function useCreateInmueble() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: api.createInmueble,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["inmuebles"] })
    },
  })
}

export function useUpdateInmueble() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Record<string, unknown> }) => api.updateInmueble(id, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["inmuebles"] }),
  })
}

export function useChangeInmuebleTrashState() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, restore }: { id: string; restore: boolean }) => api.changeInmuebleTrashState(id, restore),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["inmuebles"] }),
  })
}

export function useChangeInmueblesTrashState() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ ids, restore }: { ids: string[]; restore: boolean }) => api.changeInmueblesTrashState(ids, restore),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["inmuebles"] }),
  })
}

// Publicaciones
export function usePublications() {
  return useQuery({
    queryKey: ["publicaciones"],
    queryFn: () => api.getPublications(),
  })
}

export function usePublicationOptions() {
  return useQuery({
    queryKey: ["publicaciones", "opciones"],
    queryFn: () => api.getPublicationOptions(),
  })
}

export function useCreatePublication() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: api.createPublication.bind(api),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["publicaciones"] })
    },
  })
}

export function useUpdatePublication() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: import("@/lib/types").PropertyPublicationInput }) =>
      api.updatePublication(id, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["publicaciones"] }),
  })
}

export function useChangePublicationStatus() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      id,
      action,
    }: {
      id: string
      action: "publish" | "pause" | "withdraw"
    }) => api.changePublicationStatus(id, action),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["publicaciones"] }),
  })
}

export function useUploadPublicationPhoto() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      publicationId,
      file,
      altText,
      isCover,
    }: {
      publicationId: string
      file: File
      altText: string
      isCover: boolean
    }) => api.uploadPublicationPhoto(publicationId, file, altText, isCover),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["publicaciones"] }),
  })
}

// Facturacion
export function useBillingPricing() {
  return useQuery({
    queryKey: ["billing", "pricing"],
    queryFn: () => api.getBillingPricing(),
  })
}

export function useCreateCheckoutSession() {
  return useMutation({
    mutationFn: api.createCheckoutSession.bind(api),
  })
}

export function usePaymentOrderStatus(reference: string | null) {
  return useQuery({
    queryKey: ["billing", "order", reference],
    queryFn: () => api.getPaymentOrderStatus(reference!),
    enabled: Boolean(reference),
    refetchInterval: (query) =>
      query.state.data?.status === "Pending" ? 2500 : false,
  })
}

export function useAnalyticsFunnel(days: number) {
  return useQuery({
    queryKey: ["analytics", "funnel", days],
    queryFn: () => api.getAnalyticsFunnel(days),
  })
}

// Ley 1581
export function usePoliticaActiva() {
  return useQuery({
    queryKey: ["politica"],
    queryFn: () => api.getPoliticaActiva(),
  })
}

export function useDatosLead(leadId: string) {
  return useQuery({
    queryKey: ["datos-lead", leadId],
    queryFn: () => api.consultarDatosLead(leadId),
    enabled: !!leadId,
  })
}

export function useSuprimirDatosLead() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (leadId: string) => api.suprimirDatosLead(leadId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["leads"] })
    },
  })
}

// Sitio Web Publico (CMS)
export function useAdminWebsiteConfig() {
  return useQuery({
    queryKey: ["website-config", "admin"],
    queryFn: () => api.getAdminWebsiteConfig(),
  })
}

export function usePublicWebsiteConfig() {
  return useQuery({
    queryKey: ["website-config", "public"],
    queryFn: () => api.getPublicWebsiteConfig(),
  })
}

export function useUpdateWebsiteConfig() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: import("@/lib/types").UpdateWebsiteConfigInput) =>
      api.updateWebsiteConfig(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["website-config"] })
    },
  })
}
