<template>
  <div class="blocks-editor">
    <VueDraggable
      v-if="blocks.length"
      v-model="blocks"
      handle=".blocks-editor__drag"
      class="blocks-editor__list"
      @end="emitUpdate"
    >
      <div v-for="(block, index) in blocks" :key="block.id" class="blocks-editor__block">
        <div class="blocks-editor__header">
          <button class="blocks-editor__drag" :aria-label="t('pages.blocks.reorder')">
            <GripVertical :size="14" />
          </button>
          <span class="blocks-editor__type-label">{{ BLOCK_LABELS[block.type] }}</span>
          <div class="blocks-editor__actions">
            <button
              class="blocks-editor__action-btn"
              :disabled="index === 0"
              :aria-label="t('pages.blocks.moveUp')"
              @click="moveBlock(index, -1)"
            >
              <ChevronUp :size="14" />
            </button>
            <button
              class="blocks-editor__action-btn"
              :disabled="index === blocks.length - 1"
              :aria-label="t('pages.blocks.moveDown')"
              @click="moveBlock(index, 1)"
            >
              <ChevronDown :size="14" />
            </button>
            <button
              class="blocks-editor__action-btn blocks-editor__action-btn--danger"
              :aria-label="t('pages.blocks.remove')"
              @click="removeBlock(index)"
            >
              <Trash2 :size="14" />
            </button>
          </div>
        </div>
        <div class="blocks-editor__content">
          <RichTextBlockEditor
            v-if="block.type === 'rich-text'"
            :modelValue="block.data as RichTextBlockData"
            @update:modelValue="updateBlockData(index, $event)"
          />
          <GoogleMapBlockEditor
            v-else-if="block.type === 'google-map'"
            :modelValue="block.data as GoogleMapBlockData"
            @update:modelValue="updateBlockData(index, $event)"
          />
          <ImageGalleryBlockEditor
            v-else-if="block.type === 'image-gallery'"
            :modelValue="block.data as ImageGalleryBlockData"
            @update:modelValue="updateBlockData(index, $event)"
          />
          <HeroBlockEditor
            v-else-if="block.type === 'hero'"
            :modelValue="block.data as HeroBlockData"
            @update:modelValue="updateBlockData(index, $event)"
          />
          <FaqBlockEditor
            v-else-if="block.type === 'faq'"
            :modelValue="block.data as FaqBlockData"
            @update:modelValue="updateBlockData(index, $event)"
          />
          <CtaButtonBlockEditor
            v-else-if="block.type === 'cta-button'"
            :modelValue="block.data as CtaButtonBlockData"
            @update:modelValue="updateBlockData(index, $event)"
          />
        </div>
      </div>
    </VueDraggable>

    <p v-else class="blocks-editor__empty">{{ t('pages.blocks.noBlocks') }}</p>

    <button class="btn blocks-editor__add-btn" @click="showSelector = true">
      {{ t('pages.blocks.addBlock') }}
    </button>

    <BlockSelector
      v-if="showSelector"
      @select="addBlock"
      @close="showSelector = false"
    />
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue-i18n"
import {ref} from "vue"
import {VueDraggable} from "vue-draggable-plus"
import {GripVertical, ChevronUp, ChevronDown, Trash2} from "lucide-vue-next"
import {
  PageBlock,
  BlockType,
  BLOCK_LABELS,
  RichTextBlockData,
  GoogleMapBlockData,
  ImageGalleryBlockData,
  HeroBlockData,
  FaqBlockData,
  CtaButtonBlockData
} from "@/types/entities/pageBlock"
import BlockSelector from "@/components/blocks/BlockSelector.vue"
import RichTextBlockEditor from "@/components/blocks/editors/RichTextBlockEditor.vue"
import GoogleMapBlockEditor from "@/components/blocks/editors/GoogleMapBlockEditor.vue"
import ImageGalleryBlockEditor from "@/components/blocks/editors/ImageGalleryBlockEditor.vue"
import HeroBlockEditor from "@/components/blocks/editors/HeroBlockEditor.vue"
import FaqBlockEditor from "@/components/blocks/editors/FaqBlockEditor.vue"
import CtaButtonBlockEditor from "@/components/blocks/editors/CtaButtonBlockEditor.vue"

const {t} = useI18n()

const props = defineProps<{ modelValue: PageBlock[] }>()
const emit = defineEmits<{ 'update:modelValue': [value: PageBlock[]] }>()

const blocks = ref<PageBlock[]>([...props.modelValue])
const showSelector = ref(false)

function emitUpdate() {
  emit('update:modelValue', [...blocks.value])
}

const defaultData: Record<BlockType, () => Record<string, any>> = {
  'rich-text': () => ({ html: '' }),
  'google-map': () => ({ embedUrl: '', address: '', height: 400 }),
  'image-gallery': () => ({ images: [], columns: 3 }),
  'hero': () => ({ title: '', subtitle: '', backgroundImageUrl: '', ctaLabel: '', ctaUrl: '', overlayOpacity: 0.5 }),
  'faq': () => ({ items: [] }),
  'cta-button': () => ({ label: '', url: '', style: 'primary', alignment: 'center', openInNewTab: false })
}

function addBlock(type: BlockType) {
  blocks.value.push({
    id: crypto.randomUUID(),
    type,
    data: defaultData[type]()
  })
  showSelector.value = false
  emitUpdate()
}

function removeBlock(index: number) {
  blocks.value.splice(index, 1)
  emitUpdate()
}

function moveBlock(index: number, direction: number) {
  const target = index + direction
  if (target < 0 || target >= blocks.value.length) return
  const temp = blocks.value[index]
  blocks.value[index] = blocks.value[target]
  blocks.value[target] = temp
  emitUpdate()
}

function updateBlockData(index: number, data: Record<string, any>) {
  blocks.value[index] = { ...blocks.value[index], data }
  emitUpdate()
}
</script>

<style scoped>
.blocks-editor {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.blocks-editor__list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.blocks-editor__block {
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.5rem;
  overflow: hidden;
}

.blocks-editor__header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 0.75rem;
  background: var(--color-gray-50, #f9fafb);
  border-bottom: 1px solid var(--color-gray-200, #e5e7eb);
}

.blocks-editor__drag {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  border: none;
  background: none;
  color: #5c5c5c;
  cursor: grab;
  flex-shrink: 0;
  padding: 0;
  border-radius: 4px;
}

.blocks-editor__drag:active {
  cursor: grabbing;
}

.blocks-editor__drag:hover {
  color: #232323;
  background: #efefef;
}

.blocks-editor__type-label {
  flex: 1;
  font-weight: 600;
  font-size: 0.875rem;
}

.blocks-editor__actions {
  display: flex;
  gap: 4px;
  flex-shrink: 0;
}

.blocks-editor__action-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border: none;
  border-radius: 6px;
  background: var(--primary);
  color: #fff;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.blocks-editor__action-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

@media (hover: hover) {
  .blocks-editor__action-btn:not(:disabled):hover {
    background: color-mix(in srgb, var(--primary) 80%, black);
  }
}

.blocks-editor__action-btn--danger {
  background: #dc2626;
}

@media (hover: hover) {
  .blocks-editor__action-btn--danger:not(:disabled):hover {
    background: #b91c1c;
  }
}

.blocks-editor__content {
  padding: 1rem;
}

.blocks-editor__empty {
  color: #5c5c5c;
  font-size: 0.875rem;
  padding: 1rem 0;
}

.blocks-editor__add-btn {
  align-self: flex-start;
}
</style>
