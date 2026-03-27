<template>
  <div class="faq-block">
    <details
      v-for="(item, index) in data.items"
      :key="index"
      class="faq-block__item">
      <summary class="faq-block__question">
        <span>{{ item.question }}</span>
        <svg
          class="faq-block__chevron"
          width="20"
          height="20"
          viewBox="0 0 20 20"
          fill="none">
          <path
            d="M5 7.5L10 12.5L15 7.5"
            stroke="currentColor"
            stroke-width="2"
            stroke-linecap="round"
            stroke-linejoin="round" />
        </svg>
      </summary>
      <p class="faq-block__answer" v-for="(line, li) in item.answer.split('\n')" :key="li">{{ line }}</p>
    </details>
  </div>
</template>

<script lang="ts" setup>
import type {FaqBlockData} from "@/types/entities/pageBlock"

defineProps<{
  data: FaqBlockData
}>()
</script>

<style scoped>
.faq-block {
  margin-bottom: 2rem;
}

.faq-block__item {
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  margin-bottom: 0.75rem;
  overflow: hidden;
  transition: box-shadow 0.2s;
}

.faq-block__item[open] {
  box-shadow: 0 14px 32px rgba(0, 0, 0, 0.08);
}

.faq-block__question {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 1.25rem 1.5rem;
  cursor: pointer;
  font-weight: 600;
  font-size: 1rem;
  list-style: none;
  user-select: none;
  transition: background 0.2s;
}

.faq-block__question:hover {
  background: #f9fafb;
}

.faq-block__question::-webkit-details-marker {
  display: none;
}

.faq-block__question::marker {
  content: '';
}

.faq-block__chevron {
  flex-shrink: 0;
  transition: transform 0.25s;
  color: #6b7280;
}

.faq-block__item[open] .faq-block__chevron {
  transform: rotate(180deg);
}

.faq-block__answer {
  padding: 0 1.5rem 1.25rem;
  line-height: 1.7;
  color: #4b5563;
}
</style>
