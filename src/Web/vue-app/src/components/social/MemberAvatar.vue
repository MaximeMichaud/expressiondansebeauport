<template>
  <div
    class="member-avatar flex flex-shrink-0 items-center justify-center overflow-hidden rounded-full font-bold text-white select-none"
    :style="{
      width: size + 'px',
      height: size + 'px',
      background: color || '#1a1a1a',
      fontSize: Math.max(8, Math.round(size / 3)) + 'px'
    }"
  >
    <img
      v-if="effectiveUrl"
      :src="effectiveUrl"
      :alt="name || ''"
      class="h-full w-full object-cover"
    />
    <span v-else>{{ initials }}</span>
  </div>
</template>

<script lang="ts" setup>
import { computed } from 'vue'
import { useAvatarRegistryStore } from '@/stores/avatarRegistryStore'

const props = withDefaults(defineProps<{
  memberId?: string | null
  imageUrl?: string | null
  name?: string | null
  color?: string | null
  size?: number
}>(), {
  size: 36,
  memberId: null,
  imageUrl: null,
  name: null,
  color: null
})

const avatarRegistry = useAvatarRegistryStore()

const effectiveUrl = computed(() => {
  if (props.memberId) {
    const fromRegistry = avatarRegistry.getAvatar(props.memberId, props.imageUrl)
    return fromRegistry || null
  }
  return props.imageUrl || null
})

const initials = computed(() => {
  const n = props.name?.trim()
  if (!n) return '?'
  return n.split(' ').filter(s => s.length > 0).map(s => s[0]).join('').toUpperCase().slice(0, 2)
})
</script>
