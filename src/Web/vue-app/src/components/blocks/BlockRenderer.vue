<template>
  <component
    :is="rendererComponent"
    v-if="rendererComponent"
    :data="block.data" />
</template>

<script lang="ts" setup>
import {computed, defineAsyncComponent} from "vue"
import type {PageBlock, BlockType} from "@/types/entities/pageBlock"

const props = defineProps<{
  block: PageBlock
}>()

const rendererMap: Record<BlockType, ReturnType<typeof defineAsyncComponent>> = {
  'rich-text': defineAsyncComponent(() => import('./renderers/RichTextBlock.vue')),
  'google-map': defineAsyncComponent(() => import('./renderers/GoogleMapBlock.vue')),
  'image-gallery': defineAsyncComponent(() => import('./renderers/ImageGalleryBlock.vue')),
  'hero': defineAsyncComponent(() => import('./renderers/HeroBlock.vue')),
  'faq': defineAsyncComponent(() => import('./renderers/FaqBlock.vue')),
  'cta-button': defineAsyncComponent(() => import('./renderers/CtaButtonBlock.vue'))
}

const rendererComponent = computed(() => rendererMap[props.block.type] ?? null)
</script>
