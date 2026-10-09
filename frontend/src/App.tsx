import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom"
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { ThemeProvider } from "next-themes"
import { MainLayout } from "@/components/layout/MainLayout"
import { LoginPage } from "@/pages/LoginPage"
import { DashboardPage } from "@/pages/DashboardPage"
import { PipelinePage } from "@/pages/PipelinePage"
import { LeadsPage } from "@/pages/LeadsPage"
import { InmueblesPage } from "@/pages/InmueblesPage"
import { AlertasPage } from "@/pages/AlertasPage"
import { PublicacionesPage } from "@/pages/PublicacionesPage"
import { BillingPage } from "@/pages/BillingPage"
import { BillingResultPage } from "@/pages/BillingResultPage"
import { AnaliticaPage } from "@/pages/AnaliticaPage"
import { ContactsPage } from "@/pages/ContactsPage"
import { CalendarPage } from "@/pages/CalendarPage"
import { SettingsPage } from "@/pages/SettingsPage"

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000,
      retry: 1,
    },
  },
})

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider
        attribute="class"
        defaultTheme="light"
        enableSystem
        disableTransitionOnChange
      >
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route element={<MainLayout />}>
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/pipeline" element={<PipelinePage />} />
              <Route path="/leads" element={<LeadsPage />} />
              <Route path="/contactos" element={<ContactsPage />} />
              <Route path="/inmuebles" element={<InmueblesPage />} />
              <Route path="/publicaciones" element={<PublicacionesPage />} />
              <Route path="/alertas" element={<AlertasPage />} />
              <Route path="/facturacion" element={<BillingPage />} />
              <Route path="/facturacion/respuesta" element={<BillingResultPage />} />
              <Route path="/calendario" element={<CalendarPage />} />
              <Route path="/chat" element={<Placeholder title="Chat" />} />
              <Route path="/reportes" element={<AnaliticaPage />} />
              <Route path="/config" element={<SettingsPage />} />
            </Route>
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
          </Routes>
        </BrowserRouter>
      </ThemeProvider>
    </QueryClientProvider>
  )
}

function Placeholder({ title }: { title: string }) {
  return (
    <div className="crm-panel flex min-h-80 items-center justify-center border-t-4 border-t-accent p-8">
      <div className="text-center">
        <span className="crm-eyebrow">Próximamente</span>
        <h1 className="mt-3 font-display text-3xl text-foreground">{title}</h1>
        <p className="mt-2 text-sm text-muted-foreground">
          Módulo en construcción
        </p>
      </div>
    </div>
  )
}
