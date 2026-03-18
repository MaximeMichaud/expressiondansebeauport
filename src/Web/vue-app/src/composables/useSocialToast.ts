import { ref, readonly } from 'vue'

interface Toast {
  id: number
  message: string
  type: 'success' | 'error'
}

const toasts = ref<Toast[]>([])
let nextId = 0

function show(message: string, type: 'success' | 'error', duration = 4000) {
  const id = nextId++
  toasts.value.push({ id, message, type })
  setTimeout(() => dismiss(id), duration)
}

function dismiss(id: number) {
  toasts.value = toasts.value.filter(t => t.id !== id)
}

export function useSocialToast() {
  return {
    toasts: readonly(toasts),
    success: (msg: string) => show(msg, 'success'),
    error: (msg: string) => show(msg, 'error'),
    dismiss,
  }
}
