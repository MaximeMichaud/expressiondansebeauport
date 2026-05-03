<template>
  <label class="flex items-center justify-between cursor-pointer" :class="size === 'sm' ? 'text-sm' : ''">
    <span :style="{ color: 'var(--soc-text)' }">{{ label }}</span>
    <span class="relative inline-block" :class="size === 'sm' ? 'w-9 h-5' : 'w-11 h-6'">
      <input type="checkbox" :checked="modelValue" class="peer sr-only" @change="onChange" />
      <span class="absolute inset-0 rounded-full transition-colors" :class="modelValue ? 'bg-black' : 'bg-gray-300'"></span>
      <span
        class="absolute top-0.5 left-0.5 bg-white rounded-full transition-transform"
        :class="[
          size === 'sm' ? 'w-4 h-4' : 'w-5 h-5',
          modelValue
            ? (size === 'sm' ? 'translate-x-4' : 'translate-x-5')
            : 'translate-x-0'
        ]"
      ></span>
    </span>
  </label>
</template>

<script lang="ts" setup>
withDefaults(defineProps<{
  modelValue: boolean
  label: string
  size?: 'sm' | 'md'
}>(), { size: 'md' })

const emit = defineEmits<{ (e: 'update:modelValue', v: boolean): void }>()

function onChange(e: Event) {
  emit('update:modelValue', (e.target as HTMLInputElement).checked)
}
</script>
