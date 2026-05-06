<template>
  <div class="contact-form-editor">
    <div class="form-group">
      <label>{{ t('pages.blocks.contactForm.title') }}</label>
      <input
        type="text"
        class="form-input"
        :value="resolvedData.title"
        :placeholder="t('pages.blocks.contactForm.titlePlaceholder')"
        @input="update({ title: ($event.target as HTMLInputElement).value })"
      />
    </div>

    <div class="form-group">
      <label>{{ t('pages.blocks.contactForm.introText') }}</label>
      <textarea
        rows="3"
        class="form-input form-textarea"
        :value="resolvedData.introText ?? ''"
        :placeholder="t('pages.blocks.contactForm.introPlaceholder')"
        @input="update({ introText: ($event.target as HTMLTextAreaElement).value })"
      />
    </div>

    <div class="form-group">
      <label>{{ t('pages.blocks.contactForm.submitLabel') }}</label>
      <input
        type="text"
        class="form-input"
        :value="resolvedData.submitLabel ?? ''"
        :placeholder="t('pages.blocks.contactForm.submitPlaceholder')"
        @input="update({ submitLabel: ($event.target as HTMLInputElement).value })"
      />
    </div>

    <div class="form-group">
      <label>{{ t('pages.blocks.contactForm.successMessage') }}</label>
      <textarea
        rows="3"
        class="form-input form-textarea"
        :value="resolvedData.successMessage ?? ''"
        :placeholder="t('pages.blocks.contactForm.successPlaceholder')"
        @input="update({ successMessage: ($event.target as HTMLTextAreaElement).value })"
      />
    </div>

    <div class="form-group">
      <label>{{ t('pages.blocks.contactForm.recipientEmail') }}</label>
      <input
        type="email"
        class="form-input"
        :value="resolvedData.recipientEmail ?? ''"
        :placeholder="t('pages.blocks.contactForm.recipientPlaceholder')"
        @input="update({ recipientEmail: ($event.target as HTMLInputElement).value })"
      />
      <p class="contact-form-editor__hint">{{ t('pages.blocks.contactForm.recipientHint') }}</p>
    </div>

    <label class="contact-form-editor__toggle">
      <input
        type="checkbox"
        :checked="resolvedData.enabled ?? true"
        @change="update({ enabled: ($event.target as HTMLInputElement).checked })"
      />
      <span>{{ t('pages.blocks.contactForm.enabled') }}</span>
    </label>
  </div>
</template>

<script lang="ts" setup>
import {computed} from "vue"
import {useI18n} from "vue-i18n"
import {normalizeContactFormConfig} from "@/types/entities/pageBlock"
import type {ContactFormBlockData} from "@/types/entities/pageBlock"

const {t} = useI18n()

const props = defineProps<{ modelValue: ContactFormBlockData }>()
const emit = defineEmits<{ 'update:modelValue': [value: ContactFormBlockData] }>()

const resolvedData = computed(() => normalizeContactFormConfig(props.modelValue))

function update(partial: Partial<ContactFormBlockData>) {
  emit('update:modelValue', { ...normalizeContactFormConfig(props.modelValue), ...partial })
}
</script>

<style scoped>
.contact-form-editor {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.25rem;
  font-weight: 600;
  font-size: 0.875rem;
}

.form-input {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid var(--color-gray-300, #d1d5db);
  border-radius: 0.25rem;
}

.form-textarea {
  resize: vertical;
  font-family: inherit;
}

.contact-form-editor__hint {
  margin-top: 0.35rem;
  color: #6b7280;
  font-size: 0.75rem;
}

.contact-form-editor__toggle {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 600;
}
</style>
