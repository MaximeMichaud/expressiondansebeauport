import { ref, readonly, onMounted, onUnmounted } from "vue"
import axios from "axios"

const POLL_INTERVAL_MS = 60_000
const ENDPOINT = `${import.meta.env.VITE_API_BASE_URL}/public/version`

interface VersionPayload {
  version: string
  builtAt: number
}

const updateAvailable = ref(false)
const initialVersion = ref<string | null>(null)
const initialBuiltAt = ref<number | null>(null)
let mountedCount = 0
let timer: ReturnType<typeof setInterval> | undefined

async function fetchVersion(): Promise<VersionPayload | null> {
  try {
    const response = await axios.get<VersionPayload>(ENDPOINT, {
      headers: { "Cache-Control": "no-store" },
    })
    return response.data
  } catch {
    return null
  }
}

async function check() {
  const payload = await fetchVersion()
  if (!payload) return

  if (initialVersion.value === null) {
    initialVersion.value = payload.version
    initialBuiltAt.value = payload.builtAt
    return
  }

  if (payload.version !== initialVersion.value || payload.builtAt !== initialBuiltAt.value) {
    updateAvailable.value = true
  }
}

export function useVersionCheck() {
  onMounted(() => {
    mountedCount++
    if (mountedCount === 1) {
      void check()
      timer = setInterval(() => void check(), POLL_INTERVAL_MS)
    }
  })

  onUnmounted(() => {
    mountedCount--
    if (mountedCount === 0 && timer) {
      clearInterval(timer)
      timer = undefined
    }
  })

  return {
    updateAvailable: readonly(updateAvailable),
    reload: () => window.location.reload(),
  }
}
