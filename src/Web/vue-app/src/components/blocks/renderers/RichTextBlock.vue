<template>
  <div class="rich-text-block">
    <div class="public-page__content" v-html="data.html"></div>
  </div>
</template>

<script lang="ts" setup>
import type {RichTextBlockData} from "@/types/entities/pageBlock"

defineProps<{
  data: RichTextBlockData
}>()
</script>

<style scoped>
.rich-text-block {
  margin-bottom: 2rem;
}
</style>
