import { Outlet, Navigate } from "react-router-dom"
import { useAuthStore } from "@/stores/authStore"
import { useMe } from "@/hooks/useApi"
import { AppSidebar } from "./AppSidebar"
import { Header } from "./Header"

export function MainLayout() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const session = useMe()

  if (session.isPending && !isAuthenticated) {
    return (
      <div className="flex min-h-screen items-center justify-center text-sm text-muted-foreground">
        Verificando sesión...
      </div>
    )
  }

  if (session.isError || !isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  return (
    <div className="min-h-screen lg:flex lg:h-screen lg:overflow-hidden">
      <AppSidebar />
      <div className="flex min-w-0 flex-1 flex-col lg:h-screen lg:overflow-hidden">
        <Header />
        <main className="min-w-0 flex-1 px-4 py-6 md:px-8 md:py-8 lg:overflow-y-auto xl:px-12">
          <div className="mx-auto max-w-[1500px]"><Outlet /></div>
        </main>
      </div>
    </div>
  )
}
