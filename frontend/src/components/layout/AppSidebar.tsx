import { NavLink, useLocation } from "react-router-dom"
import { cn } from "@/lib/utils"
import { NAV_ITEMS } from "@/lib/constants"
import {
  Activity,
  Users,
  Home,
  Calendar,
  MessageSquare,
  BarChart3,
  Bell,
  Settings,
  Building2,
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
}

interface AppSidebarProps {
  collapsed?: boolean
}

export function AppSidebar({ collapsed = false }: AppSidebarProps) {
  const location = useLocation()

  return (
    <aside
      className={cn(
        "flex flex-col h-screen border-r transition-all duration-300",
        "bg-[#1a4d2e] text-white dark:bg-[#1e293b]",
        collapsed ? "w-16" : "w-64"
      )}
    >
      {/* Logo */}
      <div className="flex items-center gap-2 px-4 h-14 border-b border-white/10">
        <Building2 className="h-6 w-6 text-[#c9a84c] flex-shrink-0" />
        {!collapsed && (
          <span className="text-lg font-semibold tracking-tight">Enlyce</span>
        )}
      </div>

      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto py-3">
        <ul className="space-y-1 px-2">
          {NAV_ITEMS.map((item) => {
            const Icon = iconMap[item.icon]
            const isActive = location.pathname === item.href

            return (
              <li key={item.href}>
                <NavLink
                  to={item.href}
                  className={cn(
                    "flex items-center gap-3 px-3 py-2.5 rounded-md text-sm font-medium transition-colors",
                    isActive
                      ? "bg-white/15 text-white"
                      : "text-white/70 hover:bg-white/10 hover:text-white"
                  )}
                  title={collapsed ? item.title : undefined}
                >
                  {Icon && <Icon className="h-5 w-5 flex-shrink-0" />}
                  {!collapsed && <span>{item.title}</span>}
                </NavLink>
              </li>
            )
          })}
        </ul>
      </nav>

      {/* Footer */}
      {!collapsed && (
        <div className="px-4 py-3 border-t border-white/10">
          <p className="text-xs text-white/50">L&C Propiedad Raiz</p>
        </div>
      )}
    </aside>
  )
}
