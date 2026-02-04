<template>
  <EasyDataTable
      :empty-message="t('global.table.noData')"
      :filter-options="filterOptions"
      :headers="headers"
      :hide-footer="isSoloItem"
      :hide-rows-per-page="true"
      :items="items"
      :loading="isLoading"
      :rows-of-page-separator-message="t('global.table.of')"
      :rows-per-page="isSoloItem ? 1 : 10"
      :search-value="searchValue"
      :table-min-height="0"
      alternating
      buttons-pagination
      header-item-class-name="vue3-easy-data-table__header-item"
      theme-color="#528965"
  >
    <template #item-status="item">
      <div class="tag">
        <p>{{ item.status }}</p>
      </div>
    </template>
    <template #item-actions="item">
      <p v-if="item && item.actions" class="vue3-easy-data-table__actions">
        <router-link
            v-if="item.actions.view"
            v-tippy="t(`global.actions.view`)"
            :to="item.actions.view"
            class="vue3-easy-data-table__action"
        >
          <IconView class="icon icon--black"/>
        </router-link>
        <router-link
            v-if="item.actions.edit"
            v-tippy="t(`global.actions.update`)"
            :to="item.actions.edit"
            class="vue3-easy-data-table__action"
        >
          <IconEdit class="icon icon--black"/>
        </router-link>
        <button
            v-if="item.actions.delete && item.id"
            v-tippy="t(`global.actions.delete`)"
            class="vue3-easy-data-table__action red-bg"
            type="button"
            @click="handleDelete(item)"
        >
          <IconDelete class="icon icon--black"/>
        </button>
      </p>
    </template>

  </EasyDataTable>
</template>

<script lang="ts" setup>
import type {FilterOption, Header, Item} from "vue3-easy-data-table"
import {useI18n} from "vue3-i18n"
import IconEdit from "@/assets/icons/icon__edit.svg"
import IconDelete from "@/assets/icons/icon__delete.svg"
import IconView from "@/assets/icons/icon__view.svg"

const {t} = useI18n()

// eslint-disable-next-line
defineProps<{
  headers: Header[],
  items: Item[],
  filterOptions?: FilterOption[],
  isLoading?: boolean,
  searchValue?: string
  isSoloItem?: boolean
}>()

// eslint-disable-next-line
const emit = defineEmits<{
  (event: "delete", item: any): void
}>()

function handleDelete(item: any) {
  emit("delete", item)
}
</script>
