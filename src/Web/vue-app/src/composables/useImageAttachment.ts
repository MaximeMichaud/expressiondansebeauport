import { ref, onUnmounted, type Ref } from 'vue'

export interface UseImageAttachmentOptions {
  mode: 'single' | 'multi'
  maxFiles?: number
  maxSizeBytes?: number
  allowedTypes?: string[]
}

export interface AttachmentPreview {
  file: File
  url: string
}

export interface UseImageAttachmentReturn {
  files: Ref<File[]>
  previews: Ref<AttachmentPreview[]>
  error: Ref<string | null>
  isDraggingOver: Ref<boolean>
  handleFileInput: (e: Event) => void
  handleDrop: (e: DragEvent) => void
  handleDragEnter: (e: DragEvent) => void
  handleDragLeave: (e: DragEvent) => void
  handleDragOver: (e: DragEvent) => void
  removeFile: (index: number) => void
  clear: () => void
}

const DEFAULT_ALLOWED = ['image/jpeg', 'image/png', 'image/webp', 'image/gif']
const DEFAULT_MAX_SIZE = 10 * 1024 * 1024

export function useImageAttachment(options: UseImageAttachmentOptions): UseImageAttachmentReturn {
  const maxFiles = options.maxFiles ?? (options.mode === 'single' ? 1 : 10)
  const maxSize = options.maxSizeBytes ?? DEFAULT_MAX_SIZE
  const allowed = options.allowedTypes ?? DEFAULT_ALLOWED

  const files = ref<File[]>([])
  const previews = ref<AttachmentPreview[]>([])
  const error = ref<string | null>(null)
  const isDraggingOver = ref(false)
  let dragCounter = 0

  function addFiles(incoming: File[]) {
    error.value = null
    for (const file of incoming) {
      if (files.value.length >= maxFiles) {
        error.value = `Maximum ${maxFiles} image${maxFiles > 1 ? 's' : ''}.`
        break
      }
      if (!allowed.includes(file.type)) {
        error.value = `Format non supporté : ${file.name}`
        continue
      }
      if (file.size > maxSize) {
        error.value = `Image trop volumineuse (max ${Math.round(maxSize / 1024 / 1024)} MB) : ${file.name}`
        continue
      }
      const url = URL.createObjectURL(file)
      files.value.push(file)
      previews.value.push({ file, url })
    }
  }

  function handleFileInput(e: Event) {
    const target = e.target as HTMLInputElement
    if (!target.files) return
    addFiles(Array.from(target.files))
    target.value = ''
  }

  function handleDrop(e: DragEvent) {
    e.preventDefault()
    e.stopPropagation()
    dragCounter = 0
    isDraggingOver.value = false
    if (!e.dataTransfer?.files) return
    addFiles(Array.from(e.dataTransfer.files))
  }

  function handleDragEnter(e: DragEvent) {
    e.preventDefault()
    e.stopPropagation()
    dragCounter++
    isDraggingOver.value = true
  }

  function handleDragLeave(e: DragEvent) {
    e.preventDefault()
    e.stopPropagation()
    dragCounter--
    if (dragCounter <= 0) {
      dragCounter = 0
      isDraggingOver.value = false
    }
  }

  function handleDragOver(e: DragEvent) {
    e.preventDefault()
    e.stopPropagation()
  }

  function removeFile(index: number) {
    const removed = previews.value[index]
    if (removed) URL.revokeObjectURL(removed.url)
    files.value.splice(index, 1)
    previews.value.splice(index, 1)
    error.value = null
  }

  function clear() {
    for (const p of previews.value) URL.revokeObjectURL(p.url)
    files.value = []
    previews.value = []
    error.value = null
    dragCounter = 0
    isDraggingOver.value = false
  }

  onUnmounted(() => {
    for (const p of previews.value) URL.revokeObjectURL(p.url)
  })

  return {
    files,
    previews,
    error,
    isDraggingOver,
    handleFileInput,
    handleDrop,
    handleDragEnter,
    handleDragLeave,
    handleDragOver,
    removeFile,
    clear
  }
}
