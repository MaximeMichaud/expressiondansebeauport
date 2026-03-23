<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1 class="back-link">{{ t('routes.admin.children.media.name') }}</h1>
      <label class="btn">
        {{ t('global.addFile') }}
        <input type="file" :accept="acceptedFileTypes" multiple hidden @change="onFilesSelected" />
      </label>
    </div>

    <div class="media-filters">
      <button
        v-for="filter in filters"
        :key="filter.value"
        class="media-filters__btn"
        :class="{ 'media-filters__btn--active': activeFilter === filter.value }"
        @click="onFilterChange(filter.value)"
      >
        {{ filter.label }}
      </button>
    </div>

    <Loader v-if="isLoading" />
    <p v-else-if="!mediaFiles.length" class="media-empty">{{ t('global.table.noData') }}</p>
    <div v-else class="media-grid">
      <div
        v-for="media in mediaFiles"
        :key="media.id"
        class="media-grid__item"
        :class="{ 'media-grid__item--selected': selectedMedia?.id === media.id }"
        @click="selectMedia(media)"
      >
        <img v-if="isImage(media.contentType)" :src="media.blobUrl" :alt="media.altText || media.originalFileName" />
        <div v-else class="media-grid__file-icon" :class="'media-grid__file-icon--' + (media.fileType || '').toLowerCase()">
          <span>{{ getFileExtension(media.originalFileName) }}</span>
        </div>
        <div class="media-grid__name">{{ media.originalFileName }}</div>
      </div>
    </div>

    <div v-if="selectedMedia" class="media-detail">
      <div class="media-detail__preview">
        <img v-if="isImage(selectedMedia.contentType)" :src="selectedMedia.blobUrl" :alt="selectedMedia.altText" />
        <div v-else class="media-detail__file-preview">
          <span class="media-detail__file-ext">{{ getFileExtension(selectedMedia.originalFileName) }}</span>
          <a :href="selectedMedia.blobUrl" target="_blank" rel="noopener noreferrer" class="btn btn--small">{{ t('pages.media.downloadFile') }}</a>
        </div>
      </div>
      <div class="media-detail__info">
        <p><strong>{{ t('global.name') }}:</strong> {{ selectedMedia.originalFileName }}</p>
        <p><strong>{{ t('pages.media.type') }}:</strong> {{ selectedMedia.contentType }}</p>
        <p><strong>{{ t('pages.media.fileType') }}:</strong> {{ selectedMedia.fileType }}</p>
        <p v-if="selectedMedia.width"><strong>{{ t('pages.media.dimensions') }}:</strong> {{ selectedMedia.width }} x {{ selectedMedia.height }}px</p>
        <div v-if="isImage(selectedMedia.contentType)" class="media-detail__alt">
          <label>{{ t('pages.media.altText') }}</label>
          <input type="text" v-model="editAltText" @blur="saveAltText" placeholder="Ex: Photo de groupe lors du spectacle de juin" />
        </div>
        <button class="btn" @click="onDelete">{{ t('global.delete') }}</button>
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
import {computed, onMounted, ref} from "vue"
import {useMediaService} from "@/inversify.config"
import {MediaFile} from "@/types/entities"
import {PaginatedResponse} from "@/types/responses"
import Loader from "@/components/layouts/items/Loader.vue"

const {t} = useI18n()
const mediaService = useMediaService()

const isLoading = ref(false)
const allMediaFiles = ref<MediaFile[]>([])
const paginatedResponse = ref<PaginatedResponse<MediaFile>>({totalItems: 0})
const selectedMedia = ref<MediaFile | null>(null)
const editAltText = ref("")
const pageIndex = ref(1)
const pageSize = 24
const activeFilter = ref("all")

const acceptedFileTypes = "image/*,video/*,application/pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.odt,.ods,.odp,.csv,.txt"

const filters = computed(() => [
  { value: "all", label: t('pages.media.filterAll') },
  { value: "Image", label: t('pages.media.filterImages') },
  { value: "Document", label: t('pages.media.filterDocuments') },
  { value: "Video", label: t('pages.media.filterVideos') },
  { value: "Other", label: t('pages.media.filterOther') },
])

const mediaFiles = computed(() => {
  if (activeFilter.value === "all") return allMediaFiles.value
  return allMediaFiles.value.filter(m => m.fileType === activeFilter.value)
})

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
      allMediaFiles.value = response.items
  }
  isLoading.value = false
}

function onFilterChange(filter: string) {
  activeFilter.value = filter
  selectedMedia.value = null
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
    await mediaService.upload(file)
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
  }
}

async function onDelete() {
  if (!selectedMedia.value?.id) return

  const confirmDelete = confirm(t('pages.media.delete.confirmation'))
  if (!confirmDelete) return

  const response = await mediaService.delete(selectedMedia.value.id)
  if (response && response.succeeded) {
    allMediaFiles.value = allMediaFiles.value.filter(m => m.id !== selectedMedia.value?.id)
    selectedMedia.value = null
  }
}
</script>

<style scoped>
.media-empty {
  color: #5c5c5c;
  font-size: 0.875rem;
  padding: 16px 0;
}

.media-filters {
  display: flex;
  gap: 0.5rem;
  margin-top: 1rem;
  flex-wrap: wrap;
}

.media-filters__btn {
  padding: 0.375rem 0.75rem;
  border: 1px solid var(--color-gray-300, #d1d5db);
  border-radius: 0.25rem;
  background: white;
  cursor: pointer;
  font-size: 0.8125rem;
  transition: all 0.2s;
}

.media-filters__btn:hover {
  background: var(--color-gray-100, #f3f4f6);
}

.media-filters__btn--active {
  background: var(--primary);
  color: white;
  border-color: var(--primary);
}

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
  border-color: var(--primary);
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
  font-size: 0.875rem;
  color: var(--color-gray-500, #6b7280);
}

.media-grid__file-icon--document {
  color: #dc2626;
}

.media-grid__file-icon--video {
  color: #7c3aed;
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

.media-detail__file-preview {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  padding: 2rem;
}

.media-detail__file-ext {
  font-size: 2rem;
  font-weight: bold;
  color: var(--color-gray-500, #6b7280);
}

.btn--small {
  font-size: 0.8125rem;
  padding: 0.375rem 0.75rem;
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

@media (max-width: 767px) {
  .media-grid {
    grid-template-columns: repeat(auto-fill, minmax(110px, 1fr));
  }

  .media-detail {
    flex-direction: column;
    gap: 1rem;
    padding: 1rem;
  }

  .media-detail__preview img {
    max-width: 100%;
    max-height: 180px;
  }
}
</style>
