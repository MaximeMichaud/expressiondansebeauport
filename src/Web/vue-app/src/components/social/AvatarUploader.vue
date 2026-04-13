<template>
  <div
    class="relative inline-block group"
    :style="{ width: size + 'px', height: size + 'px' }"
  >
    <div
      class="flex h-full w-full items-center justify-center overflow-hidden font-bold select-none"
      :class="shape === 'square' ? 'rounded-lg' : 'rounded-full'"
      :style="{ background: fallbackColor, color: fallbackTextColor, fontSize: Math.max(10, Math.round(size / 3)) + 'px' }"
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
      :class="[
        'absolute inset-0 flex items-center justify-center transition cursor-pointer focus:outline-none',
        shape === 'square' ? 'rounded-lg' : 'rounded-full',
        uploading
          ? 'bg-black/50 opacity-100'
          : 'opacity-0 group-hover:opacity-100 group-hover:bg-black/45'
      ]"
      :disabled="uploading"
      title="Modifier la photo"
      @click.stop="triggerPicker"
    >
      <svg v-if="!uploading" :width="iconSize" :height="iconSize" viewBox="0 0 24 24" fill="none" stroke="#ffffff" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M12 20h9"/>
        <path d="M16.5 3.5a2.121 2.121 0 013 3L7 19l-4 1 1-4L16.5 3.5z"/>
      </svg>
      <svg v-else class="animate-spin" :width="iconSize" :height="iconSize" viewBox="0 0 24 24" fill="none" stroke="#ffffff" stroke-width="3" stroke-linecap="round">
        <path d="M21 12a9 9 0 11-6.219-8.56"/>
      </svg>
    </button>

    <button
      v-if="canEdit && imageUrl && !uploading"
      type="button"
      class="avatar-uploader__remove absolute top-0 right-0 flex items-center justify-center rounded-full opacity-0 transition cursor-pointer group-hover:opacity-100 focus:outline-none"
      :style="{
        width: removeBtnSize + 'px',
        height: removeBtnSize + 'px',
        transform: shape === 'square' ? 'translate(35%, -35%)' : 'none'
      }"
      title="Retirer la photo"
      @click.stop="emit('remove')"
    >
      <svg :width="Math.round(removeBtnSize * 0.55)" :height="Math.round(removeBtnSize * 0.55)" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">
        <line x1="18" y1="6" x2="6" y2="18"/>
        <line x1="6" y1="6" x2="18" y2="18"/>
      </svg>
    </button>

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
import { ref, computed } from 'vue'

const props = withDefaults(defineProps<{
  imageUrl?: string | null
  fallbackInitials: string
  fallbackColor: string
  fallbackTextColor?: string
  size?: number
  canEdit: boolean
  uploading?: boolean
  shape?: 'circle' | 'square'
}>(), {
  size: 80,
  imageUrl: null,
  uploading: false,
  shape: 'circle',
  fallbackTextColor: 'white'
})

const emit = defineEmits<{
  (e: 'upload', file: File): void
  (e: 'remove'): void
}>()

const fileInputRef = ref<HTMLInputElement | null>(null)

const iconSize = computed(() => Math.max(14, Math.round(props.size * 0.35)))
const removeBtnSize = computed(() => Math.max(16, Math.round(props.size * 0.3)))

function triggerPicker() {
  if (props.uploading) return
  fileInputRef.value?.click()
}

function onFileChange(e: Event) {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) emit('upload', file)
  if (input) input.value = ''
}
</script>

<style scoped>
.avatar-uploader__remove {
  background: #dc2626;
  color: white;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.3);
}
.avatar-uploader__remove:hover {
  background: #b91c1c;
}
</style>
