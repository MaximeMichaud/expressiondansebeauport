<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1 class="back-link">
        <BackLink />
        {{ isEditing ? t('pages.pages.update.title') : t('pages.pages.create.title') }}
      </h1>
    </div>
    <Loader v-if="isLoading" />
    <div v-else class="page-editor">
      <div class="page-editor__main">
        <div class="form-group">
          <label>{{ t('global.title') }}</label>
          <input type="text" v-model="page.title" class="form-input" :placeholder="t('pages.pages.placeholderTitle')" />
        </div>
        <div class="form-group">
          <label>{{ t('pages.pages.slug') }}</label>
          <input type="text" v-model="page.slug" class="form-input" :placeholder="t('pages.pages.placeholderSlug')" />
        </div>
        <template v-if="page.contentMode === 'blocks'">
          <PageBlocksEditor v-model="parsedBlocks" />
        </template>
        <template v-else>
          <FormTextEditor
            v-model="page.content"
            v-model:cssModelValue="page.customCss"
            name="content"
            :label="t('pages.pages.content')"
            :rules="[]"
          />
        </template>
        <div class="form-group">
          <label>{{ t('pages.pages.metaDescription') }}</label>
          <textarea v-model="page.metaDescription" rows="3" class="form-input form-textarea" :placeholder="t('pages.pages.placeholderMetaDescription')"></textarea>
        </div>
      </div>
      <div class="page-editor__sidebar">
        <div class="page-editor__panel">
          <h3>{{ t('pages.pages.publishPanel') }}</h3>
          <div class="form-group">
            <label>{{ t('pages.pages.status') }}</label>
            <select v-model="page.status" class="form-input">
              <option value="Draft">{{ t('pages.pages.draft') }}</option>
              <option value="Published">{{ t('pages.pages.published') }}</option>
            </select>
          </div>
          <div class="form-group">
            <label>{{ t('pages.pages.sortOrder') }}</label>
            <input type="number" v-model.number="page.sortOrder" class="form-input" />
          </div>
          <div class="form-group">
            <label>Mode de contenu</label>
            <select v-model="page.contentMode" class="form-input">
              <option value="html">Contenu libre (HTML)</option>
              <option value="blocks">Blocs visuels</option>
            </select>
          </div>
          <button class="btn" :disabled="preventMultipleSubmit" @click="onSubmit">
            {{ t('global.save') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue-i18n"
import {computed, onMounted, ref} from "vue"
import {useRoute, useRouter} from "vue-router"
import {usePageService} from "@/inversify.config"
import {Page} from "@/types/entities"
import Loader from "@/components/layouts/items/Loader.vue"
import BackLink from "@/components/layouts/items/BackLink.vue"
import FormTextEditor from "@/components/forms/FormTextEditor.vue"
import PageBlocksEditor from "@/components/blocks/PageBlocksEditor.vue"
import type {PageBlock} from "@/types/entities/pageBlock"

const {t} = useI18n()
const route = useRoute()
const router = useRouter()
const pageService = usePageService()

const isLoading = ref(false)
const preventMultipleSubmit = ref(false)
const isEditing = ref(false)
const page = ref<Page>(new Page())

const parsedBlocks = computed({
  get(): PageBlock[] {
    if (!page.value.blocks) return []
    try { return JSON.parse(page.value.blocks) } catch { return [] }
  },
  set(val: PageBlock[]) {
    page.value.blocks = JSON.stringify(val)
  }
})

onMounted(async () => {
  const id = route.params.id as string
  if (id) {
    isEditing.value = true
    isLoading.value = true
    page.value = await pageService.get(id)
    isLoading.value = false
  } else {
    page.value.status = "Draft"
    page.value.sortOrder = 0
    page.value.contentMode = "html"
  }
})

async function onSubmit() {
  if (preventMultipleSubmit.value) return
  preventMultipleSubmit.value = true

  try {
    const response = isEditing.value
      ? await pageService.update(page.value)
      : await pageService.create(page.value)

    if (response && response.succeeded) {
      router.back()
    }
  } finally {
    preventMultipleSubmit.value = false
  }
}
</script>

<style scoped>
.page-editor {
  display: grid;
  grid-template-columns: 1fr 300px;
  gap: 2rem;
  margin-top: 1rem;
}

@media (max-width: 767px) {
  .page-editor {
    grid-template-columns: 1fr;
  }
}

.page-editor__main {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.page-editor__sidebar {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.page-editor__panel {
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.5rem;
  padding: 1rem;
}

.page-editor__panel h3 {
  margin-bottom: 1rem;
  font-size: 1rem;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.25rem;
  font-weight: 600;
  font-size: 0.875rem;
}

.form-input {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid var(--color-gray-300, #d1d5db);
  border-radius: 0.25rem;
}

.form-textarea {
  resize: vertical;
  font-family: inherit;
}

.form-textarea--code {
  font-family: monospace;
  font-size: 13px;
  tab-size: 2;
}
</style>
