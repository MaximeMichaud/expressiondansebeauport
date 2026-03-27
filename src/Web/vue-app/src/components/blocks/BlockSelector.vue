<template>
  <div class="block-selector__overlay" @click.self="$emit('close')">
    <div class="block-selector__popup">
      <h3 class="block-selector__title">{{ t('pages.blocks.addBlock') }}</h3>
      <div class="block-selector__grid">
        <button
          v-for="(label, type) in BLOCK_LABELS"
          :key="type"
          class="block-selector__item"
          @click="$emit('select', type)"
        >
          <span class="block-selector__icon">{{ icons[type] }}</span>
          <span class="block-selector__label">{{ label }}</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue-i18n"
import {BLOCK_LABELS, BlockType} from "@/types/entities/pageBlock"

const {t} = useI18n()

defineEmits<{
  select: [type: BlockType]
  close: []
}>()

const icons: Record<BlockType, string> = {
  'rich-text': 'T',
  'google-map': '\u{1F4CD}',
  'image-gallery': '\u{1F5BC}',
  'hero': '\u{2B50}',
  'faq': '?',
  'cta-button': '\u{1F517}'
}
</script>

<style scoped>
.block-selector__overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.block-selector__popup {
  background: #fff;
  padding: 1.5rem;
  border-radius: 0.5rem;
  width: 100%;
  max-width: 480px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
}

.block-selector__title {
  margin-bottom: 1rem;
  font-size: 1rem;
}

.block-selector__grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.5rem;
}

.block-selector__item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 1rem 0.5rem;
  border: 1px solid var(--color-gray-300, #d1d5db);
  border-radius: 0.375rem;
  background: #fff;
  cursor: pointer;
  transition: all 0.2s;
}

.block-selector__item:hover {
  border-color: var(--primary);
  background: var(--color-gray-50, #f9fafb);
}

.block-selector__icon {
  font-size: 1.5rem;
  line-height: 1;
}

.block-selector__label {
  font-size: 0.75rem;
  font-weight: 500;
  text-align: center;
}

@media (max-width: 480px) {
  .block-selector__grid {
    grid-template-columns: repeat(2, 1fr);
  }

  .block-selector__popup {
    margin: 1rem;
    max-width: calc(100% - 2rem);
  }
}
</style>
