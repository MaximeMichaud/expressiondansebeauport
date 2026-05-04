/// <reference lib="webworker" />
import { precacheAndRoute } from 'workbox-precaching'

declare const self: ServiceWorkerGlobalScope

precacheAndRoute(self.__WB_MANIFEST)

self.addEventListener('install', () => {
  self.skipWaiting()
})

self.addEventListener('activate', (event) => {
  event.waitUntil(self.clients.claim())
})

interface PushPayload {
  title: string
  body: string
  url?: string
  tag?: string
}

self.addEventListener('push', (event) => {
  if (!event.data) return

  let payload: PushPayload
  try {
    payload = event.data.json() as PushPayload
  } catch {
    payload = { title: 'Expression Danse Beauport', body: event.data.text() }
  }

  event.waitUntil((async () => {
    const targetUrl = payload.url ?? '/social'
    const allClients = await self.clients.matchAll({ type: 'window', includeUncontrolled: true })
    const alreadyOpenAndFocused = allClients.some((c) => {
      const wc = c as WindowClient
      return wc.visibilityState === 'visible' && wc.focused && wc.url.includes(targetUrl)
    })
    if (alreadyOpenAndFocused) return

    await self.registration.showNotification(payload.title, {
      body: payload.body,
      icon: '/icons/192.png',
      badge: '/icons/badge.png',
      tag: payload.tag,
      data: { url: targetUrl }
    })
  })())
})

self.addEventListener('notificationclick', (event) => {
  event.notification.close()
  const url = (event.notification.data?.url as string) ?? '/social'

  event.waitUntil((async () => {
    const allClients = await self.clients.matchAll({ type: 'window', includeUncontrolled: true })
    const existing = allClients.find((c) => (c as WindowClient).url.includes(url))
    if (existing) {
      await (existing as WindowClient).focus()
    } else {
      await self.clients.openWindow(url)
    }
  })())
})

self.addEventListener('pushsubscriptionchange', (event: ExtendableEvent) => {
  // Browsers can silently rotate the push endpoint (rare, but happens).
  // Re-subscribe and POST the new subscription to the backend.
  event.waitUntil((async () => {
    try {
      const reg = self.registration
      // Need the same applicationServerKey we originally used. The browser doesn't expose it here,
      // so we fetch it from our backend.
      const keyRes = await fetch('/api/social/push/vapid-public-key', { credentials: 'include' })
      if (!keyRes.ok) return
      const { publicKey } = await keyRes.json() as { publicKey: string }

      const padding = '='.repeat((4 - publicKey.length % 4) % 4)
      const base64 = (publicKey + padding).replace(/-/g, '+').replace(/_/g, '/')
      const rawData = atob(base64)
      const appServerKey = new Uint8Array(rawData.length)
      for (let i = 0; i < rawData.length; i++) appServerKey[i] = rawData.charCodeAt(i)

      const newSub = await reg.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: appServerKey
      })

      const p256 = newSub.getKey('p256dh')
      const auth = newSub.getKey('auth')
      if (!p256 || !auth) return

      const toBase64Url = (buf: ArrayBuffer) => {
        let s = ''
        const bytes = new Uint8Array(buf)
        for (let i = 0; i < bytes.length; i++) s += String.fromCharCode(bytes[i])
        return btoa(s).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '')
      }

      await fetch('/api/social/push/subscriptions', {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          endpoint: newSub.endpoint,
          p256dh: toBase64Url(p256),
          auth: toBase64Url(auth)
        })
      })
    } catch {
      // Silent fail — user will need to re-subscribe manually if this fails.
    }
  })())
})
