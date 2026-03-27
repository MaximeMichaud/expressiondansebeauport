<template>
  <div class="google-map-block">
    <a
      v-if="data.address"
      :href="mapsUrl"
      target="_blank"
      rel="noopener noreferrer"
      class="google-map-block__address">
      {{ data.address }}
    </a>
    <div class="google-map-block__wrapper">
      <iframe
        :src="data.embedUrl"
        :style="{ height: (data.height ?? 400) + 'px' }"
        class="google-map-block__iframe"
        loading="lazy"
        referrerpolicy="no-referrer-when-downgrade"
        allowfullscreen>
      </iframe>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {computed} from "vue"
import type {GoogleMapBlockData} from "@/types/entities/pageBlock"

const props = defineProps<{
  data: GoogleMapBlockData
}>()

const mapsUrl = computed(() =>
  `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(props.data.address ?? '')}`
)
</script>

<style scoped>
.google-map-block {
  margin-bottom: 2rem;
}

.google-map-block__address {
  display: inline-block;
  margin-bottom: 1rem;
  color: var(--color-primary, #be1e2c);
  font-weight: 600;
  text-decoration: none;
  transition: opacity 0.2s;
}

.google-map-block__address:hover {
  opacity: 0.8;
  text-decoration: underline;
}

.google-map-block__wrapper {
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 14px 32px rgba(0, 0, 0, 0.08);
  aspect-ratio: 16 / 9;
}

.google-map-block__iframe {
  width: 100%;
  height: 100%;
  border: 0;
  display: block;
}
</style>
