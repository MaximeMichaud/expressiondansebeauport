<template>
  <Teleport to="body">
    <Transition name="lightbox">
      <div v-if="open" class="lightbox" @click.self="close">
        <button class="lightbox__close" @click="close" aria-label="Fermer">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
          </svg>
        </button>
        <img :src="displayUrl" :alt="alt || ''" class="lightbox__img" />
        <a
          v-if="originalUrl"
          :href="originalUrl"
          download
          class="lightbox__download"
          @click.stop
        >Télécharger l'original</a>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, watch } from 'vue'

interface Props {
  open: boolean
  displayUrl: string
  originalUrl?: string
  alt?: string
}

const props = defineProps<Props>()
const emit = defineEmits<{ (e: 'update:open', value: boolean): void }>()

function close() {
  emit('update:open', false)
}

function onKey(e: KeyboardEvent) {
  if (e.key === 'Escape' && props.open) close()
}

onMounted(() => window.addEventListener('keydown', onKey))
onUnmounted(() => window.removeEventListener('keydown', onKey))

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
  padding: 10px 20px;
  border-radius: 8px;
  background: white;
  color: #1a1a1a;
  font-weight: 600;
  font-size: 0.85rem;
  text-decoration: none;
  transition: background 0.15s;
}
.lightbox__download:hover { background: #f0f0f0; }

.lightbox-enter-active,
.lightbox-leave-active { transition: opacity 0.2s ease; }
.lightbox-enter-from,
.lightbox-leave-to { opacity: 0; }
</style>
