<template>
  <div
    class="cta-button-block"
    :style="{ textAlign: data.alignment ?? 'center' }">
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
import type {CtaButtonBlockData} from "@/types/entities/pageBlock"

defineProps<{
  data: CtaButtonBlockData
}>()
</script>

<style scoped>
.cta-button-block {
  margin-bottom: 2rem;
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
