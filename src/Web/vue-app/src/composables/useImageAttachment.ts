import { ref, onUnmounted, type Ref } from 'vue'

export interface UseImageAttachmentOptions {
  mode: 'single' | 'multi'
  maxFiles?: number
  maxSizeBytes?: number
  allowedTypes?: string[]
  maxVideoDurationSeconds?: number
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

const DEFAULT_ALLOWED = [
  'image/jpeg', 'image/png', 'image/webp', 'image/gif',
  'video/mp4', 'video/quicktime', 'video/webm'
]
const IMAGE_MAX_SIZE = 10 * 1024 * 1024
const VIDEO_MAX_SIZE = 2 * 1024 * 1024 * 1024
const DEFAULT_VIDEO_MAX_DURATION_SECONDS = 600

function isVideoType(type: string): boolean {
  return type.startsWith('video/')
}

function getVideoDuration(file: File): Promise<number> {
  return new Promise((resolve, reject) => {
    const url = URL.createObjectURL(file)
    const video = document.createElement('video')
    video.preload = 'metadata'
    video.muted = true
    video.onloadedmetadata = () => {
      const duration = video.duration
      URL.revokeObjectURL(url)
      if (!isFinite(duration)) reject(new Error('unreadable duration'))
      else resolve(duration)
    }
    video.onerror = () => {
      URL.revokeObjectURL(url)
      reject(new Error('failed to read metadata'))
    }
    video.src = url
  })
}

export function useImageAttachment(options: UseImageAttachmentOptions): UseImageAttachmentReturn {
  const maxFiles = options.maxFiles ?? (options.mode === 'single' ? 1 : 10)
  const explicitMaxSize = options.maxSizeBytes
  const allowed = options.allowedTypes ?? DEFAULT_ALLOWED
  const maxVideoDuration = options.maxVideoDurationSeconds ?? DEFAULT_VIDEO_MAX_DURATION_SECONDS

  const files = ref<File[]>([])
  const previews = ref<AttachmentPreview[]>([])
  const error = ref<string | null>(null)
  const isDraggingOver = ref(false)
  let dragCounter = 0

  async function addFiles(incoming: File[]) {
    error.value = null
    for (const file of incoming) {
      if (files.value.length >= maxFiles) {
        error.value = `Maximum ${maxFiles} fichier${maxFiles > 1 ? 's' : ''} atteint.`
        break
      }
      if (!allowed.includes(file.type)) {
        error.value = `Format non supporté : ${file.name}`
        continue
      }
      const isVideo = isVideoType(file.type)
      const maxSize = explicitMaxSize ?? (isVideo ? VIDEO_MAX_SIZE : IMAGE_MAX_SIZE)
      if (file.size > maxSize) {
        const label = isVideo ? 'Vidéo' : 'Image'
        const sizeLabel = maxSize >= 1024 * 1024 * 1024
          ? `${(maxSize / 1024 / 1024 / 1024).toFixed(1)} Go`
          : `${Math.round(maxSize / 1024 / 1024)} Mo`
        error.value = `${label} trop volumineuse (max ${sizeLabel}) : ${file.name}`
        continue
      }
      if (isVideo) {
        try {
          const duration = await getVideoDuration(file)
          if (duration > maxVideoDuration) {
            const minutes = Math.floor(maxVideoDuration / 60)
            error.value = `Vidéo trop longue (max ${minutes} min) : ${file.name}`
            continue
          }
        } catch {
          error.value = `Impossible de lire la vidéo : ${file.name}`
          continue
        }
      }
      const url = URL.createObjectURL(file)
      files.value.push(file)
      previews.value.push({ file, url })
    }
  }

  function handleFileInput(e: Event) {
    const target = e.target as HTMLInputElement
    if (!target.files) return
    void addFiles(Array.from(target.files))
    target.value = ''
  }

  function handleDrop(e: DragEvent) {
    e.preventDefault()
    e.stopPropagation()
    dragCounter = 0
    isDraggingOver.value = false
    if (!e.dataTransfer?.files) return
    void addFiles(Array.from(e.dataTransfer.files))
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
