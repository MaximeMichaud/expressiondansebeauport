<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1 class="back-link">{{ t('routes.admin.children.importExport.name') }}</h1>
    </div>

    <div class="import-export">
      <div class="import-export__panel">
        <h2>{{ t('pages.importExport.exportTitle') }}</h2>
        <p>{{ t('pages.importExport.exportDescription') }}</p>
        <button class="btn btn--primary" :disabled="isExporting" @click="onExport">
          {{ t('global.download') }}
        </button>
      </div>

      <div class="import-export__panel">
        <h2>{{ t('pages.importExport.importTitle') }}</h2>
        <p>{{ t('pages.importExport.importDescription') }}</p>
        <label class="btn btn--secondary">
          {{ t('global.addFile') }}
          <input type="file" accept=".json" hidden @change="onImport" />
        </label>
        <Loader v-if="isImporting" />
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {ref} from "vue"
import {useImportExportService} from "@/inversify.config"
import {notifyError, notifySuccess} from "@/notify"
import Loader from "@/components/layouts/items/Loader.vue"

const {t} = useI18n()
const importExportService = useImportExportService()

const isExporting = ref(false)
const isImporting = ref(false)

async function onExport() {
  isExporting.value = true
  try {
    const blob = await importExportService.exportData()
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `site-export-${new Date().toISOString().split('T')[0]}.json`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
    notifySuccess(t('pages.importExport.exportValidation.successMessage'))
  } catch {
    notifyError(t('pages.importExport.exportValidation.failedMessage'))
  }
  isExporting.value = false
}

async function onImport(event: Event) {
  const input = event.target as HTMLInputElement
  if (!input.files?.[0]) return

  const confirmImport = confirm(t('pages.importExport.importConfirmation'))
  if (!confirmImport) {
    input.value = ""
    return
  }

  isImporting.value = true
  const response = await importExportService.importData(input.files[0])
  if (response && response.succeeded) {
    notifySuccess(t('pages.importExport.importValidation.successMessage'))
  } else {
    notifyError(t('pages.importExport.importValidation.failedMessage'))
  }
  input.value = ""
  isImporting.value = false
}
</script>

<style scoped>
.import-export {
  margin-top: 1rem;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
}

.import-export__panel {
  padding: 2rem;
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.5rem;
}

.import-export__panel h2 {
  margin-bottom: 0.5rem;
}

.import-export__panel p {
  margin-bottom: 1rem;
  color: var(--color-gray-600, #4b5563);
  font-size: 0.875rem;
}

.btn--secondary {
  background: var(--color-gray-200, #e5e7eb);
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 0.25rem;
  cursor: pointer;
  display: inline-block;
}
</style>
