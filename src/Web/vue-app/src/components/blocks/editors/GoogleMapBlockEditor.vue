<template>
  <div class="google-map-editor">
    <div class="form-group">
      <label>{{ t('pages.blocks.googleMap.embedUrl') }}</label>
      <input
        type="text"
        class="form-input"
        :value="modelValue.embedUrl"
        :placeholder="t('pages.blocks.googleMap.embedUrlPlaceholder')"
        @input="update({ embedUrl: ($event.target as HTMLInputElement).value })"
      />
    </div>
    <div class="form-group">
      <label>{{ t('pages.blocks.googleMap.address') }}</label>
      <input
        type="text"
        class="form-input"
        :value="modelValue.address"
        :placeholder="t('pages.blocks.googleMap.addressPlaceholder')"
        @input="update({ address: ($event.target as HTMLInputElement).value })"
      />
    </div>
    <div class="form-group">
      <label>{{ t('pages.blocks.googleMap.height') }}</label>
      <input
        type="number"
        class="form-input"
        :value="modelValue.height ?? 400"
        @input="update({ height: Number(($event.target as HTMLInputElement).value) })"
      />
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue-i18n"
import {GoogleMapBlockData} from "@/types/entities/pageBlock"

const {t} = useI18n()

const props = defineProps<{ modelValue: GoogleMapBlockData }>()
const emit = defineEmits<{ 'update:modelValue': [value: GoogleMapBlockData] }>()

function update(partial: Partial<GoogleMapBlockData>) {
  emit('update:modelValue', { ...props.modelValue, ...partial })
}
</script>

<style scoped>
.google-map-editor {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.form-group label {
  font-weight: 600;
  font-size: 0.875rem;
}

.form-input {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid var(--color-gray-300, #d1d5db);
  border-radius: 0.25rem;
}
</style>
