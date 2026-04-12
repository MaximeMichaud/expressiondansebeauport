import * as signalR from '@microsoft/signalr'
import { ref, onUnmounted } from 'vue'

const connection = ref<signalR.HubConnection | null>(null)
const isConnected = ref(false)
const messageCallbacks = new Set<(data: any) => void>()

export function useSignalR() {
  function getAccessToken(): string {
    const cookies = document.cookie.split('; ')
    const accessCookie = cookies.find(c => c.startsWith('accessToken='))
    return accessCookie?.split('=')[1] ?? ''
  }

  async function start() {
    if (connection.value?.state === signalR.HubConnectionState.Connected) return
    if (!getAccessToken()) return

    connection.value = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/chat', {
        accessTokenFactory: () => getAccessToken()
      })
      .configureLogging(signalR.LogLevel.None)
      .withAutomaticReconnect()
      .build()

    // Forward SignalR events to all registered callbacks
    connection.value.on('ReceiveMessage', (data: any) => {
      messageCallbacks.forEach(cb => cb(data))
    })

    connection.value.onreconnected(() => { isConnected.value = true })
    connection.value.onclose(() => { isConnected.value = false })

    try {
      await connection.value.start()
      isConnected.value = true
    } catch {
      isConnected.value = false
    }
  }

  function onMessage(callback: (data: any) => void) {
    messageCallbacks.add(callback)
  }

  function offMessage(callback: (data: any) => void) {
    messageCallbacks.delete(callback)
  }

  async function stop() {
    if (connection.value) {
      await connection.value.stop()
      isConnected.value = false
    }
  }

  onUnmounted(() => {
    // Don't stop on unmount — connection is shared
  })

  return { connection, isConnected, start, stop, onMessage, offMessage }
}
