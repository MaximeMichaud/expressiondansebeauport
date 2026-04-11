<template>
  <div ref="rootRef" class="relative inline-block" :style="{ width: size + 'px', height: size + 'px' }">
    <div
      class="flex h-full w-full items-center justify-center overflow-hidden rounded-full font-bold text-white"
      :style="{ background: fallbackColor, fontSize: Math.max(10, Math.round(size / 3)) + 'px' }"
    >
      <img
        v-if="imageUrl"
        :src="imageUrl"
        :alt="fallbackInitials"
        class="h-full w-full object-cover"
      />
      <span v-else>{{ fallbackInitials }}</span>
    </div>

    <button
      v-if="canEdit"
      type="button"
      class="avatar-uploader__btn absolute bottom-0 right-0 flex h-7 w-7 items-center justify-center rounded-full shadow-md cursor-pointer"
      :disabled="uploading"
      title="Modifier la photo"
      @click.stop="toggleMenu"
    >
      <svg v-if="!uploading" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M23 19a2 2 0 01-2 2H3a2 2 0 01-2-2V8a2 2 0 012-2h4l2-3h6l2 3h4a2 2 0 012 2z"/>
        <circle cx="12" cy="13" r="4"/>
      </svg>
      <svg v-else class="animate-spin" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round">
        <path d="M21 12a9 9 0 11-6.219-8.56"/>
      </svg>
    </button>

    <div
      v-if="menuOpen"
      class="avatar-uploader__menu absolute right-0 z-50 mt-1 min-w-[180px] rounded-lg border py-1 shadow-xl"
      :style="{
        top: 'calc(100% + 4px)',
        background: 'var(--soc-content-bg)',
        borderColor: 'var(--soc-card-border)'
      }"
    >
      <button
        type="button"
        class="avatar-uploader__menu-item flex w-full items-center gap-2 px-3 py-2 text-left text-sm"
        @click.stop="triggerPicker"
      >
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/>
          <polyline points="17 8 12 3 7 8"/>
          <line x1="12" y1="3" x2="12" y2="15"/>
        </svg>
        Changer la photo
      </button>
      <button
        v-if="imageUrl"
        type="button"
        class="avatar-uploader__menu-item avatar-uploader__menu-item--danger flex w-full items-center gap-2 px-3 py-2 text-left text-sm"
        @click.stop="onRemoveClicked"
      >
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="3 6 5 6 21 6"/>
          <path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/>
        </svg>
        Retirer la photo
      </button>
    </div>

    <input
      ref="fileInputRef"
      type="file"
      accept="image/*"
      hidden
      @change="onFileChange"
    />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, onUnmounted } from 'vue'

withDefaults(defineProps<{
  imageUrl?: string | null
  fallbackInitials: string
  fallbackColor: string
  size?: number
  canEdit: boolean
  uploading?: boolean
}>(), {
  size: 80,
  imageUrl: null,
  uploading: false
})

const emit = defineEmits<{
  (e: 'upload', file: File): void
  (e: 'remove'): void
}>()

const menuOpen = ref(false)
const fileInputRef = ref<HTMLInputElement | null>(null)
const rootRef = ref<HTMLElement | null>(null)

function toggleMenu() {
  menuOpen.value = !menuOpen.value
}

function triggerPicker() {
  menuOpen.value = false
  fileInputRef.value?.click()
}

function onFileChange(e: Event) {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) emit('upload', file)
  if (input) input.value = ''
}

function onRemoveClicked() {
  menuOpen.value = false
  emit('remove')
}

function handleDocumentClick(e: MouseEvent) {
  if (!menuOpen.value) return
  const target = e.target as Node
  if (rootRef.value && !rootRef.value.contains(target)) {
    menuOpen.value = false
  }
}

onMounted(() => document.addEventListener('click', handleDocumentClick))
onUnmounted(() => document.removeEventListener('click', handleDocumentClick))
</script>

<style scoped>
.avatar-uploader__btn {
  background: var(--soc-text);
  color: var(--soc-content-bg);
  border: 2px solid var(--soc-content-bg);
}
.avatar-uploader__btn:hover:not(:disabled) {
  opacity: 0.9;
}
.avatar-uploader__btn:disabled {
  opacity: 0.5;
  cursor: default;
}

.avatar-uploader__menu-item {
  color: var(--soc-text);
  transition: background 0.15s;
}
.avatar-uploader__menu-item:hover {
  background: var(--soc-bar-hover);
}
.avatar-uploader__menu-item--danger {
  color: #dc2626;
}
</style>
