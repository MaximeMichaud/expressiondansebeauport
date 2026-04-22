<template>
  <template v-for="row in rows" :key="row.type === 'pair' ? `pair-${row.blocks[0].id}-${row.blocks[1].id}` : `single-${row.block.id}`">
    <div v-if="row.type === 'pair'" class="page-blocks__row">
      <div class="page-blocks__col">
        <BlockRenderer :block="row.blocks[0]" />
      </div>
      <div class="page-blocks__col">
        <BlockRenderer :block="row.blocks[1]" />
      </div>
    </div>
    <BlockRenderer v-else :block="row.block" />
  </template>
</template>

<script lang="ts" setup>
import {computed} from "vue"
import type {PageBlock} from "@/types/entities/pageBlock"
import BlockRenderer from "@/components/blocks/BlockRenderer.vue"

type Row =
  | { type: 'single'; block: PageBlock }
  | { type: 'pair'; blocks: [PageBlock, PageBlock] }

const props = defineProps<{ blocks: PageBlock[] }>()

const rows = computed<Row[]>(() => {
  const result: Row[] = []
  let i = 0
  while (i < props.blocks.length) {
    const current = props.blocks[i]
    const next = props.blocks[i + 1]
    if (current.width === 'half' && next?.width === 'half') {
      result.push({type: 'pair', blocks: [current, next]})
      i += 2
    } else {
      result.push({type: 'single', block: current})
      i += 1
    }
  }
  return result
})
</script>

<style scoped>
.page-blocks__row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
  align-items: stretch;
}

.page-blocks__col {
  min-width: 0;
  display: flex;
  flex-direction: column;
}

@media (max-width: 768px) {
  .page-blocks__row {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
}
</style>
