import { useAuthStore } from "@/stores/authStore"
import { useNavigate } from "react-router-dom"
import { LogOut, Search, Sun, Moon, User } from "lucide-react"
import { useTheme } from "next-themes"
import { useEffect, useState } from "react"

export function Header() {
  const { user, logout } = useAuthStore()
  const navigate = useNavigate()
  const { theme, setTheme } = useTheme()
  const [mounted, setMounted] = useState(false)

  useEffect(() => setMounted(true), [])

  const handleLogout = () => {
    logout()
    navigate("/login")
  }

  return (
    <header className="h-14 border-b bg-white dark:bg-[#0f172a] flex items-center justify-between px-4">
      {/* Search trigger */}
      <button className="flex items-center gap-2 px-3 py-1.5 text-sm text-muted-foreground border rounded-md hover:bg-accent/5 transition-colors">
        <Search className="h-4 w-4" />
        <span>Buscar...</span>
        <kbd className="ml-2 px-1.5 py-0.5 text-xs bg-muted rounded">
          Ctrl+K
        </kbd>
      </button>

      {/* Right side */}
      <div className="flex items-center gap-3">
        {/* Dark mode toggle */}
        <button
          onClick={() => setTheme(theme === "dark" ? "light" : "dark")}
          className="p-2 text-muted-foreground hover:text-foreground transition-colors"
        >
          {mounted && theme === "dark" ? (
            <Sun className="h-5 w-5" />
          ) : (
            <Moon className="h-5 w-5" />
          )}
        </button>

        {/* User menu */}
        <div className="flex items-center gap-2 px-3 py-1.5 rounded-md hover:bg-accent/5 transition-colors">
          <div className="h-8 w-8 rounded-full bg-[#1a4d2e] dark:bg-[#22c55e] flex items-center justify-center">
            <User className="h-4 w-4 text-white" />
          </div>
          <div className="text-sm">
            <p className="font-medium">{user?.nombre || "Usuario"}</p>
            <p className="text-muted-foreground text-xs">{user?.rol}</p>
          </div>
        </div>

        {/* Logout */}
        <button
          onClick={handleLogout}
          className="p-2 text-muted-foreground hover:text-destructive transition-colors"
          title="Cerrar sesion"
        >
          <LogOut className="h-5 w-5" />
        </button>
      </div>
    </header>
  )
}
