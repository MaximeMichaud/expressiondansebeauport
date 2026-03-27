<template>
  <div class="rich-text-editor">
    <FormTextEditor
      v-model="props.modelValue.html"
      name="block-rich-text"
      :label="t('pages.blocks.richText.content')"
      :rules="[]"
      @update:modelValue="onHtmlUpdate"
    />
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue-i18n"
import {RichTextBlockData} from "@/types/entities/pageBlock"
import FormTextEditor from "@/components/forms/FormTextEditor.vue"

const {t} = useI18n()

const props = defineProps<{ modelValue: RichTextBlockData }>()
const emit = defineEmits<{ 'update:modelValue': [value: RichTextBlockData] }>()

function onHtmlUpdate(html: string) {
  emit('update:modelValue', { ...props.modelValue, html })
}
</script>

<style scoped>
.rich-text-editor {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
</style>
