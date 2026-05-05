<template>
  <component
    :is="rendererComponent"
    v-if="rendererComponent"
    :block="block"
    :data="block.data" />

  <div v-else-if="isDev" class="block-renderer-debug">
    <strong>Aucun renderer trouvé pour ce bloc.</strong>
    <div><strong>Type reçu :</strong> {{ block.type }}</div>
    <pre>{{ formattedBlockData }}</pre>
  </div>
</template>

<script lang="ts" setup>
import {computed, defineAsyncComponent, watchEffect} from "vue"
import type {PageBlock, BlockType} from "@/types/entities/pageBlock"

const props = defineProps<{
  block: PageBlock
}>()

const isDev = import.meta.env.DEV

const rendererMap: Record<BlockType, ReturnType<typeof defineAsyncComponent>> = {
  'rich-text': defineAsyncComponent(() => import('./renderers/RichTextBlock.vue')),
  'google-map': defineAsyncComponent(() => import('./renderers/GoogleMapBlock.vue')),
  'image-gallery': defineAsyncComponent(() => import('./renderers/ImageGalleryBlock.vue')),
  'hero': defineAsyncComponent(() => import('./renderers/HeroBlock.vue')),
  'faq': defineAsyncComponent(() => import('./renderers/FaqBlock.vue')),
  'cta-button': defineAsyncComponent(() => import('./renderers/CtaButtonBlock.vue')),
  'contact-form': defineAsyncComponent(() => import('./renderers/ContactFormBlock.vue'))
}

const rendererComponent = computed(() => rendererMap[props.block.type] ?? null)
const formattedBlockData = computed(() => JSON.stringify(props.block.data ?? {}, null, 2))

if (isDev) {
  watchEffect(() => {
    if (!rendererComponent.value) {
      console.warn('[BlockRenderer] Unknown block type', {
        type: props.block.type,
        data: props.block.data,
      })
      return
    }

    console.log('[BlockRenderer] Rendering block', {
      type: props.block.type,
      data: props.block.data,
      rendererResolved: true,
    })
  })
}
</script>

<style scoped>
.block-renderer-debug {
  margin: 1rem 0 2rem;
  padding: 1rem;
  border: 1px dashed #b42318;
  border-radius: 0.75rem;
  background: #fff5f5;
  color: #7a271a;
}

.block-renderer-debug pre {
  margin: 0.75rem 0 0;
  white-space: pre-wrap;
  word-break: break-word;
  font-size: 0.8125rem;
}
</style>
