<template>
  <div class="content-grid content-grid--subpage content-grid--subpage-table">
    <div class="content-grid__header">
      <h1 class="back-link">{{ t('routes.admin.children.pages.name') }}</h1>
      <div class="hidden md:block">
        <BtnLink
          :name="t('routes.admin.children.pages.add.name')"
          :path="{ path: t('routes.admin.children.pages.add.fullPath') }"
        />
      </div>
    </div>
    <div class="md:hidden">
      <BtnLink
        class="mt-10"
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
      v-model:server-options="serverOptions"
      :search-value="''"
      @delete="onDelete"
      @duplicate="onDuplicate"
    />
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue-i18n"
import {computed, ref, watch} from "vue"
import {useRouter} from "vue-router"
import type {ServerOptions} from "vue3-easy-data-table"
import {usePageService} from "@/serviceRegistry"
import {Page} from "@/types/entities"
import {PaginatedResponse} from "@/types/responses"
import DataTable from "@/components/layouts/items/DataTable.vue"
import BtnLink from "@/components/layouts/items/BtnLink.vue"
import Loader from "@/components/layouts/items/Loader.vue"
import {Tables} from "@/types/enums"
import {notifyError} from "@/notify"

const {t} = useI18n()
const router = useRouter()
const pageService = usePageService()

const preventMultipleSubmit = ref(false)
const pagesAreLoading = ref(false)
const pageItems = ref<Page[]>([])
const paginatedResponse = ref<PaginatedResponse<Page>>({totalItems: 0})
const pageSize = Tables.DefaultRowsPerPage
const serverOptions = ref<ServerOptions>({
  page: 1,
  rowsPerPage: pageSize
})

const statusLabels: Record<string, string> = {
  Published: t('pages.pages.published'),
  Draft: t('pages.pages.draft'),
}

const tablePages = computed(() => {
  return pageItems.value.map((x: Page) => {
    return {
      id: x.id,
      title: x.title,
      slug: `/${x.slug}`,
      status: statusLabels[x.status ?? ''] ?? x.status,
      statusRaw: x.status,
      actions: {
        edit: {name: 'admin.children.pages.edit', params: {id: x.id}},
        duplicate: true,
        delete: true
      }
    }
  })
})

watch(serverOptions, async (value) => {
  await loadPages(value.page, value.rowsPerPage)
}, {deep: true, immediate: true})

async function loadPages(pageIndex: number, size: number) {
  pagesAreLoading.value = true
  const response = await pageService.getAll(pageIndex, size)
  if (response) {
    paginatedResponse.value = response
    if (response.items)
      pageItems.value = response.items
  }
  pagesAreLoading.value = false
}

async function onDuplicate(item: any) {
  if (preventMultipleSubmit.value) return
  preventMultipleSubmit.value = true

  try {
    const duplicated = await pageService.duplicate(item.id)
    if (duplicated && duplicated.id) {
      router.push({name: 'admin.children.pages.edit', params: {id: duplicated.id}})
    } else {
      notifyError(t('pages.pages.duplicate.validation.failedMessage'))
    }
  } catch {
    notifyError(t('pages.pages.duplicate.validation.failedMessage'))
  } finally {
    preventMultipleSubmit.value = false
  }
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
    await loadPages(serverOptions.value.page, serverOptions.value.rowsPerPage)
  }
  preventMultipleSubmit.value = false
}

const pageHeaders = computed(() => [
  {text: t("global.title"), value: 'title', width: 150},
  {text: t("pages.pages.slug"), value: "slug", width: 150},
  {text: t("pages.pages.status"), value: "status", width: 120},
  {text: t("global.table.actions"), value: "actions", width: 120}
])
</script>
