<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1 class="back-link">
        <BackLink />
        {{ isEditing ? t('pages.pages.update.title') : t('pages.pages.create.title') }}
      </h1>
    </div>

    <!-- Bandeau de récupération de brouillon -->
    <div v-if="localDraft" class="autosave-recovery">
      <span>Un brouillon non sauvegardé a été détecté.</span>
      <button class="btn btn--sm" @click="restoreDraft">Restaurer</button>
      <button class="btn btn--outline btn--sm" @click="dismissDraft">Ignorer</button>
    </div>

    <Loader v-if="isLoading" />
    <div v-else :key="editorKey" class="page-editor">
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

          <!-- Indicateur autosave -->
          <div v-if="isEditing && autosaveAgo" class="autosave-indicator">
            <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"/><polyline points="17 21 17 13 7 13 7 21"/><polyline points="7 3 7 8 15 8"/></svg>
            Sauvegardé {{ autosaveAgo }}
          </div>

          <div class="page-editor__actions">
            <button class="btn" :disabled="preventMultipleSubmit" @click="onSubmit">
              {{ t('global.save') }}
            </button>
            <button v-if="isEditing" class="btn btn--outline" :disabled="previewing" @click="onPreview">
              {{ previewing ? 'Chargement...' : 'Aperçu' }}
            </button>
          </div>
        </div>

        <!-- Historique des révisions -->
        <PageRevisionHistory
          v-if="isEditing && page.id"
          :page-id="page.id"
          :current-page="page"
          @restored="onRevisionRestored"
        />
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue-i18n"
import {computed, onMounted, ref} from "vue"
import {useRoute, useRouter} from "vue-router"
import {usePageService} from "@/serviceRegistry"
import {Page} from "@/types/entities"
import Loader from "@/components/layouts/items/Loader.vue"
import BackLink from "@/components/layouts/items/BackLink.vue"
import FormTextEditor from "@/components/forms/FormTextEditor.vue"
import PageBlocksEditor from "@/components/blocks/PageBlocksEditor.vue"
import PageRevisionHistory from "@/components/admin/PageRevisionHistory.vue"
import {useAutosave} from "@/composables/useAutosave"
import type {PageBlock} from "@/types/entities/pageBlock"

const {t} = useI18n()
const route = useRoute()
const router = useRouter()
const pageService = usePageService()

const isLoading = ref(false)
const preventMultipleSubmit = ref(false)
const isEditing = ref(false)
const page = ref<Page>(new Page())
const previewing = ref(false)
const localDraft = ref<Record<string, any> | null>(null)
const editorKey = ref(0)

const {
  lastSavedAgo: autosaveAgo,
  start: startAutosave,
  checkLocalDraft,
  restoreLocalDraft,
  dismissLocalDraft,
  onManualSave
} = useAutosave({page, pageService})

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

    // Vérifier les brouillons locaux
    localDraft.value = checkLocalDraft()

    // Démarrer l'autosave
    startAutosave()
  } else {
    page.value.status = "Draft"
    page.value.sortOrder = 0
    page.value.contentMode = "html"
  }
})

function restoreDraft() {
  if (localDraft.value) {
    restoreLocalDraft(localDraft.value)
    localDraft.value = null
  }
}

function dismissDraft() {
  dismissLocalDraft()
  localDraft.value = null
}

async function onSubmit() {
  if (preventMultipleSubmit.value) return
  preventMultipleSubmit.value = true

  try {
    if (isEditing.value) {
      const result = await pageService.update(page.value)
      if (result.succeeded) {
        if (result.page) page.value = result.page
        onManualSave()
      }
    } else {
      const response = await pageService.create(page.value)
      if (response && response.succeeded) {
        onManualSave()
        router.back()
      }
    }
  } finally {
    preventMultipleSubmit.value = false
  }
}

async function onPreview() {
  if (!page.value.id || previewing.value) return
  previewing.value = true

  try {
    // D'abord sauvegarder l'état actuel en autosave
    await pageService.autosave(page.value.id!, page.value)

    // Ensuite créer le token de preview
    const result = await pageService.createPreview(page.value.id!)
    if (result) {
      window.open(result.previewUrl, '_blank')
    }
  } finally {
    previewing.value = false
  }
}

function onRevisionRestored(restoredPage: Page) {
  page.value = restoredPage
  editorKey.value++
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

.page-editor__actions {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.page-editor__actions .btn {
  flex: 1;
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

.btn--sm {
  font-size: 0.8125rem;
  padding: 0.375rem 0.75rem;
}

.autosave-indicator {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  font-size: 0.75rem;
  color: var(--color-gray-500, #6b7280);
  margin-bottom: 1rem;
}

.autosave-recovery {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  background: #fffbeb;
  border: 1px solid #fbbf24;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  margin-top: 0.5rem;
}

.page-editor .btn.btn--outline {
  background: transparent;
  border: 1px solid #d1d5db;
  color: #374151;
}

.page-editor .btn.btn--outline:hover {
  background: #f9fafb;
}
</style>
