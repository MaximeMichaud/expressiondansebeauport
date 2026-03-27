<template>
  <div class="faq-editor">
    <div class="faq-editor__list">
      <div
        v-for="(item, index) in modelValue.items"
        :key="index"
        class="faq-editor__item"
      >
        <div class="form-group">
          <label>{{ t('pages.blocks.faq.question') }}</label>
          <input
            type="text"
            class="form-input"
            :value="item.question"
            :placeholder="t('pages.blocks.faq.questionPlaceholder')"
            @input="updateItem(index, 'question', ($event.target as HTMLInputElement).value)"
          />
        </div>
        <div class="form-group">
          <label>{{ t('pages.blocks.faq.answer') }}</label>
          <textarea
            class="form-input faq-editor__textarea"
            :value="item.answer"
            :placeholder="t('pages.blocks.faq.answerPlaceholder')"
            rows="3"
            @input="updateItem(index, 'answer', ($event.target as HTMLTextAreaElement).value)"
          ></textarea>
        </div>
        <button
          class="faq-editor__remove-btn"
          @click="removeItem(index)"
        >
          {{ t('global.delete') }}
        </button>
      </div>
    </div>

    <button class="btn faq-editor__add-btn" @click="addItem">
      {{ t('pages.blocks.faq.addItem') }}
    </button>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue-i18n"
import {FaqBlockData} from "@/types/entities/pageBlock"

const {t} = useI18n()

const props = defineProps<{ modelValue: FaqBlockData }>()
const emit = defineEmits<{ 'update:modelValue': [value: FaqBlockData] }>()

function update(partial: Partial<FaqBlockData>) {
  emit('update:modelValue', { ...props.modelValue, ...partial })
}

function addItem() {
  const items = [...props.modelValue.items, { question: '', answer: '' }]
  update({ items })
}

function removeItem(index: number) {
  const items = props.modelValue.items.filter((_, i) => i !== index)
  update({ items })
}

function updateItem(index: number, field: 'question' | 'answer', value: string) {
  const items = props.modelValue.items.map((item, i) =>
    i === index ? { ...item, [field]: value } : item
  )
  update({ items })
}
</script>

<style scoped>
.faq-editor {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.faq-editor__list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.faq-editor__item {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 0.75rem;
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.375rem;
  background: var(--color-gray-50, #f9fafb);
}

.faq-editor__textarea {
  resize: vertical;
  font-family: inherit;
}

.faq-editor__remove-btn {
  align-self: flex-end;
  background: none;
  border: none;
  color: #dc2626;
  font-size: 0.8125rem;
  cursor: pointer;
  padding: 0;
}

.faq-editor__remove-btn:hover {
  color: #b91c1c;
}

.faq-editor__add-btn {
  align-self: flex-start;
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
