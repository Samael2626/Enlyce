import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import { useAuthStore } from "@/stores/authStore"

// Auth
export function useLogin() {
  const setUser = useAuthStore((s) => s.setUser)
  const setToken = useAuthStore((s) => s.setToken)

  return useMutation({
    mutationFn: ({ email, password }: { email: string; password: string }) =>
      api.login(email, password),
    onSuccess: (data) => {
      setToken(data.token)
      setUser({ nombre: data.nombre, rol: data.rol, email: "" })
    },
  })
}

export function useMe() {
  const setUser = useAuthStore((s) => s.setUser)
  const token = useAuthStore((s) => s.token)

  return useQuery({
    queryKey: ["auth", "me"],
    queryFn: async () => {
      const data = await api.getMe()
      setUser({ nombre: data.nombre, rol: data.rol, email: data.email })
      return data
    },
    enabled: !!token,
    retry: false,
  })
}

// Pipeline
export function usePipeline() {
  return useQuery({
    queryKey: ["pipeline"],
    queryFn: () => api.getPipeline(),
  })
}

export function useMoveLeadInPipeline() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      leadId,
      nuevaEtapa,
    }: {
      leadId: string
      nuevaEtapa: string
    }) => api.moveLeadInPipeline(leadId, nuevaEtapa),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["pipeline"] })
    },
  })
}

export function useAssignLead() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      leadId,
      asesorId,
    }: {
      leadId: string
      asesorId: string
    }) => api.assignLeadToAsesor(leadId, asesorId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["pipeline"] })
    },
  })
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

export function useRevocarConsentimiento() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (leadId: string) => api.revocarConsentimiento(leadId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["leads"] })
    },
  })
}
