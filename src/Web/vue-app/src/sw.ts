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
    const allClients = await self.clients.matchAll({ type: 'window', includeUncontrolled: true })
    const visible = allClients.some((c) => {
      const wc = c as WindowClient
      return wc.visibilityState === 'visible' && wc.focused
    })
    if (visible) return

    await self.registration.showNotification(payload.title, {
      body: payload.body,
      icon: '/icons/192.png',
      badge: '/icons/badge.png',
      tag: payload.tag,
      data: { url: payload.url ?? '/social' }
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
