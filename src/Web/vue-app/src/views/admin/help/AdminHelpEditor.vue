<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1 class="back-link">
        <BackLink :path="{ name: 'admin.children.help.index' }" />
        {{ isEditing ? t('pages.help.editor.editTitle') : t('pages.help.editor.createTitle') }}
      </h1>
    </div>

    <Loader v-if="isLoading" />

    <div v-else class="help-editor">
      <!-- Zone principale -->
      <div class="help-editor__main">
        <div class="form-group">
          <label>{{ t('pages.help.fields.title') }}</label>
          <input
            type="text"
            v-model="article.title"
            class="form-input"
            :placeholder="t('pages.help.fields.titlePlaceholder')"
            @blur="autoFillSlug"
          />
        </div>

        <div class="form-group">
          <label>{{ t('pages.help.fields.slug') }}</label>
          <input
            type="text"
            v-model="article.slug"
            class="form-input"
            :placeholder="t('pages.help.fields.slugPlaceholder')"
            @input="slugTouched = true"
          />
        </div>

        <FormTextEditor
          v-model="contentModel"
          name="content"
          :label="t('pages.help.fields.content')"
          :rules="[]"
        />
      </div>

      <!-- Barre latérale -->
      <div class="help-editor__sidebar">
        <div class="help-editor__panel">
          <h3>{{ t('pages.help.editor.panelTitle') }}</h3>

          <!-- Catégorie -->
          <div class="form-group">
            <label>{{ t('pages.help.fields.category') }}</label>
            <select v-model="article.category" class="form-input">
              <option
                v-for="cat in HELP_CATEGORIES"
                :key="cat"
                :value="cat"
              >
                {{ t(`pages.help.categories.${cat}`) }}
              </option>
            </select>
          </div>

          <!-- Route liée -->
          <div class="form-group">
            <label>{{ t('pages.help.fields.routeHint') }}</label>
            <input
              type="text"
              v-model="routeHintModel"
              class="form-input"
              :placeholder="t('pages.help.fields.routeHintPlaceholderLong')"
            />
            <p class="form-hint">{{ t('pages.help.fields.routeHintHelp') }}</p>
          </div>

          <!-- Ordre -->
          <div class="form-group">
            <label>{{ t('pages.help.fields.sortOrder') }}</label>
            <input
              type="number"
              v-model.number="article.sortOrder"
              class="form-input"
            />
          </div>

          <!-- Toggle publié -->
          <div class="form-group">
            <label class="help-editor__toggle-label">
              <span>{{ article.isPublished ? t('pages.help.fields.published') : t('pages.help.fields.draft') }}</span>
              <button
                type="button"
                class="help-editor__toggle"
                :class="{ 'help-editor__toggle--on': article.isPublished }"
                :aria-checked="article.isPublished"
                role="switch"
                @click="article.isPublished = !article.isPublished"
              >
                <span class="help-editor__toggle-thumb" />
              </button>
            </label>
          </div>

          <!-- Actions -->
          <div class="help-editor__actions">
            <RouterLink
              :to="{ name: 'admin.children.help.index' }"
              class="btn btn--outline"
            >
              {{ t('pages.help.editor.cancelButton') }}
            </RouterLink>
            <button
              type="button"
              class="btn btn--primary"
              :disabled="preventMultipleSubmit || !article.title.trim()"
              @click="onSubmit"
            >
              {{ preventMultipleSubmit ? t('pages.help.editor.saving') : t('pages.help.editor.saveButton') }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {computed, onMounted, ref} from "vue"
import {useI18n} from "vue-i18n"
import {useRoute, useRouter, RouterLink} from "vue-router"
import {useHelpArticleService} from "@/serviceRegistry"
import {useHelpDrawerStore} from "@/stores/helpDrawerStore"
import {HELP_CATEGORIES, HelpArticle} from "@/types/entities/helpArticle"
import BackLink from "@/components/layouts/items/BackLink.vue"
import Loader from "@/components/layouts/items/Loader.vue"
import FormTextEditor from "@/components/forms/FormTextEditor.vue"
import {notifySuccess, notifyError} from "@/notify"

const {t} = useI18n()
const route = useRoute()
const router = useRouter()
const helpArticleService = useHelpArticleService()
const helpDrawerStore = useHelpDrawerStore()

const isLoading = ref(false)
const preventMultipleSubmit = ref(false)
const isEditing = ref(false)
const article = ref<HelpArticle>(new HelpArticle())
const slugTouched = ref(false)

const contentModel = computed({
  get(): string {
    return article.value.content ?? ''
  },
  set(value: string) {
    article.value.content = value
  }
})

const routeHintModel = computed({
  get(): string {
    return article.value.routeHint ?? ''
  },
  set(value: string) {
    const trimmed = value.trim()
    article.value.routeHint = trimmed === '' ? null : trimmed
  }
})

onMounted(async () => {
  await helpDrawerStore.loadPermissions()

  if (!helpDrawerStore.canEdit) {
    notifyError(t('pages.help.permissions.editingDisabled'))
    router.push({name: 'admin.children.help.index'})
    return
  }

  const id = route.params.id as string | undefined
  if (id) {
    isEditing.value = true
    isLoading.value = true
    try {
      const loaded = await helpArticleService.getById(id)
      if (loaded) {
        article.value = loaded
        slugTouched.value = true // slug déjà renseigné, ne pas écraser
      } else {
        router.push({name: 'admin.children.help.index'})
      }
    } finally {
      isLoading.value = false
    }
  }
})

function slugify(value: string): string {
  return value
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
}

function autoFillSlug() {
  if (!slugTouched.value && article.value.title) {
    article.value.slug = slugify(article.value.title)
  }
}

async function onSubmit() {
  if (preventMultipleSubmit.value) return
  preventMultipleSubmit.value = true

  try {
    if (isEditing.value) {
      const result = await helpArticleService.update(article.value)
      if (result.succeeded) {
        if (result.article) {
          article.value = result.article
        }
        helpDrawerStore.invalidate()
        notifySuccess(t('pages.help.editor.saveSuccess'))
      } else {
        notifyError(t('pages.help.editor.saveError'))
      }
    } else {
      const result = await helpArticleService.create(article.value)
      if (result.succeeded) {
        helpDrawerStore.invalidate()
        notifySuccess(t('pages.help.editor.saveSuccess'))
        if (result.article?.id) {
          router.push({name: 'admin.children.help.edit', params: {id: result.article.id}})
        } else {
          router.push({name: 'admin.children.help.index'})
        }
      } else {
        notifyError(t('pages.help.editor.saveError'))
      }
    }
  } finally {
    preventMultipleSubmit.value = false
  }
}
</script>

<style scoped>
.help-editor {
  display: grid;
  grid-template-columns: 1fr 300px;
  gap: 2rem;
  margin-top: 1rem;
}

@media (max-width: 767px) {
  .help-editor {
    grid-template-columns: 1fr;
  }
}

.help-editor__main {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.help-editor__sidebar {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.help-editor__panel {
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.5rem;
  padding: 1rem;
}

.help-editor__panel h3 {
  margin: 0 0 1rem;
  font-size: 0.9375rem;
  font-weight: 600;
  color: var(--color-gray-700, #374151);
}

.help-editor__actions {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
  margin-top: 0.25rem;
}

.help-editor__actions .btn,
.help-editor__actions a {
  flex: 1;
  text-align: center;
  justify-content: center;
}

/* Toggle switch */
.help-editor__toggle-label {
  display: flex;
  align-items: center;
  justify-content: space-between;
  cursor: pointer;
  font-weight: 600;
  font-size: 0.875rem;
  user-select: none;
}

.help-editor__toggle {
  position: relative;
  width: 2.5rem;
  height: 1.375rem;
  border-radius: 999px;
  border: none;
  background: var(--color-gray-300, #d1d5db);
  cursor: pointer;
  transition: background 0.2s;
  flex-shrink: 0;
}

.help-editor__toggle--on {
  background: #b91c1c;
}

.help-editor__toggle-thumb {
  position: absolute;
  top: 0.1875rem;
  left: 0.1875rem;
  width: 1rem;
  height: 1rem;
  border-radius: 50%;
  background: #fff;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
  transition: transform 0.2s;
  pointer-events: none;
}

.help-editor__toggle--on .help-editor__toggle-thumb {
  transform: translateX(1.125rem);
}

/* Champs formulaire */
.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.25rem;
  font-weight: 600;
  font-size: 0.875rem;
}

.form-group--inline label {
  display: inline-flex;
  margin-bottom: 0;
}

.form-input {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid var(--color-gray-300, #d1d5db);
  border-radius: 0.25rem;
}

.form-hint {
  font-size: 0.75rem;
  color: var(--color-gray-500, #6b7280);
  margin: 0.25rem 0 0;
}

/* Boutons */
.btn--primary {
  background: #b91c1c;
  color: #fff;
  border: none;
}

.btn--primary:hover:not(:disabled) {
  background: #991b1b;
}

.btn--primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn--outline {
  background: transparent;
  border: 1px solid var(--color-gray-300, #d1d5db);
  color: var(--color-gray-700, #374151);
  text-decoration: none;
  display: inline-flex;
  align-items: center;
}

.btn--outline:hover {
  background: var(--color-gray-50, #f9fafb);
}
</style>
