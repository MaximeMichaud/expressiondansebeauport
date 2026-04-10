<template>
  <Teleport to="body">
    <Transition name="lightbox">
      <div v-if="open" class="lightbox" @click.self="close">
        <button class="lightbox__close" @click="close" aria-label="Fermer">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
          </svg>
        </button>
        <video
          v-if="isVideo"
          :src="displayUrl"
          controls
          autoplay
          playsinline
          class="lightbox__img"
        />
        <img v-else :src="displayUrl" :alt="alt || ''" class="lightbox__img" />
        <button
          v-if="originalUrl"
          type="button"
          class="lightbox__download"
          @click.stop="handleSave"
        >
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/>
            <polyline points="7 10 12 15 17 10"/>
            <line x1="12" y1="15" x2="12" y2="3"/>
          </svg>
          <span>{{ saveLabel }}</span>
        </button>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, watch } from 'vue'

interface Props {
  open: boolean
  displayUrl: string
  originalUrl?: string
  contentType?: string
  alt?: string
}

const props = defineProps<Props>()
const emit = defineEmits<{ (e: 'update:open', value: boolean): void }>()

const isMobile = typeof navigator !== 'undefined' && /Mobi|Android|iPhone|iPad/i.test(navigator.userAgent)
const isVideo = computed(() => !!props.contentType && props.contentType.startsWith('video/'))
const saveLabel = computed(() => {
  if (isVideo.value) return isMobile ? 'Enregistrer la vidéo' : 'Télécharger'
  return isMobile ? 'Enregistrer la photo' : 'Télécharger'
})

function close() {
  emit('update:open', false)
}

function onKey(e: KeyboardEvent) {
  if (e.key === 'Escape' && props.open) close()
}

function filenameFromUrl(url: string): string {
  try {
    const u = new URL(url, window.location.href)
    const parts = u.pathname.split('/')
    return parts[parts.length - 1] || (isVideo.value ? 'video' : 'photo')
  } catch {
    return isVideo.value ? 'video' : 'photo'
  }
}

async function handleSave() {
  if (!props.originalUrl) return

  // Web Share API with files: on iOS/Android this opens the native share sheet,
  // which includes "Save to Photos" (iOS) / "Save image" (Android).
  if (typeof navigator !== 'undefined' && 'share' in navigator && typeof navigator.canShare === 'function') {
    try {
      const response = await fetch(props.originalUrl)
      if (response.ok) {
        const blob = await response.blob()
        const filename = filenameFromUrl(props.originalUrl)
        const fallbackType = isVideo.value ? 'video/mp4' : 'image/jpeg'
        const file = new File([blob], filename, { type: blob.type || fallbackType })
        if (navigator.canShare({ files: [file] })) {
          await navigator.share({ files: [file] })
          return
        }
      }
    } catch {
      // User cancelled the share sheet, or fetch failed — fall through to link fallback.
    }
  }

  // Fallback: trigger a regular download via an anchor. Works on desktop browsers
  // and on Android Chrome; on iOS Safari this opens the image in a new tab so
  // the user can long-press → Save to Photos.
  const a = document.createElement('a')
  a.href = props.originalUrl
  a.download = filenameFromUrl(props.originalUrl)
  a.target = '_blank'
  a.rel = 'noopener'
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
}

onMounted(() => window.addEventListener('keydown', onKey))
onUnmounted(() => {
  window.removeEventListener('keydown', onKey)
  if (typeof document !== 'undefined') {
    document.body.style.overflow = ''
  }
})

watch(() => props.open, (v) => {
  if (typeof document !== 'undefined') {
    document.body.style.overflow = v ? 'hidden' : ''
  }
})
</script>

<style scoped>
.lightbox {
  position: fixed;
  inset: 0;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  background: rgba(0, 0, 0, 0.92);
  padding: 24px;
}

.lightbox__close {
  position: absolute;
  top: 16px;
  right: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.1);
  color: white;
  cursor: pointer;
  transition: background 0.15s;
}
.lightbox__close:hover { background: rgba(255, 255, 255, 0.2); }

.lightbox__img {
  max-width: 90vw;
  max-height: 80vh;
  object-fit: contain;
  border-radius: 8px;
}

.lightbox__download {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  border-radius: 8px;
  background: white;
  color: #1a1a1a;
  font-weight: 600;
  font-size: 0.85rem;
  text-decoration: none;
  cursor: pointer;
  transition: background 0.15s;
}
.lightbox__download:hover { background: #f0f0f0; }

.lightbox-enter-active,
.lightbox-leave-active { transition: opacity 0.2s ease; }
.lightbox-enter-from,
.lightbox-leave-to { opacity: 0; }
</style>
