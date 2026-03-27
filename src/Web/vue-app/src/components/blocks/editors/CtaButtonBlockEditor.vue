<template>
  <div class="cta-button-editor">
    <div class="form-group">
      <label>{{ t('pages.blocks.ctaButton.label') }}</label>
      <input
        type="text"
        class="form-input"
        :value="modelValue.label"
        :placeholder="t('pages.blocks.ctaButton.labelPlaceholder')"
        @input="update({ label: ($event.target as HTMLInputElement).value })"
      />
    </div>
    <div class="form-group">
      <label>{{ t('pages.blocks.ctaButton.url') }}</label>
      <input
        type="text"
        class="form-input"
        :value="modelValue.url"
        placeholder="https://..."
        @input="update({ url: ($event.target as HTMLInputElement).value })"
      />
    </div>
    <div class="form-group">
      <label>{{ t('pages.blocks.ctaButton.style') }}</label>
      <select
        class="form-input"
        :value="modelValue.style ?? 'primary'"
        @change="update({ style: ($event.target as HTMLSelectElement).value as 'primary' | 'secondary' })"
      >
        <option value="primary">{{ t('pages.blocks.ctaButton.stylePrimary') }}</option>
        <option value="secondary">{{ t('pages.blocks.ctaButton.styleSecondary') }}</option>
      </select>
    </div>
    <div class="form-group">
      <label>{{ t('pages.blocks.ctaButton.alignment') }}</label>
      <select
        class="form-input"
        :value="modelValue.alignment ?? 'center'"
        @change="update({ alignment: ($event.target as HTMLSelectElement).value as 'left' | 'center' | 'right' })"
      >
        <option value="left">{{ t('pages.blocks.ctaButton.alignLeft') }}</option>
        <option value="center">{{ t('pages.blocks.ctaButton.alignCenter') }}</option>
        <option value="right">{{ t('pages.blocks.ctaButton.alignRight') }}</option>
      </select>
    </div>
    <div class="form-group">
      <label class="cta-button-editor__checkbox-label">
        <input
          type="checkbox"
          :checked="modelValue.openInNewTab ?? false"
          @change="update({ openInNewTab: ($event.target as HTMLInputElement).checked })"
        />
        {{ t('pages.blocks.ctaButton.openInNewTab') }}
      </label>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue-i18n"
import {CtaButtonBlockData} from "@/types/entities/pageBlock"

const {t} = useI18n()

const props = defineProps<{ modelValue: CtaButtonBlockData }>()
const emit = defineEmits<{ 'update:modelValue': [value: CtaButtonBlockData] }>()

function update(partial: Partial<CtaButtonBlockData>) {
  emit('update:modelValue', { ...props.modelValue, ...partial })
}
</script>

<style scoped>
.cta-button-editor {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.cta-button-editor__checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 600;
  font-size: 0.875rem;
  cursor: pointer;
}

.cta-button-editor__checkbox-label input[type="checkbox"] {
  width: auto;
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
