<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1 class="back-link">{{ t('routes.admin.children.customizer.name') }}</h1>
    </div>
    <Loader v-if="isLoading" />
    <div v-else class="customizer">
      <div class="customizer__panel">
        <h2>{{ t('pages.customizer.identity') }}</h2>
        <div class="form-group">
          <label>{{ t('pages.customizer.siteTitle') }}</label>
          <input type="text" v-model="settings.siteTitle" class="form-input" placeholder="Expression Danse Beauport" />
        </div>
        <div class="form-group">
          <label>{{ t('pages.customizer.tagline') }}</label>
          <input type="text" v-model="settings.tagline" class="form-input" placeholder="L'art du mouvement, le cœur du quartier" />
        </div>
        <div class="form-group">
          <label>{{ t('pages.customizer.primaryColor') }}</label>
          <div class="color-picker">
            <input type="color" v-model="settings.primaryColor" />
            <input type="text" v-model="settings.primaryColor" class="form-input" maxlength="7" placeholder="#be1e2d" />
          </div>
        </div>
      </div>


<div class="customizer__actions">
        <button class="btn" :disabled="isSaving" @click="onSave">{{ t('global.save') }}</button>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {onMounted, ref} from "vue"
import {useSiteSettingsService} from "@/inversify.config"
import {SiteSettings} from "@/types/entities"
import Loader from "@/components/layouts/items/Loader.vue"
import {applyThemeSettings} from "@/theme"
import {notifyError, notifySuccess} from "@/notify"

const {t} = useI18n()
const settingsService = useSiteSettingsService()

const isLoading = ref(false)
const isSaving = ref(false)
const settings = ref<SiteSettings>(new SiteSettings())


onMounted(async () => {
  isLoading.value = true
  settings.value = await settingsService.get()
  isLoading.value = false
})

async function onSave() {
  isSaving.value = true
  const response = await settingsService.update(settings.value)
  if (response.succeeded) {
    applyThemeSettings(settings.value)
    notifySuccess(t('pages.customizer.update.validation.successMessage'))
  } else {
    notifyError(t('pages.customizer.update.validation.failedMessage'))
  }
  isSaving.value = false
}
</script>

<style scoped>
.customizer {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

.customizer__panel {
  padding: 1.5rem;
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.5rem;
}

.customizer__panel h2 {
  margin-bottom: 1rem;
  font-size: 1.125rem;
}


.color-picker {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.color-picker input[type="color"] {
  width: 40px;
  height: 40px;
  border: none;
  border-radius: 0.25rem;
  cursor: pointer;
}


.customizer__actions {
  grid-column: span 2;
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


@media (max-width: 767px) {
  .customizer {
    grid-template-columns: 1fr;
  }

  .customizer__actions {
    grid-column: span 1;
  }
}

</style>
