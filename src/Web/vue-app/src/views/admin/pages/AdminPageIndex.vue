<template>
  <div class="content-grid content-grid--subpage content-grid--subpage-table">
    <div class="content-grid__header">
      <h1 class="back-link">{{ t('routes.admin.children.pages.name') }}</h1>
      <div class="content-grid__filters">
        <select v-model="statusFilter" @change="loadPages(1, pageSize)">
          <option value="">{{ t('pages.pages.allStatuses') }}</option>
          <option value="Published">{{ t('pages.pages.published') }}</option>
          <option value="Draft">{{ t('pages.pages.draft') }}</option>
        </select>
      </div>
    </div>
    <div class="content-grid__actions">
      <BtnLink
        :name="t('routes.admin.children.pages.add.name')"
        :path="{ path: t('routes.admin.children.pages.add.fullPath') }"
      />
    </div>
    <Loader v-if="preventMultipleSubmit" />
    <DataTable
      :headers="pageHeaders"
      :is-loading="pagesAreLoading"
      :items="tablePages"
      :total-items="paginatedResponse.totalItems"
      :search-value="''"
      @delete="onDelete"
      @reload="loadPages"
    />
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {computed, onMounted, ref} from "vue"
import {usePageService} from "@/inversify.config"
import {notifyError, notifySuccess} from "@/notify"
import {Page} from "@/types/entities"
import {PaginatedResponse} from "@/types/responses"
import DataTable from "@/components/layouts/items/DataTable.vue"
import BtnLink from "@/components/layouts/items/BtnLink.vue"
import Loader from "@/components/layouts/items/Loader.vue"
import {Tables} from "@/types/enums"

const {t} = useI18n()
const pageService = usePageService()

const preventMultipleSubmit = ref(false)
const pagesAreLoading = ref(false)
const pageItems = ref<Page[]>([])
const paginatedResponse = ref<PaginatedResponse<Page>>({totalItems: 0})
const statusFilter = ref("")
const pageSize = Tables.DefaultRowsPerPage

const tablePages = computed(() => {
  return pageItems.value.map((x: Page) => {
    return {
      id: x.id,
      title: x.title,
      slug: `/${x.slug}`,
      status: x.status,
      actions: {
        edit: {name: 'admin.children.pages.edit', params: {id: x.id}},
        delete: true
      }
    }
  })
})

onMounted(async () => {
  await loadPages(1, pageSize)
})

async function loadPages(pageIndex: number, size: number) {
  pagesAreLoading.value = true
  const response = await pageService.getAll(pageIndex, size, statusFilter.value || undefined)
  if (response) {
    paginatedResponse.value = response
    if (response.items)
      pageItems.value = response.items
  }
  pagesAreLoading.value = false
}

async function onDelete(item: any) {
  if (preventMultipleSubmit.value) return
  preventMultipleSubmit.value = true

  const confirmDelete = confirm(t('pages.pages.delete.confirmation'))
  if (!confirmDelete) {
    preventMultipleSubmit.value = false
    return
  }

  const response = await pageService.delete(item.id)
  if (response && response.succeeded) {
    const index = pageItems.value.findIndex(x => x.id === item.id)
    if (index !== -1) pageItems.value.splice(index, 1)
    notifySuccess(t('pages.pages.delete.validation.successMessage'))
  } else {
    notifyError(t('pages.pages.delete.validation.failedMessage'))
  }
  preventMultipleSubmit.value = false
}

const pageHeaders = computed(() => [
  {text: t("global.title"), value: 'title', width: 200},
  {text: t("pages.pages.slug"), value: "slug", width: 200},
  {text: t("pages.pages.status"), value: "status", width: 100},
  {text: t("global.table.actions"), value: "actions", width: 150}
])
</script>
