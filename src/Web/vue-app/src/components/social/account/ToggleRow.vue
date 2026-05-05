<template>
  <label
    class="flex items-center justify-between"
    :class="[size === 'sm' ? 'text-sm' : '', disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer']"
  >
    <span :style="{ color: 'var(--soc-text)' }">{{ label }}</span>
    <span class="toggle-switch relative inline-block" :class="[size === 'sm' ? 'w-9 h-5' : 'w-11 h-6', { 'is-on': modelValue }]">
      <input
        type="checkbox"
        :checked="modelValue"
        :disabled="disabled"
        class="peer sr-only"
        @change="onChange"
      />
      <span class="toggle-track absolute inset-0 rounded-full transition-colors"></span>
      <span
        class="toggle-knob absolute top-0.5 left-0.5 rounded-full transition-transform"
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
  disabled?: boolean
}>(), { size: 'md', disabled: false })

const emit = defineEmits<{ (e: 'update:modelValue', v: boolean): void }>()

function onChange(e: Event) {
  emit('update:modelValue', (e.target as HTMLInputElement).checked)
}
</script>

<style lang="scss" scoped>
.toggle-track {
  background: #d1d5db; // gray-300

  .soc--dark & {
    background: #4b5563; // gray-600
  }
}

.toggle-knob {
  background: #ffffff;

  .soc--dark & {
    background: #000000;
  }
}

.toggle-switch.is-on .toggle-track {
  background: #000000;

  .soc--dark & {
    background: #ffffff;
  }
}
</style>
