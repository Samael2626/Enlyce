import { useState, useEffect, useRef, useMemo } from "react"
import { Send, Users, MessageSquare, CheckCheck, Check, Circle } from "lucide-react"
import { api } from "@/api/client"
import { useAuthStore } from "@/stores/authStore"
import { useChatHub } from "@/hooks/useChatHub"
import type { ChatUser, ChatMessage } from "@/lib/types"

export function ChatPage() {
  const currentUser = useAuthStore((state) => state.user)
  const [users, setUsers] = useState<ChatUser[]>([])
  const [activeRecipientId, setActiveRecipientId] = useState<string | null>(null) // null = general channel
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [inputValue, setInputValue] = useState("")
  const [loadingHistory, setLoadingHistory] = useState(false)
  const [typingUsers, setTypingUsers] = useState<Record<string, boolean>>({})
  const messagesEndRef = useRef<HTMLDivElement>(null)
  const typingTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  // Real-time hub integration
  const handleReceiveMessage = (msg: ChatMessage) => {
    // If message belongs to current open view
    const isForCurrentView =
      (activeRecipientId === null && msg.recipientId === null) ||
      (activeRecipientId !== null && (msg.senderId === activeRecipientId || msg.recipientId === activeRecipientId))

    if (isForCurrentView) {
      setMessages((prev) => [...prev, msg])
      // If direct message received in current view, mark it as read immediately
      if (activeRecipientId && msg.senderId === activeRecipientId) {
        api.markChatRead(activeRecipientId).catch(() => {})
      }
    } else if (msg.senderId !== currentUser?.id) {
      // Increment unread count for sender
      setUsers((prev) =>
        prev.map((u) => (u.id === msg.senderId ? { ...u, unreadCount: u.unreadCount + 1 } : u))
      )
    }
  }

  const handleMessagesRead = (event: { readBy: string; readAtUtc: string }) => {
    if (activeRecipientId === event.readBy) {
      setMessages((prev) =>
        prev.map((m) => (m.senderId === currentUser?.id ? { ...m, isRead: true, readAtUtc: event.readAtUtc } : m))
      )
    }
  }

  const handleUserTyping = (event: { senderId: string; inChannel?: boolean }) => {
    if (
      (activeRecipientId === null && event.inChannel) ||
      (activeRecipientId !== null && activeRecipientId === event.senderId)
    ) {
      setTypingUsers((prev) => ({ ...prev, [event.senderId]: true }))
      setTimeout(() => {
        setTypingUsers((prev) => ({ ...prev, [event.senderId]: false }))
      }, 3000)
    }
  }

  const { isConnected, sendMessage: sendViaHub, sendTyping } = useChatHub(
    handleReceiveMessage,
    handleMessagesRead,
    handleUserTyping
  )

  // Load team users list
  useEffect(() => {
    api.getChatUsers()
      .then((data) => setUsers(data))
      .catch((err) => console.error("Error al cargar usuarios de chat:", err))
  }, [])

  // Load message history when changing channel / recipient
  useEffect(() => {
    setLoadingHistory(true)
    api.getChatMessages(activeRecipientId)
      .then((data) => {
        setMessages(data)
        if (activeRecipientId) {
          api.markChatRead(activeRecipientId)
            .then(() => {
              setUsers((prev) =>
                prev.map((u) => (u.id === activeRecipientId ? { ...u, unreadCount: 0 } : u))
              )
            })
            .catch(() => {})
        }
      })
      .catch((err) => console.error("Error al cargar historial de chat:", err))
      .finally(() => setLoadingHistory(false))
  }, [activeRecipientId])

  // Scroll to bottom on new messages
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" })
  }, [messages])

  const activeUser = useMemo(() => {
    return users.find((u) => u.id === activeRecipientId)
  }, [users, activeRecipientId])

  const isAnyoneTyping = useMemo(() => {
    return Object.values(typingUsers).some(Boolean)
  }, [typingUsers])

  const handleSend = async (e: React.FormEvent) => {
    e.preventDefault()
    const content = inputValue.trim()
    if (!content) return

    setInputValue("")

    // Try via SignalR hub first, fallback to REST API
    const sent = await sendViaHub(activeRecipientId, content)
    if (!sent) {
      try {
        const msg = await api.sendChatMessage({ recipientId: activeRecipientId, content })
        setMessages((prev) => [...prev, msg])
      } catch (err) {
        console.error("Error al enviar mensaje:", err)
      }
    }
  }

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setInputValue(e.target.value)
    if (!typingTimeoutRef.current) {
      sendTyping(activeRecipientId)
      typingTimeoutRef.current = setTimeout(() => {
        typingTimeoutRef.current = null
      }, 2000)
    }
  }

  const formatTime = (isoString: string) => {
    try {
      const d = new Date(isoString)
      return d.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
    } catch {
      return ""
    }
  }

  return (
    <div className="flex h-[calc(100vh-8.5rem)] flex-col gap-4 md:flex-row">
      {/* Sidebar de canales y asesores */}
      <aside className="crm-panel flex w-full flex-col p-4 md:w-80 shrink-0">
        <div className="flex items-center justify-between border-b pb-3 border-border/40">
          <div>
            <span className="crm-eyebrow">Equipo</span>
            <h2 className="font-display text-lg text-foreground">Chat interno</h2>
          </div>
          <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
            <Circle className={`h-2.5 w-2.5 fill-current ${isConnected ? "text-emerald-500" : "text-amber-500"}`} />
            <span>{isConnected ? "En vivo" : "Conectando"}</span>
          </div>
        </div>

        <div className="mt-3 flex-1 overflow-y-auto space-y-1">
          {/* Canal General */}
          <button
            type="button"
            onClick={() => setActiveRecipientId(null)}
            className={`w-full flex items-center justify-between rounded-lg px-3 py-2.5 text-left text-sm font-medium transition-colors ${
              activeRecipientId === null
                ? "bg-primary text-primary-foreground shadow-sm"
                : "text-muted-foreground hover:bg-muted/60 hover:text-foreground"
            }`}
          >
            <div className="flex items-center gap-2.5">
              <Users className="h-4 w-4" />
              <span>Canal del equipo</span>
            </div>
            <span className="text-[0.7rem] uppercase tracking-wider opacity-70">General</span>
          </button>

          {/* Separador */}
          <div className="pt-3 pb-1.5 px-3">
            <span className="text-[0.68rem] font-bold uppercase tracking-wider text-muted-foreground/80">
              Mensajes directos
            </span>
          </div>

          {/* Lista de usuarios */}
          {users.map((user) => {
            const isSelected = activeRecipientId === user.id
            const initials = user.name
              .split(" ")
              .map((n) => n[0])
              .join("")
              .slice(0, 2)
              .toUpperCase()

            return (
              <button
                key={user.id}
                type="button"
                onClick={() => setActiveRecipientId(user.id)}
                className={`w-full flex items-center justify-between rounded-lg px-3 py-2 text-left text-sm transition-colors ${
                  isSelected
                    ? "bg-primary text-primary-foreground shadow-sm"
                    : "text-muted-foreground hover:bg-muted/60 hover:text-foreground"
                }`}
              >
                <div className="flex items-center gap-2.5 min-w-0">
                  <div
                    className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-xs font-semibold ${
                      isSelected
                        ? "bg-primary-foreground/20 text-primary-foreground"
                        : "bg-muted text-foreground"
                    }`}
                  >
                    {initials}
                  </div>
                  <div className="min-w-0 truncate">
                    <p className="truncate font-medium leading-none">{user.name}</p>
                    <p className="mt-1 truncate text-xs opacity-70">{user.role}</p>
                  </div>
                </div>

                {user.unreadCount > 0 && (
                  <span className="ml-2 flex h-5 min-w-5 items-center justify-center rounded-full bg-accent px-1.5 text-[0.65rem] font-bold text-accent-foreground">
                    {user.unreadCount}
                  </span>
                )}
              </button>
            )
          })}

          {users.length === 0 && (
            <p className="px-3 py-4 text-center text-xs text-muted-foreground">
              No hay otros asesores registrados en el equipo.
            </p>
          )}
        </div>
      </aside>

      {/* Área principal de conversación */}
      <section className="crm-panel flex flex-1 flex-col overflow-hidden">
        {/* Cabecera de la conversación */}
        <div className="flex items-center justify-between border-b px-6 py-3.5 border-border/40 bg-card/40">
          <div className="flex items-center gap-3">
            <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-accent/15 text-accent font-display text-sm">
              {activeRecipientId === null ? <Users className="h-5 w-5" /> : <MessageSquare className="h-5 w-5" />}
            </div>
            <div>
              <h3 className="font-display text-base font-semibold text-foreground">
                {activeRecipientId === null ? "Canal General de la Agencia" : activeUser?.name || "Conversación"}
              </h3>
              <p className="text-xs text-muted-foreground">
                {activeRecipientId === null
                  ? "Mensajes visibles para todos los asesores y administradores"
                  : `${activeUser?.role || "Asesor"} • Comunicación directa cifrada`}
              </p>
            </div>
          </div>
        </div>

        {/* Historial de mensajes */}
        <div className="flex-1 overflow-y-auto p-6 space-y-4">
          {loadingHistory && (
            <div className="flex h-full items-center justify-center">
              <span className="text-sm text-muted-foreground animate-pulse">Cargando mensajes...</span>
            </div>
          )}

          {!loadingHistory && messages.length === 0 && (
            <div className="flex h-full flex-col items-center justify-center text-center p-8">
              <div className="flex h-12 w-12 items-center justify-center rounded-full bg-muted text-muted-foreground">
                <MessageSquare className="h-6 w-6" />
              </div>
              <h4 className="mt-3 font-display text-base font-medium text-foreground">No hay mensajes aún</h4>
              <p className="mt-1 text-xs text-muted-foreground max-w-sm">
                Sé el primero en enviar un mensaje a este canal o asesor.
              </p>
            </div>
          )}

          {messages.map((message) => {
            const isMe = message.senderId === currentUser?.id

            return (
              <div key={message.id} className={`flex flex-col ${isMe ? "items-end" : "items-start"}`}>
                <div className="flex items-baseline gap-2 mb-1">
                  {!isMe && (
                    <span className="text-xs font-semibold text-foreground">{message.senderName}</span>
                  )}
                  <span className="text-[0.68rem] text-muted-foreground/70">
                    {formatTime(message.sentAtUtc)}
                  </span>
                </div>

                <div
                  className={`relative max-w-lg rounded-2xl px-4 py-2.5 text-sm shadow-sm ${
                    isMe
                      ? "bg-primary text-primary-foreground rounded-tr-xs"
                      : "bg-muted text-foreground rounded-tl-xs"
                  }`}
                >
                  <p className="whitespace-pre-wrap break-words">{message.content}</p>

                  {isMe && message.recipientId !== null && (
                    <div className="mt-1 flex items-center justify-end gap-1 text-[0.65rem] opacity-75">
                      {message.isRead ? (
                        <>
                          <span>Leído</span>
                          <CheckCheck className="h-3 w-3 text-sky-300" />
                        </>
                      ) : (
                        <>
                          <span>Enviado</span>
                          <Check className="h-3 w-3" />
                        </>
                      )}
                    </div>
                  )}
                </div>
              </div>
            )
          })}

          {isAnyoneTyping && (
            <div className="flex items-center gap-2 text-xs italic text-muted-foreground">
              <span className="animate-pulse">Escribiendo...</span>
            </div>
          )}

          <div ref={messagesEndRef} />
        </div>

        {/* Formulario de envío */}
        <form onSubmit={handleSend} className="border-t p-4 border-border/40 bg-card/60">
          <div className="flex items-center gap-2">
            <input
              type="text"
              value={inputValue}
              onChange={handleInputChange}
              placeholder={
                activeRecipientId === null
                  ? "Escribe un mensaje para todo el equipo..."
                  : `Mensaje directo para ${activeUser?.name || "asesor"}...`
              }
              maxLength={2000}
              className="flex-1 rounded-lg border border-input bg-background px-4 py-2.5 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-accent"
            />
            <button
              type="submit"
              disabled={!inputValue.trim()}
              className="inline-flex h-10 items-center justify-center rounded-lg bg-accent px-4 py-2 text-sm font-semibold text-accent-foreground shadow hover:opacity-90 transition disabled:opacity-40 disabled:cursor-not-allowed"
            >
              <Send className="h-4 w-4 mr-1.5" />
              <span>Enviar</span>
            </button>
          </div>
        </form>
      </section>
    </div>
  )
}
