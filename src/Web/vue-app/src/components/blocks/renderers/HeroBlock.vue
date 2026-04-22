<template>
  <section
    class="hero-block"
    :style="heroStyle">
    <div
      v-if="data.backgroundImageUrl"
      class="hero-block__overlay"
      :style="{ opacity: data.overlayOpacity ?? 0.5 }">
    </div>
    <div class="hero-block__content">
      <h2 class="hero-block__title">{{ data.title }}</h2>
      <p
        v-if="data.subtitle"
        class="hero-block__subtitle">
        {{ data.subtitle }}
      </p>
      <a
        v-if="data.ctaLabel && data.ctaUrl"
        :href="data.ctaUrl"
        class="hero-block__cta">
        {{ data.ctaLabel }}
      </a>
    </div>
  </section>
</template>

<script lang="ts" setup>
import {computed} from "vue"
import type {HeroBlockData} from "@/types/entities/pageBlock"

const props = defineProps<{
  data: HeroBlockData
}>()

const heroStyle = computed(() => {
  const style: Record<string, string> = {}
  if (props.data.backgroundImageUrl) {
    style.backgroundImage = `url(${props.data.backgroundImageUrl})`
  }
  if (props.data.backgroundColor) {
    style.backgroundColor = props.data.backgroundColor
  }
  return style
})
</script>

<style scoped>
.hero-block {
  position: relative;
  min-height: 400px;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  background-size: cover;
  background-position: center;
  background-color: var(--color-primary, #be1e2c);
  border-radius: 16px;
  overflow: hidden;
  margin-bottom: 2rem;
}

.hero-block__overlay {
  position: absolute;
  inset: 0;
  background: #000;
  pointer-events: none;
}

.hero-block__content {
  position: relative;
  z-index: 1;
  padding: 3rem 2rem;
  max-width: 700px;
}

.hero-block__title {
  font-size: 2.5rem;
  font-weight: 800;
  color: #fff;
  margin: 0 0 1rem;
  line-height: 1.2;
}

.hero-block__subtitle {
  font-size: 1.25rem;
  color: rgba(255, 255, 255, 0.9);
  margin: 0 0 2rem;
  line-height: 1.6;
}

.hero-block__cta {
  display: inline-block;
  background: #fff;
  color: var(--color-primary, #be1e2c);
  padding: 14px 28px;
  border-radius: 10px;
  font-weight: 700;
  text-decoration: none;
  transition: background 0.3s, color 0.3s;
}

.hero-block__cta:hover {
  background: rgba(255, 255, 255, 0.88);
}

@media (max-width: 640px) {
  .hero-block {
    min-height: 300px;
  }

  .hero-block__title {
    font-size: 1.75rem;
  }

  .hero-block__subtitle {
    font-size: 1rem;
  }

  .hero-block__content {
    padding: 2rem 1.5rem;
  }
}
</style>
