import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { useLogin } from "@/hooks/useApi"
import { ArrowRight, Loader2 } from "lucide-react"

export function LoginPage() {
  const [email, setEmail] = useState("")
  const [password, setPassword] = useState("")
  const [error, setError] = useState("")
  const navigate = useNavigate()
  const login = useLogin()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError("")

    login.mutate(
      { email, password },
      {
        onSuccess: () => {
          navigate("/dashboard")
        },
        onError: (err) => {
          setError(err.message || "Credenciales incorrectas")
        },
      }
    )
  }

  return (
    <div className="grid min-h-screen bg-background lg:grid-cols-[1.08fr_.92fr]">
      <section className="crm-hero flex min-h-[310px] flex-col justify-between p-7 md:p-12 lg:min-h-screen lg:p-16">
        <div className="relative z-10 flex items-center gap-3">
          <span role="img" aria-label="L&C Propiedad Raíz" className="crm-brand-seal h-20 w-20" />
          <div>
            <span className="block font-display text-3xl leading-none">Enlyce</span>
            <span className="mt-1 block text-[.65rem] font-semibold uppercase tracking-[.17em] text-white/70">CRM inmobiliario</span>
          </div>
        </div>
        <div className="relative z-10 max-w-xl">
          <p className="text-[.7rem] font-bold uppercase tracking-[.18em] text-[#e7b99f]">L&C Propiedad Raíz</p>
          <p className="mt-5 font-display text-[clamp(2.8rem,5vw,5.8rem)] leading-[.98] tracking-[-.05em]">Un lugar para cada conversación importante.</p>
          <p className="mt-6 max-w-md text-sm leading-7 text-white/80">Personas, inmuebles y oportunidades en un mismo espacio de trabajo.</p>
        </div>
        <p className="relative z-10 hidden text-xs font-medium tracking-wide text-white/65 lg:block">Tu hogar, nuestra prioridad.</p>
      </section>

      <main className="flex items-center justify-center px-5 py-12 md:px-10 lg:py-16">
        <div className="w-full max-w-md">
          <span className="crm-eyebrow">Acceso privado</span>
          <h1 className="crm-page-title mt-4">Bienvenido de nuevo.</h1>
          <p className="mt-4 text-sm leading-6 text-muted-foreground">Ingresa con tu cuenta para continuar la gestión de L&C.</p>

          <form onSubmit={handleSubmit} className="crm-panel mt-9 space-y-5 border-t-4 border-t-accent p-6 md:p-8">
            <div className="space-y-2">
              <label htmlFor="email" className="text-sm font-semibold">Correo electrónico</label>
              <input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full rounded-sm border border-input bg-background px-3 py-3 text-foreground placeholder:text-muted-foreground"
                placeholder="tu@correo.com"
                autoComplete="email"
                required
              />
            </div>
            <div className="space-y-2">
              <label htmlFor="password" className="text-sm font-semibold">Contraseña</label>
              <input
                id="password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full rounded-sm border border-input bg-background px-3 py-3 text-foreground placeholder:text-muted-foreground"
                placeholder="••••••••"
                autoComplete="current-password"
                required
              />
            </div>
            {error && <p role="alert" className="rounded-sm bg-destructive/10 p-3 text-sm text-destructive">{error}</p>}
            <button type="submit" disabled={login.isPending} className="crm-button w-full">
              {login.isPending ? <><Loader2 className="h-4 w-4 animate-spin" /> Ingresando...</> : <>Iniciar sesión <ArrowRight className="h-4 w-4" /></>}
            </button>
          </form>

          <p className="mt-7 text-xs text-muted-foreground">Acceso protegido · Tratamiento de datos conforme a la Ley 1581 de 2012.</p>
        </div>
      </main>
    </div>
  )
}
