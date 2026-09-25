import { NavLink, useLocation } from "react-router-dom"
import { cn } from "@/lib/utils"
import { NAV_ITEMS } from "@/lib/constants"
import { useAuthStore } from "@/stores/authStore"
import {
  Activity,
  Users,
  Home,
  Calendar,
  MessageSquare,
  BarChart3,
  Bell,
  Settings,
  LayoutDashboard,
  CreditCard,
} from "lucide-react"

const iconMap: Record<string, React.ElementType> = {
  Activity,
  Users,
  Home,
  Calendar,
  MessageSquare,
  BarChart3,
  Bell,
  Settings,
  LayoutDashboard,
  CreditCard,
}

interface AppSidebarProps {
  collapsed?: boolean
}

export function AppSidebar({ collapsed = false }: AppSidebarProps) {
  const location = useLocation()
  const user = useAuthStore((state) => state.user)
  const visibleItems = NAV_ITEMS.filter(
    (item) => !("adminOnly" in item && item.adminOnly) || user?.rol === "Administrador"
  )

  return (
    <aside
      className={cn(
        "flex flex-col border-r border-white/10 bg-sidebar text-sidebar-foreground lg:h-screen lg:flex-shrink-0",
        collapsed ? "lg:w-20" : "lg:w-64"
      )}
    >
      <div className="flex h-24 items-center gap-3 border-b border-white/15 px-5">
        <img src="/lyc-logo.png" alt="L&C Propiedad Raíz" className="crm-brand-seal h-16 w-16" />
        {!collapsed && (
          <div>
            <span className="block font-display text-[1.65rem] leading-none tracking-tight">Enlyce</span>
            <span className="mt-1 block text-[.61rem] font-semibold uppercase tracking-[.17em] text-white/65">Espacio de trabajo</span>
          </div>
        )}
      </div>

      <nav aria-label="Navegación principal" className="overflow-x-auto lg:flex-1 lg:overflow-y-auto lg:py-7">
        {!collapsed && <p className="hidden px-6 pb-3 text-[.65rem] font-bold uppercase tracking-[.18em] text-white/45 lg:block">Operación diaria</p>}
        <ul className="flex gap-1 px-3 py-2 lg:block lg:space-y-1 lg:px-3 lg:py-0">
          {visibleItems.map((item) => {
            const Icon = iconMap[item.icon]
            const isActive = location.pathname === item.href

            return (
              <li key={item.href}>
                <NavLink
                  to={item.href}
                  className={cn(
                    "flex items-center gap-3 whitespace-nowrap rounded-md border border-transparent px-3 py-3 text-[.83rem] font-semibold transition-colors lg:border-l-2",
                    isActive
                      ? "border-white/25 bg-white/12 text-white lg:border-l-accent"
                      : "text-white/70 hover:bg-white/8 hover:text-white"
                  )}
                  title={collapsed ? item.title : undefined}
                >
                  {Icon && <Icon className={cn("h-4 w-4 flex-shrink-0", isActive && "text-[#e3be65]")} />}
                  {!collapsed && <span>{item.title}</span>}
                </NavLink>
              </li>
            )
          })}
        </ul>
      </nav>

      {!collapsed && (
        <div className="hidden border-t border-white/15 px-6 py-5 lg:block">
          <p className="text-[.67rem] font-semibold uppercase tracking-[.13em] text-white/65">L&C Propiedad Raíz</p>
          <p className="mt-1 text-xs text-white/45">Tu hogar, nuestra prioridad.</p>
        </div>
      )}
    </aside>
  )
}
