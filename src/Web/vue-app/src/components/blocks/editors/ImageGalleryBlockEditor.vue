<template>
  <div class="image-gallery-editor">
    <div class="form-group">
      <label>{{ t('pages.blocks.imageGallery.columns') }}</label>
      <select
        class="form-input"
        :value="modelValue.columns ?? 3"
        @change="update({ columns: Number(($event.target as HTMLSelectElement).value) })"
      >
        <option :value="2">2</option>
        <option :value="3">3</option>
        <option :value="4">4</option>
      </select>
    </div>

    <div class="image-gallery-editor__list">
      <div
        v-for="(image, index) in modelValue.images"
        :key="index"
        class="image-gallery-editor__item"
      >
        <div class="form-group">
          <label>{{ t('pages.blocks.imageGallery.imageUrl') }}</label>
          <input
            type="text"
            class="form-input"
            :value="image.url"
            :placeholder="https://..."
            @input="updateImage(index, 'url', ($event.target as HTMLInputElement).value)"
          />
        </div>
        <div class="form-group">
          <label>{{ t('pages.blocks.imageGallery.imageAlt') }}</label>
          <input
            type="text"
            class="form-input"
            :value="image.alt"
            :placeholder="t('pages.blocks.imageGallery.imageAltPlaceholder')"
            @input="updateImage(index, 'alt', ($event.target as HTMLInputElement).value)"
          />
        </div>
        <button
          class="image-gallery-editor__remove-btn"
          @click="removeImage(index)"
        >
          {{ t('global.delete') }}
        </button>
      </div>
    </div>

    <button class="btn image-gallery-editor__add-btn" @click="addImage">
      {{ t('pages.blocks.imageGallery.addImage') }}
    </button>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue-i18n"
import {ImageGalleryBlockData} from "@/types/entities/pageBlock"

const {t} = useI18n()

const props = defineProps<{ modelValue: ImageGalleryBlockData }>()
const emit = defineEmits<{ 'update:modelValue': [value: ImageGalleryBlockData] }>()

function update(partial: Partial<ImageGalleryBlockData>) {
  emit('update:modelValue', { ...props.modelValue, ...partial })
}

function addImage() {
  const images = [...props.modelValue.images, { url: '', alt: '' }]
  update({ images })
}

function removeImage(index: number) {
  const images = props.modelValue.images.filter((_, i) => i !== index)
  update({ images })
}

function updateImage(index: number, field: 'url' | 'alt', value: string) {
  const images = props.modelValue.images.map((img, i) =>
    i === index ? { ...img, [field]: value } : img
  )
  update({ images })
}
</script>

<style scoped>
.image-gallery-editor {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.image-gallery-editor__list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.image-gallery-editor__item {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 0.75rem;
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.375rem;
  background: var(--color-gray-50, #f9fafb);
}

.image-gallery-editor__remove-btn {
  align-self: flex-end;
  background: none;
  border: none;
  color: #dc2626;
  font-size: 0.8125rem;
  cursor: pointer;
  padding: 0;
}

.image-gallery-editor__remove-btn:hover {
  color: #b91c1c;
}

.image-gallery-editor__add-btn {
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
