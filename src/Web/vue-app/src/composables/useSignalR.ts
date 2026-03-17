import * as signalR from '@microsoft/signalr'
import { ref, onUnmounted } from 'vue'

const connection = ref<signalR.HubConnection | null>(null)
const isConnected = ref(false)

export function useSignalR() {
  function getAccessToken(): string {
    const cookies = document.cookie.split('; ')
    const accessCookie = cookies.find(c => c.startsWith('access='))
    return accessCookie?.split('=')[1] ?? ''
  }

  async function start() {
    if (connection.value?.state === signalR.HubConnectionState.Connected) return

    connection.value = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/chat', {
        accessTokenFactory: () => getAccessToken()
      })
      .withAutomaticReconnect()
      .build()

    connection.value.onreconnected(() => { isConnected.value = true })
    connection.value.onclose(() => { isConnected.value = false })

    try {
      await connection.value.start()
      isConnected.value = true
    } catch (err) {
      console.error('SignalR connection failed:', err)
      isConnected.value = false
    }
  }

  function onMessage(callback: (data: any) => void) {
    connection.value?.on('ReceiveMessage', callback)
  }

  function offMessage(callback: (data: any) => void) {
    connection.value?.off('ReceiveMessage', callback)
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
