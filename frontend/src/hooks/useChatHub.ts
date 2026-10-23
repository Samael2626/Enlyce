import { useEffect, useRef, useState, useCallback } from "react"
import * as signalR from "@microsoft/signalr"
import type { ChatMessage } from "@/lib/types"

export function useChatHub(
  onReceiveMessage?: (message: ChatMessage) => void,
  onMessagesRead?: (event: { readBy: string; readAtUtc: string }) => void,
  onUserTyping?: (event: { senderId: string; inChannel?: boolean }) => void
) {
  const connectionRef = useRef<signalR.HubConnection | null>(null)
  const [isConnected, setIsConnected] = useState(false)

  // Use refs for callbacks so connection does not recreate on callback reference change
  const onReceiveRef = useRef(onReceiveMessage)
  onReceiveRef.current = onReceiveMessage
  const onReadRef = useRef(onMessagesRead)
  onReadRef.current = onMessagesRead
  const onTypingRef = useRef(onUserTyping)
  onTypingRef.current = onUserTyping

  useEffect(() => {
    const token = localStorage.getItem("enlyce_token")
    if (!token) return

    const connection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/chat", {
        accessTokenFactory: () => localStorage.getItem("enlyce_token") || "",
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    connection.on("ReceiveMessage", (message: ChatMessage) => {
      onReceiveRef.current?.(message)
    })

    connection.on("MessagesRead", (event: { readBy: string; readAtUtc: string }) => {
      onReadRef.current?.(event)
    })

    connection.on("UserTyping", (event: { senderId: string; inChannel?: boolean }) => {
      onTypingRef.current?.(event)
    })

    connection
      .start()
      .then(() => {
        setIsConnected(true)
        connectionRef.current = connection
      })
      .catch((err) => {
        console.warn("SignalR connection error (chat will use REST fallback):", err)
        setIsConnected(false)
      })

    connection.onreconnected(() => {
      setIsConnected(true)
    })

    connection.onclose(() => {
      setIsConnected(false)
    })

    return () => {
      connection.stop()
    }
  }, [])

  const sendMessage = useCallback(async (recipientId: string | null, content: string) => {
    if (connectionRef.current && connectionRef.current.state === signalR.HubConnectionState.Connected) {
      await connectionRef.current.invoke("SendMessage", recipientId, content)
      return true
    }
    return false
  }, [])

  const sendTyping = useCallback(async (recipientId: string | null) => {
    if (connectionRef.current && connectionRef.current.state === signalR.HubConnectionState.Connected) {
      await connectionRef.current.invoke("SendTyping", recipientId)
    }
  }, [])

  const markAsRead = useCallback(async (senderId: string) => {
    if (connectionRef.current && connectionRef.current.state === signalR.HubConnectionState.Connected) {
      await connectionRef.current.invoke("MarkAsRead", senderId)
    }
  }, [])

  return { isConnected, sendMessage, sendTyping, markAsRead }
}
