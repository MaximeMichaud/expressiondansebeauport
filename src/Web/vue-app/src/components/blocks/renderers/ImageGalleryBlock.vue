<template>
  <div class="image-gallery-block">
    <div
      class="image-gallery-block__grid"
      :style="{ '--columns': data.columns ?? 3 }">
      <button
        v-for="(image, index) in data.images"
        :key="index"
        class="image-gallery-block__item"
        type="button"
        @click="openLightbox(index)">
        <img
          :src="image.url"
          :alt="image.alt"
          class="image-gallery-block__image"
          loading="lazy" />
      </button>
    </div>

    <Teleport to="body">
      <Transition name="lightbox">
        <div
          v-if="lightboxIndex !== null"
          class="image-gallery-block__lightbox"
          @click.self="closeLightbox"
          @keydown.escape="closeLightbox"
          tabindex="0"
          ref="lightboxRef">
          <button
            class="image-gallery-block__lightbox-close"
            type="button"
            @click="closeLightbox"
            aria-label="Fermer">
            &times;
          </button>
          <img
            :src="data.images[lightboxIndex].url"
            :alt="data.images[lightboxIndex].alt"
            class="image-gallery-block__lightbox-image" />
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script lang="ts" setup>
import {ref, watch, nextTick} from "vue"
import type {ImageGalleryBlockData} from "@/types/entities/pageBlock"

defineProps<{
  data: ImageGalleryBlockData
}>()

const lightboxIndex = ref<number | null>(null)
const lightboxRef = ref<HTMLElement | null>(null)

function openLightbox(index: number) {
  lightboxIndex.value = index
  document.body.style.overflow = 'hidden'
}

function closeLightbox() {
  lightboxIndex.value = null
  document.body.style.overflow = ''
}

watch(lightboxIndex, async (val) => {
  if (val !== null) {
    await nextTick()
    lightboxRef.value?.focus()
  }
})
</script>

<style scoped>
.image-gallery-block {
  margin-bottom: 2rem;
}

.image-gallery-block__grid {
  display: grid;
  grid-template-columns: repeat(var(--columns, 3), 1fr);
  gap: 1rem;
}

@media (max-width: 768px) {
  .image-gallery-block__grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 640px) {
  .image-gallery-block__grid {
    grid-template-columns: 1fr;
  }
}

.image-gallery-block__item {
  padding: 0;
  border: none;
  background: none;
  cursor: zoom-in;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 14px 32px rgba(0, 0, 0, 0.08);
  transition: transform 0.2s, box-shadow 0.2s;
}

.image-gallery-block__item:hover {
  transform: translateY(-2px);
  box-shadow: 0 18px 40px rgba(0, 0, 0, 0.12);
}

.image-gallery-block__image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.image-gallery-block__lightbox {
  position: fixed;
  inset: 0;
  z-index: 9999;
  background: rgba(0, 0, 0, 0.9);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  outline: none;
}

.image-gallery-block__lightbox-close {
  position: absolute;
  top: 1rem;
  right: 1.5rem;
  background: none;
  border: none;
  color: #fff;
  font-size: 2.5rem;
  cursor: pointer;
  line-height: 1;
  padding: 0.5rem;
  transition: opacity 0.2s;
}

.image-gallery-block__lightbox-close:hover {
  opacity: 0.7;
}

.image-gallery-block__lightbox-image {
  max-width: 100%;
  max-height: 90vh;
  object-fit: contain;
  border-radius: 8px;
}

.lightbox-enter-active,
.lightbox-leave-active {
  transition: opacity 0.25s;
}

.lightbox-enter-from,
.lightbox-leave-to {
  opacity: 0;
}
</style>
