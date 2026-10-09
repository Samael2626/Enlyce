import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import { useAuthStore } from "@/stores/authStore"

// Auth
export function useLogin() {
  const setUser = useAuthStore((s) => s.setUser)

  return useMutation({
    mutationFn: ({ email, password }: { email: string; password: string }) =>
      api.login(email, password),
    onSuccess: (data, variables) => {
      setUser({ nombre: data.nombre, rol: data.rol, email: variables.email })
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
        setUser({ nombre: data.nombre, rol: data.rol, email: data.email })
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

export function useContact(id: string) {
  return useQuery({
    queryKey: ["contacts", id],
    queryFn: () => api.getContact(id),
    enabled: !!id,
  })
}

export function useUpdateContact() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, name, phone }: { id: string; name: string; phone?: string }) =>
      api.updateContact(id, { name, phone }),
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

export function useCreateCustomerDemand() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: import("@/lib/types").CreateCustomerDemandInput) => api.createCustomerDemand(data),
    onSuccess: (_demand, variables) => queryClient.invalidateQueries({ queryKey: ["demands", variables.contactId] }),
  })
}

export function useDemandMatches(demandId: string) {
  return useQuery({
    queryKey: ["demand-matches", demandId],
    queryFn: () => api.getDemandMatches(demandId),
    enabled: !!demandId,
  })
}

export function useLinkDemandProperty() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ demandId, propertyId }: { demandId: string; propertyId: string }) => api.linkDemandProperty(demandId, propertyId),
    onSuccess: (_result, variables) => queryClient.invalidateQueries({ queryKey: ["demand-matches", variables.demandId] }),
  })
}

export function useSetDemandPropertyStatus() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ demandId, propertyId, status }: { demandId: string; propertyId: string; status: string }) => api.setDemandPropertyStatus(demandId, propertyId, status),
    onSuccess: (_result, variables) => queryClient.invalidateQueries({ queryKey: ["demand-matches", variables.demandId] }),
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
export function useVisitas() {
  return useQuery({
    queryKey: ["visitas"],
    queryFn: () => api.getVisitas(),
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

// Inmuebles
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
