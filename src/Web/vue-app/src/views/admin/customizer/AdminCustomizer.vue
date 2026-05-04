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
          <input type="text" v-model="settings.tagline" class="form-input" placeholder="L'art du mouvement, le coeur du quartier" />
        </div>
        <div class="form-group">
          <label>{{ t('pages.customizer.primaryColor') }}</label>
          <div class="color-picker">
            <input type="color" v-model="settings.primaryColor" />
            <input type="text" v-model="settings.primaryColor" class="form-input" maxlength="7" placeholder="#be1e2d" />
          </div>
        </div>
      </div>

      <div class="customizer__panel">
        <h2>{{ t('pages.customizer.footer') }}</h2>
        <div class="form-group">
          <label>{{ t('pages.customizer.footerDescription') }}</label>
          <textarea v-model="settings.footerDescription" class="form-input" rows="3" placeholder="École de danse offrant des cours pour tous les âges..." />
        </div>
        <div class="form-group">
          <label>{{ t('pages.customizer.footerAddress') }}</label>
          <input type="text" v-model="settings.footerAddress" class="form-input" placeholder="15, rue de la Promenade-des-Soeurs" />
        </div>
        <div class="form-group">
          <label>{{ t('pages.customizer.footerCity') }}</label>
          <input type="text" v-model="settings.footerCity" class="form-input" placeholder="Beauport, QC G1C 0G3" />
        </div>
        <div class="form-group">
          <label>{{ t('pages.customizer.footerPhone') }}</label>
          <input type="text" v-model="settings.footerPhone" class="form-input" placeholder="418-660-1086" />
        </div>
        <div class="form-group">
          <label>{{ t('pages.customizer.footerEmail') }}</label>
          <input type="email" v-model="settings.footerEmail" class="form-input" placeholder="info@expressiondansebeauport.com" />
        </div>
        <div class="form-group">
          <label>{{ t('pages.customizer.copyrightText') }}</label>
          <input type="text" v-model="settings.copyrightText" class="form-input" placeholder="Expression Danse de Beauport inc. Tous droits réservés." />
        </div>
      </div>

      <!-- Social Links -->
      <div class="customizer__panel">
        <h2>{{ t('pages.customizer.socialLinks') }}</h2>
        <div v-if="socialLinks.length" class="collection-list">
          <div v-for="link in socialLinks" :key="link.id" class="collection-item">
            <div class="collection-item__icon">
              <component :is="getSocialIcon(link.platform)" :size="18" />
            </div>
            <div class="collection-item__info">
              <span class="collection-item__label">{{ getSocialLabel(link.platform) }}</span>
              <span class="collection-item__url">{{ link.url }}</span>
            </div>
            <button class="collection-item__delete" @click="removeSocialLink(link)">
              <Trash2 :size="14" />
            </button>
          </div>
        </div>
        <p v-else class="collection-empty">{{ t('pages.customizer.noSocialLinks') }}</p>
        <div class="collection-add">
          <select v-model="newSocialPlatform" class="form-input">
            <option value="" disabled>{{ t('pages.customizer.selectPlatform') }}</option>
            <option v-for="p in availablePlatforms" :key="p.value" :value="p.value">{{ p.label }}</option>
          </select>
          <input type="url" v-model="newSocialUrl" class="form-input" placeholder="https://..." />
          <button class="btn btn--small" :disabled="!newSocialPlatform || !newSocialUrl" @click="addSocialLink">{{ t('global.add') }}</button>
        </div>
      </div>

      <!-- Footer Partners -->
      <div class="customizer__panel">
        <h2>{{ t('pages.customizer.partners') }}</h2>
        <div v-if="footerPartners.length" class="collection-list">
          <div v-for="partner in footerPartners" :key="partner.id" class="collection-item">
            <img v-if="partner.mediaUrl" :src="partner.mediaUrl" :alt="partner.altText" class="collection-item__thumb" />
            <div class="collection-item__info">
              <span class="collection-item__label">{{ partner.altText }}</span>
              <span v-if="partner.url" class="collection-item__url">{{ partner.url }}</span>
            </div>
            <button class="collection-item__delete" @click="removePartner(partner)">
              <Trash2 :size="14" />
            </button>
          </div>
        </div>
        <p v-else class="collection-empty">{{ t('pages.customizer.noPartners') }}</p>
        <div class="collection-add">
          <select v-model="newPartnerMediaFileId" class="form-input">
            <option value="" disabled>{{ t('pages.customizer.selectImage') }}</option>
            <option v-for="media in availableImages" :key="media.id" :value="media.id">{{ media.originalFileName }}</option>
          </select>
          <input type="text" v-model="newPartnerAltText" class="form-input" :placeholder="t('pages.media.altText')" />
          <input type="url" v-model="newPartnerUrl" class="form-input" placeholder="https:// (optionnel)" />
          <button class="btn btn--small" :disabled="!newPartnerMediaFileId || !newPartnerAltText" @click="addPartner">{{ t('global.add') }}</button>
        </div>
      </div>

      <div class="customizer__panel customizer__panel--maintenance">
        <h2>{{ t('pages.customizer.maintenance.title') }}</h2>
        <p class="customizer__description">{{ t('pages.customizer.maintenance.description') }}</p>
        <div class="maintenance-toggle">
          <label class="toggle-label">
            <input type="checkbox" v-model="settings.isMaintenanceMode" class="toggle-input" />
            <span class="toggle-switch"></span>
            <span class="toggle-text">{{ settings.isMaintenanceMode ? t('pages.customizer.maintenance.enabled') : t('pages.customizer.maintenance.disabled') }}</span>
          </label>
        </div>
        <div class="form-group" style="margin-top:1rem">
          <label>{{ t('pages.customizer.maintenance.message') }}</label>
          <textarea v-model="settings.maintenanceMessage" class="form-input" rows="2" :placeholder="t('pages.customizer.maintenance.messagePlaceholder')" />
        </div>
        <div class="form-group">
          <label>{{ t('pages.customizer.maintenance.retryAfter') }}</label>
          <input type="number" v-model.number="settings.maintenanceRetryAfter" class="form-input" min="60" max="86400" />
        </div>
      </div>

      <div class="customizer__actions">
        <button class="btn" :disabled="isSaving" @click="onSave">{{ t('global.save') }}</button>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue-i18n"
import {computed, onMounted, ref} from "vue"
import {useMediaService, useSiteSettingsService} from "@/serviceRegistry"
import {FooterPartner, MediaFile, SiteSettings, SocialLink} from "@/types/entities"
import Loader from "@/components/layouts/items/Loader.vue"
import {applyThemeSettings} from "@/theme"
import {notifyError, notifySuccess} from "@/notify"
import {Trash2} from "lucide-vue-next"
import {getSocialIcon, getSocialLabel, socialLinkPlatforms} from "@/lib/socialLinks"

const {t} = useI18n()
const settingsService = useSiteSettingsService()
const mediaService = useMediaService()

const isLoading = ref(false)
const isSaving = ref(false)
const settings = ref<SiteSettings>(new SiteSettings())
const socialLinks = ref<SocialLink[]>([])
const footerPartners = ref<FooterPartner[]>([])
const allMedia = ref<MediaFile[]>([])

const newSocialPlatform = ref("")
const newSocialUrl = ref("")
const newPartnerMediaFileId = ref("")
const newPartnerAltText = ref("")
const newPartnerUrl = ref("")

const availablePlatforms = computed(() => {
  const used = new Set(socialLinks.value.map(l => l.platform?.toLowerCase()))
  return socialLinkPlatforms.filter(p => !used.has(p.value))
})

const availableImages = computed(() =>
  allMedia.value.filter(m => m.contentType?.startsWith("image/"))
)

onMounted(async () => {
  isLoading.value = true
  try {
    const [settingsData, mediaResponse] = await Promise.all([
      settingsService.get(),
      mediaService.getAll(1, 100)
    ])
    settings.value = settingsData
    socialLinks.value = settingsData.socialLinks || []
    footerPartners.value = settingsData.footerPartners || []
    if (mediaResponse?.items) allMedia.value = mediaResponse.items
  } catch {
    notifyError(t('pages.customizer.update.validation.failedMessage'))
  } finally {
    isLoading.value = false
  }
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

async function addSocialLink() {
  if (!newSocialPlatform.value || !newSocialUrl.value) return
  const link = await settingsService.addSocialLink({
    platform: newSocialPlatform.value,
    url: newSocialUrl.value,
  })
  if (link) {
    socialLinks.value.push(link)
    newSocialPlatform.value = ""
    newSocialUrl.value = ""
  } else {
    notifyError(t('pages.customizer.update.validation.failedMessage'))
  }
}

async function removeSocialLink(link: SocialLink) {
  if (!link.id) return
  const response = await settingsService.deleteSocialLink(link.id)
  if (response.succeeded) {
    socialLinks.value = socialLinks.value.filter(l => l.id !== link.id)
  } else {
    notifyError(t('pages.customizer.update.validation.failedMessage'))
  }
}

async function addPartner() {
  if (!newPartnerMediaFileId.value || !newPartnerAltText.value) return
  const partner = await settingsService.addFooterPartner({
    mediaFileId: newPartnerMediaFileId.value,
    altText: newPartnerAltText.value,
    url: newPartnerUrl.value || undefined,
  })
  if (partner) {
    footerPartners.value.push(partner)
    newPartnerMediaFileId.value = ""
    newPartnerAltText.value = ""
    newPartnerUrl.value = ""
  } else {
    notifyError(t('pages.customizer.update.validation.failedMessage'))
  }
}

async function removePartner(partner: FooterPartner) {
  if (!partner.id) return
  const response = await settingsService.deleteFooterPartner(partner.id)
  if (response.succeeded) {
    footerPartners.value = footerPartners.value.filter(p => p.id !== partner.id)
  } else {
    notifyError(t('pages.customizer.update.validation.failedMessage'))
  }
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

.collection-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.collection-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.625rem 0.75rem;
  border-radius: 0.375rem;
  background: #f8f8f8;
}

.collection-item__icon {
  display: flex;
  align-items: center;
  color: var(--primary);
  flex-shrink: 0;
}

.collection-item__thumb {
  width: 40px;
  height: 40px;
  object-fit: contain;
  border-radius: 0.25rem;
  flex-shrink: 0;
}

.collection-item__info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.collection-item__label {
  font-weight: 600;
  font-size: 0.875rem;
}

.collection-item__url {
  color: #5c5c5c;
  font-size: 0.75rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.collection-item__delete {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border: none;
  border-radius: 0.375rem;
  background: var(--primary);
  color: #fff;
  cursor: pointer;
  flex-shrink: 0;
}

.collection-empty {
  color: #5c5c5c;
  font-size: 0.875rem;
  margin-bottom: 1rem;
}

.collection-add {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.btn--small {
  font-size: 0.8125rem;
  padding: 0.375rem 0.75rem;
}

.customizer__panel--maintenance {
  border-left: 4px solid #be1e2c;
}

.customizer__description {
  font-size: 0.9rem;
  color: #666;
  margin-bottom: 1rem;
}

.maintenance-toggle {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.toggle-label {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  cursor: pointer;
  user-select: none;
}

.toggle-input {
  display: none;
}

.toggle-switch {
  width: 44px;
  height: 24px;
  background: #ccc;
  border-radius: 999px;
  position: relative;
  transition: background 0.2s;
  flex-shrink: 0;
}

.toggle-switch::after {
  content: '';
  position: absolute;
  top: 3px;
  left: 3px;
  width: 18px;
  height: 18px;
  background: #fff;
  border-radius: 50%;
  transition: left 0.2s;
}

.toggle-input:checked + .toggle-switch {
  background: #be1e2c;
}

.toggle-input:checked + .toggle-switch::after {
  left: 23px;
}

.toggle-text {
  font-size: 0.95rem;
  font-weight: 600;
  color: #1a1a1a;
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
