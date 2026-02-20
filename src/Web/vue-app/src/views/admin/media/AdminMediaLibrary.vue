<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1 class="back-link">{{ t('routes.admin.children.media.name') }}</h1>
    </div>
    <div class="content-grid__actions">
      <label class="btn btn--primary">
        {{ t('global.addFile') }}
        <input type="file" accept="image/*,application/pdf" multiple hidden @change="onFilesSelected" />
      </label>
    </div>
    <Loader v-if="isLoading" />
    <div v-else class="media-grid">
      <div
        v-for="media in mediaFiles"
        :key="media.id"
        class="media-grid__item"
        :class="{ 'media-grid__item--selected': selectedMedia?.id === media.id }"
        @click="selectMedia(media)"
      >
        <img v-if="isImage(media.contentType)" :src="media.blobUrl" :alt="media.altText || media.originalFileName" />
        <div v-else class="media-grid__file-icon">
          <span>{{ getFileExtension(media.fileName) }}</span>
        </div>
        <div class="media-grid__name">{{ media.originalFileName }}</div>
      </div>
    </div>

    <div v-if="selectedMedia" class="media-detail">
      <div class="media-detail__preview">
        <img v-if="isImage(selectedMedia.contentType)" :src="selectedMedia.blobUrl" :alt="selectedMedia.altText" />
      </div>
      <div class="media-detail__info">
        <p><strong>{{ t('global.name') }}:</strong> {{ selectedMedia.originalFileName }}</p>
        <p><strong>Type:</strong> {{ selectedMedia.contentType }}</p>
        <p v-if="selectedMedia.width"><strong>{{ t('pages.media.dimensions') }}:</strong> {{ selectedMedia.width }} x {{ selectedMedia.height }}px</p>
        <div class="media-detail__alt">
          <label>{{ t('pages.media.altText') }}</label>
          <input type="text" v-model="editAltText" @blur="saveAltText" />
        </div>
        <button class="btn btn--danger" @click="onDelete">{{ t('global.delete') }}</button>
      </div>
    </div>

    <div v-if="paginatedResponse.totalItems && paginatedResponse.totalItems > pageSize" class="media-pagination">
      <button :disabled="pageIndex <= 1" @click="loadMedia(pageIndex - 1, pageSize)">&#8592;</button>
      <span>{{ pageIndex }} / {{ Math.ceil((paginatedResponse.totalItems || 0) / pageSize) }}</span>
      <button :disabled="pageIndex >= Math.ceil((paginatedResponse.totalItems || 0) / pageSize)" @click="loadMedia(pageIndex + 1, pageSize)">&#8594;</button>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {onMounted, ref} from "vue"
import {useMediaService} from "@/inversify.config"
import {notifyError, notifySuccess} from "@/notify"
import {MediaFile} from "@/types/entities"
import {PaginatedResponse} from "@/types/responses"
import Loader from "@/components/layouts/items/Loader.vue"

const {t} = useI18n()
const mediaService = useMediaService()

const isLoading = ref(false)
const mediaFiles = ref<MediaFile[]>([])
const paginatedResponse = ref<PaginatedResponse<MediaFile>>({totalItems: 0})
const selectedMedia = ref<MediaFile | null>(null)
const editAltText = ref("")
const pageIndex = ref(1)
const pageSize = 24

onMounted(async () => {
  await loadMedia(1, pageSize)
})

async function loadMedia(page: number, size: number) {
  isLoading.value = true
  pageIndex.value = page
  const response = await mediaService.getAll(page, size)
  if (response) {
    paginatedResponse.value = response
    if (response.items)
      mediaFiles.value = response.items
  }
  isLoading.value = false
}

function selectMedia(media: MediaFile) {
  selectedMedia.value = media
  editAltText.value = media.altText || ""
}

function isImage(contentType?: string): boolean {
  return contentType?.startsWith("image/") || false
}

function getFileExtension(fileName?: string): string {
  if (!fileName) return ""
  return fileName.split(".").pop()?.toUpperCase() || ""
}

async function onFilesSelected(event: Event) {
  const input = event.target as HTMLInputElement
  if (!input.files) return

  isLoading.value = true
  for (const file of Array.from(input.files)) {
    const response = await mediaService.upload(file)
    if (response?.id) {
      notifySuccess(t('pages.media.upload.validation.successMessage'))
    } else {
      notifyError(t('pages.media.upload.validation.failedMessage'))
    }
  }
  input.value = ""
  await loadMedia(1, pageSize)
}

async function saveAltText() {
  if (!selectedMedia.value?.id) return
  if (editAltText.value === (selectedMedia.value.altText || "")) return

  const response = await mediaService.update(selectedMedia.value.id, editAltText.value)
  if (response && response.succeeded) {
    selectedMedia.value.altText = editAltText.value
    notifySuccess(t('pages.media.update.validation.successMessage'))
  } else {
    notifyError(t('pages.media.update.validation.failedMessage'))
  }
}

async function onDelete() {
  if (!selectedMedia.value?.id) return

  const confirmDelete = confirm(t('pages.media.delete.confirmation'))
  if (!confirmDelete) return

  const response = await mediaService.delete(selectedMedia.value.id)
  if (response && response.succeeded) {
    mediaFiles.value = mediaFiles.value.filter(m => m.id !== selectedMedia.value?.id)
    selectedMedia.value = null
    notifySuccess(t('pages.media.delete.validation.successMessage'))
  } else {
    notifyError(t('pages.media.delete.validation.failedMessage'))
  }
}
</script>

<style scoped>
.media-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 1rem;
  margin-top: 1rem;
}

.media-grid__item {
  border: 2px solid transparent;
  border-radius: 0.5rem;
  overflow: hidden;
  cursor: pointer;
  background: var(--color-gray-100, #f3f4f6);
  transition: border-color 0.2s;
}

.media-grid__item:hover {
  border-color: var(--color-gray-300, #d1d5db);
}

.media-grid__item--selected {
  border-color: #be1e2c;
}

.media-grid__item img {
  width: 100%;
  height: 120px;
  object-fit: cover;
}

.media-grid__file-icon {
  width: 100%;
  height: 120px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  color: var(--color-gray-500, #6b7280);
}

.media-grid__name {
  padding: 0.5rem;
  font-size: 0.75rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.media-detail {
  margin-top: 2rem;
  display: flex;
  gap: 2rem;
  padding: 1.5rem;
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.5rem;
}

.media-detail__preview img {
  max-width: 300px;
  max-height: 300px;
  object-fit: contain;
}

.media-detail__info {
  flex: 1;
}

.media-detail__alt {
  margin: 1rem 0;
}

.media-detail__alt label {
  display: block;
  margin-bottom: 0.25rem;
  font-weight: 600;
}

.media-detail__alt input {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid var(--color-gray-300, #d1d5db);
  border-radius: 0.25rem;
}

.media-pagination {
  margin-top: 1.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
}

.media-pagination button {
  padding: 0.5rem 1rem;
  border: 1px solid var(--color-gray-300, #d1d5db);
  border-radius: 0.25rem;
  background: white;
  cursor: pointer;
}

.media-pagination button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
