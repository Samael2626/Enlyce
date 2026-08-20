import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { useLogin } from "@/hooks/useApi"
import { useAuthStore } from "@/stores/authStore"
import { Building2, Loader2 } from "lucide-react"

export function LoginPage() {
  const [email, setEmail] = useState("")
  const [password, setPassword] = useState("")
  const [error, setError] = useState("")
  const navigate = useNavigate()
  const login = useLogin()
  const setToken = useAuthStore((s) => s.setToken)
  const setUser = useAuthStore((s) => s.setUser)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError("")

    login.mutate(
      { email, password },
      {
        onSuccess: (data) => {
          setToken(data.token)
          setUser({ nombre: data.nombre, rol: data.rol, email })
          navigate("/dashboard")
        },
        onError: (err) => {
          setError(err.message || "Credenciales incorrectas")
        },
      }
    )
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-background">
      <div className="w-full max-w-md p-8 space-y-6">
        {/* Logo */}
        <div className="flex flex-col items-center gap-3">
          <div className="h-14 w-14 rounded-xl bg-[#1a4d2e] dark:bg-[#22c55e] flex items-center justify-center">
            <Building2 className="h-8 w-8 text-white" />
          </div>
          <div className="text-center">
            <h1 className="text-2xl font-bold text-foreground">Enlyce</h1>
            <p className="text-sm text-muted-foreground">
              CRM Inmobiliario — L&C Propiedad Raiz
            </p>
          </div>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <label
              htmlFor="email"
              className="text-sm font-medium text-foreground"
            >
              Correo electronico
            </label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-3 py-2 border rounded-md bg-background text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-[#1a4d2e] dark:focus:ring-[#22c55e]"
              placeholder="admin@enlyce.com"
              required
            />
          </div>

          <div className="space-y-2">
            <label
              htmlFor="password"
              className="text-sm font-medium text-foreground"
            >
              Contrasena
            </label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-3 py-2 border rounded-md bg-background text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-[#1a4d2e] dark:focus:ring-[#22c55e]"
              placeholder="••••••••"
              required
            />
          </div>

          {error && (
            <div className="p-3 text-sm text-destructive bg-destructive/10 rounded-md">
              {error}
            </div>
          )}

          <button
            type="submit"
            disabled={login.isPending}
            className="w-full py-2.5 px-4 bg-[#1a4d2e] hover:bg-[#154225] dark:bg-[#22c55e] dark:hover:bg-[#16a34a] text-white font-medium rounded-md transition-colors disabled:opacity-50 flex items-center justify-center gap-2"
          >
            {login.isPending ? (
              <>
                <Loader2 className="h-4 w-4 animate-spin" />
                Ingresando...
              </>
            ) : (
              "Iniciar sesion"
            )}
          </button>
        </form>

        <p className="text-xs text-center text-muted-foreground">
          Ley 1581 de 2012 — Proteccion de datos personales
        </p>
      </div>
    </div>
  )
}
