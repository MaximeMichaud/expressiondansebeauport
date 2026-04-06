import {ref, onUnmounted, type Ref} from 'vue'
import type {Page} from '@/types/entities'
import type {IPageService} from '@/injection/interfaces'

interface AutosaveOptions {
  page: Ref<Page>
  pageService: IPageService
  localIntervalMs?: number
  serverIntervalMs?: number
}

export function useAutosave(options: AutosaveOptions) {
  const {page, pageService, localIntervalMs = 15000, serverIntervalMs = 60000} = options

  const lastSavedAt = ref<string | null>(null)
  const lastSavedAgo = ref('')
  const isSaving = ref(false)
  const hasLocalDraft = ref(false)

  let localTimer: ReturnType<typeof setInterval> | null = null
  let serverTimer: ReturnType<typeof setInterval> | null = null
  let agoTimer: ReturnType<typeof setInterval> | null = null
  let lastLocalSnapshot = ''
  let lastServerSnapshot = ''

  function getStorageKey() {
    return page.value.id ? `page-draft-${page.value.id}` : null
  }

  function getSnapshot(): string {
    const p = page.value
    return JSON.stringify({
      title: p.title,
      content: p.content,
      customCss: p.customCss,
      contentMode: p.contentMode,
      blocks: p.blocks,
      metaDescription: p.metaDescription
    })
  }

  function saveLocal() {
    const key = getStorageKey()
    if (!key) return

    const snapshot = getSnapshot()
    if (snapshot === lastLocalSnapshot) return

    sessionStorage.setItem(key, snapshot)
    lastLocalSnapshot = snapshot
  }

  async function saveServer() {
    const id = page.value.id
    if (!id || isSaving.value) return

    const snapshot = getSnapshot()
    if (snapshot === lastServerSnapshot) return

    isSaving.value = true
    try {
      const result = await pageService.autosave(id, page.value)
      if (result) {
        lastSavedAt.value = new Date().toISOString()
        lastServerSnapshot = snapshot
        updateAgo()
        // Nettoyer le brouillon local après sauvegarde serveur réussie
        const key = getStorageKey()
        if (key) sessionStorage.removeItem(key)
      }
    } finally {
      isSaving.value = false
    }
  }

  function updateAgo() {
    if (!lastSavedAt.value) {
      lastSavedAgo.value = ''
      return
    }
    const seconds = Math.floor((Date.now() - new Date(lastSavedAt.value).getTime()) / 1000)
    if (seconds < 5) lastSavedAgo.value = "à l'instant"
    else if (seconds < 60) lastSavedAgo.value = `il y a ${seconds}s`
    else lastSavedAgo.value = `il y a ${Math.floor(seconds / 60)}min`
  }

  function checkLocalDraft(): Record<string, any> | null {
    const key = getStorageKey()
    if (!key) return null

    const stored = sessionStorage.getItem(key)
    if (!stored) return null

    try {
      const draft = JSON.parse(stored)
      const current = getSnapshot()
      if (stored !== current) {
        hasLocalDraft.value = true
        return draft
      }
    } catch { /* ignore */ }
    return null
  }

  function restoreLocalDraft(draft: Record<string, any>) {
    page.value.title = draft.title ?? page.value.title
    page.value.content = draft.content ?? page.value.content
    page.value.customCss = draft.customCss ?? page.value.customCss
    page.value.contentMode = draft.contentMode ?? page.value.contentMode
    page.value.blocks = draft.blocks ?? page.value.blocks
    page.value.metaDescription = draft.metaDescription ?? page.value.metaDescription
    hasLocalDraft.value = false
    dismissLocalDraft()
  }

  function dismissLocalDraft() {
    hasLocalDraft.value = false
    const key = getStorageKey()
    if (key) sessionStorage.removeItem(key)
  }

  function start() {
    const snapshot = getSnapshot()
    lastLocalSnapshot = snapshot
    lastServerSnapshot = snapshot
    localTimer = setInterval(saveLocal, localIntervalMs)
    serverTimer = setInterval(saveServer, serverIntervalMs)
    agoTimer = setInterval(updateAgo, 5000)

    window.addEventListener('beforeunload', onBeforeUnload)
  }

  function stop() {
    if (localTimer) clearInterval(localTimer)
    if (serverTimer) clearInterval(serverTimer)
    if (agoTimer) clearInterval(agoTimer)
    window.removeEventListener('beforeunload', onBeforeUnload)
  }

  function onBeforeUnload() {
    saveLocal()
    // Tentative de sauvegarde serveur avec fetch keepalive (supporte les headers, contrairement à sendBeacon)
    const id = page.value.id
    if (id && getSnapshot() !== lastServerSnapshot) {
      const url = `${import.meta.env.VITE_API_BASE_URL}/admin/pages/${id}/autosave`
      const token = document.cookie.split('accessToken=')[1]?.split(';')[0]
      if (token) {
        fetch(url, {
          method: 'POST',
          headers: {'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token},
          body: JSON.stringify(page.value),
          keepalive: true
        }).catch(() => {})
      }
    }
  }

  function onManualSave() {
    const key = getStorageKey()
    if (key) sessionStorage.removeItem(key)
    const snapshot = getSnapshot()
    lastLocalSnapshot = snapshot
    lastServerSnapshot = snapshot
    lastSavedAt.value = new Date().toISOString()
    updateAgo()
  }

  onUnmounted(stop)

  return {
    lastSavedAgo,
    isSaving,
    hasLocalDraft,
    start,
    stop,
    checkLocalDraft,
    restoreLocalDraft,
    dismissLocalDraft,
    onManualSave
  }
}
