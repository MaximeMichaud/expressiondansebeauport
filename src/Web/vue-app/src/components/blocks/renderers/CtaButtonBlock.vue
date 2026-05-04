<template>
  <div
    class="cta-button-block"
    :class="'cta-button-block--' + alignment">
    <a
      :href="data.url"
      :target="data.openInNewTab ? '_blank' : undefined"
      :rel="data.openInNewTab ? 'noopener noreferrer' : undefined"
      class="cta-button-block__link"
      :class="'cta-button-block__link--' + (data.style ?? 'primary')">
      {{ data.label }}
    </a>
  </div>
</template>

<script lang="ts" setup>
import {computed} from "vue"
import type {CtaButtonBlockData} from "@/types/entities/pageBlock"

const props = defineProps<{
  data: CtaButtonBlockData
}>()

const alignment = computed(() => {
  const value = props.data.alignment

  return value === 'left' || value === 'right' || value === 'center'
    ? value
    : 'center'
})
</script>

<style scoped>
.cta-button-block {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 10rem;
  padding: 2rem 0;
  margin-bottom: 2rem;
}

.cta-button-block--left {
  align-items: flex-start;
  text-align: left;
}

.cta-button-block--center {
  align-items: center;
  text-align: center;
}

.cta-button-block--right {
  align-items: flex-end;
  text-align: right;
}

.cta-button-block__link {
  display: inline-block;
  padding: 14px 28px;
  border-radius: 10px;
  font-weight: 700;
  text-decoration: none;
  transition: background 0.3s, color 0.3s, border-color 0.3s;
  font-size: 1rem;
}

.cta-button-block__link--primary {
  background: var(--color-primary, #be1e2c);
  color: #fff;
  border: 2px solid var(--color-primary, #be1e2c);
}

.cta-button-block__link--primary:hover {
  background: #9e1824;
  border-color: #9e1824;
}

.cta-button-block__link--secondary {
  background: transparent;
  color: var(--color-primary, #be1e2c);
  border: 2px solid var(--color-primary, #be1e2c);
}

.cta-button-block__link--secondary:hover {
  background: var(--color-primary, #be1e2c);
  color: #fff;
}
</style>
