<template>
  <div class="hero-editor">
    <div class="form-group">
      <label>{{ t('pages.blocks.hero.title') }}</label>
      <input
        type="text"
        class="form-input"
        :value="modelValue.title"
        :placeholder="t('pages.blocks.hero.titlePlaceholder')"
        @input="update({ title: ($event.target as HTMLInputElement).value })"
      />
    </div>
    <div class="form-group">
      <label>{{ t('pages.blocks.hero.subtitle') }}</label>
      <input
        type="text"
        class="form-input"
        :value="modelValue.subtitle"
        :placeholder="t('pages.blocks.hero.subtitlePlaceholder')"
        @input="update({ subtitle: ($event.target as HTMLInputElement).value })"
      />
    </div>
    <div class="form-group">
      <label>{{ t('pages.blocks.hero.backgroundImageUrl') }}</label>
      <input
        type="text"
        class="form-input"
        :value="modelValue.backgroundImageUrl"
        placeholder="https://..."
        @input="update({ backgroundImageUrl: ($event.target as HTMLInputElement).value })"
      />
    </div>
    <div class="form-group">
      <label>{{ t('pages.blocks.hero.ctaLabel') }}</label>
      <input
        type="text"
        class="form-input"
        :value="modelValue.ctaLabel"
        :placeholder="t('pages.blocks.hero.ctaLabelPlaceholder')"
        @input="update({ ctaLabel: ($event.target as HTMLInputElement).value })"
      />
    </div>
    <div class="form-group">
      <label>{{ t('pages.blocks.hero.ctaUrl') }}</label>
      <input
        type="text"
        class="form-input"
        :value="modelValue.ctaUrl"
        placeholder="https://..."
        @input="update({ ctaUrl: ($event.target as HTMLInputElement).value })"
      />
    </div>
    <div class="form-group">
      <label>{{ t('pages.blocks.hero.backgroundColor') }}</label>
      <div class="hero-editor__color">
        <input
          type="color"
          :value="modelValue.backgroundColor ?? '#be1e2c'"
          @input="update({ backgroundColor: ($event.target as HTMLInputElement).value })"
        />
        <span class="hero-editor__color-hint">{{ t('pages.blocks.hero.backgroundColorHint') }}</span>
      </div>
    </div>
    <div class="form-group">
      <label>{{ t('pages.blocks.hero.overlayOpacity') }} ({{ modelValue.overlayOpacity ?? 0.5 }})</label>
      <input
        type="range"
        min="0"
        max="1"
        step="0.1"
        :value="modelValue.overlayOpacity ?? 0.5"
        @input="update({ overlayOpacity: Number(($event.target as HTMLInputElement).value) })"
      />
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue-i18n"
import {HeroBlockData} from "@/types/entities/pageBlock"

const {t} = useI18n()

const props = defineProps<{ modelValue: HeroBlockData }>()
const emit = defineEmits<{ 'update:modelValue': [value: HeroBlockData] }>()

function update(partial: Partial<HeroBlockData>) {
  emit('update:modelValue', { ...props.modelValue, ...partial })
}
</script>

<style scoped>
.hero-editor {
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

input[type="range"] {
  width: 100%;
  cursor: pointer;
}

.hero-editor__color {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.hero-editor__color input[type="color"] {
  width: 2.5rem;
  height: 2.5rem;
  padding: 0.1rem;
  border: 1px solid var(--color-gray-300, #d1d5db);
  border-radius: 0.25rem;
  cursor: pointer;
}

.hero-editor__color-hint {
  font-size: 0.8rem;
  color: #6b7280;
}
</style>
