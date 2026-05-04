<template>
  <EasyDataTable
      :empty-message="t('global.table.noData')"
      :filter-options="filterOptions"
      :headers="headers"
      :hide-footer="isSoloItem"
      :hide-rows-per-page="true"
      :items="items"
      :loading="isLoading"
      :server-items-length="totalItems"
      :server-options="serverOptions ?? null"
      :rows-of-page-separator-message="t('global.table.of')"
      :rows-per-page="serverOptions?.rowsPerPage ?? (isSoloItem ? 1 : 10)"
      :search-value="searchValue"
      :table-min-height="0"
      alternating
      buttons-pagination
      header-item-class-name="vue3-easy-data-table__header-item"
      :theme-color="primaryColor"
      @update:server-options="handleServerOptionsUpdate"
  >
    <template #item-status="item">
      <span class="tag" :class="item.statusRaw === 'Published' ? 'tag--published' : item.statusRaw === 'Draft' ? 'tag--draft' : ''">
        {{ item.status }}
      </span>
    </template>
    <template #item-actions="item">
      <p v-if="item && item.actions" class="vue3-easy-data-table__actions">
        <router-link
            v-if="item.actions.view"
            :to="item.actions.view"
            :aria-label="t('global.actions.view')"
            class="vue3-easy-data-table__action"
            :title="t('global.actions.view')"
        >
          <Eye :size="16" color="white" />
        </router-link>
        <router-link
            v-if="item.actions.edit"
            :to="item.actions.edit"
            :aria-label="t('global.actions.update')"
            class="vue3-easy-data-table__action"
            :title="t('global.actions.update')"
        >
          <Pencil :size="16" color="white" />
        </router-link>
        <button
            v-if="item.actions.duplicate && item.id"
            class="vue3-easy-data-table__action"
            type="button"
            aria-label="Dupliquer"
            title="Dupliquer"
            @click="handleDuplicate(item)"
        >
          <Copy :size="16" color="white" />
        </button>
        <button
            v-if="item.actions.delete && item.id"
            class="vue3-easy-data-table__action"
            type="button"
            :aria-label="t('global.actions.delete')"
            :title="t('global.actions.delete')"
            @click="handleDelete(item)"
        >
          <Trash2 :size="16" color="white" />
        </button>
      </p>
    </template>

  </EasyDataTable>
</template>

<script lang="ts" setup>
import Vue3EasyDataTable from "vue3-easy-data-table"
import "vue3-easy-data-table/dist/style.css"
import type {FilterOption, Header, Item, ServerOptions} from "vue3-easy-data-table"
import {useI18n} from "vue-i18n"
import { Copy, Eye, Pencil, Trash2 } from "lucide-vue-next"
import { computed } from "vue"
import type { Component } from "vue"

const EasyDataTable: Component = Vue3EasyDataTable
const {t} = useI18n()

const primaryColor = computed(() =>
  getComputedStyle(document.documentElement).getPropertyValue('--primary').trim() || '#be1e2c'
)

withDefaults(defineProps<{
  headers: Header[],
  items: Item[],
  filterOptions?: FilterOption[],
  isLoading?: boolean,
  searchValue?: string
  isSoloItem?: boolean
  totalItems?: number
  serverOptions?: ServerOptions
}>(), {
  totalItems: 0,
  serverOptions: undefined
})

const emit = defineEmits<{
  (event: "delete", item: any): void
  (event: "duplicate", item: any): void
  (event: "update:serverOptions", options: ServerOptions): void
}>()

function handleDelete(item: any) {
  emit("delete", item)
}

function handleDuplicate(item: any) {
  emit("duplicate", item)
}

function handleServerOptionsUpdate(options: ServerOptions) {
  emit("update:serverOptions", options)
}
</script>
