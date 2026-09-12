import { useAuthStore } from "@/stores/authStore"
import { useNavigate } from "react-router-dom"
import { useLocation } from "react-router-dom"
import { LogOut, Sun, Moon, User } from "lucide-react"
import { useTheme } from "next-themes"
import { NAV_ITEMS } from "@/lib/constants"

export function Header() {
  const { user, logout } = useAuthStore()
  const navigate = useNavigate()
  const { theme, setTheme } = useTheme()
  const location = useLocation()
  const pageTitle = NAV_ITEMS.find((item) => item.href === location.pathname)?.title || "Panel"

  const handleLogout = () => {
    logout()
    navigate("/login")
  }

  return (
    <header className="flex min-h-20 items-center justify-between gap-4 border-b border-border bg-card/90 px-4 md:px-8 xl:px-12">
      <div className="min-w-0">
        <p className="crm-eyebrow">L&C / Enlyce</p>
        <p className="mt-1 truncate font-display text-xl font-semibold tracking-tight">{pageTitle}</p>
      </div>
      <div className="flex items-center gap-2 md:gap-4">
        <button
          onClick={() => setTheme(theme === "dark" ? "light" : "dark")}
          className="rounded-md p-2 text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
          aria-label="Cambiar tema"
        >
          {theme === "dark" ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
        </button>

        <div className="flex items-center gap-3 border-l border-border pl-3 md:pl-4">
          <div className="flex h-9 w-9 items-center justify-center rounded-md border border-border bg-muted">
            <User className="h-4 w-4 text-foreground" />
          </div>
          <div className="hidden text-sm sm:block">
            <p className="font-semibold leading-tight">{user?.nombre || "Usuario"}</p>
            <p className="text-xs text-muted-foreground">{user?.rol}</p>
          </div>
        </div>

        <button
          onClick={handleLogout}
          className="rounded-md p-2 text-muted-foreground transition-colors hover:bg-muted hover:text-destructive"
          title="Cerrar sesión"
          aria-label="Cerrar sesión"
        >
          <LogOut className="h-5 w-5" />
        </button>
      </div>
    </header>
  )
}
