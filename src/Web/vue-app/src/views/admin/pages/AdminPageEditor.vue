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
          <input type="text" v-model="page.title" class="form-input" />
        </div>
        <div class="form-group">
          <label>{{ t('pages.pages.slug') }}</label>
          <input type="text" v-model="page.slug" class="form-input" />
        </div>
        <div class="form-group">
          <label>{{ t('pages.pages.content') }}</label>
          <textarea v-model="page.content" rows="20" class="form-input form-textarea"></textarea>
        </div>
        <div class="form-group">
          <label>{{ t('pages.pages.metaDescription') }}</label>
          <textarea v-model="page.metaDescription" rows="3" class="form-input form-textarea"></textarea>
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
          <button class="btn btn--primary" :disabled="preventMultipleSubmit" @click="onSubmit">
            {{ t('global.save') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {onMounted, ref} from "vue"
import {useRoute, useRouter} from "vue-router"
import {usePageService} from "@/inversify.config"
import {notifyError, notifySuccess} from "@/notify"
import {Page} from "@/types/entities"
import Loader from "@/components/layouts/items/Loader.vue"
import BackLink from "@/components/layouts/items/BackLink.vue"

const {t} = useI18n()
const route = useRoute()
const router = useRouter()
const pageService = usePageService()

const isLoading = ref(false)
const preventMultipleSubmit = ref(false)
const isEditing = ref(false)
const page = ref<Page>(new Page())

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
  }
})

async function onSubmit() {
  if (preventMultipleSubmit.value) return
  preventMultipleSubmit.value = true

  const response = isEditing.value
    ? await pageService.update(page.value)
    : await pageService.create(page.value)

  if (response && response.succeeded) {
    const msgKey = isEditing.value
      ? 'pages.pages.update.validation.successMessage'
      : 'pages.pages.create.validation.successMessage'
    notifySuccess(t(msgKey))
    setTimeout(() => router.back(), 1500)
  } else {
    const msgKey = isEditing.value
      ? 'pages.pages.update.validation.failedMessage'
      : 'pages.pages.create.validation.failedMessage'
    notifyError(t(msgKey))
  }
  preventMultipleSubmit.value = false
}
</script>

<style scoped>
.page-editor {
  display: grid;
  grid-template-columns: 1fr 300px;
  gap: 2rem;
  margin-top: 1rem;
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
</style>
