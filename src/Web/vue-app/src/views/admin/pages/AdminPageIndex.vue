<template>
  <div class="content-grid content-grid--subpage content-grid--subpage-table">
    <div class="content-grid__header">
      <h1 class="back-link">{{ t(`routes.admin.children.pages.name`) }}</h1>
    </div>
    <DataTable
        :headers="pageHeaders"
        :is-loading="pagesAreLoading"
        :items="tablePages"
        :total-items="pages.length"
    />
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n";
import {computed, onMounted, ref} from "vue";
import {usePageService} from "@/inversify.config";
import {Page} from "@/types/entities";
import DataTable from "@/components/layouts/items/DataTable.vue";

const {t} = useI18n()
const pageService = usePageService()

const pagesAreLoading = ref(false);
const pages = ref<Page[]>([]);

const tablePages = computed(() => {
  return pages.value.map((x: Page) => {
    return {
      id: x.id,
      title: x.title,
      slug: x.slug,
      sectionsCount: x.sections?.length ?? 0,
      actions: {
        edit: {name: `admin.children.pages.edit`, params: {id: x.id}},
      }
    }
  })
})

onMounted(async () => {
  await loadPages();
});

async function loadPages() {
  pagesAreLoading.value = true;
  const response = await pageService.getAll();
  if (response) {
    pages.value = response;
  }
  pagesAreLoading.value = false;
}

const pageHeaders = computed(() => [
  {
    text: t("pages.pages.title"),
    value: 'title',
    width: 200,
  },
  {
    text: t("pages.pages.slug"),
    value: "slug",
    width: 200,
  },
  {
    text: t("pages.pages.sectionsCount"),
    value: "sectionsCount",
    width: 100,
  },
  {
    text: t("global.table.actions"),
    value: "actions",
    width: 100
  },
])
</script>
