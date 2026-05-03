import { ref, computed, onMounted } from 'vue'
import { usePushService } from '@/serviceRegistry'

function urlBase64ToUint8Array(base64String: string): Uint8Array {
  const padding = '='.repeat((4 - (base64String.length % 4)) % 4)
  const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/')
  const rawData = atob(base64)
  const output = new Uint8Array(rawData.length)
  for (let i = 0; i < rawData.length; ++i) output[i] = rawData.charCodeAt(i)
  return output
}

function arrayBufferToBase64Url(buffer: ArrayBuffer): string {
  let binary = ''
  const bytes = new Uint8Array(buffer)
  for (let i = 0; i < bytes.length; i++) binary += String.fromCharCode(bytes[i])
  return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '')
}

export function usePushSubscription() {
  const pushService = usePushService()

  const permission = ref<NotificationPermission>('default')
  const subscription = ref<PushSubscription | null>(null)
  const isSubscribing = ref(false)

  const isPwaInstalled = computed(() => {
    if (typeof window === 'undefined') return false
    const standalone = window.matchMedia('(display-mode: standalone)').matches
    const iosStandalone = (navigator as unknown as { standalone?: boolean }).standalone === true
    return standalone || iosStandalone
  })

  const isSupported = computed(() => {
    return typeof window !== 'undefined'
      && 'serviceWorker' in navigator
      && 'PushManager' in window
      && 'Notification' in window
  })

  const isSubscribed = computed(() => subscription.value !== null && permission.value === 'granted')

  async function refreshState(): Promise<void> {
    if (!isSupported.value) return
    permission.value = Notification.permission
    try {
      const reg = await navigator.serviceWorker.ready
      subscription.value = await reg.pushManager.getSubscription()
    } catch {
      subscription.value = null
    }
  }

  async function subscribe(): Promise<boolean> {
    if (!isSupported.value || !isPwaInstalled.value) return false
    isSubscribing.value = true
    try {
      const perm = await Notification.requestPermission()
      permission.value = perm
      if (perm !== 'granted') return false

      const reg = await Promise.race([
        navigator.serviceWorker.ready,
        new Promise<never>((_, reject) =>
          setTimeout(() => reject(new Error('Service worker non disponible (timeout 8s). L\'app doit être installée sur l\'écran d\'accueil.')), 8000)
        )
      ])

      const publicKey = await pushService.getVapidPublicKey()
      const sub = await reg.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: urlBase64ToUint8Array(publicKey)
      })

      const p256dhBuf = sub.getKey('p256dh')
      const authBuf = sub.getKey('auth')
      if (!p256dhBuf || !authBuf) return false

      await pushService.createSubscription({
        endpoint: sub.endpoint,
        p256dh: arrayBufferToBase64Url(p256dhBuf),
        auth: arrayBufferToBase64Url(authBuf)
      })

      subscription.value = sub
      return true
    } finally {
      isSubscribing.value = false
    }
  }

  async function unsubscribe(): Promise<void> {
    if (subscription.value) {
      const endpoint = subscription.value.endpoint
      try { await subscription.value.unsubscribe() } catch { /* ignore */ }
      try { await pushService.deleteSubscription(endpoint) } catch { /* ignore */ }
      subscription.value = null
    }
  }

  onMounted(refreshState)

  return {
    permission,
    subscription,
    isSubscribing,
    isPwaInstalled,
    isSupported,
    isSubscribed,
    subscribe,
    unsubscribe,
    refreshState
  }
}
