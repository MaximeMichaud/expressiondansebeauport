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
          <input type="text" v-model="settings.siteTitle" class="form-input" />
        </div>
        <div class="form-group">
          <label>{{ t('pages.customizer.tagline') }}</label>
          <input type="text" v-model="settings.tagline" class="form-input" />
        </div>
      </div>

      <div class="customizer__panel">
        <h2>{{ t('pages.customizer.colors') }}</h2>
        <div class="customizer__colors">
          <div class="form-group">
            <label>{{ t('pages.customizer.primaryColor') }}</label>
            <div class="color-picker">
              <input type="color" v-model="settings.primaryColor" />
              <input type="text" v-model="settings.primaryColor" class="form-input" maxlength="7" />
            </div>
          </div>
          <div class="form-group">
            <label>{{ t('pages.customizer.secondaryColor') }}</label>
            <div class="color-picker">
              <input type="color" v-model="settings.secondaryColor" />
              <input type="text" v-model="settings.secondaryColor" class="form-input" maxlength="7" />
            </div>
          </div>
        </div>
      </div>

      <div class="customizer__panel">
        <h2>{{ t('pages.customizer.typography') }}</h2>
        <div class="form-group">
          <label>{{ t('pages.customizer.headingFont') }}</label>
          <select v-model="settings.headingFont" class="form-input">
            <option v-for="font in availableFonts" :key="font" :value="font">{{ font }}</option>
          </select>
        </div>
        <div class="form-group">
          <label>{{ t('pages.customizer.bodyFont') }}</label>
          <select v-model="settings.bodyFont" class="form-input">
            <option v-for="font in availableFonts" :key="font" :value="font">{{ font }}</option>
          </select>
        </div>
      </div>

      <div class="customizer__panel">
        <h2>{{ t('pages.customizer.branding') }}</h2>
        <div class="form-group">
          <label>{{ t('pages.customizer.logo') }}</label>
          <div v-if="settings.logoUrl" class="customizer__current-media">
            <img :src="settings.logoUrl" alt="Logo" class="customizer__preview-img" />
            <button class="btn btn--small btn--danger" @click="settings.logoMediaFileId = undefined; settings.logoUrl = undefined">{{ t('global.removeFile') }}</button>
          </div>
          <label v-else class="btn btn--secondary">
            {{ t('global.addFile') }}
            <input type="file" accept="image/*" hidden @change="onLogoSelected" />
          </label>
        </div>
        <div class="form-group">
          <label>{{ t('pages.customizer.favicon') }}</label>
          <div v-if="settings.faviconUrl" class="customizer__current-media">
            <img :src="settings.faviconUrl" alt="Favicon" class="customizer__preview-img customizer__preview-img--small" />
            <button class="btn btn--small btn--danger" @click="settings.faviconMediaFileId = undefined; settings.faviconUrl = undefined">{{ t('global.removeFile') }}</button>
          </div>
          <label v-else class="btn btn--secondary">
            {{ t('global.addFile') }}
            <input type="file" accept="image/*" hidden @change="onFaviconSelected" />
          </label>
        </div>
      </div>

      <div class="customizer__actions">
        <button class="btn btn--primary" :disabled="isSaving" @click="onSave">{{ t('global.save') }}</button>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {onMounted, ref} from "vue"
import {useSiteSettingsService, useMediaService} from "@/inversify.config"
import {notifyError, notifySuccess} from "@/notify"
import {SiteSettings} from "@/types/entities"
import Loader from "@/components/layouts/items/Loader.vue"

const {t} = useI18n()
const settingsService = useSiteSettingsService()
const mediaService = useMediaService()

const isLoading = ref(false)
const isSaving = ref(false)
const settings = ref<SiteSettings>(new SiteSettings())

const availableFonts = [
  'Montserrat', 'Karla', 'Roboto', 'Open Sans', 'Lato', 'Poppins',
  'Inter', 'Nunito', 'Raleway', 'Playfair Display', 'Merriweather'
]

onMounted(async () => {
  isLoading.value = true
  settings.value = await settingsService.get()
  isLoading.value = false
})

async function onLogoSelected(event: Event) {
  const input = event.target as HTMLInputElement
  if (!input.files?.[0]) return

  const uploadResponse = await mediaService.upload(input.files[0])
  if (uploadResponse?.id) {
    settings.value.logoMediaFileId = uploadResponse.id
    settings.value.logoUrl = uploadResponse.blobUrl
    notifySuccess(t('pages.customizer.logoUpdated'))
  } else {
    notifyError(t('validation.errorOccured'))
  }
  input.value = ""
}

async function onFaviconSelected(event: Event) {
  const input = event.target as HTMLInputElement
  if (!input.files?.[0]) return

  const uploadResponse = await mediaService.upload(input.files[0])
  if (uploadResponse?.id) {
    settings.value.faviconMediaFileId = uploadResponse.id
    settings.value.faviconUrl = uploadResponse.blobUrl
    notifySuccess(t('pages.customizer.faviconUpdated'))
  } else {
    notifyError(t('validation.errorOccured'))
  }
  input.value = ""
}

async function onSave() {
  isSaving.value = true
  const response = await settingsService.update(settings.value)
  if (response && response.succeeded) {
    notifySuccess(t('pages.customizer.update.validation.successMessage'))
  } else {
    notifyError(t('pages.customizer.update.validation.failedMessage'))
  }
  isSaving.value = false
}
</script>

<style scoped>
.customizer {
  margin-top: 1rem;
}

.customizer__panel {
  padding: 1.5rem;
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.5rem;
  margin-bottom: 1.5rem;
}

.customizer__panel h2 {
  margin-bottom: 1rem;
  font-size: 1.125rem;
}

.customizer__colors {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
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

.customizer__current-media {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.customizer__preview-img {
  max-width: 200px;
  max-height: 80px;
  object-fit: contain;
}

.customizer__preview-img--small {
  max-width: 48px;
  max-height: 48px;
}

.customizer__actions {
  margin-top: 2rem;
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

.btn--small {
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
}

.btn--danger {
  background: #be1e2c;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 0.25rem;
  cursor: pointer;
}

.btn--secondary {
  background: var(--color-gray-200, #e5e7eb);
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 0.25rem;
  cursor: pointer;
}
</style>
